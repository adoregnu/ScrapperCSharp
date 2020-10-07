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

        protected Dictionary<string, string> _xpathDic;
        protected int _state = -1;

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

        public virtual Cookie CreateCookie() { return null; }

        bool _isCookieSet = false;
        public void SetCookies()
        {
            if (_isCookieSet) return;
            var cookie = CreateCookie();
            if (cookie == null) return;

            var cookieManager = Cef.GetGlobalCookieManager();
            cookieManager.SetCookieAsync(URL, cookie);
            _isCookieSet = true;
        }

        public virtual void OnScrapCompleted(string path = null)
        {
            Browser.StopScrapping();
            MessengerInstance.Send(
                new NotificationMessage<string>(MediaFolder, "mediaUpdated"));
        }

        protected void ParsePage(IScrapItem item)
        {
            foreach (var xpath in _xpathDic)
            {
                Browser.ExecJavaScript(xpath.Value, item, xpath.Key);
            }
            _state = -1;
        }

        public abstract void Scrap();

        public override string ToString()
        {
            return URL;
        }
    }
}
