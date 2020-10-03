﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;

using Unosquare.FFME.Common;

using GalaSoft.MvvmLight.Command;

using Scrapper.Model;
using System.Threading;
using Unosquare.FFME;
using System.Windows;
using Scrapper.Extension;

namespace Scrapper.ViewModel.MediaPlayer
{
    class PlayerViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        bool _isPropertiesPanelOpen = App.IsInDesignMode;
        bool _isPlayerLoaded = App.IsInDesignMode;
        bool _isPlaying = false;
        MediaItem _mediaItem;

        public MediaItem MediaItem
        {
            get => _mediaItem;
            private set => Set(ref _mediaItem, value);
        }
        public Unosquare.FFME.MediaElement MediaPlayer { get; private set; }
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

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value);
        }

        //public ICommand MouseEnterCommand { get; private set; }
        //public ICommand MouseLeaveCommand { get; private set; }
        public ICommand KeyDownCommand { get; private set; }

        public ICommand PlayCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public PlayerViewModel()
        {
            MediaPlayer = new Unosquare.FFME.MediaElement
            {
                Background = Brushes.Black,
                LoadedBehavior = MediaPlaybackState.Stop,
                // https://stackoverflow.com/questions/24321237/switching-a-control-over-different-windows-inside-contentcontrol
                UnloadedBehavior = MediaPlaybackState.Manual,
                IsDesignPreviewEnabled = true,
                IsMuted = true
            };

            InitMediaEventHandler();

            KeyDownCommand = new RelayCommand<KeyEventArgs>(e => OnKeyDown(e));

            PlayCommand = new RelayCommand(async () => await MediaPlayer.Play());
            PauseCommand = new RelayCommand(async () => await MediaPlayer.Pause());
            StopCommand = new RelayCommand(async () => await MediaPlayer.Stop());
            CloseCommand = new RelayCommand(async () => await MediaPlayer.Close());

            Controller = new ControllerViewModel(this);
            Controller.OnApplicationLoaded();
            IsPlayerLoaded = true;
            Log.Print("PlayerViewModel");
        }

        void InitMediaEventHandler()
        {
            //MediaPlayer.MediaReady += OnMediaReady;
            //MediaPlayer.MediaInitializing += OnMediaInitializing;
            MediaPlayer.MediaOpening += OnMediaOpening;

            MediaPlayer.WhenChanged(() =>  
                IsPlaying = MediaPlayer.IsPlaying ||
                    MediaPlayer.IsSeeking ||
                    MediaPlayer.MediaState == MediaPlaybackState.Pause,
                nameof(MediaPlayer.IsPlaying));
        }

        async public void SetMediaItem(MediaItem media)
        {
            if (media != MediaItem)
            {
                if (MediaPlayer.IsOpen)
                {
                    await MediaPlayer.Close();
                }

                if (media != null)
                {
                    MediaItem = media;
                    await MediaPlayer.Open(new Uri(media.MediaPath));
                }
            }
            else //in case of updating cover image only
            {
                RaisePropertyChanged("MediaItem");
            }
        }

        void OnMediaOpening(object sender, MediaOpeningEventArgs e)
        {
            CurrentMediaOptions = e.Options;
        }

#if false
        void OnMediaReady(object sender, EventArgs e)
        {
            //Log.Print($"OnMediaReady");
        }

        void OnMediaInitializing(object sender, MediaInitializingEventArgs e)
        { 
            //Log.Print($"OnMediaInitializing");
        }
#endif

        static readonly Key[] TogglePlayPauseKeys = {
            Key.Play, Key.MediaPlayPause, Key.Space
        };
        async public void OnKeyDown(KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox)
                return;

            // Pause
            if (TogglePlayPauseKeys.Contains(e.Key) && MediaPlayer.IsPlaying)
            {
                PauseCommand.Execute(null);
                return;
            }
            // Play
            if (TogglePlayPauseKeys.Contains(e.Key) && MediaPlayer.IsPlaying == false)
            {
                PlayCommand.Execute(null);
                return;
            }

            // Seek to left
            if (e.Key == Key.Left)
            {
                var fpos = MediaPlayer.FramePosition;
                fpos -= TimeSpan.FromSeconds(5);
                await MediaPlayer.Seek(fpos);
                //await MediaPlayer.StepBackward();
                return;
            }

            // Seek to right
            if (e.Key == Key.Right)
            {
                var fpos = MediaPlayer.FramePosition;
                fpos += TimeSpan.FromSeconds(5);
                await MediaPlayer.Seek(fpos);
                //await MediaPlayer.StepForward();
                return;
            }

            // Volume Up
            if (e.Key == Key.Add || e.Key == Key.VolumeUp)
            {
                MediaPlayer.Volume += MediaPlayer.Volume >= 1 ? 0 : 0.05;
                //ViewModel.NotificationMessage = $"Volume: {Media.Volume:p0}";
                return;
            }

            // Volume Down
            if (e.Key == Key.Subtract || e.Key == Key.VolumeDown)
            {
                MediaPlayer.Volume -= MediaPlayer.Volume <= 0 ? 0 : 0.05;
                //ViewModel.NotificationMessage = $"Volume: {Media.Volume:p0}";
                return;
            }

            // Mute/Unmute
            if (e.Key == Key.M || e.Key == Key.VolumeMute)
            {
                MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                //ViewModel.NotificationMessage = Media.IsMuted ? "Muted." : "Unmuted.";
                return;
            }

            // Increase speed
            if (e.Key == Key.Up)
            {
                MediaPlayer.SpeedRatio += 0.05;
                return;
            }

            // Decrease speed
            if (e.Key == Key.Down)
            {
                MediaPlayer.SpeedRatio -= 0.05;
                return;
            }
        }
    }
}
