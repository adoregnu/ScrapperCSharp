﻿using System;
using System.Linq;
using System.Windows.Input;

using FileSystemModels;
using FileSystemModels.Models.FSItems.Base;
using FileListView.Interfaces;

using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.CommandWpf;

using Scrapper.ViewModel.Base;
using Scrapper.ViewModel.MediaPlayer;
using Scrapper.Model;

namespace Scrapper.ViewModel
{
    class MediaViewModel : Pane, IFileListNotifier, IMediaListNotifier
    {
        int _viewType = 1;
        public int ViewType
        {
            get => _viewType;
            set => Set(ref _viewType, value);
        }

        public MediaListViewModel MediaList { get; private set; }
        public PlayerViewModel MediaPlayer { get; private set; }
        public FileListViewModel FileList { get; private set; }

        //public ICommand KeyDownCommand { get; private set; }
        public ICommand CmdEscPress { get; set; }

        public MediaViewModel()
        {
            Title = "Media";

            FileList = new FileListViewModel(this);
            MediaPlayer = new PlayerViewModel();
            MediaList = new MediaListViewModel(this);

            CmdEscPress = new RelayCommand(() => {
                MediaPlayer.SetMediaItem(null);
                ViewType = 1; 
            });
            MessengerInstance.Register<NotificationMessage<string>>(
                this, OnMediaUpdated);

            MessengerInstance.Register<NotificationMessageAction<string>>(
                this, OnQueryMediaPath);

            var path = PathFactory.Create(App.DataPath);
            FileList.NavigateToFolder(path);
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (ViewType == 2)
            {
                MediaPlayer.OnKeyDown(e);
            }
        }

        void IMediaListNotifier.OnMediaItemMoved(string path)
        {
            //FileList.RefreshCommand.Execute(null);
            FileList.RemoveItem(path);
        }

        void IMediaListNotifier.OnMediaItemDoubleClicked(MediaItem mitem)
        {
            ViewType = 2;
            MediaPlayer.SetMediaItem(mitem);
        }

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

        void IFileListNotifier.OnFileSelected(ILVItemViewModel fsItem)
        {
            MediaItem media = null;
            if (fsItem != null)
            {
                if (fsItem.ItemType == FSItemType.File)
                {
                    media = new MediaItem();
                    media.UpdateField(fsItem.ItemPath);
                    if (!media.IsMediaFolder || media.IsImage) media = null;
                }
                else
                {
                    media = MediaList.GetMedia(fsItem.ItemPath);
                }
            }

            if (media != null)
            {
                ViewType = 2;
                MediaPlayer.SetMediaItem(media);
            }
            else
            { 
                MediaPlayer.SetMediaItem(null);
                ViewType = 1;
            }
        }

        void IFileListNotifier.OnCheckboxChanged(ILVItemViewModel item)
        {
            if (item.ItemType != FSItemType.Folder)
                return;
            if (item.IsChecked)
                MediaList.AddMedia(item.ItemPath);
            else
                MediaList.RemoveMedia(item.ItemPath);
        }

        void IFileListNotifier.OnFileDeleted(ILVItemViewModel fsItem)
        {
            MediaPlayer.SetMediaItem(null);
            MediaList.RemoveMedia(fsItem.ItemPath);
            ViewType = 1;
        }

        /// <summary>
        /// mediaAdded   : A message from SpiderSehuatang's OnScrapCompleted 
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
        }

        /// <summary>
        ///  querySelectedPath: A message from Spider's Navigate method
        /// </summary>
        /// <param name="msgAction"></param>
        void OnQueryMediaPath(NotificationMessageAction<string> msgAction)
        {
            if (msgAction.Notification == "queryCurrentPath")
                msgAction.Execute(FileList.SelectedFolder);
        }
    }
}
