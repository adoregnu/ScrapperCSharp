﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp;
using HtmlAgilityPack;
using Scrapper.Extension;
using Scrapper.Model;
using Scrapper.ScrapItems;
using Scrapper.ViewModel;

namespace Scrapper.Spider
{
    class SpiderAVE : SpiderBase
    {
        public SpiderAVE(BrowserViewModel browser) : base(browser)
        {
            Name = "AVE";
            URL = "https://www.aventertainments.com/";
        }

        public override List<Cookie> CreateCookie()
        {
            return new List<Cookie> 
            {
                new Cookie
                { 
                    Name = "__utmt",
                    Value = "1",
                    Domain = ".aventertainments.com",
                    Path = "/"
                }
            };
        }

        void OnMultiResult(List<object> list)
        { 
            Log.Print($"OnMultiResult : {list.Count} items found!");
            if (list.IsNullOrEmpty())
            {
                Browser.StopScrapping(Media);
                return;
            }

            _state = 1;
            if (list.Count > 1)
            {
                Log.Print("Multiple matched. Select manually!");
            }
            else
            {
                var url = HtmlEntity.DeEntitize(list[0] as string);
                Browser.Address = url;
            }
        }

        public override void Navigate(MediaItem mitem)
        {
            base.Navigate(mitem);
            Browser.Address = $"{URL}search_Products.aspx?languageID=1" +
                $"&dept_id=29&keyword={Media.Pid}&searchby=keyword";
        }

        public override void Scrap()
        {
            var xpathDic = new Dictionary<string, string>
            {
                { "cover", XPath("//span[@class='grid-gallery']/a/@href") },
                { "title", XPath("//div[@class='section-title']/h3/text()") },
                { "product-info", XPath("//div[@class='single-info']") },

            };
            switch (_state)
            {
                case 0:
                    Browser.ExecJavaScript(
                        XPath("//p[@class='product-title']/a/@href"),
                        OnMultiResult);
                    break;
                case 1:
                    ParsePage(new ItemAVE(this)
                    {
                        NumItemsToScrap = xpathDic.Count
                    }, xpathDic);
                    _state = 2;
                    break;
            }
        }
    }
}
