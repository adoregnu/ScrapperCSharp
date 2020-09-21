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

        public string ThumbnailPath
        {
            get { return (string)GetValue(ThumbnailPathProperty); }
            set { SetValue(ThumbnailPathProperty, value); }
        }

        public static DependencyProperty MediaSourceProperty =
           DependencyProperty.Register("MediaSource", typeof(string),
               typeof(MediaElementView),
               new FrameworkPropertyMetadata(null,
                   FrameworkPropertyMetadataOptions.None,
                   OnMediaSourceChanged));

        public static DependencyProperty ThumbnailPathProperty =
           DependencyProperty.Register("ThumbnailPath", typeof(string),
               typeof(MediaElementView),
               new FrameworkPropertyMetadata(null,
                   FrameworkPropertyMetadataOptions.None,
                   OnMediaSourceChanged));

        static void OnMediaSourceChanged(DependencyObject src,
            DependencyPropertyChangedEventArgs e)
        {
            var media = src as MediaElementView;
            //var model = media.DataContext as MediaElementViewModel;
            //model.MediaSource = e.NewValue as string;
            //if (!media.CheckThumbnail())
            //    media.OpenMedia();
            if (e.Property.Name == "ThumbnailPath")
            {
                //Log.Print($"Property changed ThumbnailPath {e.NewValue}");
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
            try
            {
                using (var tmp = new Bitmap(ThumbnailPath))
                {
                    //BgImage.Source = FileToImageConverter.ConvertBitmap(tmp);
                    BgImage.Source = FileToImageConverter.ConvertBitmap(tmp, 240);
                    _hasTakenThumbnail = true;
                }
            }
            catch (Exception ex)
            {
                Log.Print(ex.Message, ex);
            }
        }

        readonly DispatcherTimer _stopTimer = new DispatcherTimer();
        readonly DispatcherTimer _startTimer = new DispatcherTimer();
        void InitEventHandler()
        { 
            Media.MediaReady += OnMediaReady;
            Media.RenderingVideo += OnRenderingVideo;
            _stopTimer.Interval = TimeSpan.FromSeconds(1);
            _stopTimer.Tick += new EventHandler(OnStopTimer);

            _startTimer.Interval = TimeSpan.FromSeconds(1);
            _startTimer.Tick += new EventHandler(OnStartTimer);
        }

        TimeSpan _lastPosition = TimeSpan.FromSeconds(300);
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
                ThumbnailPath = $"{path}\\{fileName}";
                Log.Print("SnapThumbnail : " + ThumbnailPath);
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
                await Media.Open(new Uri(MediaSource));
                _isOpened = true;
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
            Log.Print($"Cloing {MediaSource}");
            _lastPosition = Media.FramePosition;
            if (Media.IsOpen)
                await Media.Stop();

            await Media.Close();
            _isOpened = false;
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
