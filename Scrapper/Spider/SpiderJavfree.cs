using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Scrapper.Extension;
using Scrapper.ViewModel;

namespace Scrapper.Spider
{
    class SpiderJavfree : SpiderBase
    {
        public SpiderJavfree(BrowserViewModel browser) : base(browser)
        {
            Name = "Javfree";
            URL = "https://javfree.me/";
        }

        public void OnMultiResult(List<object> list)
        { 
            Log.Print($"OnMultiResult : {list.Count} items found!");
            if (list.IsNullOrEmpty())
            {
                Browser.StopScrapping();
                return;
            }
            var regex = new Regex($@"/{Pid.ToLower()}");
            string exactUrl = null;
            foreach (string url in list)
            {
                var m = regex.Match(url);
                if (m.Success)
                {
                    exactUrl = url;
                    break;
                }
            }
            if (exactUrl != null)
            {
                _state = 1;
                Browser.Address = exactUrl;
            }
            else
            {
                Log.Print("No matched Pid!");
                Browser.StopScrapping();
            }
        }

        public override void Navigate()
        {
            base.Navigate();
            Browser.Address = $"{URL}?s={Pid}";
        }

        public override void Scrap()
        {
            switch (_state)
            {
                case 0:
                    Browser.ExecJavaScript(
                        XPath("//h2[@class='entry-title']/a/@href"),
                        OnMultiResult);
                    break;
                case 1:
                    Browser.StopScrapping();
                    break;
            }
        }
    }
}
