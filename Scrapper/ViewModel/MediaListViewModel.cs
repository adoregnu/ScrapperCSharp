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

using FileListView.Interfaces;
using FileSystemModels.Models.FSItems.Base;

using Scrapper.Extension;
using Scrapper.Model;
using Scrapper.Tasks;
using FFmpeg.AutoGen;

namespace Scrapper.ViewModel
{
    enum MediaListMenuType
    { 
        excluded, downloaded
    }
    class MediaListViewModel : ViewModelBase
    {
        //readonly FileSystemWatcher _fsWatcher;
        Dictionary<string, MediaItem> _mediaCache
            = new Dictionary<string, MediaItem>();
        IEnumerable<ILVItemViewModel> _currentFiles;
        bool _isBrowsing = false;

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
        public string CurrentFolder { get; set; }
        public bool IsBrowsing
        {
            get => _isBrowsing;
            set
            {
                if (_isBrowsing != value)
                {
                    _isBrowsing = value;
                    RaisePropertyChanged("IsBrowsing");
                }
            }
        }

        public ICommand CmdExclude { get; set; }
        public ICommand CmdDownload { get; set; }

        public MediaListViewModel()
        {
            CmdExclude = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.excluded));
            CmdDownload = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.downloaded));

#if false
            try
            {
                UpdateMedia();
                _fsWatcher = new FileSystemWatcher
                {
                    Path = CurrentFolder,
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
        }

        readonly List<MediaItem> _deselectedItems = new List<MediaItem>();
        public void UpdateMediaList(ILVItemViewModel item)
        {
            if (item.IsChecked)
            {
                var medias = _deselectedItems.FindAll(
                                i => i.MediaPath.StartsWith(item.ItemPath));
                foreach (var media in medias)
                {
                    MediaList.InsertInPlace(media, i => i.DownloadDt);
                    _deselectedItems.Remove(media);
                }
            }
            else
            {
                var medias = MediaList.Cast<MediaItem>().Where(
                    i => i.MediaPath.StartsWith(item.ItemPath,
                        StringComparison.CurrentCultureIgnoreCase)).ToList();
                medias.ForEach(x => MediaList.Remove(x));
                _deselectedItems.AddRange(medias);
            }
        }

        public void RefreshMediaList(IEnumerable<ILVItemViewModel> currentFiles)
        {
            MediaList.Clear();
            _currentFiles = currentFiles;

            IsBrowsing = true;

            Task.Run(() => {
                if (_deselectedItems.Count > 0)
                    _deselectedItems.Clear();

                foreach (var file in _currentFiles)
                {
                    if (!file.IsChecked) continue;
                    if (file.ItemType == FSItemType.Folder)
                    {
                        UpdateMediaListInternal(file.ItemPath);
                    }
                }
                UiServices.Invoke(delegate {
                    IsBrowsing = false;
                });
            });
        }

        void UpdateMediaListInternal(string path)
        {
            var fsEntries = Directory.GetDirectories(path);
            if (fsEntries.Length == 0)
            {
                InsertMedia(path);
            }
            else
            {
                foreach (var fsEntry in fsEntries)
                {
                    UpdateMediaListInternal(fsEntry);
                }
            }
        }

        public MediaItem GetMedia(string path, bool updateCache = false)
        {
            MediaItem item = null;
            bool isCached = false;
            if (_mediaCache.ContainsKey(path))
            {
                if (!updateCache) return _mediaCache[path];
                item = _mediaCache[path];
                isCached = true;
            }

            if (!isCached) item = new MediaItem();
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
            if (!isCached) _mediaCache.Add(path, item);
            return item;
        }

        public void InsertMedia(string path)
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

        public void RemoveMedia(string path)
        {
            var medias = MediaList.Where(i => i.MediaPath.StartsWith(path,
                    StringComparison.CurrentCultureIgnoreCase)).ToList();
            medias.ForEach(x => MediaList.Remove(x));
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
            var dir = Path.GetDirectoryName(item.MediaPath);
            if (type == MediaListMenuType.downloaded)
            {
                try
                {
                    var torrent = Path.GetFileName(item.Torrent);
                    File.Copy(item.Torrent, @"z:\Downloads\" + torrent);
                    File.Create($"{dir}\\.{type}").Dispose();
                    MediaList.Remove(item);
                    Log.Print($"Makrk downloaded {item.Torrent}");
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
                Log.Print($"Mark excluded {item.Torrent}");
            }
            _mediaCache.Remove(dir);
        }
    }
}
