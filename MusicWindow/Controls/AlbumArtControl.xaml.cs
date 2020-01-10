using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
           
            var visibilityBind = new Binding();
            visibilityBind.Mode = BindingMode.OneWay;
            visibilityBind.Source = infoControl;
            visibilityBind.Converter = new VisibilityConverter();
            visibilityBind.Path = new PropertyPath("ShowTagOptions");
           // updateTagButton.DataContext = infoControl;
            updateTagButton.SetBinding(VisibilityProperty, visibilityBind);
            discardTagButton.SetBinding(VisibilityProperty, visibilityBind);
        }

     //   private Song song { get; private set; }
        ImageConverter imageConverter;
        bool hideArt;
        bool blockSwap = false;

        internal Window SetWindow(Window win)
        {
            if (window != null)
                window.LocationChanged-= Window_LocationChanged;
            window = win;
            window.Title = infoControl.viewModel.Title;
            window.LocationChanged += Window_LocationChanged;
            return win;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            blockSwap = true;
        }

        void HideSaveTagButtons()
        {
           infoControl.ShowTagOptions = false;
        }

        //https://social.msdn.microsoft.com/Forums/vstudio/en-US/98cc1596-0fe7-42b1-b796-dec075ce0b84/programmatically-add-a-wpf-usercontrol-to-a-wpf-window?forum=wpf
        static public Window CreateWindowHostingUserControl(UserControl userControlToHost,double width,double height)
        {
            //Create a border with the initial height and width of the user control.  
            Border borderWithInitialDimensions = new Border();

            borderWithInitialDimensions.Height = height;
            borderWithInitialDimensions.Width = width;
            userControlToHost.Height = height;
            userControlToHost.Width = width;

            //Set the user control's dimensions to double.NaN so that it auto sizes  
            //to fill the window.  
            userControlToHost.Height = double.NaN;
            userControlToHost.Width = double.NaN;
            
            //Create a grid hosting both the border and the user control.  The   
            //border results in the grid and window (created below) having initial  
            //dimensions.  
            Grid hostGrid = new Grid();

            hostGrid.Children.Add(borderWithInitialDimensions);
            hostGrid.Children.Add(userControlToHost);
            
            //Create a window that resizes to fit its content with the grid as its   
            //content.  
            Window hostWindow = new Window();

            hostWindow.Content = hostGrid;
           // hostWindow.SizeToContent = SizeToContent.WidthAndHeight;
            hostWindow.Width = width;
            hostWindow.Height = height;
            
            return hostWindow;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           
            //ImageBox.Source = new BitmapImage(new Uri("music_gui_logo.png", UriKind.Relative));
        }

        //dep
        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            if (!(sender is Song))
                return;
            //song = sender as Song;
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
            if(blockSwap)
            {
                blockSwap = false;
                return;
            }
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
          //  Swap();
        }

        private void discardTagButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            infoControl.viewModel.ReloadTags();
            HideSaveTagButtons();
            Swap(); //hack to stop the controls from swapping 
            //when buttoned is click, 
            //because we use the click event for the entire control to swap

        }

        private void updateTagButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            infoControl.viewModel.SaveTags();
            HideSaveTagButtons();
            Swap();  //hack, see above
        }

        private void ImageBox_Drop(object sender, DragEventArgs e)
        {
            string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (data == null)
                return;

            if (data.Length > 0)
            {
                foreach (string s in data)
                {
                    if (tool.IsImageFile(s))
                    {
                        if (File.Exists(s))
                        {
                            System.Drawing.Image img = System.Drawing.Image.FromFile(s);
                            BitmapImage i = GetWpfImage(img);
                            if (i == null)
                                break;

                            ImageBox.Source = i;
                            infoControl.viewModel.Picture = img;
                            infoControl.ShowTagOptions = true;
                            return;
                        }
                    }
                }
            }
        }

        internal Window window;

        

        private void infoControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
              Swap(); //hack.  see above
            //blockSwap = true;
        }

        private void DockPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Swap(); //actual swap
        }

        private void RowDefinition_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
         //   Swap();
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
                Swap();  //actual swap
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (window == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
                window.DragMove();
        }
    }
}
