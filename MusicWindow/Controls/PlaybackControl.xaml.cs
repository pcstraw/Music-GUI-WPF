using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
                    Track = value.Name;
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
            player.TrackChangeEvent += Player_TrackChangeEvent;
            player.PauseEvent += Player_PauseEvent;
        }

        private void Player_PauseEvent(object sender, EventArgs args)
        {
            UpdatePlayState();
        }

        internal void UpdatePlayState()
        {
            switch (player.PlayState)
            {
                case PlayState.IsPlaying:
                    PlayButton.Content = "Pause";
                    break;
                case PlayState.IsPaused:
                    PlayButton.Content = "Play";
                    break;
                case PlayState.Stopped:
                    PlayButton.Content = "Play";
                    break;
                default:
                    break;
            }
        }

        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            UpdatePlayState();
        }

        private void Player_PlayEvent(object sender, EventArgs args)
        {
            Song = player.CurrentSong;
            UpdatePlayState();
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
            if (Song == null || !File.Exists(Song.Filepath))
            {
                List<string> file = tool.SelectFiles(false, false,"Select Audio File");
                if (file.Count == 1)
                    AddFile(file[0]);
            }
            else
                tool.OpenFileDirectory(Song.Filepath);
        }
        //dep
        public void AddFile(string path)
        {
            Song = SongInfo.Instance.GetInfo(path);
            player.CurrentSong = Song;
        }

        private void Click_Replay_Button(object sender, RoutedEventArgs e)
        {
            player.Replay();
            UpdatePlayState();
            e.Handled = true;
        }

        private void Click_Stop_Button(object sender, RoutedEventArgs e)
        {
            player.Stop();
            UpdatePlayState();
            e.Handled = true;
        }
        

        private void Click_Last_Button(object sender, RoutedEventArgs e)
        {
            player.PrevTrack();
            e.Handled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
            UpdatePlayState();
        }
        
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            player.NextTrack();
            e.Handled = true;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            switch (player.PlayState)
            {
                case PlayState.IsPlaying:
                    player.Pause();
                    return;
                case PlayState.IsPaused:
                    player.Resume();
                    return;
                case PlayState.Stopped:
                    player.Play();
                    break;
                default:
                    break;
            }
        }

        bool _dark;
        void ToggleSkinMode()
        {
            _dark = !_dark;
            if(_dark)
                Background = new SolidColorBrush(Color.FromRgb(181,181,182));
            else
                Background = new SolidColorBrush(Color.FromRgb(243, 165, 81));
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToggleSkinMode();
        }
    }
}
