using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glaxion.ViewModel;
using System.IO;

namespace MusicWindow
{
    public class SaveSettings
    {
        public static void SaveMainControl(MainControl mainControl)
        {
           // StoreMusicFileDirectories(mainControl.fileControl.musicFileControl);
           // StorePlaylistFileDirectories(mainControl.fileControl.playlistFileControl);
            StorePlaylistControl(mainControl.playlistControl);
            StoreCurrentSong(MusicPlayer.Player);
            StoreVolumeLevel(MusicPlayer.Player);
            StoreCurrentTrackPosition(MusicPlayer.Player);
            StoreLastPlaylist(MusicPlayer.Player);
            StoreCurrentPlayState(MusicPlayer.Player);
            StoreEditingProgram(tool.MusicEditingProgram);
            StoreShowConsole(mainControl.consoleCheckBox.IsChecked.Value);
            Save();
        }

        private static void StoreShowConsole(bool state)
        {
            Properties.Settings.Default.ShowConsole = state;
        }

        private static void RestoreShowConsole(MainControl mainControl)
        {
            mainControl.consoleCheckBox.IsChecked = Properties.Settings.Default.ShowConsole;
        }

        private static void StoreEditingProgram(string musicEditingProgram)
        {
            if(File.Exists(musicEditingProgram))
            {
                Properties.Settings.Default.EditingProgram = musicEditingProgram;
            }
        }

        public static void RestoreMainControl(MainControl mainControl)
        {
           // RestoreMusicFileDirectories(mainControl.fileControl.musicFileControl);
            Database.Instance.RetreiveMusicFiles();
           // mainControl.fileControl.musicFileControl.ViewModel.
            mainControl.fileControl.musicFileControl.ViewModel.LoadDirectoriesToTree();

            //RestorePlaylistFileDirectories(mainControl.fileControl.playlistFileControl);
            Database.Instance.RetreivePlaylistFiles();
            //mainControl.fileControl.playlistFileControl.ViewModel.LoadFilesToTree();
            mainControl.fileControl.playlistFileControl.ViewModel.LoadDirectoriesToTree();

            RestorePlaylistControl(mainControl.playlistControl);
            RestoreShowConsole(mainControl);
            //RestoreCurrentPlayState(MusicPlayer.Player);
          //  RestoreCurrentSong(MusicPlayer.Player, SongInfo.Instance);
            RestoreVolumeLevel(MusicPlayer.Player);
            RestoreMusicEditingProgram();
        }

        private static void RestoreMusicEditingProgram()
        {
            string filePath = Properties.Settings.Default.EditingProgram;
            if (File.Exists(filePath))
                tool.MusicEditingProgram = filePath;
        }

        public static void StoreMusicFileDirectories(MusicFileControl musicFileControl)
        {
            Properties.Settings.Default.MusicFileDirectories.Clear();
            foreach(FileDirectory dir in musicFileControl.ViewModel.fileLoader.Directories)
                Properties.Settings.Default.MusicFileDirectories.Add(dir.directory);
        }

        public static void StorePlaylistFileDirectories(PlaylistFileControl playlistFileControl)
        {
            Properties.Settings.Default.PlaylistDirectories.Clear();
            foreach (FileDirectory dir in playlistFileControl.ViewModel.fileLoader.Directories)
                Properties.Settings.Default.PlaylistDirectories.Add(dir.directory);
        }
        
        public static void StorePlaylistControl(PlaylistControl playlistControl)
        {
            Properties.Settings.Default.ManagedPlaylists.Clear();
            List<string> plist = playlistControl.listView.playlistManager.GetPlaylistsAsFiles();
            foreach (string s in plist)
                Properties.Settings.Default.ManagedPlaylists.Add(s);
        }

        public static void StoreCurrentSong(MusicPlayer player)
        {
            if(player != null && player.CurrentSong != null)
                Properties.Settings.Default.CurrentSong = player.CurrentTrackIndex;
        }

