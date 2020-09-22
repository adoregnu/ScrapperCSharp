using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scriban;

using CefSharp;

using Scrapper.ViewModel;
using Scrapper.ScrapItems;

namespace Scrapper.Spider
{
    delegate void OnJsResult(List<object> items);

    abstract class SpiderBase
    {
        public BrowserViewModel Browser { get; private set; }
        public string URL = null;
        public string Name { get; protected set; } = "";

        protected Dictionary<string, string> _xpathDic;

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

        public virtual string GetAddress(string param = null)
        {
            return URL;// $"{URL}?keyword={param}";
        }

        public virtual void SetCookies() { }

        public virtual void OnBeforeDownload(object sender, DownloadItem e) { }

        public virtual void OnDownloadUpdated(object sender, DownloadItem e) { }

        public virtual void OnScrapCompleted() { }

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

        public abstract void Navigate();

        public override string ToString()
        {
            return URL;
        }
    }
}
