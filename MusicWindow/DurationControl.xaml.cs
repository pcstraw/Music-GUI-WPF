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

        MusicPlayer player;

        int _tp;
        public int TrackPosition { get { return _tp; } set { if (_tp == value) return; _tp = value; OnPropertyChanged(); } }
        int _td;
        public int TrackDuration { get { return _td; } set { _td = value; OnPropertyChanged(); } }

        private void Loaded_Event(object sender, RoutedEventArgs e)
        {
            player = MusicPlayer.Player;
            player.TickEvent += Player_TickEvent;
            TrackDuration = 100;
        }

        private void Player_TickEvent(object sender, EventArgs args)
        {
            if (player != null && player.windowsMediaPlayer.currentMedia != null)
            {
                if((int)player.windowsMediaPlayer.currentMedia.duration != TrackDuration)
                    TrackDuration = (int)player.trackDuration;
                if (!isUsingMouseWheel)
                {
                    if (!isUserdragging)
                        TrackPosition = (int)player.windowsMediaPlayer.controls.currentPosition;
                    else
                        SetTrackPosition();
                }
                isUsingMouseWheel = false;
            }
        }

        bool isUserdragging;
        void SetTrackPosition()
        {
            double dblValue;
            Point pointToWindow = Mouse.GetPosition(this);
            //Point pointToScreen = PointToScreen(pointToWindow);
            double X = pointToWindow.X;
            dblValue = (X / slider.ActualWidth) * slider.Maximum; 
            slider.Value = dblValue;
            player.windowsMediaPlayer.controls.currentPosition = dblValue;
        }


        private void slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isUserdragging = true;
        }

        private void slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isUserdragging = false;
        }

        bool isUsingMouseWheel;
        private void slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
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

            slider.Value = newVal;
            player.windowsMediaPlayer.controls.currentPosition = newVal;
            // slider.Value += slider.SmallChange * e.Delta;
        }
    }
}
