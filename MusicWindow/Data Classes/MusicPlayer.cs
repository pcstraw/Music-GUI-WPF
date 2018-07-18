using System;
using System.Collections.Generic;
using System.Linq;
using WMPLib;
using Glaxion.Tools;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace Glaxion.Music
{
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
            PrePlayEvent += MusicPlayer_BeforePlayEvent;
            MusicUpdatedEvent += MusicPlayer_MusicUpdated;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += t_Tick;
            CreateTempPlaybackFiles = true;
            _isRunning = false;
            //initialise to -1.  If we start at 0 then loading trackmanager
            //will highlight the  first track of the playlist before we start playing anything
            currentTrack = 0;
            Volume = 10;
            loop = false;
        }
        //used to call updatePlayliststate in trackmanager
        private void MusicPlayer_BeforePlayEvent(object sender, EventArgs args)
        {
        }
        
        public static void Create(string[] args)
        {
            //Player = new MusicPlayer();
           // Player._startPlayer = true;
            Player.startArguments = args;
        }
        
        //public static MusicControlGUI WinFormApp; 
       // public static Image MusicGUILogo = Glaxion.Music.Properties.Resources.music_gui_logo;
        public bool CreateTempPlaybackFiles { get; private set; }
       // bool _loop;
        public bool loop { get;set; }

        public WindowsMediaPlayer windowsMediaPlayer;
        public double positionIndex;
        public double trackDuration;
        public bool Stopped;

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

        public bool IsPlaying;
        public bool IsPaused;
        public bool mediaLoaded;
        public bool mediaLoading;
        public bool ProgramInitialized;
        //public bool NodeDrop;
        public bool Loop;
        public bool Muted;
        public bool InitializeDirectories;
        public bool Initialized;
        bool _isRunning;
        public Song prevTrack;
        //public string currentTrackString;
        public string[] startArguments;
        public int currentTrack;
        public int trackbarValue;
        private int prevVolume;
        public int Volume;
        public Playlist currentList;
       // private FileLoader fileLoader;
        public System.Windows.Forms.Timer timer; //dispose
        public static Color PlayColor = Color.Aquamarine;
        public static Color PreviousPlayColor = Color.DarkCyan;
        public static Color OldPlayColor = Color.MediumTurquoise;
        public static Color MissingColor = Color.OrangeRed;
        public static Color RepeatColor = Color.DarkSlateBlue;
        public static Color ConflictColor = Color.MediumVioletRed;
        internal static Color IsPlayingColor = Color.Yellow;

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
            tool.debugError("Playback Failed event is being called twice");
            return;
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
        
        public bool Start()
        {
            //needs to be called from music file manager
            //GetSavedDirectories();
            //maybe be more explicit and loading the file loader
            //fileLoader = FileLoader.Instance;
            //fileLoader.Load();
            timer.Start();
            return true;
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
                    Resume(positionIndex);
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
            currentTrack = index;
            p.trackIndex = currentTrack;
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
                    Resume(positionIndex);
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

        public bool PlayPlaylist(Playlist playlist,Song song)
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
        
        public bool Mute()
        {
            Muted = !Muted;

            if (Muted)
            {
                prevVolume = Volume;
                SetVolume(0);
                return Muted;
            }
            else
            {
                SetVolume(prevVolume);
                return Muted;
            }
        }

        
        void t_Tick(object sender, EventArgs e)
        {
            if(!_isRunning)
            {
                SetVolume(Volume); //default volume may have been set through the playbackVolumeConbtrol.  Update it here 
                _isRunning = true;
            }

            TickEvent(sender, e);
            if (windowsMediaPlayer.playState == WMPPlayState.wmppsStopped)
            {
                if (loop)
                    PrevTrack();
                else
                {
                    if (IsPlaying)
                        NextTrack();
                }
            }
            
            if (windowsMediaPlayer != null && windowsMediaPlayer.currentMedia != null)
            {
                if (windowsMediaPlayer.openState == WMPLib.WMPOpenState.wmposMediaOpen)
                {
                   // mediaLoading = false;
                    if (!mediaLoaded)
                    {
                        if (MediaLoadedEvent != null)
                        {
                            MediaLoadedEvent(sender, e);
                            mediaLoaded = true;
                        }
                        trackDuration = windowsMediaPlayer.currentMedia.duration;
                    }
                    trackbarValue = (int)windowsMediaPlayer.controls.currentPosition;/// trackBar.Maximum;
                }
                else
                    mediaLoaded = false;
            }
        }
        
        
        public bool IsPlayingTrack(string path)
        {
            if (path == CurrentSong.Filepath)
                return true;
            return false;
        }

        //used to update file paths based on file name
        /*
        public List<string> SearchMusicFiles(string fileName)
        {
            List<string> ls = new List<string>();
            tool.debugWarning("Warning:  Lock fileLoader.MusicFiles before using it for searching");
            foreach (KeyValuePair<string, string> kv in fileLoader.MusicFiles)
            {
                if (kv.Value == fileName)
                    ls.Add(kv.Key);
            }
            return ls;
        }
        */

        public bool HasStopped()
        {
            if (windowsMediaPlayer.playState == WMPPlayState.wmppsStopped)
                return true;
            else
                return false;
        }
        
        public void NextTrack()
        {
            if (currentList == null)
                return;

            int nextindex = currentTrack + 1;
            if (nextindex >= currentList.songs.Count)
                nextindex = 0;
            
            if (File.Exists(currentList.songs[nextindex].Filepath))
            {
                if (IsPlaying)
                {
                    Song next = currentList.songs[nextindex];
                    Play(next);
                    return;
                }
                else
                {
                    currentTrack = nextindex;
                }
                NextEvent(null, EventArgs.Empty);
            }
            //else call playback failed?
        }

        public void PrevTrack()
        {
            if (currentList == null)
                return;

            int nextindex = currentTrack-1;
            if (nextindex < 0)
                nextindex = currentList.songs.Count-1;
            if (IsPlaying)
            {
                 Song next = currentList.songs[nextindex];
                 Play(next);
            }
            else
            {
                currentTrack = nextindex;
            }
            PrevEvent(null, EventArgs.Empty);
        }

        public bool Play()
        {
            return Play(CurrentSong);
        }

        public bool Play(Song song)
        {
            if (song == null)
            {
                PlaybackFailedEvent(null, EventArgs.Empty);
                //Stop();
                tool.show(2,"Playback Failed", "Song is null");
                return false;
            }
            string file = song.Filepath;
            if (!File.Exists(song.Filepath))
            {
                PlaybackFailedEvent(file, EventArgs.Empty);
                //Stop();
                tool.show(2, "INVALID FILE", "", file, "", "Playback Failed");
                return false;
            }
            
            PrePlayEvent(file, EventArgs.Empty);

            string f = file;
            if (CreateTempPlaybackFiles)
                f = CreateTempPlayFile(file);  //quickly create a temp file for playback so when can edit the tags

            windowsMediaPlayer.URL = f;
            windowsMediaPlayer.controls.play();
            if (CreateTempPlaybackFiles)
                DeleteTMPPlayedFiles();
            mediaLoading = true;
            IsPaused = false;
            IsPlaying = true;
            Stopped = false;
            /*
            if (index != currentTrack)
            {
                TrackChangeEvent(index, EventArgs.Empty);
            }
            */
            currentTrack = currentList.songs.IndexOf(song);
            CurrentSong = song;
            PlayEvent(song, EventArgs.Empty);
            return true;
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
            string temp_dir = tool.GetTempFolder();
            string file_name = Path.GetFileName(file);
            string ext = Path.GetExtension(file);
            string new_file = string.Concat(temp_dir, file_name,ext);

            if(File.Exists(new_file))
                return new_file;
            
            File.Copy(file, new_file, true);
            _currentPlayedTMP = new_file;
            return new_file;
        }
        
        public bool PlayFile(string file)
        {
            Song s = SongInfo.Instance.GetInfo(file);
            return Play(s);
        }
        
        public void Replay()
        {
            Stop();
            Play(CurrentSong);
        }

        public void Stop()
        {
            positionIndex = windowsMediaPlayer.controls.currentPosition;
            windowsMediaPlayer.controls.stop();
            IsPlaying = false;
            IsPaused = false;
            Stopped = true;
            StopEvent(null, EventArgs.Empty);
        }

        public void Pause()
        {
            windowsMediaPlayer.controls.pause();
            IsPlaying = false;
            IsPaused = true;
            positionIndex = windowsMediaPlayer.controls.currentPosition;
            PauseEvent(null, EventArgs.Empty);
        }

        public void Resume(double position)
        {
            windowsMediaPlayer.controls.play();
            windowsMediaPlayer.controls.currentPosition = position;
            IsPaused = false;
            IsPlaying = true;
            Stopped = false;
            positionIndex = position;
            ResumeEvent(null, EventArgs.Empty);
        }

        public void Resume()
        {
            Resume(positionIndex);
        }

        public void SetVolume(int value)
        {
            Volume = value;
            if (Muted)
                windowsMediaPlayer.settings.volume = 0;
            else
                windowsMediaPlayer.settings.volume = Volume;
        }
    }
}
