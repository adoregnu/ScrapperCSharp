﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scrapper.Model
{
    public class MediaItem
    {
        public DateTime DownloadDt;

        public string MediaName { get; private set; }
        public string MediaPath { get; private set; }
        public string Torrent { get; private set; }
        public string BgImagePath { get; private set; }
        public string Pid
        {
            get => Path.GetDirectoryName(MediaPath).Split('\\').Last();
        }
        public bool IsDownload { get; private set; } = false;
        public bool IsExcluded { get; private set; } = false;
        public bool IsImage { get; private set; } = true;
        public bool IsMediaDir
        {
            get { return !string.IsNullOrEmpty(MediaPath); }
        }
        public List<string> Screenshots { get; private set; } = new List<string>();

        public static string[] VideoExts = new string[] {
            ".mp4", ".avi", ".mkv", ".ts", ".wmv", ".m4v"
        };

        public void UpdateField(string path)
        {
            string fname = Path.GetFileName(path);
            if (fname.EndsWith("torrent"))
            {
                Torrent = path;
            }
            else if (fname.Contains("screenshot"))
            {
                Screenshots.Add(path);
            }
            else if (fname.Contains("_poster."))
            {
                BgImagePath = path;
            }
            else if (string.IsNullOrEmpty(BgImagePath) && fname.Contains("_thumbnail."))
            { 
                BgImagePath = path;
            }
            else if (fname.EndsWith(".downloaded"))
            {
                IsDownload = true;
            }
            else if (fname.EndsWith(".excluded"))
            {
                IsExcluded = true;
            }
            else if (fname.Contains("_cover."))
            {
                var dir = Path.GetDirectoryName(path).Split('\\').Last();
                MediaPath = path;
                DownloadDt = File.GetLastWriteTime(path);
                MediaName = $"{dir} / " + DownloadDt.ToString("%M-%d %h:%m:%s");
            }
            else if (VideoExts.Any(s => fname.EndsWith(s, StringComparison.CurrentCultureIgnoreCase)))
            {
                var dir = Path.GetDirectoryName(path).Split('\\').Last();
                MediaPath = path;
                DownloadDt = File.GetLastWriteTime(path);
                MediaName = $"{dir} / " + DownloadDt.ToString("%M-%d %h:%m:%s");
                IsImage = false;
            }
        }
    }
}
