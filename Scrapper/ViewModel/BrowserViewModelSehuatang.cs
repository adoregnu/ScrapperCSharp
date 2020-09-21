﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Scrapper.Spider;
using Scrapper.ViewModel.Base;

namespace Scrapper.ViewModel
{
    partial class BrowserViewModel : Pane
    {
        public ICommand CmdStart { get; private set; }
        public ICommand CmdStop { get; private set; }

        public int NumPage
        {
            set
            {
                if (_selectedSpider is SpiderSehuatang ss)
                    ss.NumPage = value;
            }
            get
            {
                if (_selectedSpider is SpiderSehuatang ss)
                    return ss.NumPage;
                return 0;
            }
        }

        public List<string> Boards
        {
            get
            {
                if (_selectedSpider is SpiderSehuatang ss)
                {
                    return ss.Boards;
                }
                return null;
            }
        }
        public string SelectedBoard
        {
            get
            {
                if (_selectedSpider is SpiderSehuatang ss)
                    return ss.SelectedBoard;
                return "";
            }
            set
            {
                if (_selectedSpider is SpiderSehuatang ss)
                {
                    ss.SelectedBoard = value;
                }
            }
        }

        public bool StopOnExistingId { get; set; } = true;
    }
}
