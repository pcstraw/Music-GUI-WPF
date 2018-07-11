using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for VolumeControl.xaml
    /// </summary>
    public partial class VolumeControl
    {
        public VolumeControl()
        {
            InitializeComponent();
            vmVolume = new VMVolume();
            DataContext = vmVolume;
        }

        VMVolume vmVolume;

        private void Loaded_Event(object sender, System.Windows.RoutedEventArgs e)
        {
            vmVolume.Load();
        }

        private void Slider_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            vmVolume.MouseWheelChange(e.Delta);
        }

        private void slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pointToWindow = Mouse.GetPosition(this);
            vmVolume.JumpToPosition(pointToWindow.X, slider.ActualWidth);
        }
    }
}
