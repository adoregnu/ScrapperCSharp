using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using CefSharp;
using Scrapper.ScrapItems;
using Scrapper.ViewModel;
namespace Scrapper.Spider
{
    class SpiderJavlibrary : SpiderBase
    {
        int state = -1;
        public SpiderJavlibrary(BrowserViewModel browser) : base(browser)
        {
            Name = "javlibrary";
            URL = "http://www.javlibrary.com/en/";
            _xpathDic = new Dictionary<string, string>
            {
                { "videos", XPath("//div[@class='videos']/div/a") },
                { "title",  XPath("//*[@id='video_title']/h3/a/text()") },
                { "id",     XPath("//*[@id='video_id']//td[2]/text()") },
                { "date",   XPath("//*[@id='video_date']//td[2]/text()") },
                { "director", XPath("//*[@id='video_director']//*[@class='director']/a/text()") },
                { "studio", XPath("//*[@id='video_maker']//*[@class='maker']/a/text()") },
                { "cover",  XPath("//*[@id='video_jacket_img']/@src") },
                { "rating", XPath("//*[@id='video_review']//*[@class='score']/text()") },
                { "genre",  XPath("//*[@id='video_genres']//*[@class='genre']//text()") },
                { "actor",  XPath("//*[@id='video_cast']//*[@class='star']//text()") },
            };
        }

        bool _isCookieSet = false;
        public override void SetCookies()
        {
            if (_isCookieSet) return;

            var cookieManager = Cef.GetGlobalCookieManager();
            var cookie = new Cookie
            {
                Name = "over18",
                Value = "18",
                Domain = "www.javlibrary.com",
                Path = "/"
            };
            cookieManager.SetCookieAsync(URL, cookie);
            _isCookieSet = true;
        }

        public override string GetAddress(string param)
        {
            return $"{URL}vl_searchbyid.php?keyword={param}";
        }

        public override void OnScrapCompleted()
        {
            Browser.StopAll();
        }

        void OnMultiResult(List<object> list)
        {
            if (list == null || list.Count == 0)
            {
                ParsePage();
                return;
            }

            Regex regex = new Regex(@"href=""\./(?<href>.+)"" title=""(?<id>[\w\d-_]+)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);
            Match m = null;
            foreach (string a in list)
            {
                m = regex.Match(a);
                if (m.Success && m.Groups["id"].Value == Browser.Pid)
                    break;
            }
            if (!m.Success) return;
            state = 1;
            Browser.Address = $"{URL}{m.Groups["href"].Value}";
        }

        void ParsePage()
        {
            var item = new ItemJavlibrary(this);
            foreach (var xpath in _xpathDic)
            {
                if (xpath.Key == "videos") continue;
                ExecJavaScript(item, xpath.Key);
            }
            state = -1;
        }

        public override void Navigate()
        {
#if false
            if (state == -1)
            {
                if (!string.IsNullOrEmpty(Browser.Pid))
                {
                    Browser.Address = GetAddress(Browser.Pid);
                    state = 0;
                }
                return;
            }
#endif
            switch (state)
            {
            case 0:
                Browser.ExecJavaScript(_xpathDic["videos"], OnMultiResult);
                break;
            default:
                ParsePage();
                break;
            }
        }
    }
}
