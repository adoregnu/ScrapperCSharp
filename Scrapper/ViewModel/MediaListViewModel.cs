using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Unosquare.FFME;
using Unosquare.FFME.Common;

using Scrapper.Extension;
using Scrapper.Model;
using Scrapper.ViewModel.Base;

namespace Scrapper.ViewModel
{
    enum MediaListMenuType
    { 
        excluded, downloaded
    }
    class MediaListViewModel : Pane
    {
        readonly FileSystemWatcher _fsWatcher;
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
        MediaItem _mediaItem = null;
        public MediaItem MediaItem
        {
            get => _mediaItem;
            set
            {
                _mediaItem = value;
                RaisePropertyChanged("MediaItem");
            }
        }

        public ObservableCollection<MediaItem> MediaList { get; set; } =
            new ObservableCollection<MediaItem>();
        public List<string> Screenshots { get; set; } = null;

        public ICommand CmdExclude { get; set; }
        public ICommand CmdDownload { get; set; }

        public MediaListViewModel()
        {
            Title = "Media List";

            CmdExclude = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.excluded));
            CmdDownload = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.downloaded));

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
            MessengerInstance.Register<NotificationMessageAction<string>>(
                this, OnQueryMediaPath);

            MediaElement.FFmpegMessageLogged +=
                OnMediaFFmpegMessageLogged;
        }

        void OnMediaFFmpegMessageLogged(object sender, MediaLogMessageEventArgs e)
        {
            if (e.MessageType != MediaLogMessageType.Warning &&
                e.MessageType != MediaLogMessageType.Error)
                return;

            if (string.IsNullOrWhiteSpace(e.Message) == false &&
                e.Message.ContainsOrdinal("Using non-standard frame rate"))
                return;

            //Debug.WriteLine(e);
            Log.Print(e.Message);
        }

        void UpdateMedia()
        {
            MediaList.Clear();
            if (!File.GetAttributes(MediaPath).HasFlag(FileAttributes.Directory))
            {
                return;
            }

            var fsEntries = Directory.GetDirectories(MediaPath);
            Task.Run(() => {
                if (IterateDirectories(fsEntries, 0))
                    return;

                UiServices.Invoke(delegate {
                    MediaItem = GetMedia(MediaPath);
                }, true);
            });
#if false
            //MediaList = new ObservableCollection<MediaItem>();
            var attr = File.GetAttributes(MediaPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                var fsEntries = Directory.GetDirectories(MediaPath);
                if (fsEntries.Length > 0)
                {
                    MediaItem = null;
                    Task.Run(() => IterateDirectories(fsEntries));
                }
                else
                {
                    MediaItem = GetMedia(MediaPath);
                    RaisePropertyChanged("MediaItem");
                    if (string.IsNullOrEmpty(MediaItem.BgImagePath))
                        MessengerInstance.Send(new NotificationMessage<string>(
                            MediaPath, "MediaPath"));
                }
            }
#endif
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

            _mediaCache.Add(item.MediaPath, item);
            return item;
        }

        void InsertMedia(string path)
        {
            var item = GetMedia(path);
            if (item == null) return;

            var idx = MediaList.FindItem(item, i => i.DownloadDt);
            UiServices.Invoke(delegate
            {
                //MediaList.InsertInPlace(item, i => i.DownloadDt);
                MediaList.Insert(idx, item);
            }, true);
            Thread.Sleep(50);
        }

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
        }

        public void OnQueryMediaPath(NotificationMessageAction<string> msgAction)
        {
            msgAction.Execute(MediaPath);
        }
    }
}
