using System;
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
using log4net.Config;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

using Scrapper.Model;

namespace Scrapper.ViewModel
{
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
        public ICommand CmdBrowsePicture { get; private set; }
        public ICommand CmdAddNewName { get; private set; }
        public ICommand CmdDoubleClick { get; private set; }
        public ICommand CmdActorAlphabet { get; private set; }
        readonly IDialogService _dialogService;

        public ActorEditorViewModel(IDialogService dlgSvc)
        {
            _dialogService = dlgSvc;
            CmdBrowsePicture = new RelayCommand(() => OnFileBrowse());
            CmdAddNewActor = new RelayCommand(() => OnAddNewActor());
            CmdDeleteActor = new RelayCommand(() => OnDeleteActor());
            CmdAddNewName = new RelayCommand(() => OnAddNewName());
            CmdDoubleClick = new RelayCommand(() => OnDoubleClicked());
            CmdActorAlphabet = new RelayCommand<object>((p) => OnActorAlphabet(p));

            OnActorAlphabet('A');
        }

        void OnFileBrowse()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "Select Actor Pciture",
                InitialDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.MyPictures),
                Filter = "Image files (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*"
            };

            bool? success = _dialogService.ShowOpenFileDialog(this, settings);
            if (success == true)
            {
                try
                {
                    var fileName = Path.GetFileName(settings.FileName);
                    File.Copy(settings.FileName,
                        $"{App.CurrentPath}\\db\\{fileName}", true);
                    PicturePath = settings.FileName;
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message);
                }
            }
        }

        void OnAddNewActor()
        {
            if (string.IsNullOrEmpty(ActorName))
                return;

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
                AllNames.Add(name);
                NewName = "";
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message);
            }
        }

        void OnActorAlphabet(object p)
        {
            string searcStr = $"{p}%";

            //Log.Print($"{searcStr}");
            NamesOfActor = null;
            SelectedActor = null;

            var names = App.DbContext.ActorNames
                .Include("Actor")
                .Where(n => DbFunctions.Like(n.Name, searcStr))
                .OrderBy(n => n.Name)
                .ToList();

            if (names != null && names.Count > 0)
            {
                AllNames = new ObservableCollection<AvActorName>(names);
                Actors = new ObservableCollection<AvActor>(
                    names.Select(n => n.Actor).Distinct());
            }
            else
            { 
                Actors.Clear();
                AllNames.Clear();
            }
        }

        void OnDoubleClicked()
        {
            MessengerInstance.Send(new NotificationMessage<AvActor>(
                SelectedActor, "doubleClicked"));
        }
    }
}
