using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scrapper.Model;
using Scrapper.ViewModel.Base;
namespace Scrapper.ViewModel
{
    class AvDbViewModel : Pane
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

            MediaList = new MediaListViewModel();
            SourceTypes = new List<string>
            {
                "Actor",
                "Studio",
                "Genre"
            };
            SelectedType = "Actor";
        }

        void InitActorList()
        { 
            var _actors = new List<string>();
            var actors = _context.Actors.Include("Names").ToList();
            foreach (var actor in actors)
            {
#if false
                int idx = 0;
                string itemToAdd = "";
                foreach (var name in actor.Names)
                {
                    if (idx == 0)
                        itemToAdd = name.Name;
                    else if (idx == 1)
                        itemToAdd += $"({name.Name}";
                    else
                        itemToAdd += $", {name.Name}";
                    idx++;
                }
                if (idx > 1) itemToAdd += ")";
                _actors.Add(itemToAdd);
#else
                foreach (var name in actor.Names)
                {
                    _actors.Add(name.Name);
                    break;
                }
#endif
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
            if (SelectedType == "Actor")
            {
                var actor = _context.Actors
                    .Include("Names").Include("Items")
                    .FirstOrDefault(a => a.Names.Any(n => n.Name == SelectedItem));
                if (actor == null) return;
                foreach (var item in actor.Items)
                {
                    Log.Print(item.Pid);
                }
            }
        }
    }
}
