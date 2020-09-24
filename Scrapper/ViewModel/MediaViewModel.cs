using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Unosquare.FFME;
using Unosquare.FFME.Common;

using Scrapper.ViewModel.Base;
using FileSystemModels;

namespace Scrapper.ViewModel
{
    class MediaViewModel : Pane, IFileListNotifier
    {
        public int ViewType { get; set; } = 1;

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

            var path = PathFactory.Create(@"d:\tmp\sehuatang");
            FileList.NavigateToFolder(path);
        }

        void IFileListNotifier.OnDirectoryChanged(string path)
        {
            UiServices.Invoke(delegate {
                MediaList.MediaPath = path;
            }, true);
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
    }
}
