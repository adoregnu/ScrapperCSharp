using CefSharp;
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
            URL = "http://www.r18.com/";
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
                { "thumb",    XPath("div.box01.mb10.detail-view > img::attr(src)") },
                { "actor_thumb", XPath("ul.cmn-list-product03.clearfix.mr07 > li > a > p > img::attr(src)") },
            };
        }

        public override Cookie CreateCookie()
        {
            return new Cookie
            { 
                Name = "lg",
                Value = "en",
                Domain = "r18.com",
                Path = "/",
            };
        }

        void OnMultiResult(List<object> list)
        {
            if (list == null || list.Count == 0)
                return;

            if (list.Count == 1)
            {
                _state = 1;
                Browser.Address = list[0] as string;
                return;
            }
            //var pattern = @"id=(?:h_)?(?:\d+)?([a-z0-9]+[0-9]{3,5})(?:.+)?/";
            //Regex reg = new Regex(pattern, RegexOptions.Compiled);
        }

        void ParsePage()
        { 
        }

        public override void Navigate()
        {
            base.Navigate();
            _state = 0;
            Browser.Address = $"{URL}common/search/searchword={Pid}";
        }

        public override void Scrap()
        {
            string pids = XPath("ul.cmn-list-product01.type01 > li > a::attr(href)");
            switch (_state)
            {
                case 0:
                    Browser.ExecJavaScript(pids, OnMultiResult);
                    break;
                default:
                    ParsePage();
                    break;
            }
        }
    }
}
