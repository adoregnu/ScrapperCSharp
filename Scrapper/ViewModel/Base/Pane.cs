using System.ComponentModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;


namespace Scrapper.ViewModel.Base
{
    class Pane : ViewModelBase
    {
        public Pane()
        {
            Title = "Unknown";
        }

        private string _title = null;
        [Browsable(false)]
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        private bool _isSelected = false;
        [Browsable(false)]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }
        public bool CanHide { get; set; }
    }
}
