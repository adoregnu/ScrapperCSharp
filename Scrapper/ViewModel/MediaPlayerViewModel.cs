using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.ViewModel
{
    class MediaPlayerViewModel : ViewModelBase
    {
        public string MediaPath { get; private set; }
        public string BgImagePath { get; private set; }
    }
}
