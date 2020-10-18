using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using FileListView.Interfaces;
using FileSystemModels;
using FileSystemModels.Browse;
using FileSystemModels.Events;
using FileSystemModels.Interfaces;
using FileSystemModels.Models.FSItems.Base;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Scrapper.Tasks;

namespace Scrapper.ViewModel
{
    class FileListViewModel : ViewModelBase
    {
		readonly SemaphoreSlim _SlowStuffSemaphore;
		readonly CancellationTokenSource _CancelTokenSource;
		readonly OneTaskLimitedScheduler _OneTaskScheduler;
		readonly IFileListNotifier _fileListNotifier;
		bool _disposed = false;
		bool _isAllChecked = false;

		/// <summary>
		/// Expose a viewmodel that can support a listview showing folders and files
		/// with their system specific icon.
		/// </summary>
		public IFileListViewModel FolderItemsView { get; protected set; }

		/// <summary>
		/// Gets/sets the currently selected folder path string.
		/// </summary>
		string _selectedFoder = string.Empty;
		public string SelectedFolder
		{
			get => _selectedFoder;
			set
			{
				if (_selectedFoder != value)
				{
					_isAllChecked = false;
					_fileListNotifier.OnDirectoryChanged(value);
				}
				Set(ref _selectedFoder, value);
			}
		}
		public bool IsAllChecked
		{
			get => _isAllChecked;
			set
			{
				if (_isAllChecked != value)
				{
					_isAllChecked = value;
					OnCheckAll(value);
				}
			}
		}

		public ICommand CmdUpDir { get; set; }
		public ICommand CmdRefreshDir { get; set; }
		public ICommand CmdFileSelect { get; set; }
		public ICommand CmdCheckChange { get; set; }
		//public ICommand DeleteCommand { get; set; }

		public FileListViewModel(IFileListNotifier notifier)
		{
			_SlowStuffSemaphore = new SemaphoreSlim(1, 1);
			_CancelTokenSource = new CancellationTokenSource();
			_OneTaskScheduler = new OneTaskLimitedScheduler();
			_fileListNotifier = notifier;

			FolderItemsView = FileListView.Factory.CreateFileListViewModel();
			CmdUpDir = new RelayCommand<object>(p => OnUpCommand(), p => CanUpCommand());
			CmdRefreshDir = new RelayCommand<object>(
				p => NavigateToFolder(PathFactory.Create(_selectedFoder)));
			CmdFileSelect = new RelayCommand<object>( p => {
                if (p is ILVItemViewModel fsItem)
                {
                    _fileListNotifier.OnFileSelected(fsItem);
                }
			});
			CmdCheckChange = new RelayCommand<object>(
				p => _fileListNotifier.OnCheckboxChanged(p as ILVItemViewModel));
			//DeleteCommand = new RelayCommand<object>(p => OnDeleteFile(p));

			// This is fired when the current folder in the listview changes to another existing folder
			WeakEventManager<ICanNavigate, BrowsingEventArgs>
				.AddHandler(FolderItemsView, "BrowseEvent", Control_BrowseEvent);

            // This event is fired when a user opens a file
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .AddHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);

            MessengerInstance.Register<NotificationMessage<bool>>(
                this, OnDialogMessage);
        }

        void OnUpCommand()
		{
			var dir = Directory.GetParent(_selectedFoder);
			NavigateToFolder(PathFactory.Create(dir.FullName));
		}

		bool CanUpCommand()
		{
            if (string.IsNullOrEmpty(_selectedFoder))
                return false;

			try
			{
				if (Directory.GetParent(_selectedFoder) != null)
					return true;
			}
			catch
			{ }
			return false;
		}

		void OnDialogMessage(NotificationMessage<bool> msg)
		{ 
			NavigateToFolder(PathFactory.Create(_selectedFoder));
		}

		public void RemoveItem(string path)
		{ 
			var items = FolderItemsView.CurrentItems
							as ObservableCollection<ILVItemViewModel>;
			var item = items.FirstOrDefault(i => i.ItemPath == path);
			if (item != null)
				items.Remove(item);
		}
#if false
		void OnDeleteFile(object param)
		{
			var fsItem = param as ILVItemViewModel;
			_fileListNotifier.OnFileDeleted(fsItem);
			var items = FolderItemsView.CurrentItems
							as ObservableCollection<ILVItemViewModel>;
			items.Remove(fsItem);

			Task.Run(() => {
				Thread.Sleep(300);
				try
				{
					if (fsItem.ItemType == FSItemType.Folder)
						Directory.Delete(fsItem.ItemPath, true);
					else
						File.Delete(fsItem.ItemPath);
				}
				catch (Exception ex)
				{
					Log.Print(ex.Message);
				}
			});
		}
#endif
		void OnCheckAll(bool check)
		{ 
			foreach (var item in FolderItemsView.CurrentItems)
			{
				item.IsChecked = check;
			}
		}

