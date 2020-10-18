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
using HtmlAgilityPack;
using Scrapper.Model;

namespace Scrapper.ScrapItems
{
    class ItemJavlibrary : ItemBase, IScrapItem
    {
        public ItemJavlibrary(SpiderBase spider) :base(spider)
        {
        }

        //protected override void UdpateAvItem() { }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            e.SuggestedFileName = $"{posterPath}{ext}";
        }
        void ParseActor(List<object> items)
        {
            List<List<AvActorName>> ll = new List<List<AvActorName>>();
            HtmlDocument doc = new HtmlDocument();
            foreach (string cast in items)
            {
                var l = new List<AvActorName>();
                doc.LoadHtml(cast);
                foreach (var span in doc.DocumentNode.SelectNodes("//span/span"))
                {
                    if (span.Attributes["class"].Value.Contains("icn_")) continue;
                    var name = span.InnerText.Trim().Split(' ').Reverse();
                    l.Add(new AvActorName { Name = string.Join(" ", name)});
                }
                if (l.Count > 0) ll.Add(l);
            }
            if (ll.Count > 0)
                UpdateActor2(ll);
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);
            if (!items.IsNullOrEmpty())
            {
                _numValidItems++;
                if (name == "actor")
                    ParseActor(items);
                else if (name == "cover")
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
