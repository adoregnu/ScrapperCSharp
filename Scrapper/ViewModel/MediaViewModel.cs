
using Unosquare.FFME;
using Unosquare.FFME.Common;

using FileSystemModels;
using FileSystemModels.Models.FSItems.Base;
using FileListView.Interfaces;

using GalaSoft.MvvmLight.Messaging;

using Scrapper.ViewModel.Base;
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
        public MediaPlayerViewModel MediaPlayer { get; private set; }
        public FileListViewModel FileList { get; private set; }

        public MediaViewModel()
        {
            Title = "Media";
            MediaElement.FFmpegMessageLogged += OnMediaFFmpegMessageLogged;

            FileList = new FileListViewModel(this);
            MediaList = new MediaListViewModel();
            MediaPlayer = new MediaPlayerViewModel();

            MessengerInstance.Register<NotificationMessage<string>>(
                this, OnMediaUpdated);

            MessengerInstance.Register<NotificationMessageAction<string>>(
                this, OnQueryMediaPath);

            var path = PathFactory.Create(App.CurrentPath);
            FileList.NavigateToFolder(path);
        }

        void UpdateViewType(string path)
        {
            var media = MediaList.GetMedia(path);
            if (media == null)
            {
                ViewType = 1;
            }
            else
            {
                MediaPlayer.SetMediaItem(media);
                ViewType = 2;
            }
        }

        void IFileListNotifier.OnDirectoryChanged(string path)
        {
            UiServices.Invoke(delegate {
                UpdateViewType(path);
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
            UpdateViewType(path);
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

        void OnMediaFFmpegMessageLogged(object sender, MediaLogMessageEventArgs e)
        {
            if (e.MessageType != MediaLogMessageType.Warning &&
                e.MessageType != MediaLogMessageType.Error)
                return;

            if (string.IsNullOrWhiteSpace(e.Message) == false &&
                e.Message.ContainsOrdinal("Using non-standard frame rate"))
                return;

            Log.Print(e.Message);
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
