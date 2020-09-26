using GalaSoft.MvvmLight;
using Scrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.ViewModel
{
    class MediaPlayerViewModel : ViewModelBase
    {
        MediaItem _mediaItem;
        public string MediaPath
        {
            get
            {
                if (_mediaItem != null)
                    return _mediaItem.MediaPath;
                else
                    return null;
            }
        }
        public string BgImagePath
        {
            get
            {
                if (_mediaItem != null)
                    return _mediaItem.BgImagePath;
                else
                    return null;
            }
        }

        public void SetMediaItem(MediaItem media)
        {
            _mediaItem = media;
            RaisePropertyChanged("MediaPath");
            RaisePropertyChanged("BgImagePath");
        }
    }
}
