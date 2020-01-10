using System;
using WMPLib;
using Glaxion.Tools;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Glaxion.Music
{
    public enum PlayState
    {
        Stopped,
        IsPlaying,
        IsPaused
    }

    public class MusicPlayer
    {
        private MusicPlayer()
        {
            Construction();
        }
        public static MusicPlayer Player { get { return Nested.instance; } }
        private class Nested
        {
            static Nested() { }//lazy singleton
            internal static readonly MusicPlayer instance = new MusicPlayer();
        }

        private void Construction()
        {
            windowsMediaPlayer = new WindowsMediaPlayer();
            PlayEvent += On_Play;
            PauseEvent += On_Pause;
            ResumeEvent += On_Resume;
            NextEvent += On_Next;
            PrevEvent += On_Prev;
            StopEvent += On_Stop;
            TickEvent += On_Tick;
            DirectoriesLoadedEvent += On_DirectoriesLoaded;
            TrackChangeEvent += On_TrackChange;
            PlaybackFailedEvent += On_PlaybackFailed;
           // PrePlayEvent += MusicPlayer_BeforePlayEvent;
            MusicUpdatedEvent += MusicPlayer_MusicUpdated;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += t_Tick;
            CreateTempPlaybackFiles = true;
            _isRunning = false;
            //LoopPlaylist = true;
            //initialise to -1.  If we start at 0 then loading trackmanager
            //will highlight the  first track of the playlist before we start playing anything
            CurrentTrackIndex = 0;
            Volume = 10;
        }

        //used to call updatePlayliststate in trackmanager
        private void MusicPlayer_BeforePlayEvent(object sender, EventArgs args)
        {
        }
        
        public static void Create(string[] args)
        {
            Player.startArguments = args;
        }
        
        //public static MusicControlGUI WinFormApp; 
       // public static Image MusicGUILogo = Glaxion.Music.Properties.Resources.music_gui_logo;
        public bool CreateTempPlaybackFiles { get; private set; }
       // bool _loop;
       // public bool loop { get;set; }

        public WindowsMediaPlayer windowsMediaPlayer;
        public double trackPosition;
        public double trackDuration;
        public PlayState PlayState;
        Song _currentSong;

        public Song CurrentSong
        {
            get { return _currentSong; }
            set
            {
                if (_currentSong != value)
                    _currentSong = value;
                TrackChangeEvent(value, EventArgs.Empty);
            }
        }

        public bool mediaLoaded;
        //public bool LoopPlaylist;
       // public bool mediaLoading;
        public bool ProgramInitialized;
        public bool Loop;
        public bool Muted;
        public bool InitializeDirectories;
        public bool Initialized;
        bool _isRunning;
        public Song prevTrack;
        public string[] startArguments;
        public int CurrentTrackIndex;
        public int trackbarValue;
        private int prevVolume;
        public int Volume;
        public Playlist currentList;
        public System.Windows.Forms.Timer timer; //dispose
        public static Color PlayColor = Color.Aquamarine;
        public static Color PreviousPlayColor = Color.DarkCyan;
        public static Color OldPlayColor = Color.MediumTurquoise;
        public static Color MissingColor = Color.OrangeRed;
        public static Color RepeatColor = Color.DarkSlateBlue;
        public static Color ConflictColor = Color.MediumVioletRed;
        internal static Color IsPlayingColor = Color.Yellow;

        public bool IsPlaying { get { if (PlayState == PlayState.IsPlaying)
                    return true;
                return false;
            } }

        public bool IsPaused
        {
            get
            {
                if (PlayState == PlayState.IsPaused)
                    return true;
                return false;
            }
        }

        public bool HasStopped
        {
            get
            {
                if (PlayState == PlayState.Stopped)
                    return true;
                return false;
            }
        }

        //event handlers
        public delegate void MusicUpdatedEventHandler(object sender, EventArgs args);
        public event MusicUpdatedEventHandler MusicUpdatedEvent;
        protected void MusicPlayer_MusicUpdated(object sender, EventArgs e)
        {
        }
        
        public delegate void PlaybackFailedEventHandler(object sender, EventArgs args);
        public event PlaybackFailedEventHandler PlaybackFailedEvent;
        protected void On_PlaybackFailed(object sender, EventArgs e)
        {
            tool.ShowConsole();
            if (sender is string)
                tool.debugError("Unable to play file: ", sender as string);
            else
                tool.debugError("Playback Failed");
        }

        public delegate void PlayEventHandler(object sender, EventArgs args);
        public event PlayEventHandler PlayEvent;
        protected void On_Play(object sender, EventArgs e)
        {
        }
        
        public delegate void PauseEventHandler(object sender, EventArgs args);
        public event PauseEventHandler PauseEvent;
        protected void On_Pause(object sender, EventArgs e)
        {
        }
        
        public delegate void NextEventHandler(object sender, EventArgs args);
        public event NextEventHandler NextEvent;
        protected void On_Next(object sender, EventArgs e)
        {
        }
        
        public delegate void PrevEventHandler(object sender, EventArgs args);
        public event PrevEventHandler PrevEvent;
        protected void On_Prev(object sender, EventArgs e)
        {
        }
        
        public delegate void StopEventHandler(object sender, EventArgs args);
        public event StopEventHandler StopEvent;
        protected void On_Stop(object sender, EventArgs e)
        {
        }
        
        public delegate void ResumeEventHandler(object sender, EventArgs args);
        public event ResumeEventHandler ResumeEvent;
        protected void On_Resume(object sender, EventArgs e)
        {
        }

        public delegate void TickEventHandler(object sender, EventArgs args);
        public event TickEventHandler TickEvent;
        protected void On_Tick(object sender, EventArgs e)
        {
        }

        public delegate void TrackChangeEventHandler(object sender, EventArgs args);
        public event TrackChangeEventHandler TrackChangeEvent;
        protected void On_TrackChange(object sender, EventArgs e)
        {
        }

        public delegate void MediaLoadedEventHandler(object sender, EventArgs args);
        public event MediaLoadedEventHandler MediaLoadedEvent;
        protected void On_MediaLoaded(object sender, EventArgs e)
        {
        }

        public delegate void DirectoriesLoadedEventHandler(object sender, EventArgs args);
        public event DirectoriesLoadedEventHandler DirectoriesLoadedEvent;
        protected void On_DirectoriesLoaded(object sender, EventArgs e)
        {
        }

        public delegate void DirectoriesAddedEventHandler(object sender, EventArgs args);
        public event DirectoriesAddedEventHandler DirectoriesAddedEvent;
        protected void On_DirectoriesAdded(object sender, EventArgs e)
        {
        }
        
        public delegate void BeforePlayEventHandler(object sender, EventArgs args);
        public event BeforePlayEventHandler PrePlayEvent;
        protected void On_BeforePlay(object sender, EventArgs e)
        {
        }
        
        public void Start()
        {
            timer.Start();
        }

        public void UseMediaKeys(Keys KeyCode)
        {
           // Tool.Debug(k.KeyCode.ToString());
            if (KeyCode == Keys.MediaPlayPause)
            {
                if (IsPlaying)
                {
                    Pause();
                    return;
                }
                if (IsPaused)
                {
                    Resume(trackPosition);
                    return;
                }
                if (!IsPlaying && !IsPaused)
                {
                    Play();
                    return;
                }
            }

            if (KeyCode == Keys.MediaNextTrack)
            {
                NextTrack();
                return;
            }
            if (KeyCode == Keys.MediaPreviousTrack)
            {
                PrevTrack();
                return;
            }
            if (KeyCode == Keys.MediaStop)
            {
                Stop();
                return;
            }
            if (KeyCode == Keys.VolumeMute)
            {
                Mute();
                return;
            }
        }
        
        public void UpdateMusicPlayer(Playlist p,int index)
        {
            if (p == null)
            {
                tool.Show(5,"error: playlist is null");
                return;
            }
            if (index < 0)
            {
                index = 0;
            }
            if (index > p.songs.Count)
                index = p.songs.Count - 1;

            currentList = p;
            CurrentTrackIndex = index;
            p.trackIndex = CurrentTrackIndex;
            MusicUpdatedEvent(p, EventArgs.Empty);
        }
        
        public void OnKeydown(object o, System.Windows.Forms.KeyEventArgs k)
        {
            tool.debug(k.KeyCode.ToString());
            if (k.KeyCode == Keys.MediaPlayPause)
            {
                if (IsPlaying)
                {
                    Pause();
                    return;
                }
                if (IsPaused)
                {
                    Resume(trackPosition);
                    return;
                }
                if (!IsPlaying && !IsPaused)
                {
                    Play();
                    return;
                }
            }

            if (k.KeyCode == Keys.MediaNextTrack)
            {
                NextTrack();
                return;
            }
            if (k.KeyCode == Keys.MediaPreviousTrack)
            {
                PrevTrack();
                return;
            }
            if (k.KeyCode == Keys.MediaStop)
            {
                Stop();
                return;
            }
            if (k.KeyCode == Keys.VolumeMute)
            {
                Mute();
                return;
            }
        }
        
        public bool Mute()
        {
            Muted = !Muted;
            if (Muted){
                windowsMediaPlayer.settings.volume = 0;
                return Muted;
            } else{
                windowsMediaPlayer.settings.volume = Volume;
            }
            return Muted;
        }
        
        void t_Tick(object sender, EventArgs e)
        {
            if(!_isRunning)
            {
                SetVolume(Volume); //default volume may have been set through the playbackVolumeConbtrol.  Update it here 
                _isRunning = true;
            }

            if (IsPaused)
            {
                if (!Muted)
                    Mute();
                windowsMediaPlayer.controls.pause();
            }else{
                if (Muted)
                    Mute();
            }

            TickEvent(sender, e);
            
            if (windowsMediaPlayer.playState == WMPPlayState.wmppsStopped)
            {
                if (IsPlaying)
                    NextTrack();
            }
            
            if (windowsMediaPlayer.currentMedia != null)
            {
                if (windowsMediaPlayer.openState == WMPLib.WMPOpenState.wmposMediaOpen)
                {
                    if (!mediaLoaded)
                    {
                        if (MediaLoadedEvent != null)
                        {
                            MediaLoadedEvent(sender, e);
                            mediaLoaded = true;
                        }
                        trackDuration = windowsMediaPlayer.currentMedia.duration;
                    }
                    if (!IsPaused)
                    {
                        trackPosition = windowsMediaPlayer.controls.currentPosition;
                        trackbarValue = (int)trackPosition;/// trackBar.Maximum;
                    }
                }else
                    mediaLoaded = false;
            }
        }
        
        
        public bool IsPlayingTrack(string path)
        {
            if (path == CurrentSong.Filepath)
                return true;
            return false;
        }

        bool LoadTrack(int index)
        {
            if (!LoadIntoWMP(index))
                return false;
            CurrentTrackIndex = index;
            SetPlayingSong(currentList.songs[index]);
            PlayEvent(CurrentSong, EventArgs.Empty);
            return true;
        }
        
        public void NextTrack()
        {
            int next = CurrentTrackIndex + 1;
            if (next >= currentList.songs.Count)
            {
                Stop();
                return;
            }
            
            while( next < currentList.songs.Count)
            {
                if (!IsValidIndex(next))
                {
                    Stop();
                    return;
                }
                if (IsPlaying)
                {
                    if (Play(next))
                    {
                        NextEvent(this, EventArgs.Empty);
                        return;
                    }
                }else{
                    if (LoadTrack(next))
                    {
                        NextEvent(this, EventArgs.Empty);
                        trackPosition = 0;
                        return;
                    }
                }
                next++;
            }
        }

        public void PrevTrack()
        {
            int next = CurrentTrackIndex-1;
            if (next < 0)
                next = 0;

            while (next > -1)
            {
                if (!IsValidIndex(next))
                {
                    Stop();
                    return;
                }
                if (IsPlaying)
                {
                    if (Play(next))
                    {
                        NextEvent(this, EventArgs.Empty);
                        return;
                    }
                } else{
                    if (LoadTrack(next))
                    {
                        NextEvent(this, EventArgs.Empty);
                        trackPosition = 0;
                        return;
                    }
                }
                next--;
            }
        }

        public bool PlayPlaylist(Playlist playlist, int song)
        {
            if (playlist == null)
                return false;

            if (currentList != playlist)
            {
                currentList = playlist;
                MusicUpdatedEvent(playlist, EventArgs.Empty);
            }
            return Play(song);
        }

        public bool Play()
        {
            return Play(CurrentTrackIndex);
        }
        
        public bool IsValidIndex(int index)
        {
            if (currentList == null)
            {
                tool.show(2, "Playlist is null", index.ToString());
                return false;
            }
            if (currentList.songs == null)
            {
                PlaybackFailedEvent(index, EventArgs.Empty);
                tool.show(2, "Tracks are null", index.ToString());
                return false;
            }
            if(currentList.songs.Count == 0)
            {
                PlaybackFailedEvent(index, EventArgs.Empty);
                tool.show(2, "track count is 0", index.ToString());
                return false;
            }
            if (currentList.songs[index] == null)
            {
                tool.show(2, "Song at index " + index.ToString() + " is null");
                PlaybackFailedEvent(index, EventArgs.Empty);
                return false;
            }
            return true;
        }

        internal bool LoadIntoWMP(int index)
        {
            if (!IsValidIndex(index))
                return false;

            Song song = currentList.songs[index];
            string file = song.Filepath;

            if (!File.Exists(song.Filepath))
            {
                //tool.show(2, "INVALID FILE", "", file, "", "Playback Failed");
                PlaybackFailedEvent(file, EventArgs.Empty);
                return false;
            }

            //PrePlayEvent(file, EventArgs.Empty);
            windowsMediaPlayer.URL = file;
            return true;
        }

        public bool Play(int index)
        {
            if (LoadIntoWMP(index))
            {
                string file = currentList.songs[index].Filepath;
                if (CreateTempPlaybackFiles)
                {
                    string temp_f = CreateTempPlayFile(file);  //quickly create a temp file for playback so we can edit the tags
                    windowsMediaPlayer.URL = temp_f;
                }
                windowsMediaPlayer.controls.play();
                if (CreateTempPlaybackFiles)
                    DeleteTMPPlayedFiles();
            }else
                return false;

            PlayState = PlayState.IsPlaying;
            CurrentTrackIndex = index;
            SetPlayingSong(currentList.songs[index]);
            PlayEvent(CurrentSong, EventArgs.Empty);
            return true;
        }

        Song _lastPlayed;
        void SetPlayingSong(Song song)
        {
            if(_lastPlayed != null)
                _lastPlayed.State = SongState.HasPlayed;

            if (CurrentSong != null)
            {
                CurrentSong.State = SongState.WasPlaying;
                _lastPlayed = CurrentSong;
            }
            CurrentSong = song;
            CurrentSong.State = SongState.IsPlaying;
        }

        async void DeleteTMPPlayedFiles()
        {
            if (_lastPlayedTMP == null)
            {
                _lastPlayedTMP = _currentPlayedTMP;
                return;
            }
            await tool.DeleteAsyncFile(_lastPlayedTMP);
            _lastPlayedTMP = _currentPlayedTMP;
        }
        
        string _lastPlayedTMP;
        string _currentPlayedTMP;
        private string CreateTempPlayFile(string file)
        {
            if(!File.Exists(file))
            {
                return file;
            }
            string temp_dir = tool.GetTempFolder();
            string file_name = Path.GetFileNameWithoutExtension(file);
            string ext = Path.GetExtension(file);
            string new_file = Path.Combine(temp_dir, string.Concat(file_name, ext));

            if (File.Exists(new_file))
            {
                _currentPlayedTMP = new_file;
                return new_file;
            }
            
            File.Copy(file, new_file, true);
            _lastPlayedTMP = _currentPlayedTMP;
            _currentPlayedTMP = new_file;
            return new_file;
        }
        
        public void Replay()
        {
            Stop();
            Play(CurrentTrackIndex);
        }

        public void Stop()
        {
            trackPosition = windowsMediaPlayer.controls.currentPosition;
            windowsMediaPlayer.controls.stop();
            PlayState = PlayState.Stopped;
            StopEvent(null, EventArgs.Empty);
        }

        public void Pause()
        {
            windowsMediaPlayer.controls.pause();
            PlayState = PlayState.IsPaused;
            PauseEvent(this, EventArgs.Empty);
        }

        public void Resume(double position)
        {
            windowsMediaPlayer.controls.play();
            windowsMediaPlayer.controls.currentPosition = position;
            PlayState = PlayState.IsPlaying;
            trackPosition = position;
            ResumeEvent(this, EventArgs.Empty);
            PlayEvent(this, EventArgs.Empty);
        }

        public void Resume()
        {
            Resume(trackPosition);
        }

        public void SetVolume(int value)
        {
            Volume = value;
            windowsMediaPlayer.settings.volume = Volume;
        }
    }
}
