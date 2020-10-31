﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CefSharp;

using Scrapper.Spider;
namespace Scrapper.ScrapItems
{
    class ItemMgstage : AvItemBase, IScrapItem
    {
        int _numDownloadCnt = 0;
        public ItemMgstage(SpiderBase spider) : base(spider)
        {
        }

        //protected override void UdpateAvItem() { }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        { 
            var ext = Path.GetExtension(e.SuggestedFileName);
            if (_numDownloadCnt == 0)
                e.SuggestedFileName = $"{PosterPath}{ext}";
            else
                e.SuggestedFileName = $"{_spider.Media.MediaFolder}\\" +
                    $"{_spider.Media.Pid}_screenshot{_numDownloadCnt}{ext}";
            Interlocked.Increment(ref _numDownloadCnt);
        }

        void ParseItem(string name, List<object> items)
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
                Interlocked.Increment(ref _numItemsToScrap);
            }
            else if (name == "studio")
            {
                var m = Regex.Match(items[0] as string, @"\]=([\w\d]+)");
                if (m.Success)
                {
                    Log.Print($"\tstudio:{m.Groups[1].Value}");
                    UpdateStudio(m.Groups[1].Value);
                }
            }
            else if (name == "title")
            {
                 UpdateTitle(items[0] as string);
            }
            else if (name == "releasedate")
            { 
                var strdate = (items[0] as string).Trim(); ;
                try
                {
                    _avItem.ReleaseDate = DateTime.ParseExact(
                        strdate, "yyyy/MM/dd", enUS);
                }
                catch (Exception e)
                {
                    Log.Print(e.Message);
                }
            }
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);
            if (items != null && items.Count > 0)
            {
                _numValidItems++;
                ParseItem(name, items);
            }
            CheckCompleted();
        }
    }
}
