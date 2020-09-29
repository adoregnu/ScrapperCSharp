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
    class ItemJavlibrary : ItemBase, IScrapItem
    {
        public ItemJavlibrary(SpiderBase spider) :base(spider)
        {
            NumItemsToScrap = 9;
        }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{_spider.MediaPath}\\{Pid}_poster{ext}";
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            Log.Print("{0} : scrapped {1}",
                name, items != null ? items.Count : 0);
            if (items == null || items.Count == 0)
            {
                CheckCompleted();
                return;
            }

            if (name == "id")
            {
                Pid = items[0] as string;
            }
            else if (name == "cover")
            {
                var url = items[0] as string;
                if (!url.StartsWith("http"))
                {
                    _spider.Browser.Download(_spider.URL + url);
                }
                else
                {
                    _spider.Browser.Download(url);
                }
                Interlocked.Increment(ref NumItemsToScrap);
            }
            CheckCompleted();
        }
    }
}
