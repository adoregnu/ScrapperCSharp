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
using GalaSoft.MvvmLight.Messaging;

using FileListView.Interfaces;
using FileSystemModels.Models.FSItems.Base;

using Scrapper.Extension;
using Scrapper.Model;
using Scrapper.Tasks;
using Scrapper.Spider;

namespace Scrapper.ViewModel
{
    using SpiderEnum = IEnumerable<SpiderBase>;

    enum MediaListMenuType
    { 
        excluded, downloaded, scrap
    }
    class MediaListViewModel : ViewModelBase
    {
        static readonly SerialQueue _serialQueue = new SerialQueue();

        MediaItem _selectedMedia = null;
        bool _isBrowsing = false;

        public MediaItem SelectedMedia
        {
            get => _selectedMedia;
            set
            {
                if (_selectedMedia != value)
                {
                    MessengerInstance.Send(new NotificationMessage<MediaItem>(
                        value, "mediaSelected"));
                }
                Set(ref _selectedMedia, value);
            }
        }
        public bool IsBrowsing
        {
            get => _isBrowsing;
            set => Set(ref _isBrowsing, value);
        }
        public ObservableCollection<MediaItem> MediaList { get; private set; }
        public SpiderEnum SpiderList { get; private set; }
        public string CurrentFolder { get; set; }

        public ICommand CmdExclude { get; set; }
        public ICommand CmdDownload { get; set; }
        public ICommand CmdMoveItem { get; set; }
        public ICommand CmdDeleteItem { get; set; }
        public ICommand CmdClearDb { get; set; }
        public ICommand CmdEditItem { get; set; }
        public ICommand CmdDoubleClick { get; set; }

        readonly IMediaListNotifier _mediaListNotifier;
        public MediaListViewModel(IMediaListNotifier notifier)
        {
            _mediaListNotifier = notifier;
            MediaList = new ObservableCollection<MediaItem>();

            CmdExclude = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.excluded));
            CmdDownload = new RelayCommand<MediaItem>(
                p => OnContextMenu(p, MediaListMenuType.downloaded));
            CmdMoveItem = new RelayCommand<object>(p => OnMoveItem(p));
            CmdDeleteItem = new RelayCommand<object>(p => OnDeleteItem(p));
            CmdClearDb = new RelayCommand<object>(p => OnClearDb(p));
            CmdEditItem = new RelayCommand<object>(p => OnEditItem(p));
            CmdDoubleClick = new RelayCommand(() => OnDoubleClicked());

            MessengerInstance.Register<NotificationMessage<SpiderEnum>>(this,
                (msg) => SpiderList = msg.Content.Where(i => i.Name != "sehuatang"));
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
            var medias = MediaList.Where(i => i.MediaFile.StartsWith(path,
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
            if (fsEntries.Length == 0 || fsEntries[0].EndsWith(".actors"))
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

        public MediaItem GetMedia(string path)
        {
            try
            {
                var item = new MediaItem(path);
                if (!item.IsExcluded && !item.IsDownload && item.IsMediaFolder)
                    return item;
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
            return null;
        }

        public void InsertMedia(string path)
        {
            UiServices.Invoke(delegate
           {
               var item = GetMedia(path);
               if (item == null) return;

               int idx = -1;
               if (item.IsImage)
               {
                   idx = MediaList.FindItem(item, i => i.DownloadDt);
               }
                //MediaList.InsertInPlace(item, i => i.DownloadDt);
                if (idx >= 0)
                   MediaList.Insert(idx, item);
               else
                   MediaList.Add(item);
           }, true);
            //Thread.Sleep(10);
        }
        void OnMoveItem(object param)
        {
            if (!(param is IList<object> items) || items.Count == 0)
                    return;

            foreach (var item in items.Cast<MediaItem>().ToList())
            {
                var path = item.MediaFolder;
                if (item.MoveItem())
                {
                    MediaList.Remove(item);
                    _mediaListNotifier.OnMediaItemMoved(path);
                }
            }
        }

        void OnDeleteItem(object param)
        {
            if (!(param is IList<object> items) || items.Count == 0)
                return;

            foreach (var item in items.Cast<MediaItem>().ToList())
            {
                var path = item.MediaFolder;
                if (item.DeleteItem())
                {
                    MediaList.Remove(item);
                   _mediaListNotifier.OnMediaItemMoved(path);
                }
            }
        }

        void OnClearDb(object param)
        {
            if (param is MediaItem item && item.AvItem != null)
            {
                App.DbContext.Items.Remove(item.AvItem);
                App.DbContext.SaveChanges();
                item.AvItem = null;
            }
        }

        void OnEditItem(object param)
        {
            if (param is MediaItem item && item.AvItem != null)
            {
                MessengerInstance.Send(new NotificationMessage<MediaItem>(
                    item, "editAv"));
            }
        }

        void OnDoubleClicked()
        {
            _mediaListNotifier.OnMediaItemDoubleClicked(SelectedMedia);
        }

        void OnContextMenu(MediaItem item, MediaListMenuType type)
        {
            if (item == null) return;
            var dir = Path.GetDirectoryName(item.MediaFile);
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
        }
    }
}
