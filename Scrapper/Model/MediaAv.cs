using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.Model
{
    class MediaAv : MediaBase
    {
        public static string[] VideoExts = new string[] {
            ".mp4", ".avi", ".mkv", ".ts", ".wmv", ".m4v"
        };

        string _backgroundImage;

        public string Folder { get; private set; }
        public string BackgroundImage
        {
            get => Folder + _backgroundImage;
            private set => Set(ref _backgroundImage, value);
        }
    }
}
