using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Glaxion.Music;
using Glaxion.Tools;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for VolumeControl.xaml
    /// </summary>
    public partial class VolumeControl : INotifyPropertyChanged
    {
        public VolumeControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        MusicPlayer player;
        public int Volume { get { return player.Volume; } set { player.SetVolume(value); VolumeAsText = value.ToString(); OnPropertyChanged(); } }
        public string _volumeAsText;
        public string VolumeAsText { get { return _volumeAsText; } set { _volumeAsText = value; OnPropertyChanged(); } }

        private void Loaded_Event(object sender, System.Windows.RoutedEventArgs e)
        {
            player = MusicPlayer.Player;
            Volume = 5;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Slider_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
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
        }
    }
}
