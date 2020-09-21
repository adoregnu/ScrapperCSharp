using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using GalaSoft.MvvmLight;
using Unosquare.FFME;
using Unosquare.FFME.Common;
using System.Windows.Input;

namespace Scrapper.ViewModel
{
    class MediaElementViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        public string CoverImage { get; set; }
        public string MediaSource { get; set; }
        public MediaElement Media { get; private set; }

        public MediaElementViewModel()
        {
            Media = new MediaElement
            {
                Background = Brushes.Black,
                LoadedBehavior = MediaPlaybackState.Pause,
                IsMuted = true,
            };

            Media.MouseEnter += OnMouseEnter;
            Media.MouseLeave += OnMouseLeave;
            Media.MediaReady += OnMediaReady;
        }

        void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Log.Print($"OnMouseEnter {MediaSource}");
        }

        void OnMouseLeave(object sender, MouseEventArgs e)
        { 
            Log.Print($"OnMouseLeave {MediaSource}");
        }

        void OnMediaReady(object sender, EventArgs e)
        { 
        }
    }
}
