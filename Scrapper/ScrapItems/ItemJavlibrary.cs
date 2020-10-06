﻿using System;
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
        }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{_spider.MediaPath}\\{_spider.Pid}_poster{ext}";
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);
            if (items != null && items.Count == 0)
            {
                if (name == "cover")
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
            }
            CheckCompleted();
        }
    }
}
