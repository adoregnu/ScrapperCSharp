using System;
using System.Collections.Generic;
using System.Linq;

using CefSharp;
using Scrapper.ScrapItems;
using Scrapper.ViewModel;
namespace Scrapper.Spider
{
    class SpiderMgstage : SpiderBase
    {
        public SpiderMgstage(BrowserViewModel browser) : base(browser)
        {
            Name = "MGStage";
            URL = "https://www.mgstage.com/";

            _xpathDic = new Dictionary<string, string>
            {
                { "title",   XPath("//div[@class='common_detail_cover']/h1[@class='tag']/text()") },
                { "cover",   XPath("//a[@id='EnlargeImage']/@href") },
                { "studio",  XPath("//th[contains(., 'メーカー：')]/following-sibling::td/a/@href") },
                { "runtime", XPath("//th[contains(., '収録時間：')]/following-sibling::td/text()") },
                { "id",      XPath("//th[contains(., '品番：')]/following-sibling::td/text()") },
                { "releasedate", XPath("//th[contains(., '配信開始日：')]/following-sibling::td/text()") },
                { "rating",  XPath("//th[contains(., '評価：')]/following-sibling::td//text()") },
            };
        }

        public override Cookie CreateCookie()
        {
            return new Cookie
            {
                Name = "adc",
                Value = "1",
                Domain = "mgstage.com",
                Path = "/"
            };
        }

        public override void Navigate()
        {
            base.Navigate();
            Browser.Address = $"{URL}product/product_detail/{Pid}/";
        }

        public override void Scrap()
        {
            ParsePage(new ItemMgstage(this)
            {
                NumItemsToScrap = _xpathDic.Count
            });
        }
    }
}
