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
            StoreMusicFileDirectories(mainControl.fileControl.musicFileControl);
            StorePlaylistFileDirectories(mainControl.fileControl.playlistFileControl);
            StorePlaylistControl(mainControl.playlistControl);
            StoreCurrentSong(MusicPlayer.Player);
            StoreVolumeLevel(MusicPlayer.Player);
            StoreCurrentTrackPosition(MusicPlayer.Player);
            StoreLastPlaylist(MusicPlayer.Player);
            StoreCurrentPlayState(MusicPlayer.Player);
            Save();
        }
        
        public static void RestoreMainControl(MainControl mainControl)
        {
            RestoreMusicFileDirectories(mainControl.fileControl.musicFileControl);
            mainControl.fileControl.musicFileControl.ViewModel.LoadFilesToTree();

            RestorePlaylistFileDirectories(mainControl.fileControl.playlistFileControl);
            mainControl.fileControl.playlistFileControl.ViewModel.LoadFilesToTree();
            RestorePlaylistControl(mainControl.playlistControl);

           // RestoreCurrentPlayState(MusicPlayer.Player);
            RestoreCurrentSong(MusicPlayer.Player, SongInfo.Instance);
            RestoreVolumeLevel(MusicPlayer.Player);
            
        }

        public static void StoreMusicFileDirectories(MusicFileControl musicFileControl)
        {
            Properties.Settings.Default.MusicFileDirectories.Clear();

            foreach(string dir in musicFileControl.ViewModel.fileLoader.Directories)
                Properties.Settings.Default.MusicFileDirectories.Add(dir);
        }

        public static void StorePlaylistFileDirectories(PlaylistFileControl playlistFileControl)
        {
            Properties.Settings.Default.PlaylistDirectories.Clear();

            foreach (string dir in playlistFileControl.ViewModel.fileLoader.Directories)
            {
                Properties.Settings.Default.PlaylistDirectories.Add(dir);
            }
        }
        
        public static void StorePlaylistControl(PlaylistControl playlistControl)
        {
            Properties.Settings.Default.ManagedPlaylists.Clear();
            List<string> plist = playlistControl.viewModel.GetPlaylistsAsFiles();
            foreach (string s in plist)
                Properties.Settings.Default.ManagedPlaylists.Add(s);
        }

        public static void StoreCurrentSong(MusicPlayer player)
        {
            if(player != null && player.CurrentSong != null)
                Properties.Settings.Default.CurrentSong = player.CurrentSong.Filepath;
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
                playlistControl.viewModel.AddPlaylistFromFile(s);
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

            Song s = SongInfo.Instance.GetInfo(Properties.Settings.Default.CurrentSong);
            if(!File.Exists(s.Filepath))
            {
                Properties.Settings.Default.CurrentSong = "";
                return;
            }
            if (p.failed)
                player.Play(s);
            else
                player.PlayPlaylist(p,s);
            
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
