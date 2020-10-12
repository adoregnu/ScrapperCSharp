using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.Model
{
    class MediaTorrent : MediaBase
    {
        public string Torrent { get; private set; }

        public bool IsDownload { get; private set; } = false;
        public bool IsExcluded { get; private set; } = false;

        public List<string> Screenshorts { get; private set; }

        public void MarkDownloaded()
        {
            try
            {
                var torrent = Path.GetFileName(Torrent);
                var dir = Path.GetDirectoryName(Torrent);
                File.Copy(Torrent, @"z:\Downloads\" + torrent);
                File.Create($"{dir}\\.downloaded").Dispose();
                Log.Print($"Makrk downloaded {Torrent}");
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message, ex);
            }
        }

        public void MarkExcluded()
        {
            var dir = Path.GetDirectoryName(Torrent);
            File.Create($"{dir}\\.excluded").Dispose();
                Log.Print($"Mark excluded {Torrent}");
        }
    }
}
