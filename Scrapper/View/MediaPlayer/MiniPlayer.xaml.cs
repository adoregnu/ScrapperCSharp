using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Scrapper.Model;
using Scrapper.ViewModel.MediaPlayer;

namespace Scrapper.View.MediaPlayer
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public partial class MiniPlayer : UserControl
    {
        public MediaItem MediaSource
        {
            get { return (MediaItem)GetValue(MediaSourceProperty); }
            set { SetValue(MediaSourceProperty, value); }
        }

        public static DependencyProperty MediaSourceProperty =
           DependencyProperty.Register("MediaSource", typeof(MediaItem),
               typeof(MiniPlayer),
               new FrameworkPropertyMetadata(null,
                   FrameworkPropertyMetadataOptions.None,
                   OnPropertyChanged));

        bool IsControllerHideCompleted;
        DateTime LastMouseMoveTime;
        Point LastMousePosition;
        DispatcherTimer MouseMoveTimer;

        Storyboard HideControllerAnimation =>
            FindResource("HideControlOpacity") as Storyboard;
        Storyboard ShowControllerAnimation =>
            FindResource("ShowControlOpacity") as Storyboard;

        Unosquare.FFME.MediaElement _me;

        public MiniPlayer()
        {
            InitializeComponent();
            LayoutRoot.DataContext = new PlayerViewModel(true);
            InitializePlayer();
        }

        static void OnPropertyChanged(DependencyObject src,
            DependencyPropertyChangedEventArgs e)
        {
            var media = src as MiniPlayer;
            var modelView = media.LayoutRoot.DataContext as PlayerViewModel;
            modelView.SetMediaItem(media.MediaSource);
        }
        void InitializePlayer()
        {
            LastMouseMoveTime = DateTime.UtcNow;

            Loaded += (s, e) =>
            {
                Storyboard.SetTarget(HideControllerAnimation, MiniControllerPanel);
                Storyboard.SetTarget(ShowControllerAnimation, MiniControllerPanel);

                HideControllerAnimation.Completed += (es, ee) =>
                {
                    MiniControllerPanel.Visibility = Visibility.Hidden;
                    IsControllerHideCompleted = true;
                };

                ShowControllerAnimation.Completed += (es, ee) =>
                {
                    IsControllerHideCompleted = false;
                };
                _me = (LayoutRoot.DataContext as PlayerViewModel).MediaPlayer;
            };

            MouseMove += (s, e) =>
            {
                var currentPosition = e.GetPosition(this);
                if (Math.Abs(currentPosition.X - LastMousePosition.X) > double.Epsilon ||
                    Math.Abs(currentPosition.Y - LastMousePosition.Y) > double.Epsilon)
                    LastMouseMoveTime = DateTime.UtcNow;

                LastMousePosition = currentPosition;
            };

            MouseLeave += (s, e) =>
            {
                LastMouseMoveTime = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(10));
            };

            MouseMoveTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(150),
                IsEnabled = true
            };

            MouseMoveTimer.Tick += (s, e) =>
            {
                var elapsedSinceMouseMove = DateTime.UtcNow.Subtract(LastMouseMoveTime);
                if (elapsedSinceMouseMove.TotalMilliseconds >= 3000 &&
                    _me.IsOpen && MiniControllerPanel.IsMouseOver == false)
                {
                    if (IsControllerHideCompleted) return;
                    Cursor = Cursors.None;
                    HideControllerAnimation?.Begin();
                    IsControllerHideCompleted = false;
                }
                else
                {
                    Cursor = Cursors.Arrow;
                    MiniControllerPanel.Visibility = Visibility.Visible;
                    ShowControllerAnimation?.Begin();
                }
            };

            MouseMoveTimer.Start();
        }
    }
}
