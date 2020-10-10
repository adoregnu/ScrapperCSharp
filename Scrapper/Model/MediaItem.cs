using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFmpeg.AutoGen;
using GalaSoft.MvvmLight;

namespace Scrapper.Model
{
    public class MediaItem : ViewModelBase
    {
        private string _bgImagePath;
        public DateTime DownloadDt;

        public string MediaName { get; private set; }
        public string MediaFile { get; private set; }
        public string MediaFolder { get; private set; }
        public string Torrent { get; private set; }
        public string BgImagePath
        {
            get => !string.IsNullOrEmpty(_bgImagePath) ?
                        $"{MediaFolder}\\{_bgImagePath}" : null;
            private set => Set(ref _bgImagePath, value);
        }
        public string Pid { get; private set; }

        public bool IsDownload { get; private set; } = false;
        public bool IsExcluded { get; private set; } = false;
        public bool IsImage { get; private set; } = true;
        public bool IsMediaFolder
        {
            get { return !string.IsNullOrEmpty(MediaFile); }
        }
        public List<string> Screenshots { get; private set; } = new List<string>();

        public static string[] VideoExts = new string[] {
            ".mp4", ".avi", ".mkv", ".ts", ".wmv", ".m4v"
        };

        public string Info 
        {
            get
            {
                if (_avItem == null) return Pid;
                return $"{_avItem.Pid} / {_avItem.Studio.Name}";
            }
        }

        AvItem _avItem = null;

        public MediaItem(string path = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                MediaFolder = path;
                UpdateFields();
            }
        }

        public bool MoveItem()
        {
            if (_avItem == null)
            {
                Log.Print($"No info for {Pid}");
                return false;
            }
            var studio = _avItem.Studio.Name;
            var targetPath = $"{App.JavPath}{studio}";

            if (MediaFolder.Equals(targetPath + "\\" + Pid,
                StringComparison.OrdinalIgnoreCase))
            {
                Log.Print($"{Pid} already moved!");
                return false;
            }

            if (!new DirectoryInfo(targetPath).Exists)
            {
                Directory.CreateDirectory(targetPath);
            }
            try
            {
                targetPath += "\\" + Pid;
                Directory.Move(MediaFolder, targetPath);
                MediaFolder = targetPath;
                _avItem.Path = MediaFolder;
                App.DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
            return false;
        }

        void UpdateMediaField(string path)
        {
            MediaFile = path;
            MediaFolder = Path.GetDirectoryName(path);
            Pid = MediaFolder.Split('\\').Last();
            DownloadDt = File.GetLastWriteTime(path);
            MediaName = $"{Pid} / " + DownloadDt.ToString("%M-%d %h:%m:%s");
        }

        void ReloadAvItem()
        {
            _avItem = App.DbContext.Items
                .Include("Studio")
                .FirstOrDefault(i => i.Pid == Pid);
            RaisePropertyChanged("Info");
        }

        public void UpdateFields()
        {
            foreach (var file in Directory.GetFiles(MediaFolder))
            {
                UpdateField(file);
                if (IsExcluded || IsDownload) return;
            }
            ReloadAvItem();
        }

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
                BgImagePath = fname;
            }
            else if (string.IsNullOrEmpty(BgImagePath) && fname.Contains("_thumbnail."))
            { 
                BgImagePath = fname;
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
                UpdateMediaField(path);
            }
            else if (VideoExts.Any(s => fname.EndsWith(s, StringComparison.CurrentCultureIgnoreCase)))
            {
                IsImage = false;
                UpdateMediaField(path);
                if (_avItem == null)
                {
                    ReloadAvItem();
                }
            }
        }
    }
}
