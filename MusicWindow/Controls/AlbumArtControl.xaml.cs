using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Glaxion.Music;
using Glaxion.Tools;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for AlbumArtControl.xaml
    /// </summary>
    public partial class AlbumArtControl : UserControl
    {
        public Song song { get; private set; }

        public AlbumArtControl()
        {
            InitializeComponent();
            hideArt = false;
        }

        bool hideArt;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Player.TrackChangeEvent += Player_TrackChangeEvent;
            //ImageBox.Source = GetWpfImage(null);
        }

        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            if (!(sender is Song))
                return;
            song = sender as Song;
            SetAlbumart(song);
        }

        void SetAlbumart(Song s)
        {
            BitmapImage i = GetWpfImage(s.image);
            if (i == null)
            {
                tool.Show(2, "Image is null");
                return;
            }
            ImageBox.Source = i;
            titleLabel.Content = s.Title;
        }

        public BitmapImage GetWpfImage(System.Drawing.Image img)
        {
            if(img == null)
            {
                BitmapImage i = new BitmapImage(new Uri("music_gui_logo.png", UriKind.Relative));
                return i;
            }
            else
            {
                BitmapImage ix = new BitmapImage();
                MemoryStream ms = new MemoryStream();  // no using here! BitmapImage will dispose the stream after loading
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                ix.BeginInit();
                ix.StreamSource = ms;
                ix.EndInit();
                return ix;
            }
        }

        void Swap()
        {
            hideArt = !hideArt;
            if (hideArt)
            {
                ImageBox.Visibility = Visibility.Hidden;
                infoControl.Visibility = Visibility.Visible;
            }
            else
            {
                ImageBox.Visibility = Visibility.Visible;
                infoControl.Visibility = Visibility.Hidden;
            }
            
        }

        private void Viewbox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Swap();
        }

        private void ImageBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Swap();
        }
    }
}
