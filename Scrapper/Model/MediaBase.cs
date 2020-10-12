using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

namespace Scrapper.Model
{
    class MediaBase : ViewModelBase
    {
        public DateTime DownloadDt { get; private set; }

        public string MediaTitle { get; private set; }
        public string MediaFile { get; private set; }

        protected void UpdateField(string path)
        {
            MediaFile = path;
            DownloadDt = File.GetLastWriteTime(path);
        }
    }
}
