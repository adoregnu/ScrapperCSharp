using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Unosquare.FFME;
using Unosquare.FFME.Common;

using Scrapper.ViewModel.Base;
namespace Scrapper.ViewModel
{
    class MediaViewModel : Pane
    {
        public int ViewType { get; set; } = 1;
        public MediaListViewModel MediaList { get; private set; }
        public MediaPlayerViewModel MediaPlayer { get; private set; }

        public MediaViewModel()
        {
            Title = "Media";
            MediaElement.FFmpegMessageLogged += OnMediaFFmpegMessageLogged;

            MediaList = new MediaListViewModel();
            MediaPlayer = new MediaPlayerViewModel();
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
