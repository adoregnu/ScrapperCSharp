using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Scrapper.Extension;
using Scrapper.Model;
using Scrapper.Tasks;

namespace Scrapper.ViewModel
{
    enum MediaListMenuType
    { 
        excluded, downloaded
    }
    class MediaListViewModel : ViewModelBase
    {
		private readonly SemaphoreSlim _SlowStuffSemaphore;
		private readonly CancellationTokenSource _CancelTokenSource;
		private readonly OneTaskLimitedScheduler _OneTaskScheduler;

        //readonly FileSystemWatcher _fsWatcher;
        Dictionary<string, MediaItem> _mediaCache
            = new Dictionary<string, MediaItem>();

        string _mediaPath;
        public string MediaPath
        {
            get => _mediaPath;
            set
            {
                _mediaPath = value;
                UpdateMedia();
                RaisePropertyChanged("MediaPath");
            }
        }
        MediaItem _selectedMedia = null;
        public MediaItem SelectedMedia
        {
            get => _selectedMedia;
            set
            {
                _selectedMedia = value;
                if (value != null)
                {
                    Screenshots = value.Screenshots;
                }
                else
                {
                    Screenshots = null;
                }
                RaisePropertyChanged("Screenshots");
            }
        }

        //public ObservableCollection<MediaItem> MediaList { get; set; } =
        //    new ObservableCollection<MediaItem>();
        public List<MediaItem> MediaList { get; set; }// = new List<MediaItem>();
        public List<string> Screenshots { get; set; } = null;

        public ICommand CmdExclude { get; set; }
        public ICommand CmdDownload { get; set; }

        public MediaListViewModel()
        {
			_SlowStuffSemaphore = new SemaphoreSlim(1, 1);
			_CancelTokenSource = new CancellationTokenSource();
			_OneTaskScheduler = new OneTaskLimitedScheduler();

            CmdExclude = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.excluded));
            CmdDownload = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.downloaded));

#if false
            _mediaPath = App.CurrentPath;
            try
            {
                UpdateMedia();
                _fsWatcher = new FileSystemWatcher
                {
                    Path = MediaPath,
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.FileName,
                    IncludeSubdirectories = true
                };
                _fsWatcher.Created += new FileSystemEventHandler(OnChanged);
                _fsWatcher.Deleted += new FileSystemEventHandler(OnChanged);
                _fsWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message, ex);
            }
#endif
            MessengerInstance.Register<NotificationMessageAction<string>>(
                this, OnQueryMediaPath);
            
            MessengerInstance.Register<NotificationMessage<string>>(
                this, OnMediaUpdated);
        }

        List<MediaItem> _tempList;
        void UpdateMedia()
        {
            //MediaList.Clear();
            _tempList = new List<MediaItem>();
            if (!File.GetAttributes(MediaPath).HasFlag(FileAttributes.Directory))
            {
                return;
            }

            var fsEntries = Directory.GetDirectories(MediaPath);
            Task.Run(() => {
                IterateDirectories(fsEntries, 0);
                UiServices.Invoke(delegate {
                    MediaList = _tempList;
                    RaisePropertyChanged("MediaList");
                }, true);
            });
        }

        bool IterateDirectories(string[] directories, int level)
        {
            if (directories.Length == 0)
                return false;

            try
            {
                foreach (var dir in directories)
                {
                    var dirs = Directory.GetDirectories(dir);
                    if (!IterateDirectories(dirs, level + 1))
                    {
                        InsertMedia(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
            return true;
        }
#if false
        void UpdateCache()
        {
            var files = Directory.GetFileSystemEntries(MediaPath);
            foreach (var file in files)
            {
                MediaItem.SetField(file);
            }
            RaisePropertyChanged("MediaItem");
        }
#endif

        MediaItem GetMedia(string path)
        {
            if (_mediaCache.ContainsKey(path))
            {
                return _mediaCache[path];
            }

            var item = new MediaItem();
            var files = Directory.GetFileSystemEntries(path);
            foreach (var file in files)
            {
                item.SetField(file);
                if (item.IsExcluded || item.IsDownload)
                    return null;
            }

            if (!item.IsMediaDir)
            {
                return null;
            }

            _mediaCache.Add(path, item);
            return item;
        }

        void InsertMedia(string path)
        {
            var item = GetMedia(path);
            if (item == null) return;

            //var idx = MediaList.FindItem(item, i => i.DownloadDt);
            //UiServices.Invoke(delegate
            {
                //MediaList.InsertInPlace(item, i => i.DownloadDt);
                //MediaList.Insert(idx, item);
                _tempList.Add(item);
                Log.Print(item.MediaPath);
            }//, true);
            //Thread.Sleep(50);
        }
#if false
        void AddNewMedia(string path)
        {
            Log.Print($"AddNewMedia: {path}");
        }

        void RemoveMedia(string path)
        {
            Log.Print($"RemoveMedia : {path}");
        }
        void OnChanged(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    RemoveMedia(e.FullPath);
                    break;
                case WatcherChangeTypes.Created:
                    AddNewMedia(e.FullPath);
                    break;
            }
        }
#endif

        void OnContextMenu(MediaItem item, MediaListMenuType type)
        {
            if (item == null) return;
            Log.Print(item.MediaPath);
            var dir = Path.GetDirectoryName(item.MediaPath);
            if (type == MediaListMenuType.downloaded)
            {
                try
                {
                    var torrent = Path.GetFileName(item.Torrent);
                    File.Copy(item.Torrent, @"z:\Downloads\" + torrent);
                    File.Create($"{dir}\\.{type}").Dispose();
                    MediaList.Remove(item);
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, ex);
                }
            }
            else
            { 
                File.Create($"{dir}\\.{type}").Dispose();
                MediaList.Remove(item);
            }
            _mediaCache.Remove(dir);
        }

        void OnQueryMediaPath(NotificationMessageAction<string> msgAction)
        {
            msgAction.Execute(MediaPath);
        }

        void OnMediaUpdated(NotificationMessage<string> msg)
        {
            if (msg.Notification == "mediaUpdated")
            {
                //if (MediaItem != null) UpdateCache();
                //else InsertMedia(msg.Content);
            }
        }
    }
}
