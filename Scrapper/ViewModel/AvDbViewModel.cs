using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Messaging;

using Scrapper.Model;
using Scrapper.Spider;
using Scrapper.ViewModel.Base;
namespace Scrapper.ViewModel
{
    class AvDbViewModel : Pane, IMediaListNotifier
    {
        List<string> _items;
        readonly AvDbContext _context;
        string _selectedType;
        string _selectedItem;

        public MediaListViewModel MediaList { get; set; }
        public List<string> SourceTypes { get; set; }
        public List<string> ItemsSource
        {
            get => _items;
            set => Set(ref _items, value);
        }
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    UpdateList();
                }
            }
        }
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    UpdateMedia();
                }
            }
        }

        public AvDbViewModel()
        {
            Title = "Db Viewer";
            _context = new AvDbContext("avDb");

            MediaList = new MediaListViewModel(this);
            SourceTypes = new List<string>
            {
                "Actor",
                "Studio",
                "Genre"
            };
            SelectedType = "Actor";

            MessengerInstance.Register<NotificationMessage<AvActor>>(
                this, OnActorDoubleClicked);
        }

        void OnActorDoubleClicked(NotificationMessage<AvActor> msg)
        {
            _selectedType = "Actor";
            var actor = msg.Content;
            if (actor == null) return;

            IsSelected = true;
            MediaList.ClearMedia();
            foreach (var item in actor.Items.ToList())
            {
                MediaList.AddMedia(item.Path);
            }
        }

        void IMediaListNotifier.OnMediaItemMoved(string path)
        {
        }

        void InitActorList()
        { 
            var _actors = new List<string>();
            var actors = _context.Actors.Include("Names").ToList();
            foreach (var actor in actors)
            {
                foreach (var name in actor.Names)
                {
                    _actors.Add(name.Name);
                    break;
                }
            }
            ItemsSource = _actors;
        }

        void InitStudioList()
        {
            var _studios = new List<string>();
            foreach (var studio in _context.Studios.ToList())
            {
                _studios.Add(studio.Name);
            }
            ItemsSource = _studios;
        }

        void InitGenreList()
        {
            var _genres = new List<string>();
            foreach (var genre in _context.Genres.ToList())
            {
                _genres.Add(genre.Name);
            }
            ItemsSource = _genres;
        }

        void UpdateList()
        {
            if (SelectedType == "Actor")
            {
                InitActorList();
            }
            else if (SelectedType == "Studio")
            {
                InitStudioList();
            }
            else if (SelectedType == "Genre")
            {
                InitGenreList();
            }
        }

        void UpdateMedia()
        {
            MediaList.ClearMedia();
            if (SelectedType == "Actor")
            {
                var actor = _context.Actors
                    .Include("Names").Include("Items")
                    .FirstOrDefault(a => a.Names.Any(n => n.Name == SelectedItem));
                if (actor == null) return;

                foreach (var item in actor.Items)
                {
                    //Log.Print(item.Pid);
                    MediaList.AddMedia(item.Path);
                }
            }
            else if (SelectedType == "Studio")
            {
                var items = _context.Items
                    .Where(x => x.Studio.Name == SelectedItem)
                    .ToList();
                foreach (var item in items)
                {
                    MediaList.AddMedia(item.Path);
                }
            }
        }
    }
}
