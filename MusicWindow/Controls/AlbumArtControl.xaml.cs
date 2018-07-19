using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
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
    /// One-way converter from System.Drawing.Image to System.Windows.Media.ImageSource
    /// </summary>
    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }

            var image = (System.Drawing.Image)value;
            // Winforms Image we want to get the WPF Image from...
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            MemoryStream memoryStream = new MemoryStream();
            // Save to a memory stream...
            image.Save(memoryStream, image.RawFormat);
            // Rewind the stream...
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // empty images are empty...
            if (value == null) { return null; }

            bool show = (bool)value;
            if (show)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Interaction logic for AlbumArtControl.xaml
    /// </summary>
    public partial class AlbumArtControl : UserControl
    {
        public AlbumArtControl()
        {
            InitializeComponent();
            hideArt = false;
            Song s = new Song("Track Name");
            s.LoadAlbumArt();
            var binding = new Binding();
            binding.Mode = BindingMode.TwoWay;
            binding.Source = infoControl.viewModel;
            binding.Path = new PropertyPath("Picture");
            imageConverter = new ImageConverter();
            binding.Converter = imageConverter;
           // ImageBox.DataContext = infoControl.viewModel;
            ImageBox.SetBinding(Image.SourceProperty,binding);

            //var labelbinding = new Binding();
            //labelbinding.Mode = BindingMode.TwoWay;
            //labelbinding.Source = infoControl.viewModel;
            //labelbinding.Path = new PropertyPath("Title");
            titleLabel.DataContext = infoControl.viewModel;
            titleLabel.SetBinding(Label.ContentProperty, "Title");
            infoControl.viewModel.SetSong(s);

            var visibilityBind = new Binding();
            visibilityBind.Mode = BindingMode.OneWay;
            visibilityBind.Source = infoControl;
            visibilityBind.Converter = new VisibilityConverter();
            visibilityBind.Path = new PropertyPath("ShowTagOptions");
           // updateTagButton.DataContext = infoControl;
            updateTagButton.SetBinding(VisibilityProperty, visibilityBind);
            discardTagButton.SetBinding(VisibilityProperty, visibilityBind);
        }

        public Song song { get; private set; }
        ImageConverter imageConverter;
        bool hideArt;

        void HideSaveTagButtons()
        {
           infoControl.ShowTagOptions = false;
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Player.TrackChangeEvent += Player_TrackChangeEvent;
            //ImageBox.Source = new BitmapImage(new Uri("music_gui_logo.png", UriKind.Relative));
        }

        //dep
        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            if (!(sender is Song))
                return;
           // song = sender as Song;
           // SetAlbumart(song);
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

        private void discardTagButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            infoControl.viewModel.ReloadTags();
            HideSaveTagButtons();
        }

        private void updateTagButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            infoControl.viewModel.SaveTags();
            HideSaveTagButtons();
        }
    }
}
