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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using FileListView.Interfaces;
using FileSystemModels.Models.FSItems.Base;

using Scrapper.Extension;
using Scrapper.Model;
using Scrapper.Tasks;
using Scrapper.ViewModel.MediaPlayer;
using System.Data.Entity.Core.Metadata.Edm;

namespace Scrapper.ViewModel
{
    enum MediaListMenuType
    { 
        excluded, downloaded, scrap
    }
    class MediaListViewModel : ViewModelBase
    {
        readonly Dictionary<string, MediaItem> _mediaCache;
        readonly SerialQueue _serialQueue = new SerialQueue();

        MediaItem _selectedMedia = null;
        bool _isBrowsing = false;

        public MediaItem SelectedMedia
        {
            get => _selectedMedia;
            set => Set(ref _selectedMedia, value);
        }
        public bool IsBrowsing
        {
            get => _isBrowsing;
            set => Set(ref _isBrowsing, value);
        }
        public ObservableCollection<MediaItem> MediaList { get; private set; }
        public PlayerViewModel Player { get; set; }
        public string CurrentFolder { get; set; }

        public ICommand CmdExclude { get; set; }
        public ICommand CmdDownload { get; set; }
        public ICommand CmdScrap { get; set; }

        public MediaListViewModel()
        {
            MediaList = new ObservableCollection<MediaItem>();
            _mediaCache = new Dictionary<string, MediaItem>();

            CmdExclude = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.excluded));
            CmdDownload = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.downloaded));
            CmdScrap = new RelayCommand<object>( p => OnScrap(p));
        }
        public void ClearMedia()
        {
            IsBrowsing = true;
            MediaList.Clear();
            IsBrowsing = false;
        }

        public void AddMedia(string itemPath)
        {
            _serialQueue.Enqueue(() => UpdateMediaListInternal(itemPath));
        }

        public void RemoveMedia(string path)
        {
            IsBrowsing = true;
            var medias = MediaList.Where(i => i.MediaPath.StartsWith(path,
                    StringComparison.CurrentCultureIgnoreCase)).ToList();
            medias.ForEach(x => MediaList.Remove(x));
            IsBrowsing = false; 
        }

        public void RefreshMediaList(IEnumerable<ILVItemViewModel> currentFiles)
        {
            IsBrowsing = true;
            MediaList.Clear();
            Task.Run(() => {
                foreach (var file in currentFiles)
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
                item.UpdateField(file);
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
            Thread.Sleep(10);
        }

        void OnScrap(object param)
        {
            var items = param as IList<object>;
            foreach (MediaItem it in items)
            {
                Log.Print(it.MediaPath);
            }
        }

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
