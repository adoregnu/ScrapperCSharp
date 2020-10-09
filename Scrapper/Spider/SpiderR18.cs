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
                { "actor_thumb", XPath("//ul[contains(@class,'cmn-list-product03')]//img") },
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
            Log.Print($"OnMultiResult : {list.Count} items found!");
            if (list == null || list.Count == 0) goto NotFound;

            string exactUrl = null;
            if (list.Count == 1)
            {
                exactUrl = list[0] as string;
                goto Found;
            }
            var tmp = Pid.Split('-', '_');
            if (tmp.Length < 2) goto NotFound;

            var pattern = tmp[0].ToLower() + @"\d+";
            foreach (string url in list)
            {
                Log.Print(url);
                var m = Regex.Match(url, pattern);
                if (m.Success)
                {
                    exactUrl = url;
                    goto Found;
                }
            }
            if (string.IsNullOrEmpty(exactUrl)) goto NotFound;
        Found:
            _state = 1;
            Browser.Address = exactUrl;
            return;
        NotFound:
            Browser.StopScrapping();
        }

        public override void Navigate()
        {
            base.Navigate();
            Browser.Address = $"{URL}common/search/searchword={Pid}/";
        }

        public override void Scrap()
        {
            switch (_state)
            {
            case 0:
                Browser.ExecJavaScript(
                    XPath("//li[starts-with(@class,'item-list')]/a/@href"),
                    OnMultiResult);
                break;
            case 1:
                ParsePage(new ItemR18(this)
                {
                    NumItemsToScrap = _xpathDic.Count
                });
                _state = 2;
                break;
            }
        }
    }
}
