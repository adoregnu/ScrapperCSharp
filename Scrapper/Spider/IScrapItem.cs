﻿using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrapper.Spider
{
    interface IScrapItem
    {
        //Dictionary<string, string> ItemSet { get; set; }
        void OnJsResult(string name, List<object> items);
    }
}
