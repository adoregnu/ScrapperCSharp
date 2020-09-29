using System;
using System.Linq;
using System.Windows.Input;

using Unosquare.FFME;
using Unosquare.FFME.Common;

using FileSystemModels;
using FileSystemModels.Models.FSItems.Base;
using FileListView.Interfaces;

using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;

using Scrapper.ViewModel.Base;
using Scrapper.ViewModel.MediaPlayer;

namespace Scrapper.ViewModel
{
    class MediaViewModel : Pane, IFileListNotifier
    {
        int _viewType = 1;
        public int ViewType
        {
            get => _viewType;
            set
            {
                if (_viewType != value)
                {
                    _viewType = value;
                    RaisePropertyChanged("ViewType");
                }
            }
        }

        public MediaListViewModel MediaList { get; private set; }
        public PlayerViewModel MediaPlayer { get; private set; }
        public FileListViewModel FileList { get; private set; }

        //public ICommand KeyDownCommand { get; private set; }
        public MediaViewModel()
        {
            Title = "Media";

            FileList = new FileListViewModel(this);
            MediaList = new MediaListViewModel();
            MediaPlayer = new PlayerViewModel();

            //KeyDownCommand = new RelayCommand<EventArgs>(e => Log.Print(e.ToString()));
            MessengerInstance.Register<NotificationMessage<string>>(
                this, OnMediaUpdated);

            MessengerInstance.Register<NotificationMessageAction<string>>(
                this, OnQueryMediaPath);

            var path = PathFactory.Create(App.CurrentPath);
            FileList.NavigateToFolder(path);
        }
#if false
        void UpdateViewType(string path)
        {
            bool folderExist = FileList.FolderItemsView.CurrentItems
                                .Any(i => i.ItemType == FSItemType.Folder);
            var media = MediaList.GetMedia(path);
            if (folderExist || media == null)
            {
                ViewType = 1;
            }
            else
            {
                MediaPlayer.SetMediaItem(media);
                ViewType = 2;
            }
            MediaList.CurrentFolder = path;
            MediaList.RefreshMediaList(FileList.FolderItemsView.CurrentItems);
        }
#endif
        void IFileListNotifier.OnDirectoryChanged(string path)
        {
            bool folderExist = FileList.FolderItemsView.CurrentItems
                                .Any(i => i.ItemType == FSItemType.Folder);
            var media = MediaList.GetMedia(path);

            UiServices.Invoke(delegate {
                if (folderExist || media == null)
                {
                    ViewType = 1;
                }
                MediaList.CurrentFolder = path;
                MediaList.RefreshMediaList(FileList.FolderItemsView.CurrentItems);
            }, true);
        }

        string _selectedFile;
        void IFileListNotifier.OnFileSelected(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ViewType = 1;
                return;
            }
            _selectedFile = path;
            var media = MediaList.GetMedia(path);
            if (media == null) return;
            if (_viewType != 2) ViewType = 2;

            MediaPlayer.SetMediaItem(media);
        }

        void IFileListNotifier.OnCheckboxChanged(ILVItemViewModel item)
        {
			//Log.Print($"path :{item.ItemPath}, IsChecked: {item.IsChecked}");
            if (item.ItemType == FSItemType.Folder)
                MediaList.UpdateMediaList(item);
        }

        void IFileListNotifier.OnFileDeleted(ILVItemViewModel item)
        {
            //working on it...
            MediaList.RemoveMedia(item.ItemPath);
            //ViewType = 1;
        }

        /// <summary>
        /// Callend when it receives message from Spider class' OnScrapCompleted
        /// </summary>
        /// <param name="msg"></param>
        void OnMediaUpdated(NotificationMessage<string> msg)
        {
            if (msg.Notification == "mediaAdded")
            {
                if (ViewType == 1)
                {
                    MediaList.InsertMedia(msg.Content);
                }
            }
            else if (msg.Notification == "mediaUpdated")
            {
                if (ViewType == 2)
                {
                    var media = MediaList.GetMedia(msg.Content, true);
                    MediaPlayer.SetMediaItem(media);
                }
            }
        }

        void OnQueryMediaPath(NotificationMessageAction<string> msgAction)
        {
            if (msgAction.Notification == "queryCurrentPath")
                msgAction.Execute(FileList.SelectedFolder);
            else if (msgAction.Notification == "querySelectedPath")
                msgAction.Execute(_selectedFile);
        }
    }
}
