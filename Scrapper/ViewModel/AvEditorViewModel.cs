﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using MvvmDialogs;

using Scrapper.Model;
namespace Scrapper.ViewModel
{
    class AvEditorViewModel : ViewModelBase, IModalDialogViewModel
    {
        readonly MediaItem _mediaItem;
        bool? _dialogResult;

        public AvItem Av { get; private set; }
        public ObservableCollection<AvActor> Actors { get; set; }
        public ObservableCollection<AvGenre> Genres { get; set; }

        public IEnumerable<AvSeries> AllSeries { get; private set; }
        public IEnumerable<AvGenre> AllGenres { get; private set; }
        public IEnumerable<AvActor> AllActors { get; private set; }
        public IEnumerable<AvStudio> AllStudios { get; private set; }

        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }

        public string Title { get; private set; }
        public AvStudio SelectedStudio { get; set; }
        public AvSeries SelectedSeries { get; set; }
        public AvActor SelectedActor { get; set; }
        public AvGenre SelectedGenre { get; set; }

        public AvActor SelectedAvActor { get; set; }
        public AvGenre SelectedAvGenre { get; set; }

        public ICommand CmdSetStudio { get; private set; }
        public ICommand CmdSetSeries { get; private set; }
        public ICommand CmdAddActor { get; private set; }
        public ICommand CmdAddGenre { get; private set; }
        public ICommand CmdSave { get; private set; }

        public AvEditorViewModel(MediaItem mediaItem)
        {
            _mediaItem = mediaItem;
            Title = mediaItem.Pid;

            Av = mediaItem.AvItem;
            Actors = new ObservableCollection<AvActor>(Av.Actors);
            Genres = new ObservableCollection<AvGenre>(Av.Genres);

            CmdSetStudio = new RelayCommand(() => Av.Studio = SelectedStudio);
            CmdSetSeries = new RelayCommand(() => Av.Series = SelectedSeries);
            CmdAddActor = new RelayCommand(() => OnAddActor());
            CmdAddGenre = new RelayCommand(() => OnAddGnere());
            CmdSave = new RelayCommand(() => OnSave());

            AllSeries = App.DbContext.Series.ToList();
            AllGenres = App.DbContext.Genres.ToList();
            AllActors = App.DbContext.Actors.ToList();
            AllStudios = App.DbContext.Studios.ToList();
        }

        bool _actorChanged = false;
        void OnAddActor()
        {
            if (!Actors.Any(a => a == SelectedActor))
            {
                Actors.Add(SelectedActor);
                _actorChanged = true;
            }
        }

        bool _genreChanges = false;
        void OnAddGnere()
        {
            if (!Genres.Any(g => g == SelectedGenre))
                Genres.Add(SelectedGenre);
        }

        void OnSave()
        {
            if (_actorChanged)
                Av.Actors = Actors;
            if (_genreChanges)
                Av.Genres = Genres;
            App.DbContext.SaveChanges();

            _mediaItem.ReloadAvItem();
        }
    }
}