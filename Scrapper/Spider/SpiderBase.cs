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
using System.IO;

namespace Scrapper.Spider
{
    delegate void OnJsResult(List<object> items);

    abstract class SpiderBase : ViewModelBase
    {
        public BrowserViewModel Browser { get; private set; }
        public string URL = null;
        public string Name { get; protected set; } = "";
        public string Pid { get; set; }
        public string MediaPath { get; set; }

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
            MessengerInstance.Send(new NotificationMessageAction<string>(
                "querySelectedPath", p => { MediaPath = p; }));

            if (!File.GetAttributes(MediaPath).HasFlag(FileAttributes.Directory))
            {
                Log.Print($"SpiderJavlibrary: {MediaPath} is not a directory!");
                return;
            }
            Pid = Path.GetFileName(MediaPath);
        }

        public virtual Cookie CreateCookie() { return null; }
        public virtual void OnDownloadUpdated(object sender, DownloadItem e) { }

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
            Browser.StopAll();
            MessengerInstance.Send(
                new NotificationMessage<string>(MediaPath, "mediaUpdated"));
        }

        protected virtual void PrintResult(List<object> items)
        {
            Log.Print("{0} items scrapped!", items.Count);
            foreach (string item in items)
            {
                Log.Print(item.TrimEnd(' ', '\r', '\n'));
            }
        }

        protected void ExecJavaScript(IScrapItem item, string name)
        { 
            Browser.ExecJavaScript(_xpathDic[name], item, name);
        }

        public abstract void Scrap();

        public override string ToString()
        {
            return URL;
        }
    }
}
