using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Spider;
namespace Scrapper.ScrapItems
{
    abstract class ItemBase
    {
        readonly protected SpiderBase _spider;
        protected int _numScrapedItem = 0;
        public string Pid;
        public int NumItemsToScrap;

        protected abstract void OnBeforeDownload(object sender, DownloadItem e);
        protected virtual void OnDownloadUpdated(object sender, DownloadItem e)
        {
            if (e.IsComplete)
            {
                Log.Print($"{Pid} download completed: {e.FullPath}");
                CheckCompleted();
            };
        }

        public ItemBase(SpiderBase spider)
        {
            _spider = spider;

            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired += OnBeforeDownload;
            dh.OnDownloadUpdatedFired += OnDownloadUpdated;
        }

        protected virtual void Clear()
        { 
            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired -= OnBeforeDownload;
            dh.OnDownloadUpdatedFired -= OnDownloadUpdated;
            Log.Print("ItemBase::Clear()");
        }

        protected void CheckCompleted()
        {
            Interlocked.Increment(ref _numScrapedItem);
            if (_numScrapedItem == NumItemsToScrap)
            {
                Clear();
                _spider.OnScrapCompleted();
            }
        }
    }
}
