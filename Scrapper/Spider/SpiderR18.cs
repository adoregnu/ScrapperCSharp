using CefSharp;
using Scrapper.ScrapItems;
using Scrapper.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scrapper.Spider
{
    class SpiderR18 : SpiderBase
    {
        public SpiderR18(BrowserViewModel browser) : base(browser)
        {
            Name = "R18";
            URL = "https://www.r18.com/";
            _xpathDic = new Dictionary<string, string>
            {
                { "title",    XPath("//meta[@property='og:title']/@content") },
                { "releasedate", XPath("//dt[contains(.,'Release Date:')]/following-sibling::dd[1]/text()") },
                { "runtime",  XPath("//dt[contains(.,'Runtime:')]/following-sibling::dd[1]/text()") },
                { "director", XPath("//dt[contains(.,'Director:')]/following-sibling::dd[1]/text()") },
                { "set",      XPath("//dt[contains(.,'Series:')]/following-sibling::dd[1]/a") },
                { "studio",   XPath("//dt[contains(.,'Studio:')]/following-sibling::dd[1]/a/text()") },
                { "label",    XPath("//dt[contains(.,'Label:')]/following-sibling::dd[1]/text()") },
                { "actor",    XPath("//label[contains(.,'Actress(es):')]/following-sibling::div[1]/span/a/span/text()") },
                { "genre",    XPath("//label[contains(.,'Categories:')]/following-sibling::div[1]/a/text()") },
                { "plot",     XPath("//h1[contains(., 'Product Description')]/following-sibling::p/text()") },
                { "cover",    XPath("//div[contains(@class,'box01')]/img/@src") },
                { "actor_thumb", XPath("//ul[contains(@class,'cmn-list-product03')]//img/@src") },
            };
        }

        public override Cookie CreateCookie()
        {
            return new Cookie
            { 
                Name = "mack",
                Value = "1",
                Domain = "www.r18.com",
                Path = "/",
            };
        }

        void OnMultiResult(List<object> list)
        {
            Log.Print($"{list.Count} items found!");
            if (list == null || list.Count == 0)
            {
                Browser.StopAll();
                return;
            }

            if (list.Count == 1)
            {
                _state = 1;
                Browser.Address = list[0] as string;
                return;
            }
            //var pattern = @"id=(?:h_)?(?:\d+)?([a-z0-9]+[0-9]{3,5})(?:.+)?/";
            //Regex reg = new Regex(pattern, RegexOptions.Compiled);
        }

        public override void Navigate()
        {
            base.Navigate();
            Browser.Address = $"{URL}common/search/searchword={Pid}/";
        }

        public override void Scrap()
        {
            string pids = XPath("//li[@class='item-list']/a/@href");
            switch (_state)
            {
                case 0:
                    Browser.ExecJavaScript(pids, OnMultiResult);
                    break;
                default:
                    ParsePage(new ItemR18(this)
                    {
                        NumItemsToScrap = _xpathDic.Count
                    });
                    break;
            }
        }
    }
}
