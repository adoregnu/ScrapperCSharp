using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scriban;

using Scrapper.ViewModel;
using CefSharp;

namespace Scrapper.Spider
{
    delegate void OnJsResult(List<object> items);

    abstract class SpiderBase
    {
        public BrowserViewModel Browser { get; private set; }
        public string URL = null;//"http://www.javlibrary.com/en/vl_searchbyid.php";
        public string Name { get; protected set; } = "";

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

        public virtual void SetCookies()
        { 
        }

        public virtual void OnBeforeDownload(object sender, DownloadItem e)
        { 
        }

        public virtual void OnDownloadUpdated(object sender, DownloadItem e)
        { 
        }

        protected virtual void PrintResult(List<object> items)
        {
            Log.Print("{0} items scrapped!", items.Count);
            foreach (string item in items)
            {
                Log.Print(item.TrimEnd(' ', '\r', '\n'));
            }
        }

        public abstract void Navigate();
        public override string ToString()
        {
            return URL;
        }
    }
}