        public static void StoreVolumeLevel(MusicPlayer player)
        {
            if (player != null)
                Properties.Settings.Default.Volume = player.Volume;
        }

        public static void StoreLastPlaylist(MusicPlayer player)
        {
            if (player != null && player.currentList != null)
            {
                Properties.Settings.Default.LastPlaylist = player.currentList.Filepath;
            }
        }

        public static void StoreCurrentTrackPosition(MusicPlayer player)
        {
            if (player != null && player.CurrentSong != null)
                Properties.Settings.Default.TrackPosition = player.trackPosition;
        }

        public static void StoreCurrentPlayState(MusicPlayer player)
        {
            if (player != null)
                Properties.Settings.Default.PlayState = (int)player.PlayState;
        }

        //dep
        public static void RestoreCurrentPlayState(MusicPlayer player)
        {
            if (player != null)
                player.PlayState = (PlayState)Properties.Settings.Default.PlayState;
        }
        
        public static void RestoreMusicFileDirectories(MusicFileControl musicFileControl)
        {
            foreach (string dir in Properties.Settings.Default.MusicFileDirectories)
                musicFileControl.ViewModel.fileLoader.AddDirectory(dir);
        }

        public static void RestorePlaylistFileDirectories(PlaylistFileControl playlistFileControl)
        {
            foreach (string dir in Properties.Settings.Default.PlaylistDirectories)
                playlistFileControl.ViewModel.AddDirectory(dir);
        }

        public static void RestorePlaylistControl(PlaylistControl playlistControl)
        {
            foreach (string s in Properties.Settings.Default.ManagedPlaylists)
                playlistControl.listView.playlistManager.AddPlaylistFromFile(s);

            if (playlistControl.listView.Items.Count < 0)
                return;
            VMPlaylist p = playlistControl.FindPlaylist(Properties.Settings.Default.LastPlaylist);
            if (p == null)
                return;

            TrackControl  tc = playlistControl.AddDockColumn(p);
            tc.viewModel.PlaySong(Properties.Settings.Default.CurrentSong, false);

            PlayState ps = (PlayState)Properties.Settings.Default.PlayState;
            if (ps == PlayState.IsPaused)
                MusicPlayer.Player.Pause();
            if (ps == PlayState.Stopped)
                MusicPlayer.Player.Stop();
            if (ps == PlayState.IsPlaying)
                MusicPlayer.Player.Resume(Properties.Settings.Default.TrackPosition);
        }
        

        public static void RestoreCurrentSong(MusicPlayer player,SongInfo fileLoader)
        {
            if (player == null)
            {
                tool.debugError("Error restoring current song:  Player is null");
                return;
            }
            if(fileLoader == null)
            {
                tool.debugError("Error restoring current song:  fileLoader is null");
                return;
            }
            
            Playlist p = new Playlist(Properties.Settings.Default.LastPlaylist, true);
            PlayState ps = (PlayState)Properties.Settings.Default.PlayState;

            int current_song = Properties.Settings.Default.CurrentSong;
            if (current_song < 0 || current_song >= p.songs.Count)
                return;

            Song s = p.songs[Properties.Settings.Default.CurrentSong];
            if(!File.Exists(s.Filepath))
            {
                Properties.Settings.Default.CurrentSong = -1;
                return;
            }
            if (p.failed)
                player.Play(current_song);
            else
                player.PlayPlaylist(p,current_song);
            
            if (ps == PlayState.IsPaused)
                player.Pause();
            if (ps == PlayState.Stopped)
                player.Stop();
            if(ps == PlayState.IsPlaying)
                player.Resume(Properties.Settings.Default.TrackPosition);
        }

        public static void RestoreVolumeLevel(MusicPlayer player)
        {
            if (player == null)
                return;
            player.SetVolume(Properties.Settings.Default.Volume);
        }
        
        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