		/// <summary>
		/// Master controller interface method to navigate all views
		/// to the folder indicated in <paramref name="folder"/>
		/// - updates all related viewmodels.
		/// </summary>
		/// <param name="itemPath"></param>
		/// <param name="requestor"</param>
		public void NavigateToFolder(IPathModel itemPath)
		{
			// XXX Todo Keep task reference, support cancel, and remove on end?
			try
			{
				// XXX Todo Keep task reference, support cancel, and remove on end?
				var timeout = TimeSpan.FromSeconds(5);
				var actualTask = new Task(() =>
				{
					var request = new BrowseRequest(itemPath, _CancelTokenSource.Token);

					var t = Task.Factory.StartNew(() => NavigateToFolderAsync(request, null),
														request.CancelTok,
														TaskCreationOptions.LongRunning,
														_OneTaskScheduler);

					if (t.Wait(timeout) == true)
						return;

					_CancelTokenSource.Cancel();       // Task timed out so lets abort it
					return;                     // Signal timeout here...
				});

				actualTask.Start();
				actualTask.Wait();
			}
			catch (AggregateException e)
			{
				Log.Print(e.Message, e);
			}
			catch (Exception e)
			{
				Log.Print(e.Message, e);
			}
		}

		/// <summary>
		/// Master controler interface method to navigate all views
		/// to the folder indicated in <paramref name="folder"/>
		/// - updates all related viewmodels.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="requestor"</param>
		private async Task<FinalBrowseResult> NavigateToFolderAsync(
			BrowseRequest request, object sender)
		{
			// Make sure the task always processes the last input but is not started twice
			await _SlowStuffSemaphore.WaitAsync();
			try
			{
				var newPath = request.NewLocation;
				var cancel = request.CancelTok;

				if (cancel != null)
					cancel.ThrowIfCancellationRequested();

				FolderItemsView.SetExternalBrowsingState(true);
				FinalBrowseResult browseResult = null;
                // Navigate Folder/File ListView to this folder
                browseResult = await FolderItemsView.NavigateToAsync(request);

				if (cancel != null)
					cancel.ThrowIfCancellationRequested();
				if (browseResult.Result == BrowseResult.Complete)
				{
					SelectedFolder = newPath.Path;
				}
				return browseResult;
			}
			catch (Exception exp)
			{
				var result = FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
				result.UnexpectedError = exp;
				return result;
			}
			finally
			{
				FolderItemsView.SetExternalBrowsingState(false);

				_SlowStuffSemaphore.Release();
			}
		}
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Source: http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed == false)
			{
				if (disposing == true)
				{
					// Dispose of the curently displayed content
					_OneTaskScheduler.Dispose();
					_SlowStuffSemaphore.Dispose();
					_CancelTokenSource.Dispose();
				}

				// There are no unmanaged resources to release, but
				// if we add them, they need to be released here.
			}

			_disposed = true;

			//// If it is available, make the call to the
			//// base class's Dispose(Boolean) method
			////base.Dispose(disposing);
		}

		/// <summary>
		/// Executes when the file open event is fired and class was constructed 
		/// with statndard constructor.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FolderItemsView_OnFileOpen(object sender, FileOpenEventArgs e)
		{
			MessageBox.Show("File Open:" + e.FileName);
		}

		/// <summary>
		/// A control has send an event that it has (been) browsing to a new location.
		/// Lets sync this with all other controls.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Control_BrowseEvent(object sender, BrowsingEventArgs e)
		{
			if (e.IsBrowsing == false && e.Result == BrowseResult.Complete)
			{
				var request = new BrowseRequest(e.Location);
                // Navigate Folder/File ListView to this folder
                FinalBrowseResult browseResult
                    = FolderItemsView.NavigateTo(request);
                if (browseResult.Result != BrowseResult.Complete)
                    return;
				SelectedFolder = e.Location.Path;
			}
			else
			{
				if (e.IsBrowsing == true)
				{
					if (FolderItemsView != sender)
					{
						// Navigate Folder/File ListView to this folder
						FolderItemsView.SetExternalBrowsingState(true);
					}
				}
			}
		}
	}
}
