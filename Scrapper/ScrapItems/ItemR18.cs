using CefSharp;
using Scrapper.Spider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.ScrapItems
{
    class ItemR18 : ItemBase, IScrapItem
    {
        public ItemR18(SpiderBase spider) : base(spider)
        { 
        }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        { 
            Log.Print("{0} : scrapped {1}", name, items != null ? items.Count : 0);
            foreach (string it in items)
            {
                Log.Print($"\t{name}: {it.Trim()}");
            }

            CheckCompleted();
        }
    }
}
