using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Spider;
namespace Scrapper.ScrapItems
{
    class ItemJavlibrary : IScrapItem
    {
        readonly SpiderBase _spider;
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

        void OnBeforeDownload(object sender, DownloadItem e)
        { 
        }

        void OnDownloadUpdated(object sender, DownloadItem e)
        { 
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        { 
        }
    }
}
