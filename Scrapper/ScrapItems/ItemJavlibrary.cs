using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Spider;
using Scrapper.Extension;
namespace Scrapper.ScrapItems
{
    class ItemJavlibrary : ItemBase, IScrapItem
    {
        public ItemJavlibrary(SpiderBase spider) :base(spider)
        {
        }

        protected override void UdpateAvItem()
        { }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{_spider.MediaFolder}\\{_spider.Pid}_poster{ext}";
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
                    Interlocked.Increment(ref NumItemsToScrap);
                    if (!url.StartsWith("http"))
                    {
                        _spider.Browser.Download(_spider.URL + url);
                    }
                    else
                    {
                        _spider.Browser.Download(url);
                    }
                }
            }
            CheckCompleted();
        }
    }
}
