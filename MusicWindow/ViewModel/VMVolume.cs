using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Glaxion.Music;
using Glaxion.Tools;

namespace Glaxion.ViewModel
{
    public class VMVolume : INotifyPropertyChanged
    {
        public VMVolume()
        {
        }

        MusicPlayer player;

        double _sliderValue;
        public double SliderValue
        {
            get { return _sliderValue; }
            set { _sliderValue = value;
                int _vol = (int)value;
                player.SetVolume(_vol);
                VolumeAsText = _vol.ToString();
                OnPropertyChanged(); }
        }
        
        /*
        public int Volume
        {
            get { return player.Volume; }
            set { player.SetVolume(value); VolumeAsText = value.ToString(); OnPropertyChanged(); }
        }
        */

        public string _volumeAsText;
        public string VolumeAsText
        {
            get { return _volumeAsText; }
            private set { _volumeAsText = value; OnPropertyChanged(); }
        }
        
        double _sliderMax;
        public double SliderMaximum
        {
            get { return _sliderMax; }
            private set { _sliderMax = value; OnPropertyChanged(); }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Load()
        {
            player = MusicPlayer.Player;
            SliderValue = player.Volume;
            SliderMaximum = 100;
        }

        internal void MouseWheelChange(double delta)
        {
            double change = delta;
            if (change < 0)
                change = -1;
            else
                change = 1;

            double newVal = _sliderValue + change;
            if (newVal > SliderMaximum)
                newVal = SliderMaximum;
            if (newVal < 0)
                newVal = 0;

            SliderValue = newVal;
            //Volume = SliderValue;
        }

        //the x or y value to jump
        //typically this will be one of the mouse coordinatesrelative to the window.
        //depending on the orientation of the slider
        //sliderWidth:  the width of the slider, not the control.
        public void JumpToPosition(double position, double sliderWidth)
        {
            SliderValue = GetMousePositionRelativeToSlider(position, sliderWidth);
        }

        internal double GetMousePositionRelativeToSlider(double position,double sliderWidth)
        {
            return (position / sliderWidth) * SliderMaximum;
        }
    }
}
