using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.IO;

namespace MusicWindow
{
    public partial class PlaybackControl : INotifyPropertyChanged
    {
        public PlaybackControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        Song _song;
        string _track;
        string _defaultText;
        string _trackLength;
        string _trackPosition;

        public MusicPlayer player { get { return MusicPlayer.Player; } }

        public string trackLength
        {
            get { return _trackLength; }
            private set
            {
                if (_trackLength != value)
                {
                    _trackLength = value;
                    OnPropertyChanged();
                }
            }
        }

        public string trackPosition
        {
            get { return _trackPosition; }
            private set
            {
                if (_trackPosition != value)
                {
                    _trackPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public Song Song
        {
            get { return _song; }
            set
            {
                if (_song != value)
                {
                    _song = value;
                    Track = value.name;
                }
            }
        }
        
        public string Track {
            get { return _track; }
            set { if(_track != value){
                    _track = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void Loaded_Trackname_Label(object sender, RoutedEventArgs e)
        {
            
        }

        private void Start()
        {
            _defaultText = "Select Audio File";
            Track = _defaultText;
            //Directories = Application.Current.Properties["PlaylistDirectories"] as List<string>;
            trackLength = "End";
            trackPosition = "Start";
            player.TickEvent += Player_TickEvent;
            player.Start(); //should be called at the start of the program
            player.PlayEvent += Player_PlayEvent; //unsubcribe on dispose/close
        }

        private void Player_PlayEvent(object sender, EventArgs args)
        {
            Song = player.CurrentSong;
        }

        private void Player_TickEvent(object sender, EventArgs args)
        {
            if (player != null && player.windowsMediaPlayer.currentMedia != null)
            {
                trackLength = player.windowsMediaPlayer.currentMedia.durationString;
                trackPosition = player.windowsMediaPlayer.controls.currentPositionString;
            }
        }

        private void DoubleClick_Trackname_Label(object sender, MouseButtonEventArgs e)
        {
            if (Song == null || !File.Exists(Song.path))
            {
                List<string> file = tool.SelectFiles(false, false,"Select Audio File");
                if (file.Count == 1)
                    AddFile(file[0]);
            }
            else
                tool.OpenFileDirectory(Song.path);
        }

        public void AddFile(string path)
        {
            Song = SongInfo.Instance.GetInfo(path);
            player.CurrentSong = Song;
        }

        private void MouseLeftButtonUp_Play_Button(object sender, MouseButtonEventArgs e)
        {
            if(Track == _defaultText)
            {
                tool.show(4, "Click on 'Select Audio File' to browse for files");
                return;
            }
            player.Play();
        }
        
        private void Click_Stop_Button(object sender, RoutedEventArgs e)
        {
            player.Stop();
        }

        private void Click_Next_Button(object sender, RoutedEventArgs e)
        {
            player.NextTrack();
        }

        private void Click_Last_Button(object sender, RoutedEventArgs e)
        {
            player.PrevTrack();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        }
    }
}
