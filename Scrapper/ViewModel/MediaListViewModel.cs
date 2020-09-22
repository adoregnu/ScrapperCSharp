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
using CefSharp.Enums;
using System.Windows.Interop;

namespace Scrapper.ViewModel
{
    enum MediaListMenuType
    { 
        excluded, downloaded
    }
    class MediaListViewModel : Pane
    {
        readonly FileSystemWatcher _fsWatcher;

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
            //MediaList = new ObservableCollection<MediaItem>();
            var attr = File.GetAttributes(MediaPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                //Task.Run(() => Parallel.ForEach(fsEntries, GetMedia));
                var fsEntries = Directory.GetDirectories(MediaPath);
                if (fsEntries.Length > 0)
                {
                    Task.Run(() => IterateDirectories(fsEntries));
                }
                else
                {
                    //var dirName = Path.GetFileName(MediaPath);
                    MessengerInstance.Send(new NotificationMessage<string>(MediaPath, "MediaPath"));
                }
            }
        }

        bool IterateDirectories(string[] directories)
        {
            if (directories.Length == 0)
                return false;

            try
            {
                foreach (var dir in directories)
                {
                    if (!IterateDirectories(Directory.GetDirectories(dir)))
                    {
                        GetMedia(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
            return true;
        }


        void GetMedia(string path)
        {
            var item = new MediaItem();
            foreach (var file in Directory.GetFileSystemEntries(path))
            {
                item.SetField(file);
                if (item.IsExcluded || item.IsDownload)
                    return;
            }

            if (!item.IsMediaDir)
                return;

            var idx = MediaList.FindItem(item, i => i.DownloadDt);
            UiServices.Invoke(delegate
            {
                //MediaList.InsertInPlace(item, i => i.DownloadDt);
                MediaList.Insert(idx, item);
            }, true);
            Thread.Sleep(40);
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
