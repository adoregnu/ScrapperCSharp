using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scriban;

using CefSharp;

using Scrapper.ViewModel;
using Scrapper.ScrapItems;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace Scrapper.Spider
{
    delegate void OnJsResult(List<object> items);

    abstract class SpiderBase : ViewModelBase
    {
        public BrowserViewModel Browser { get; private set; }
        public string URL = null;
        public string Name { get; protected set; } = "";
        public string Pid { get => Browser.Pid; }
        public virtual string MediaFolder
        {
            get => Browser.SelectedMedia.MediaFolder;
            protected set { _ = value; }
        }
        protected int _state = -1;
        protected string _linkName;

        public SpiderBase(BrowserViewModel br)
        {
            Browser = br;
        }

        string XPath(string xpath, string jsPath)
        {
            var template = Template.Parse(App.ReadResource(jsPath));
            var result = template.Render(new { XPath = xpath });
            //Log.Print(result);
            return result;
        }

        protected string XPath(string xpath)
        { 
            return XPath(xpath, @"XPathMulti.sbn.js");
        }
        protected string XPathClick(string xpath)
        { 
            return XPath(xpath, @"XPathClick.sbn.js");
        }

        public virtual void Navigate()
        {
            _state = 0;
        }

        public virtual void Navigate(string name, string url)
        {
            _linkName = name;
            Browser.Address = url;
        }


        public virtual List<Cookie> CreateCookie() { return null; }

        bool _isCookieSet = false;
        public void SetCookies()
        {
            if (_isCookieSet) return;
            var cookies = CreateCookie();
            if (cookies == null) return;

            var cookieManager = Cef.GetGlobalCookieManager();
            foreach (var cookie in cookies)
            {
                cookieManager.SetCookieAsync(URL, cookie);
            }
            _isCookieSet = true;
        }

        public virtual void OnScrapCompleted(bool isValid, string path = null)
        {
            Browser.StopScrapping();
        }

        protected void ParsePage(IScrapItem item, Dictionary<string, string> dic)
        {
            foreach (var xpath in dic )
            {
                Browser.ExecJavaScript(xpath.Value, item, xpath.Key);
            }
            _linkName = null;
        }

        public abstract void Scrap();

        public override string ToString()
        {
            return URL;
        }
    }
}
