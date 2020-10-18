﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Data.Entity;
using System.Globalization;
using System.Data.Entity.Validation;

using CefSharp;

using Scrapper.Model;
using Scrapper.Spider;
namespace Scrapper.ScrapItems
{
    abstract class ItemBase
    {
        public static CultureInfo enUS = new CultureInfo("en-US");

        readonly protected SpiderBase _spider;
        readonly protected AvDbContext _context;

        public int NumItemsToScrap;

        protected int _numScrapedItem = 0;
        protected int _numValidItems = 0;
        protected AvItem _avItem;

        protected string posterPath
        {
            get => $"{_spider.MediaFolder}\\{_spider.Pid}_poster";
        }


        public ItemBase(SpiderBase spider)
        {
            _spider = spider;
            _avItem = new AvItem
            {
                Pid = spider.Pid,
                Path = spider.MediaFolder,
                IsCensored = true,
            };
            _context = App.DbContext;

            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired += OnBeforeDownload;
            dh.OnDownloadUpdatedFired += OnDownloadUpdated;
        }

        protected abstract void OnBeforeDownload(object sender, DownloadItem e);
        protected virtual void OnDownloadUpdated(object sender, DownloadItem e)
        {
            if (e.IsComplete)
            {
                Log.Print($"{_spider.Pid} download completed: {e.FullPath}");
                CheckCompleted();
            };
        }

        protected virtual void Clear()
        {
            var dh = _spider.Browser.DownloadHandler;
            dh.OnBeforeDownloadFired -= OnBeforeDownload;
            dh.OnDownloadUpdatedFired -= OnDownloadUpdated;
            Log.Print("ItemBase::Clear()");
        }

        protected void CheckCompleted()
        {
            Interlocked.Increment(ref _numScrapedItem);
            Log.Print($"{_numScrapedItem}/{NumItemsToScrap}");
            if (_numScrapedItem == NumItemsToScrap)
            {
                Log.Print($"Num valid items {_numValidItems}");
                if (_numValidItems > 0) lock (_context)
                {
                    UdpateAvItem();
                }
                _spider.OnScrapCompleted(_numValidItems > 0);
                Clear();
            }
        }

        protected void PrintItem(string name, List<object> items)
        {
            Log.Print("{0} : scrapped {1}", name,
                items != null ? items.Count : 0);
            if (items == null) return;
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

                UiServices.Invoke(delegate
                {
                    var entity = _context.Genres.FirstOrDefault(x => x.Name == genre);
                    if (entity == null)
                        entity = _context.Genres.Add(new AvGenre { Name = genre });
                    _genres.Add(entity);
                });
            }
        }

        AvStudio _studio;
        protected void UpdateStudio(string studio)
        {
            if (string.IsNullOrEmpty(studio)) return;

            UiServices.Invoke(delegate
            {
                studio = NameMap.StudioName(studio);
                _studio = _context.Studios.FirstOrDefault(x => x.Name == studio);
                if (_studio == null)
                    _studio = _context.Studios.Add(new AvStudio { Name = studio });
            });
        }

        AvSeries _series;
        protected void UpdateSeries(string series)
        { 
            if (string.IsNullOrEmpty(series)) return;
            UiServices.Invoke(delegate
            {
                _series = _context.Series.FirstOrDefault(x => x.Name == series);
                if (_series == null)
                    _series = _context.Series.Add(new AvSeries { Name = series });
            });
        }

        List<AvActor> _actors;
        void UpdateActorInternal(List<List<AvActorName>> listOfNameList)
        {
            foreach (var list in listOfNameList)
            {
                AvActorName aan = null;
                foreach (var aname in list)
                {
                    aan = _context.ActorNames
                        .Where(n => n.Name.Equals(aname.Name,
                                StringComparison.OrdinalIgnoreCase))
                        .Include("Actor")
                        .FirstOrDefault();
                    if (aan != null) break;

                }

                if (aan != null)
                {
                    var dbActor = aan.Actor;
                    foreach (var aname in list)
                    {
                        if (!dbActor.Names.Any(n => n.Name.Equals(aname.Name,
                            StringComparison.OrdinalIgnoreCase)))
                        {
                            dbActor.Names.Add(aname);
                        }
                    }
                    _actors.Add(dbActor);
                }
                else
                {
                    var actor = new AvActor();
                    list.ForEach(i => i.Actor = actor);
                    actor.Names = list;

                    _context.Actors.Add(actor);
                    _actors.Add(actor);
                }
            }
        }

        protected void UpdateActor2(List<List<AvActorName>> listOfNameList)
        {
            _actors = new List<AvActor>();
            UiServices.Invoke(delegate
            {
                UpdateActorInternal(listOfNameList);
            });
        }

        protected virtual void UdpateAvItem()
        {
            UiServices.Invoke(delegate
            {
                var item = _context.Items.FirstOrDefault(i => i.Pid == _avItem.Pid);
                //if (_context.Items.Any(x => x.Pid == _avItem.Pid))
                if (item != null) _avItem = item;

                if (_series != null) _avItem.Series = _series;
                if (_studio != null) _avItem.Studio = _studio;
                if (_actors != null) _avItem.Actors = _actors;
                if (_genres != null) _avItem.Genres = _genres;

                try
                {
                    if (item == null)
                        _context.Items.Add(_avItem);
                    _context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Log.Print("Entity of type \"{0}\" in state \"{1}\"" +
                            " has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Log.Print("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                }
                finally
                {
                    _series = null;
                    _studio = null;
                    _actors = null;
                    _genres = null;
                }

            });
        }
    }
}
