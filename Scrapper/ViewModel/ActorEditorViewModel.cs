using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<AvActor> Actors { get; private set; }
        public IEnumerable<AvActorName> NamesOfActor
        {
            get => _namesOfActor;
            private set => Set(ref _namesOfActor, value);
        }
        public ObservableCollection<AvActorName> AllNames { get; private set; }

        public AvActor SelectedActor
        {
            get => _actor;
            set
            {
                Set(ref _actor, value);
                NamesOfActor = _actor.Names.ToList();
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
        public ICommand CmdBrowsePicture { get; private set; }
        public ICommand CmdAddNewName { get; private set; }
        public ICommand CmdAssigneName { get; private set; }
        public ICommand CmdDoubleClick { get; private set; }

        readonly IDialogService _dialogService;

        public ActorEditorViewModel(IDialogService dlgSvc)
        {
            _dialogService = dlgSvc;
            var actors = App.DbContext.Actors
                .Include("Names")
                .ToList();
            Actors = new ObservableCollection<AvActor>(actors);

            CmdBrowsePicture = new RelayCommand(() => OnFileBrowse());
            CmdAddNewActor = new RelayCommand(() => OnAddNewActor());
            CmdAddNewName = new RelayCommand(() => OnAddNewName());
            CmdAssigneName = new RelayCommand(() => OnAssigneName());
            CmdDoubleClick = new RelayCommand(() => OnDoubleClicked());

            AllNames = new ObservableCollection<AvActorName>(
                App.DbContext.ActorNames.ToList());
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

        void OnAddNewName()
        { 
        }

        void OnAssigneName()
        { 
        }

        void OnDoubleClicked()
        {
            MessengerInstance.Send(new NotificationMessage<AvActor>(
                SelectedActor, "doubleClicked"));
        }
    }
}
