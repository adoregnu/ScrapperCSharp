using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;

using Unosquare.FFME;
using Unosquare.FFME.Common;

using GalaSoft.MvvmLight.Command;

using Scrapper.Model;
using System.Windows;
using System.Windows.Media.Animation;

namespace Scrapper.ViewModel.MediaPlayer
{
    class PlayerViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private bool _isPropertiesPanelOpen = App.IsInDesignMode;
        private bool _isPlayerLoaded = App.IsInDesignMode;

        public MediaItem MediaItem { get; private set; }
        public MediaElement MediaPlayer { get; private set; }
        public MediaOptions CurrentMediaOptions { get; set; }
        public ControllerViewModel Controller { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is properties panel open.
        /// </summary>
        public bool IsPropertiesPanelOpen
        {
            get => _isPropertiesPanelOpen;
            set => Set(ref _isPropertiesPanelOpen, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is application loaded.
        /// </summary>
        public bool IsPlayerLoaded
        {
            get => _isPlayerLoaded;
            set => Set(ref _isPlayerLoaded, value);
        }

        public ICommand MouseEnterCommand { get; private set; }
        public ICommand MouseLeaveCommand { get; private set; }
        public ICommand KeyDownCommand { get; private set; }

        public ICommand PlayCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public PlayerViewModel()
        {
            MediaPlayer = new MediaElement
            {
                Background = Brushes.Black,
                LoadedBehavior = MediaPlaybackState.Pause,
                IsDesignPreviewEnabled = true,
                IsMuted = true
            };

            InitMediaEventHandler();
            MouseEnterCommand = new RelayCommand<MouseEventArgs>(e => OnMouseEnter(e));
            MouseLeaveCommand = new RelayCommand<MouseEventArgs>(e => OnMouseLeave(e));
            KeyDownCommand = new RelayCommand<EventArgs>(e => OnKeyDown(e));

            PlayCommand = new RelayCommand(async () => await MediaPlayer.Play());
            PauseCommand = new RelayCommand(async () => await MediaPlayer.Pause());
            StopCommand = new RelayCommand(async () => await MediaPlayer.Stop());
            CloseCommand = new RelayCommand(async () => await MediaPlayer.Close());

            Controller = new ControllerViewModel(this);
            Controller.OnApplicationLoaded();
            IsPlayerLoaded = true;
            Log.Print("Player Created!");
        }

        void InitMediaEventHandler()
        {
            MediaPlayer.MediaReady += OnMediaReady;
            //MediaPlayer.MediaInitializing += OnMediaInitializing;
            MediaPlayer.MediaOpening += OnMediaOpening;
        }

        public void SetMediaItem(MediaItem media)
        {
            MediaItem = media;
            MediaPlayer.Open(new Uri(media.MediaPath));
        }

        void OnMediaOpening(object sender, MediaOpeningEventArgs e)
        {
            CurrentMediaOptions = e.Options;
        }

        //TimeSpan _lastPosition = TimeSpan.FromSeconds(0);
        void OnMediaReady(object sender, EventArgs e)
        {
            //Log.Print($"OnMediaReady {_lastPosition}");
            //MediaPlayer.Position = _lastPosition;
            //await MediaPlayer.Seek(_lastPosition);
            //await MediaPlayer.Play();
        }
        void OnMediaInitializing(object sender, MediaInitializingEventArgs e)
        { 
        }

        void OnMouseEnter(MouseEventArgs e)
        {
            Log.Print("OnMouseEnter");
        }

        void OnMouseLeave(MouseEventArgs e)
        { 
            Log.Print("OnMouseLeave");
        }

        void OnKeyDown(EventArgs e)
        {
            //var pressedKey = (e != null) ? (KeyEventArgs)e : null;
            //if (!(e is KeyEventArgs ke)) return;
            Log.Print(e.ToString());
        }
    }
}
