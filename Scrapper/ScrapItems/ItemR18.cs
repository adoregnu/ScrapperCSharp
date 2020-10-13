using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefSharp;

using System.Data.Entity;
using System.Linq.Expressions;
using System.Collections.Concurrent;

using Scrapper.Model;
using Scrapper.Spider;
using Scrapper.Extension;
using System.Text.RegularExpressions;

namespace Scrapper.ScrapItems
{
    class ItemR18 : ItemBase, IScrapItem
    {
        readonly string _actorPicturePath = $"{App.CurrentPath}\\db";
        readonly ConcurrentDictionary<string, string> _downloadUrls;
        readonly Dictionary<string, string> _actorPicturs;
        readonly List<AvActorName> _actorNames;

        string posterPath
        {
            get => $"{_spider.MediaFolder}\\{_spider.Pid}_poster";
        }

        public ItemR18(SpiderBase spider) : base(spider)
        { 
            _downloadUrls = new ConcurrentDictionary<string, string>();
            _actorPicturs = new Dictionary<string, string>();
            _actorNames = new List<AvActorName>();
        }

        protected override void UdpateAvItem()
        {
            foreach (var pic in _actorPicturs)
            {
                var actorName = _actorNames.FirstOrDefault(x => x.Name == pic.Key);
                if (actorName != null)
                {
                    //Log.Print($"UpdateAvItem:: name: {pic.Key}, path:{pic.Value} ");
                    actorName.Actor.PicturePath = pic.Value;
                }
            }
            base.UdpateAvItem();
        }

        protected override void OnBeforeDownload(object sender, DownloadItem e)
        {
            var ext = Path.GetExtension(e.SuggestedFileName);
            if (_downloadUrls[e.OriginalUrl] == "cover")
            {
                e.SuggestedFileName = $"{posterPath}{ext}";
            }
            else
            {
                //_downloadUrls[e.OriginalUrl] contains actor name
                _actorPicturs.Add(_downloadUrls[e.OriginalUrl], e.SuggestedFileName);
                e.SuggestedFileName = $"{_actorPicturePath}\\{e.SuggestedFileName}";
            }
        }

        void ParseDate(string strDate)
        {
            if (string.IsNullOrEmpty(strDate)) return;
            try
            {
                var m = Regex.Match(strDate, @"([\w\.]+) (\d+), (\d+)");
                if (!m.Success) return;
                var newdate = string.Format("{0} {1} {2}",
                    m.Groups[1].Value.Substring(0, 3),
                    m.Groups[2].Value,
                    m.Groups[3].Value);

                _avItem.ReleaseDate = DateTime.ParseExact(newdate, "MMM d yyyy", enUS);
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
        }

        /// <summary>
        /// input string example :
        ///   "Tsubasa Hachino"  or
        ///   "Elly Akira (Elly Arai, Yuka Osawa)"
        /// </summary>
        /// <param name="items"></param>
        void ParseActorName(List<object> items)
        {
            Dictionary<string, List<AvActorName>> nameDic
                = new Dictionary<string, List<AvActorName>>();
            foreach (string item in items)
            {
                var name = item.Trim();
                if (string.IsNullOrEmpty(name))
                    continue;

                var list = new List<AvActorName>();
                var m = Regex.Match(name, @"([\w\s]+) \((.+)\)");
                if (m.Success)
                {
                    name = m.Groups[1].Value.Trim();
                    var regex = new Regex(@"([\w\s]+),?");
                    foreach (Match mm in regex.Matches(m.Groups[2].Value))
                        list.Add(new AvActorName {
                            Name = mm.Groups[1].Value.Trim()
                        });
                }
                list.Insert(0, new AvActorName { Name = name});
                nameDic.Add(name, list);
                _actorNames.AddRange(list);
            }
            UpdateActor(nameDic);
        }

        void ParseActorThumb(List<object> items)
        {
            Regex regex = new Regex(@"alt=""([\w\s]+)"" src=""(.+)"" width");
            foreach (string img in items)
            {
                var m = regex.Match(img);
                if (!m.Success)
                    continue;

                var url = m.Groups[2].Value;
                var file = $"{_actorPicturePath}\\{url.Split('/').Last()}";
                if (File.Exists(file)) continue;

                Interlocked.Increment(ref NumItemsToScrap);
                var name = m.Groups[1].Value.Trim();
                //Log.Print($"ParseActorThumb: name:{name}, url:{url}");
                if (!url.EndsWith("nowprinting.gif"))
                {
                    _downloadUrls.TryAdd(url, name);
                    _spider.Browser.Download(url);
                }
            }
        }

        void ParseCover(string name, string url)
        {
            var ext = url.Split('.').Last();
            if (File.Exists($"{posterPath}.{ext}")) return;

            Interlocked.Increment(ref NumItemsToScrap);
            _downloadUrls.TryAdd(url, name);
            _spider.Browser.Download(url);
        }

        void IScrapItem.OnJsResult(string name, List<object> items)
        {
            PrintItem(name, items);
            if (!items.IsNullOrEmpty())
            {
                _numValidItems++;
                if (name == "cover")
                {
                    var url = items[0] as string;
                    ParseCover(name, url.Trim());
                }
                else if (name == "actor_thumb")
                {
                    ParseActorThumb(items);
                }
                else if (name == "genre")
                {
                    UpdateGenre(items);
                }
                else if (name == "studio")
                {
                    var studio = (items[0] as string).Trim();
                    UpdateStudio(studio);
                }
                else if (name == "actor")
                {
                    ParseActorName(items);
                }
                else if (name == "releasedate")
                {
                    ParseDate((items[0] as string).Trim());
                }
                else if (name == "title")
                {
                    _avItem.Title = (items[0] as string).Trim();
                }
                else if (name == "set")
                {
                    //var series = (items[0] as string).Trim();
                    //UpdateSeries(series);
                }
                else if (name == "plot")
                {
                    if (items[0] is string plot && string.IsNullOrEmpty(plot))
                        _avItem.Plot = plot.Trim();
                }
            }

            CheckCompleted();
        }
    }
}
