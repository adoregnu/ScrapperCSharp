using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using CefSharp;

using Scrapper.ViewModel;
namespace Scrapper.Spider
{
    class SpiderJavlibrary : SpiderBase
    {
        int state = 0;
        readonly Dictionary<string, string> _xpathDic;
        public SpiderJavlibrary(BrowserViewModel browser) : base(browser)
        {
            Name = "javlibrary";
            URL = "http://www.javlibrary.com/en/";// vl_searchbyid.php";
            _xpathDic = new Dictionary<string, string>
            {
                { "videos", "//div[@class='videos']/div/a" },
                { "title",  "//*[@id='video_title']/h3/a/text()" },
                { "id",     "//*[@id='video_id']//td[2]/text()" },
                { "date",   "//*[@id='video_date']//td[2]/text()" },
                { "director", "//*[@id='video_director']//*[@class='director']/a/text()" },
                { "studio", "//*[@id='video_maker']//*[@class='maker']/a/text()" },
                { "thumb",  "//*[@id='video_jacket_img']/@src" },
                { "rating", "//*[@id='video_review']//*[@class='score']/text()" },
                { "genre",  "//*[@id='video_genres']//*[@class='genre']//text()" },
                { "actor",  "//*[@id='video_cast']//*[@class='star']//text()" },
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
            cookieManager.SetCookieAsync(GetAddress(null), cookie);
            _isCookieSet = true;
        }

        string _searchId = null;
        public override string GetAddress(string param)
        {
            _searchId = param;
            if (string.IsNullOrEmpty(param))
                _searchId = "MIDE-023";
            return $"{URL}vl_searchbyid.php?keyword={_searchId}";
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
                if (m.Success && m.Groups["id"].Value == _searchId)
                    break;
            }
            if (!m.Success) return;
            state = 1;
            Browser.Address = $"{URL}{m.Groups["href"].Value}";
        }

        void ParsePage()
        {
            Browser.ExecJavaScript(XPath(_xpathDic["id"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["date"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["actor"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["title"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["genre"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["thumb"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["studio"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["rating"]), PrintResult);
            Browser.ExecJavaScript(XPath(_xpathDic["director"]), PrintResult);
        }

        public override void Navigate()
        {
            switch (state)
            {
            case 0:
                Browser.ExecJavaScript(XPath(_xpathDic["videos"]), OnMultiResult);
                break;
            default:
                ParsePage();
                break;
            }
        }
    }
}
