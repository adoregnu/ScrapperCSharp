using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Spider;
using Scrapper.Extension;

namespace Scrapper.ScrapItems
{
    class ItemR18 : ItemBase, IScrapItem
    {
        public ItemR18(SpiderBase spider) : base(spider)
        { 
        }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{_spider.MediaPath}\\{_spider.Pid}_poster{ext}";
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);
            if (!items.IsNullOrEmpty())
            {
                if (name == "cover")
                {
                    _spider.Browser.Download(items[0] as string);
                    Interlocked.Increment(ref NumItemsToScrap);
                }
            }

            CheckCompleted();
        }
    }
}
