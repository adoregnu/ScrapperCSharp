using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Unosquare.FFME;
using Unosquare.FFME.Common;

using Scrapper.Utils;
using System.IO;
using Scrapper.ViewModel;
using System.Drawing;
using Scrapper.Converter;

namespace Scrapper.View
{
    /// <summary>
    /// Interaction logic for MediaElementView.xaml
    /// </summary>
    public partial class MediaElementView : UserControl
    {
        bool _isOpened = false;
        bool _hasTakenThumbnail = false;
        public string MediaSource
        {
            get { return (string)GetValue(MediaSourceProperty); }
            set { SetValue(MediaSourceProperty, value); }
        }

        public string BgImagePath
        {
            get { return (string)GetValue(BgImagePathProperty); }
            set { SetValue(BgImagePathProperty, value); }
        }

        public int MediaWidth
        {
            get { return (int)GetValue(MediaWidthProperty); }
            set { SetValue(MediaWidthProperty, value); }
        }

        public static DependencyProperty MediaSourceProperty =
           DependencyProperty.Register("MediaSource", typeof(string),
               typeof(MediaElementView),
               new FrameworkPropertyMetadata(null,
                   FrameworkPropertyMetadataOptions.None,
                   OnMediaSourceChanged));

        public static DependencyProperty BgImagePathProperty =
           DependencyProperty.Register("BgImagePath", typeof(string),
               typeof(MediaElementView),
               new FrameworkPropertyMetadata(null,
                   FrameworkPropertyMetadataOptions.None,
                   OnMediaSourceChanged));

        public static DependencyProperty MediaWidthProperty =
           DependencyProperty.Register("MediaWidth", typeof(int),
               typeof(MediaElementView), new UIPropertyMetadata(0));

        static void OnMediaSourceChanged(DependencyObject src,
            DependencyPropertyChangedEventArgs e)
        {
            var media = src as MediaElementView;
            if (e.Property.Name == "BgImagePath")
            {
                media.UpdateBgimage();
            }
        }

        public MediaElementView()
        {
            InitializeComponent();
            InitEventHandler();
        }

        void UpdateBgimage()
        {
            if (string.IsNullOrEmpty(BgImagePath))
                return;
            try
            {
                using (var tmp = new Bitmap(BgImagePath))
                {
                    BgImage.Source = FileToImageConverter.ConvertBitmap(tmp, MediaWidth);
                    _hasTakenThumbnail = true;
                }
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message, ex);
            }
        }

        const int DELAY_ON_PLAY_MS = 500;
        readonly DispatcherTimer _stopTimer = new DispatcherTimer();
        readonly DispatcherTimer _startTimer = new DispatcherTimer();
        void InitEventHandler()
        { 
            Media.MediaReady += OnMediaReady;
            Media.RenderingVideo += OnRenderingVideo;
            _stopTimer.Interval = TimeSpan.FromMilliseconds(DELAY_ON_PLAY_MS);
            _stopTimer.Tick += new EventHandler(OnStopTimer);

            _startTimer.Interval = TimeSpan.FromMilliseconds(DELAY_ON_PLAY_MS);
            _startTimer.Tick += new EventHandler(OnStartTimer);
        }

        TimeSpan _lastPosition = TimeSpan.FromSeconds(10);
        async void OnMediaReady(object sender, EventArgs e)
        {
            //Log.Print($"OnMediaReady {_lastPosition}");
            //Media.Position = _lastPosition;
            await Media.Seek(_lastPosition);
            await Media.Play();
        }

        void SnapThumbnail(BitmapDataBuffer bitmap)
        {
            var path = Path.GetDirectoryName(MediaSource);
            using (var tmp = bitmap.CreateDrawingBitmap())
            {
                var fileName = ThumbnailGenerator.SnapThumbnail(tmp, path);
                BgImagePath = $"{path}\\{fileName}";
                Log.Print("SnapThumbnail : " + BgImagePath);
                UpdateBgimage();
            }
        }

        void OnRenderingVideo(object sender, RenderingVideoEventArgs e)
        {
            const double snapshotPosition = 3;

            if (_hasTakenThumbnail) return;

            var state = e.EngineState;
            if (state.Source == null)
                return;

            if (!state.HasMediaEnded && state.Position.TotalSeconds < snapshotPosition &&
                (!state.PlaybackEndTime.HasValue || state.PlaybackEndTime.Value.TotalSeconds > snapshotPosition))
                return;

            SnapThumbnail(e.Bitmap);
            _hasTakenThumbnail = true;
        }
        async void OpenMedia()
        {
            if (_isOpened) return;
            try
            {
                Log.Print($"Opening {MediaSource}");
                _isOpened = true;
                await Media.Open(new Uri(MediaSource));
                BgImage.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message, ex);
            }
        }

        async void CloseMedia()
        {
            if (!_isOpened) return;
            Log.Print($"closing {MediaSource}");
            _lastPosition = Media.FramePosition;
            _isOpened = false;
            if (Media.IsOpen)
                await Media.Stop();

            await Media.Close();
            BgImage.Visibility = Visibility.Visible;
        }

        void MediaPlayerOnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!_isOpened)
            {
                _startTimer.Start();
            }
            else
            {
                _stopTimer.Stop();
            }
        }

        void MediaPlayerOnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_startTimer.IsEnabled)
            {
                _startTimer.Stop();
            }
            else
            {
                _stopTimer.Start();
            }
        }

        void OnStopTimer(object sender, EventArgs e)
        {
            _stopTimer.Stop();
            CloseMedia();
        }
        void OnStartTimer(object sender, EventArgs e)
        { 
            _startTimer.Stop();
            OpenMedia();
        }
    }
}
