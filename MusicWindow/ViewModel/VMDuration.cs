using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Glaxion.Music;
using Glaxion.Tools;

namespace Glaxion.ViewModel
{
    public interface IDurationMouseInput
    {
        double GetMouseInput();
        double GetSliderLength();
        double GetSliderValue();
    }
    
    public class VMDuration : INotifyPropertyChanged
    {
        public VMDuration(IDurationMouseInput MouseInterface)
        {
            _mouseInterface = MouseInterface;
            AllowSkipPreview = true;
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
            private set { _tp = value;
                OnPropertyChanged(); }
        }

        double _td;
        public double TrackDuration
        {
            get { return _td; }
            private set { _td = value; OnPropertyChanged(); }
        }

        double _sliderMax;
        public double SliderMaximum
        {
            get { return _sliderMax; }
            set { _sliderMax = value; OnPropertyChanged(); }
        }
        public bool AllowSkipPreview { get; private set; }

        /*
double _sliderValue;
public double SliderValue
{
get { return _sliderValue; }
set
{
_sliderValue = value;
//int _vol = (int)value;
OnPropertyChanged();
}
}
*/


        MusicPlayer player;
        IDurationMouseInput _mouseInterface;
        bool isUserdragging;
        bool wasdragging = false;
        public double manipulatedPosition;
        public void SetTrackPosition(double Value)
        {
            player.windowsMediaPlayer.controls.currentPosition = Value;
        }
        
        internal void Load()
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
                if (player.windowsMediaPlayer.currentMedia.duration != TrackDuration)
                {
                    TrackDuration = player.trackDuration;
                    SliderMaximum = player.trackDuration;
                }
                
                //we need to block the tick update while the user changes the slider value

                if (!AllowSkipPreview)
                {
                    if (isUserdragging)
                    {
                        double mouseInput = _mouseInterface.GetMouseInput();
                        double sliderLength = _mouseInterface.GetSliderLength();
                        manipulatedPosition = GetMousePositionRelativeToSlider(mouseInput, sliderLength);
                        TrackPosition = manipulatedPosition;
                        wasdragging = true;
                    }
                    else
                    {
                        if (wasdragging)
                        {
                            player.windowsMediaPlayer.controls.currentPosition = manipulatedPosition;
                            TrackPosition = player.windowsMediaPlayer.controls.currentPosition;
                            wasdragging = false;
                        }
                        else
                            TrackPosition = player.windowsMediaPlayer.controls.currentPosition;
                    }
                }
                else
                {
                    if (isUserdragging)
                    {
                        double mouseInput = _mouseInterface.GetMouseInput();
                        double sliderLength = _mouseInterface.GetSliderLength();
                        manipulatedPosition = GetMousePositionRelativeToSlider(mouseInput, sliderLength);
                        player.windowsMediaPlayer.controls.currentPosition = manipulatedPosition;

                    }
                    else
                        manipulatedPosition = player.windowsMediaPlayer.controls.currentPosition;

                    TrackPosition = manipulatedPosition;
                }
            }
        }

        double GetMousePositionRelativeToSlider(double position, double sliderWidth)
        {
            return (position / sliderWidth) * SliderMaximum;
        }
        
        internal void SetUserIsDragging(bool Value)
        {
            isUserdragging = Value;
        }

        internal void MouseWheelChange(double delta)
        {
            //we need to block the tick update while the user changes the slider value
           // isUsingMouseWheel = true;
            double change = delta;
            if (change < 0)
                change = -5;
            else
                change = 5;

            double sv = _mouseInterface.GetSliderValue();
            double newVal = sv + change;
            if (newVal > SliderMaximum)
                newVal = SliderMaximum;
            if (newVal < 0)
                newVal = 0;

            SetTrackPosition(newVal);
            // slider.Value += slider.SmallChange * e.Delta;
        }
    }
}
