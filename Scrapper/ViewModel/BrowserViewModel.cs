using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

using Scrapper.Spider;
using Scrapper.ScrapItems;
using Scrapper.BrowserHandler;
using Scrapper.ViewModel.Base;
using System.IO;

namespace Scrapper.ViewModel
{
    partial class BrowserViewModel : Pane
    {
        bool _isLoading = true;
        bool _bStarted = false;

        string address;
        public string Address
        {
            get { return address; }
            set { Set(ref address, value); }
        }

        IWpfWebBrowser webBrowser;
        public IWpfWebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { Set(ref webBrowser, value); }
        }

        public string Pid { get; set; }
        public List<SpiderBase> Spiders { get; set; }

        SpiderBase _selectedSpider;
        public SpiderBase SelectedSpider
        {
            get => _selectedSpider;
            set
            {
                if (value != null)
                {
                    _selectedSpider = value;
                    value.SetCookies();
                    Address = value.URL;
                    if (value is SpiderSehuatang ss)
                    { 
                        SelectedBoard = ss.Boards[0];
                    }
                }
                RaisePropertyChanged("SelectedSpider");
            }
        }
        public string MediaPath { get; set; }

        public BrowserViewModel()
        {
            CmdStart = new RelayCommand(() => OnStart());
            CmdStop = new RelayCommand(() => StopAll());

            Spiders = new List<SpiderBase>
            {
                new SpiderSehuatang(this),
                new SpiderJavlibrary(this)
            };
            _selectedSpider = Spiders[0];
            Title = Address = _selectedSpider.URL;

            PropertyChanged += OnPropertyChanged;

            MessengerInstance.Register<NotificationMessage<string>>(
                this, OnMediaPathChanged);
        }

        public void OnStart()
        {
            if (_isLoading)
                Log.MessageBox(Log.Tag.Warn, "Loading... ");
            else
            {
                _bStarted = true;
                SelectedSpider.Navigate();
            }
        }

        public void StopAll()
        {
            webBrowser.Stop();
            _bStarted = false;
        }

        public DownloadHandler DownloadHandler { get; private set; }
        public void InitBrowser()
        {
            DownloadHandler = new DownloadHandler();

            WebBrowser.MenuHandler = new MenuHandler();
            WebBrowser.DownloadHandler = DownloadHandler;
            WebBrowser.LifeSpanHandler = new PopupHandler();

            WebBrowser.ConsoleMessage += (s, e) =>
            {
                MessengerInstance.Send(new NotificationMessage<ConsoleMessageEventArgs>(e, "log"));
            };
            WebBrowser.StatusMessage += OnStatusMessage;
            WebBrowser.LoadingStateChanged += OnStateChanged;
            _selectedSpider.SetCookies();
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WebBrowser):
                    if (WebBrowser == null) break;
                    InitBrowser();
                    Log.Print("WebBrowser changed!");

                    break;
                case nameof(Address):
                    Title = Address;
                    Log.Print("Address changed: " + Address);
                    break;
            }
        }

        void OnStatusMessage(object sender, StatusMessageEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Value))
                Log.Print("status: " + e.Value);
        }

        void OnStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            _isLoading = e.IsLoading;
            if (!e.IsLoading && _bStarted)
            {
                SelectedSpider.Navigate();
            }
        }

        public void Download(string url)
        {
            var host = webBrowser.GetBrowserHost();
            host.StartDownload(url);
        }

        public void ExecJavaScript(string s, OnJsResult callback = null)
        {
            webBrowser.EvaluateScriptAsync(s).ContinueWith(x =>
            {
                var response = x.Result;
                if (!response.Success)
                {
                    Log.Print(response.Message);
                    return;
                }
                if (response.Result == null)
                {
                    callback?.Invoke(null);
                }
                else if (response.Result is List<object> list)
                {
                    callback?.Invoke(list);
                }
                else
                {
                    Log.Print("Result is not list!!");
                }
            });
        }

        public void ExecJavaScript(string s, IScrapItem item, string name)
        {
            webBrowser.EvaluateScriptAsync(s).ContinueWith(x =>
            {
                if (!x.Result.Success)
                {
                    Log.Print(x.Result.Message);
                    return;
                }
                item.OnJsResult(name, x.Result.Result as List<object>);
            });
        }

        public void SetStausMessage(string msg)
        {
            MessengerInstance.Send(new NotificationMessage<string>(msg, "UpdateStatus"));
        }

        void OnMediaPathChanged(NotificationMessage<string> msg)
        {
            MediaPath = msg.Content;
            Pid = Path.GetFileName(MediaPath);
            RaisePropertyChanged("Pid");
            if (!(_selectedSpider is SpiderSehuatang))
            {
                _bStarted = true;
                Address = _selectedSpider.GetAddress(Pid);
            }
        }
    }
}
