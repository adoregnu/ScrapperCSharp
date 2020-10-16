using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Messaging;
using ICSharpCode.AvalonEdit.Snippets;
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
                    Set(ref _selectedType, value);
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
                    Set(ref _selectedItem, value);
                    UpdateMedia();
                }
            }
        }

        public AvDbViewModel()
        {
            Title = "Db Viewer";
            _context = App.DbContext;

            MediaList = new MediaListViewModel(this);
            SourceTypes = new List<string>
            {
                "Actor",
                "Studio",
                "Genre",
                "NoActor"
            };
            SelectedType = "Actor";

            MessengerInstance.Register<NotificationMessage<AvActor>>(
                this, OnActorDoubleClicked);
        }

        void OnActorDoubleClicked(NotificationMessage<AvActor> msg)
        {
            SelectedType = "Actor";
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
            ItemsSource = _context.ActorNames
                .OrderBy(n => n.Name)
                .Select(n => n.Name)
                .ToList();
        }

        void InitStudioList()
        {
            ItemsSource = _context.Studios
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .ToList();
        }

        void InitGenreList()
        {
            ItemsSource = _context.Genres
                .OrderBy(s => s.Name)
                .Select(s => s.Name)
                .ToList();
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
            else if (SelectedType == "NoActor")
            {
                ItemsSource = null;
                var items = _context.Items
                    .Include("Actors")
                    .Where(x => x.Actors.Count == 0)
                    .ToList();

                foreach (var item in items)
                {
                    MediaList.AddMedia(item.Path);
                }
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
