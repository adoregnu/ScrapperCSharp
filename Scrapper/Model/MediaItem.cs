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
                if (AvItem == null) return Pid;
                return $"{AvItem.Pid} / {AvItem.Studio.Name}";
            }
        }

        public string Actors
        {
            get
            {
                if (AvItem == null) return "Not Scrapped";
                return AvItem.ActorsName();
            }
        }

        public AvItem AvItem { get; private set; } = null;

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
            if (AvItem == null)
            {
                Log.Print($"No info for {Pid}");
                return false;
            }
            var studio = AvItem.Studio.Name;
            var targetPath = $"{App.JavPath}{studio}";

            if (MediaFolder.Equals(targetPath + "\\" + Pid,
                StringComparison.OrdinalIgnoreCase))
            {
                Log.Print($"{Pid} already moved!");
                return false;
            }

            try
            {
                if (!new DirectoryInfo(targetPath).Exists)
                {
                    Directory.CreateDirectory(targetPath);
                }

                targetPath += "\\" + Pid;
                Directory.Move(MediaFolder, targetPath);
                MediaFolder = targetPath;
                AvItem.Path = MediaFolder;
                App.DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
            return false;
        }

        public bool DeleteItem()
        {
            try
            {
                Directory.Delete(MediaFolder, true);
                if (AvItem != null)
                {
                    App.DbContext.Items.Remove(AvItem);
                    App.DbContext.SaveChanges();
                    AvItem = null;
                }
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
                return false;
            }
            return true;
        }

        void UpdateMediaField(string path)
        {
            MediaFile = path;
            MediaFolder = Path.GetDirectoryName(path);
            Pid = MediaFolder.Split('\\').Last();
            DownloadDt = File.GetLastWriteTime(path);
            MediaName = $"{Pid} / " + DownloadDt.ToString("%M-%d %h:%m:%s");
        }

        public void ReloadAvItem()
        {
            AvItem = App.DbContext.Items
                .Include("Studio")
                .Include("Genres")
                .Include("Actors")
                .FirstOrDefault(i => i.Pid == Pid);
            RaisePropertyChanged("Info");
            RaisePropertyChanged("Actors");
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
            else if (fname.Contains("-fanart."))
            {
                var ext = Path.GetExtension(fname);
                var head = path.Substring(0, path.LastIndexOf('-'));
                var target = $"{head}_poster{ext}";
                if (!File.Exists(target))
                {
                    File.Move(path, target);
                    BgImagePath = Path.GetFileName(target);
                }
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
            else if (VideoExts.Any(s => fname.EndsWith(s,
                StringComparison.CurrentCultureIgnoreCase)))
            {
                IsImage = false;
                UpdateMediaField(path);
                if (AvItem == null)
                {
                    ReloadAvItem();
                }
            }
        }
    }
}
