using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Spider;
namespace Scrapper.ScrapItems
{
    class ItemJavlibrary : IScrapItem
    {
        readonly SpiderBase _spider;
        int _numScrapedItem = 0;

        public string Pid;
        public ItemJavlibrary(SpiderBase spider)
        {
            _spider = spider;
            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired += OnBeforeDownload;
            dh.OnDownloadUpdatedFired += OnDownloadUpdated;
        }

        void Clear()
        {
            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired -= OnBeforeDownload;
            dh.OnDownloadUpdatedFired -= OnDownloadUpdated;
            Log.Print("ItemJavlibrary::Clear()");
        }

        void CheckCompleted()
        {
            Interlocked.Increment(ref _numScrapedItem);
            if (_numScrapedItem == 10)
            {
                Clear();
                _spider.OnScrapCompleted();
            }
        }

        void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{_spider.Browser.MediaPath}\\{Pid}_poster{ext}";
        }

        void OnDownloadUpdated(object sender, DownloadItem e)
        {
            if (e.IsComplete)
            { 
                Log.Print($"{Pid} download completed: {e.FullPath}");
                CheckCompleted();
            }
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            Log.Print("{0} : scrapped {1}", name,
                items != null ? items.Count : 0);
            if (name == "id")
            {
                Pid = items[0] as string;
            }
            else if (name == "cover")
            {
                var f = items[0] as string;
                if (!f.StartsWith("http"))
                {
                    _spider.Browser.Download(_spider.URL + f);
                }
                else
                {
                    _spider.Browser.Download(f);
                }
            }
            CheckCompleted();
        }
    }
}
