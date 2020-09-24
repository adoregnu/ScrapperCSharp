using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileListView.Interfaces;
using FileSystemModels.Interfaces;

using GalaSoft.MvvmLight;

namespace Scrapper.ViewModel
{
    class FileListViewModel : ViewModelBase
    {
		/// <summary>
		/// Expose a viewmodel that can support a listview showing folders and files
		/// with their system specific icon.
		/// </summary>
		public IFileListViewModel FolderItemsView { get; protected set; }

		/// <summary>
		/// Gets/sets the currently selected folder path string.
		/// </summary>
		string SelectedFolder { get; }


		public FileListViewModel()
		{ 
			FolderItemsView = FileListView.Factory.CreateFileListViewModel();
		}

		/// <summary>
		/// Master controler interface method to navigate all views
		/// to the folder indicated in <paramref name="folder"/>
		/// - updates all related viewmodels.
		/// </summary>
		/// <param name="itemPath"></param>
		/// <param name="requestor"</param>
		void NavigateToFolder(IPathModel itemPath)
		{
		}


	}
}
