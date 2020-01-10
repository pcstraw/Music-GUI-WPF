using System.Windows;
using System.Windows.Input;
using Glaxion.ViewModel;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for DurationControl.xaml
    /// </summary>
    public partial class DurationControl : IDurationMouseInput
    {
        public DurationControl()
        {
            InitializeComponent();
            vmDuration = new VMDuration(this);
            DataContext = vmDuration;
        }

        VMDuration vmDuration;
        private void Loaded_Event(object sender, RoutedEventArgs e)
        {
            vmDuration.Load();
        }
        
        //we need to block the tick update while the user changes the slider value
        private void slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vmDuration.SetUserIsDragging(true);
        }

        private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vmDuration.SetTrackPositionFromSlider();
            vmDuration.SetUserIsDragging(false);
        }
        
        private void slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            vmDuration.MouseWheelChange(e.Delta);
        }

        public double GetMouseInput()
        {
            return Mouse.GetPosition(this).X;
        }

        public double GetSliderLength()
        {
            return slider.ActualWidth;
        }
        

        public double GetSliderValue()
        {
            return slider.Value;
        }
    }
}
