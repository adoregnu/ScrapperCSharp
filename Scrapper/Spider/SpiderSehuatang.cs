﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Interop;

using CefSharp;
using Scrapper.ViewModel;
using Scrapper.ScrapItems;
using GalaSoft.MvvmLight.Messaging;
using FFmpeg.AutoGen;

namespace Scrapper.Spider
{
    class SpiderSehuatang : SpiderBase
    {
        int _pageNum = 1;
        int _index = 0;
        bool _isPageChanged = false;
        string _currentPage = null;
        string _selectedBoard = "censored";
        List<object> _articlesInPage = null;

        public int NumPage = 1;
        public List<string> Boards;
        public override string MediaFolder { get; protected set; }

        public string SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                if (_selectedBoard != value)
                {
                    _selectedBoard = value;
                    MediaFolder = $"{App.DataPath}sehuatang\\{value}\\";
                }
            }
        }

        public SpiderSehuatang(BrowserViewModel browser) : base(browser)
        {
            Name = "sehuatang";
            URL = "https://www.sehuatang.org/";
            _xpathDic = new Dictionary<string, string>
            {
                { "censored",   XPath("//a[contains(., '亚洲有码原创')]/@href") },
                { "uncensored", XPath("//a[contains(., '亚洲无码原创')]/@href") },
                { "subtitle",   XPath("//a[contains(., '高清中文字幕')]/@href") },
                { "articles",   XPath("//tbody[contains(@id, 'normalthread_')]" +
                                    "/tr/td[1]/a/@href") },
                { "pid",  XPath("//span[@id='thread_subject']/text()") },
                { "date", XPath("(//em[contains(@id, 'authorposton')]" +
                                    "/span/@title)[1]") },
                { "files",  XPathClick("//a[contains(., '.torrent')]") },
                { "images", XPath("(//td[contains(@id, 'postmessage_')])[1]" +
                                    "//img[contains(@id, 'aimg_')]/@file") }
            };
            Boards = new List<string>
            {
                "censored", "uncensored", "subtitle"
            };
            MediaFolder = $"{App.DataPath}sehuatang\\{SelectedBoard}\\";
        }

        void ParsePage()
        {
            string[] keys = { "pid", "date", "files", "images" };
            var item = new ItemSehuatang(this) { NumItemsToScrap = keys.Length  };
            var list = _xpathDic.Where(i => keys.Contains(i.Key));
            foreach (var xpath in list)
            {
                //ExecJavaScript(item, xpath);
                Browser.ExecJavaScript(xpath.Value, item, xpath.Key);
            }
        }

        string GetNextPage(string str)
        {
            var m = Regex.Match(str, @"-(?<page>\d+)\.html");
            if (!m.Success)
            {
                Log.Print("Invalid page url format! {0}", str);
                return null;
            }
            _pageNum = int.Parse(m.Groups["page"].Value) + 1;
            if (_pageNum > NumPage)
                return null;
            return Regex.Replace(str, @"\d+\.html", $"{_pageNum}.html");
        }

        void MovePage(List<object> items)
        {
            _state = 1;
            _index = 0;
            _isPageChanged = true;
            if (items != null)
            {
                _currentPage = items[0].ToString();
            }
            else
            {
                _currentPage = GetNextPage(_currentPage);
            }
            if (!string.IsNullOrEmpty(_currentPage))
            {
                Browser.Address = URL + _currentPage;
                Log.Print("Move Page to " + Browser.Address);
            }
            else
            {
                Browser.StopScrapping();
            }
        }

        public void MoveArticle(List<object> items)
        {
            if (_isPageChanged)
            {
                _articlesInPage = items;
                _isPageChanged = false;
            }
            MessengerInstance.Send(new NotificationMessage<string>(
                $"{_index}/{_articlesInPage.Count} {_pageNum}/{NumPage}",
                "UpdateStatus"));

            if (_articlesInPage.Count > _index)
            {
                _state = 2;
                string article = _articlesInPage[_index++].ToString();
                Browser.Address = URL + article;
            }
            else
            {
                MovePage(null);
            }
        }

        public override void OnScrapCompleted(bool isValid, string path)
        {
            if (!isValid && Browser.StopOnExistingId)
            {
                Browser.StopScrapping();
            }
            else
            {
                if (!string.IsNullOrEmpty(path))
                    MessengerInstance.Send(new NotificationMessage<string>(
                        path, "mediaAdded"));
                else
                    Log.Print(" Continue next Item!");
                MoveArticle(null);
            }
        }

        public override void Navigate()
        {
            _state = 0;
            Browser.Address = URL;
        }

        public override void Scrap()
        {
            switch (_state)
            {
            case 0:
                Browser.ExecJavaScript(_xpathDic[SelectedBoard], MovePage);
                break;
            case 1:
                Browser.ExecJavaScript(_xpathDic["articles"], MoveArticle);
                break;
            case 2:
                ParsePage();
                break;
            default:
                Log.Print("Not implementd state{0}", _state);
                return;
            }
        }
    }
}
