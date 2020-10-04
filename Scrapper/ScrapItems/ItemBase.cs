using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefSharp;

using Scrapper.Model;
using Scrapper.Spider;
using Scrapper.Extension;
using System.Data.Entity;
using System.Globalization;
using Scrapper.ViewModel;

namespace Scrapper.ScrapItems
{
    abstract class ItemBase
    {
        public static CultureInfo enUS = new CultureInfo("en-US");

        readonly protected SpiderBase _spider;
        readonly protected AvDbContext _context;

        public string Pid;
        public int NumItemsToScrap;

        protected int _numScrapedItem = 0;
        protected AvItem _avItem;
        public ItemBase(SpiderBase spider)
        {
            _spider = spider;
            _avItem = new AvItem
            {
                Pid = spider.Pid,
                Path = spider.MediaPath,
            };
            _context = new AvDbContext("avDb");

            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired += OnBeforeDownload;
            dh.OnDownloadUpdatedFired += OnDownloadUpdated;
        }

        protected abstract void OnBeforeDownload(object sender, DownloadItem e);
        protected virtual void OnDownloadUpdated(object sender, DownloadItem e)
        {
            if (e.IsComplete)
            {
                Log.Print($"{Pid} download completed: {e.FullPath}");
                CheckCompleted();
            };
        }

        protected virtual void Clear()
        {
            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired -= OnBeforeDownload;
            dh.OnDownloadUpdatedFired -= OnDownloadUpdated;
            _context.Dispose();
            Log.Print("ItemBase::Clear()");
        }

        protected void CheckCompleted()
        {
            Interlocked.Increment(ref _numScrapedItem);
            if (_numScrapedItem == NumItemsToScrap)
            {
                _spider.OnScrapCompleted();
                UdpateAvItem();
                Clear();
            }
        }

        protected void PrintItem(string name, List<object> items)
        {
            Log.Print("{0} : scrapped {1}", name, items != null ? items.Count : 0);
            foreach (string it in items)
            {
                Log.Print($"\t{name}: {it.Trim()}");
            }
        }

        List<AvGenre> _genres;
        protected void UpdateGenre(List<object> items)
        {
            _genres = new List<AvGenre>();
            foreach (string item in items)
            {
                var genre = item.Trim();
                if (string.IsNullOrEmpty(genre)) continue;

                lock (_context)
                {
                    var entity = _context.Genres.FirstOrDefault(x => x.Name == genre);
                    if (entity == null)
                        entity = _context.Genres.Add(new AvGenre { Name = genre });
                    _genres.Add(entity);
                }
            }
        }

        AvStudio _studio;
        protected void UpdateStudio(List<object> items)
        {
            var studio = (items[0] as string).Trim();
            if (string.IsNullOrEmpty(studio)) return;

            lock (_context)
            {
                _studio = _context.Studios.FirstOrDefault(x => x.Name == studio);
                if (_studio == null)
                    _studio = _context.Studios.Add(new AvStudio { Name = studio });
            }
        }

        List<AvActor> _actors;
        protected void UpdateActor(Dictionary<string, List<AvActorName>> nameDic)
        {
            _actors = new List<AvActor>();
            lock (_context)
            {
                foreach (var namePair in nameDic)
                {
                    var dbName = _context.ActorNames
                        .Where(n => n.Name == namePair.Key)
                        .Include("Actor")
                        .FirstOrDefault();

                    if (dbName != null)
                    {
                        _actors.Add(dbName.Actor);
                        continue;
                    }

                    var actor = new AvActor
                    {
                        Names = namePair.Value
                    };
                    foreach (var name in namePair.Value)
                        name.Actor = actor;

                    _context.Actors.Add(actor);
                    _actors.Add(actor);
                }
            }
        }

        protected virtual void UdpateAvItem()
        {
            lock (_context)
            {
                if (!_context.Items.Any(x => x.Pid == _avItem.Pid))
                {
                    _avItem.Studio = _studio;
                    _avItem.Actors = _actors;
                    _avItem.Genres = _genres;

                    _context.Items.Add(_avItem);
                    _context.SaveChanges();
                }
            }
        }
    }
}
