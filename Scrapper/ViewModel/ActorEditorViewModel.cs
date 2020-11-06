﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

using Scrapper.Model;

namespace Scrapper.ViewModel
{
    class ActorInitial : ViewModelBase
    {
        bool _isChecked = false;
        public ActorEditorViewModel ActorEditor;
        public string Initial { get; set; }
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                ActorEditor.OnActorAlphabet(Initial, value);
            }
        }
        public void UnCheck()
        {
            _isChecked = false; 
            RaisePropertyChanged("IsChecked");
        }
    }

    class ActorEditorViewModel : ViewModelBase, IModalDialogViewModel
    {
        AvActor _actor;
        string _picturePath;
        string _actorName;
        string _newName;
        IEnumerable<AvActorName> _namesOfActor;

        bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => Set(nameof(DialogResult), ref _dialogResult, value);
        }

        ObservableCollection<AvActor> _actors
            = new ObservableCollection<AvActor>();
        public ObservableCollection<AvActor> Actors
        {
            get => _actors;
            private set => Set(ref _actors, value);
        }

        public IEnumerable<AvActorName> NamesOfActor
        {
            get => _namesOfActor;
            private set => Set(ref _namesOfActor, value);
        }
        ObservableCollection<AvActorName> _allNames
            = new ObservableCollection<AvActorName>();
        public ObservableCollection<AvActorName> AllNames
        {
            get => _allNames;
            private set => Set(ref _allNames, value);
        }

        public List<ActorInitial> ActorInitials { get; private set; }

        public AvActor SelectedActor
        {
            get => _actor;
            set
            {
                Set(ref _actor, value);
                if (value != null)
                {
                    NamesOfActor = _actor.Names.ToList();
                }
            }
        }

        AvActorName _selectedActorName;
        public AvActorName SelectedActorName
        {
            get => _selectedActorName;
            set
            {
                Set(ref _selectedActorName, value);
                SelectedActor = value.Actor;
            }
        }

        public string ActorName
        {
            get => _actorName;
            set => Set(ref _actorName, value);
        }

        public string NewName
        {
            get => _newName;
            set => Set(ref _newName, value);
        }

        public string PicturePath
        {
            get => _picturePath;
            set => Set(ref _picturePath, value);
        }

        public ICommand CmdAddNewActor { get; private set; }
        public ICommand CmdDeleteActor { get; private set; }
        public ICommand CmdChangePicture { get; private set; }
        public ICommand CmdBrowsePicture { get; private set; }
        public ICommand CmdAddNewName { get; private set; }
        public ICommand CmdDoubleClick { get; private set; }
        public ICommand CmdMergeActors { get; private set; }

        readonly IDialogService _dialogService;

        public ActorEditorViewModel(IDialogService dlgSvc)
        {
            _dialogService = dlgSvc;
            CmdBrowsePicture = new RelayCommand(() => PicturePath = ChoosePicture());
            CmdAddNewActor = new RelayCommand(() => OnAddNewActor());
            CmdDeleteActor = new RelayCommand(() => OnDeleteActor());
            CmdChangePicture = new RelayCommand(() => OnChangePicture());
            CmdAddNewName = new RelayCommand(() => OnAddNewName());
            CmdDoubleClick = new RelayCommand(() => OnDoubleClicked());
            CmdMergeActors = new RelayCommand<object>(
                p => OnMergeActors(p), 
                p => p is IList<object> list && list.Count > 1);

            ActorInitials = Enumerable.Range('A', 'Z' - 'A' + 1).
                      Select(c => new ActorInitial
                      {
                          ActorEditor = this,
                          Initial = ((char)c).ToString(),
                      }).ToList();
            ActorInitials.Insert(0, new ActorInitial
            {
                ActorEditor = this,
                Initial = "All",
            });
            ActorInitials[1].IsChecked = true;

            //OnActorAlphabet("A", true);
        }

        string ChoosePicture()
        { 
            var settings = new OpenFileDialogSettings
            {
                Title = "Select Actor Pciture",
                InitialDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.MyPictures),
                Filter = "Image files (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*"
            };

            bool? success = _dialogService.ShowOpenFileDialog(this, settings);
            if (success != true)
                return null;

            try
            {
                var fileName = Path.GetFileName(settings.FileName);
                File.Copy(settings.FileName, $"{App.CurrentPath}\\db\\{fileName}", true);
                return settings.FileName;
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
            return null;
        }

        void OnAddNewActor()
        {
            if (string.IsNullOrEmpty(ActorName) ||
                string.IsNullOrEmpty(PicturePath))
            {
                Log.Print("Actor name or picture path is empty!");
                return;
            }

            if (App.DbContext.ActorNames.Any(i => i.Name == ActorName))
            {
                Log.Print($"{NewName} is already exists.");
                return;
            }

            var actor = new AvActor
            {
                PicturePath = Path.GetFileName(PicturePath),
                Names = new List<AvActorName>
                {
                    new AvActorName { Name = ActorName }
                }
            };
            App.DbContext.Actors.Add(actor);
            App.DbContext.SaveChanges();

            Actors.Add(actor);
        }

        void OnDeleteActor()
        {
            if (SelectedActor == null) return;

            var names = SelectedActor.Names.ToList();
            foreach (var name in names)
            {
                App.DbContext.ActorNames.Remove(name);
            }
            App.DbContext.Actors.Remove(SelectedActor);
            App.DbContext.SaveChanges();
            Actors.Remove(SelectedActor);

            SelectedActor = null;
            NamesOfActor = null;
        }

        void OnChangePicture()
        {
            var file = ChoosePicture();
            if (file == null)
                return;

            SelectedActor.PicturePath = Path.GetFileName(file);
            App.DbContext.SaveChanges();
        }

        void OnAddNewName()
        {
            if (SelectedActor == null) return;
            if (App.DbContext.ActorNames.Any(i => i.Name == NewName))
            {
                Log.Print($"{NewName} is already exists.");
                return;
            }

            var name = new AvActorName { Name = NewName };
            App.DbContext.ActorNames.Add(name);
            SelectedActor.Names.Add(name);

            try
            {
                App.DbContext.SaveChanges();
                RaisePropertyChanged("SelectedActor");
                //AllNames.Add(name);
                NewName = "";
                NamesOfActor = _actor.Names.ToList();

            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
        }

        public void OnActorAlphabet(string p, bool isSelected)
        {
            NamesOfActor = null;
            SelectedActor = null;
            List<AvActorName> names = null;
            if (p == "All")
            {
                foreach (var initial in ActorInitials)
                {
                    if (initial.Initial != "All") initial.UnCheck();
                }
                if (isSelected)
                {
                    names = App.DbContext.ActorNames
                        .Include("Actor")
                        .OrderBy(n => n.Name)
                        .ToList();
                    AllNames = new ObservableCollection<AvActorName>(names);
                    Actors = new ObservableCollection<AvActor>(
                        names.Select(n => n.Actor).Distinct());
                }
                else
                {
                    AllNames.Clear();
                    Actors.Clear();
                }
                return;
            }

            string searcStr = $"{p}%";
            names = App.DbContext.ActorNames
                .Include("Actor")
                .Where(n => DbFunctions.Like(n.Name, searcStr))
                .OrderBy(n => n.Name)
                .ToList();

            if (names == null || names.Count == 0)
                return;

            foreach (var name in names)
            {
                if (isSelected)
                    AllNames.Add(name);
                    //AllNames.InsertInPlace(name, n => n.Name);
                else
                    AllNames.Remove(name);
            }
            var actors = names.Select(n => n.Actor).Distinct();
            foreach (var actor in actors)
            {
                if (isSelected)
                    Actors.Add(actor);
                    //Actors.InsertInPlace(actor, a => a.ToString());
                else
                    Actors.Remove(actor);
            }
        }

        void OnDoubleClicked()
        {
            MessengerInstance.Send(new NotificationMessage<AvActor>(
                SelectedActor, "doubleClicked"));
        }

        void OnMergeActors(object p)
        {
            AvActor tgtActor = null;
            foreach (AvActor actor in p as IList<object>)
            {
                if (tgtActor == null)
                {
                    tgtActor = actor;
                    continue;
                }

                var avItems = actor.Items.ToList();
                foreach (var item in avItems)
                {
                    item.Actors.Remove(actor);
                    item.Actors.Add(tgtActor);
                }
                foreach (var name in actor.Names)
                {
                    tgtActor.Names.Add(name);
                }
                App.DbContext.Actors.Remove(actor);
                Actors.Remove(actor);
            }
            App.DbContext.SaveChanges();
        }
    }
}
