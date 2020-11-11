﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Spider;
using Scrapper.Extension;
using System.Threading;

namespace Scrapper.ScrapItems
{
    class ItemJavfree : AvItemBase, IScrapItem
    {
        public ItemJavfree(SpiderBase spider) : base(spider)
        { 

        }

        protected override void UdpateAvItem() { }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{PosterPath}{ext}";
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);
            if (!items.IsNullOrEmpty())
            {
                _numValidItems++;
                if (name == "cover")
                {
                    var url = items[0] as string;
                    Interlocked.Increment(ref _numItemsToScrap);
                    _spider.Browser.Download(url);
                }
            }
            CheckCompleted();
        }
    }
}
