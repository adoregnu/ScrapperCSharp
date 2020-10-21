﻿using CefSharp;
using HtmlAgilityPack;
using Scrapper.ScrapItems;
using Scrapper.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.Spider
{
    class SpiderJavDb : SpiderBase
    {
        public SpiderJavDb(BrowserViewModel browser) : base(browser)
        { 
            Name = "JavDB";
            URL = "https://javdb.com/";

            _xpathDic = new Dictionary<string, string>
            {
                { "title", XPath("//h2[contains(@class, 'title')]/strong/text()") },
                { "cover", XPath("//div[@class='column column-video-cover']/a/@href") },
                { "pid", XPath("//nav[@class='panel video-panel-info']/div[1]/span/text()") },
                { "date", XPath("//nav[@class='panel video-panel-info']/div[2]/span/text()") },
                { "genre", XPath("//nav[@class='panel video-panel-info']/div[7]/span/a/text()") },
                { "actor", XPath("//nav[@class='panel video-panel-info']/div[8]/span//text()") },
            };
        }

        public override List<Cookie> CreateCookie()
        {
            return new List<Cookie>
            {
                new Cookie {
                    Name = "over18",
                    Value = "1",
                    Domain = "javdb.com",
                    Path = "/"
                },
                new Cookie { 
                    Name = "locale",
                    Value = "en",
                    Domain = "javdb.com",
                    Path = "/"
                }
            };
        }

        void OnMultiResult(List<object> list)
        { 
            Log.Print($"OnMultiResult : {list.Count} items found!");
            if (list == null || list.Count == 0)
            {
                Browser.StopScrapping();
                return;
            }
            HtmlDocument doc = new HtmlDocument();
            HtmlNode a = null;
            foreach (string it in list)
            {
                doc.LoadHtml(it);
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='uid']");
                var pid = node.InnerText.Trim();
                if (pid.Equals(Pid))
                {
                    a = doc.DocumentNode.FirstChild;
                    break;
                }
            }
            if (a == null)
            {
                Browser.StopScrapping();
                return;
            }
            _state = 1;
            Browser.Address = $"{URL}{a.Attributes["href"].Value.Substring(1)}";
        }

        public override void Navigate()
        {
            base.Navigate();
            Browser.Address = $"{URL}search?q={Pid}&f=all";
        }

        public override void Scrap()
        {
            switch (_state)
            {
                case 0:
                    Browser.ExecJavaScript(
                        XPath("//a[@class='box']"),
                        OnMultiResult);
                    break;
                case 1:
                    ParsePage(new ItemJavDb(this)
                    {
                        NumItemsToScrap = _xpathDic.Count
                    });
                    _state = 2;
                    break;
            }
        }
    }
}
