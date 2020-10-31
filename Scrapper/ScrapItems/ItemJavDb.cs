using CefSharp;
using Scrapper.Extension;
using Scrapper.Spider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scrapper.ScrapItems
{
    class ItemJavDb : AvItemBase, IScrapItem
    {
        public ItemJavDb(SpiderBase spider) : base(spider)
        {
            _avItem.IsCensored = false;
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
