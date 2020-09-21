using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using MvvmDialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

using Scrapper.Spider;
using Scrapper.ViewModel.Base;
using Scrapper.View;

namespace Scrapper.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        public ICommand CmdFileToFolder { get; private set; }
        
        public ObservableCollection<Pane> Docs { get; }
            = new ObservableCollection<Pane>();
        public ObservableCollection<Pane> Anchors { get; }
            = new ObservableCollection<Pane>();

        public string Status { get; set; }

        readonly IDialogService _dialogService;

        public MainViewModel(IDialogService dialogService)
        {
            Anchors.Add(new DebugLogViewModel());

            Docs.Add( new MediaListViewModel());
            Docs.Add(new BrowserViewModel());

            CmdFileToFolder = new RelayCommand(() => OnFileToFolder());
            _dialogService = dialogService;

            MessengerInstance.Register<NotificationMessage<string>>(
                this, OnStatusMessage);
        }

        void OnFileToFolder()
        {
            var dialog = new FileToFolderViewModel();
            _dialogService.ShowDialog<FileToFolderDialog>(this, dialog);
        }

        void OnStatusMessage(NotificationMessage<string> msg)
        {
            if (msg.Notification == "UpdateStatus")
            {
                Status = msg.Content;
            }
            else if (msg.Notification == "ClearStatus")
            { 
                Status = "";
            }
            RaisePropertyChanged(Status);
        }
    }
}
