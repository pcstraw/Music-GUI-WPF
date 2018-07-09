using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for DurationControl.xaml
    /// </summary>
    public partial class DurationControl : INotifyPropertyChanged
    {
        public DurationControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        double _tp;
        public double TrackPosition
        {
            get { return _tp; }
            set {  _tp = value; OnPropertyChanged(); }
        }

        double _td;
        public double TrackDuration
        {
            get { return _td; }
            set { _td = value; OnPropertyChanged(); }
        }

        MusicPlayer player;
        bool isUserdragging;
        bool isUsingMouseWheel;

        private void Loaded_Event(object sender, RoutedEventArgs e)
        {
            player = MusicPlayer.Player;
            //TODO: change this to dedicated TrackPositionChanged event in music player
            //should stop controls.currentPosition from sending an invalid value
            //and means we won't have to check whether the media is loaded
            player.TickEvent += Player_TickEvent; 
            TrackDuration = 100;
        }

        private void Player_TickEvent(object sender, EventArgs args)
        {
            if (player != null && player.windowsMediaPlayer.currentMedia != null)
            {
                if(player.windowsMediaPlayer.currentMedia.duration != TrackDuration)
                    TrackDuration = player.trackDuration;
                //we need to block the tick update while the user changes the slider value
                if (!isUsingMouseWheel)
                {
                    if (!isUserdragging)
                        TrackPosition = player.windowsMediaPlayer.controls.currentPosition;
                    else
                        SetTrackPosition(GetMousePositionRelativeToSlider());
                }
                isUsingMouseWheel = false;
            }
        }

        double GetMousePositionRelativeToSlider()
        {
            double dblValue;
            Point pointToWindow = Mouse.GetPosition(this);
            //Point pointToScreen = PointToScreen(pointToWindow);
            double X = pointToWindow.X;
            dblValue = (X / slider.ActualWidth) * slider.Maximum;
            return dblValue;
        }
        
        void SetTrackPosition(double Value)
        {
            slider.Value = Value;
            player.windowsMediaPlayer.controls.currentPosition = Value;
        }

        //we need to block the tick update while the user changes the slider value
        private void slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            isUserdragging = true;
        }

        private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isUserdragging = false;
        }
        
        private void slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //we need to block the tick update while the user changes the slider value
            isUsingMouseWheel = true;
            double change = e.Delta;
            if (change < 0)
                change = -1;
            else
                change = 1;

            double newVal = slider.Value + change;
            if (newVal > slider.Maximum)
                newVal = slider.Maximum;
            if (newVal < 0)
                newVal = 0;

            SetTrackPosition(newVal);
            // slider.Value += slider.SmallChange * e.Delta;
        }
    }
}
