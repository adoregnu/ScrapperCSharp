using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using GalaSoft.MvvmLight;
using Scrapper.Model;
using Scrapper.ViewModel.MediaPlayer;

namespace Scrapper.View.MediaPlayer
{
    class MiniPlayerViewModel : ViewModelBase
    {
        public PlayerViewModel MediaPlayer { get; private set; }

        public MiniPlayerViewModel()
        {
            MediaPlayer = new PlayerViewModel();
        }
    }

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
               typeof(MediaElementView),
               new FrameworkPropertyMetadata(null,
                   FrameworkPropertyMetadataOptions.None,
                   OnPropertyChanged));

        public MiniPlayer()
        {
            InitializeComponent();
            DataContext = new MiniPlayerViewModel();
        }

        static void OnPropertyChanged(DependencyObject src,
            DependencyPropertyChangedEventArgs e)
        {
            var media = src as MiniPlayer;
            var modelView = media.DataContext as MiniPlayerViewModel;
            modelView.MediaPlayer.SetMediaItem(media.MediaSource);
        }
    }
}
