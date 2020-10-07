using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using Scrapper.Spider;

namespace Scrapper.ScrapItems
{
    class ItemSehuatang : ItemBase, IScrapItem
    {
        public DateTime DateTime;

        string _pid = null;
        string _outPath = null;
        int _downloadCount = 0;
        bool _bStop = false;
        Dictionary<string, int> _images = null;

        public ItemSehuatang(SpiderBase spider) : base(spider)
        {
        }

        void PrepareDirectory()
        {
            var di = new DirectoryInfo(_outPath);
            if (!di.Exists)
            {
                Directory.CreateDirectory(_outPath);
            }
            else 
            {
                _bStop = true;
                Log.Print($"Already downloaded! {_outPath}");
                if (_spider.Browser.StopOnExistingId)
                {
                    Log.Print("Stop Scrapping!");
                    _spider.Browser.StopScrapping();
                }
                else
                {
                    Log.Print(" Continue next Item!");
                    _spider.OnScrapCompleted(null);
                }
                Clear();
            }
        }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            if (e.SuggestedFileName.EndsWith("torrent"))
            {
                e.SuggestedFileName = _outPath + e.SuggestedFileName;
            }
            else
            {
                string postfix;
                var idx = _images[e.SuggestedFileName];
                var ext = Path.GetExtension(e.SuggestedFileName);
                if (idx == 0)
                    postfix = "cover";
                else
                    postfix = $"screenshot{idx}";
                e.SuggestedFileName = _outPath + $"{_pid}_{postfix}{ext}";
            }
            Log.Print($"{_pid} file to store: {e.SuggestedFileName}");
        }

        protected override void OnDownloadUpdated(object sender, DownloadItem e)
        {
            if (e.IsComplete)
            {
                Log.Print($"{_pid} download completed: {e.FullPath}");
                Interlocked.Decrement(ref _downloadCount);
                File.SetLastWriteTime(e.FullPath, DateTime);
                if (_downloadCount == 0)
                {
                    Clear();
                    _spider.OnScrapCompleted(Path.GetDirectoryName(e.FullPath));
                }
            }
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);

            if (_bStop) return;
            if (name == "pid")
            {
                string title = items[0] as string;
                var m = Regex.Match(title, @"[\d\w\-_]+", RegexOptions.CultureInvariant);

                _outPath = _spider.MediaFolder;
                if (m.Success)
                {
                    _pid = m.Groups[0].Value;
                    _outPath += _pid + "\\";
                    PrepareDirectory();
                }
                else
                {
                    Log.Print($"Could not find pid pattern in [] {title}");
                }
            }
            else if (name == "date")
            {
                try
                {
                    DateTime = DateTime.Parse(items[0] as string);
                }
                catch (Exception)
                {
                    DateTime = DateTime.Now;
                }
            }
            else if (name == "images")
            {
                _images = new Dictionary<string, int>();
                int i = 0;
                foreach (string f in items)
                {
                    Log.Print(f);
                    _images.Add(f.Split('/').Last(), i);
                    if (!f.StartsWith("http"))
                    {
                        _spider.Browser.Download(_spider.URL + f);
                    }
                    else
                    {
                        _spider.Browser.Download(f);
                    }
                    i++;
                    Interlocked.Increment(ref _downloadCount);
                }
            }
            else if (name == "files")
            {
                Interlocked.Increment(ref _downloadCount);
            }
        }
    }
}
