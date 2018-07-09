using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Save();
        }
        
        public static void RestoreMainControl(MainControl mainControl)
        {
            RestoreMusicFileDirectories(mainControl.fileControl.musicFileControl);
            mainControl.fileControl.musicFileControl.ViewModel.LoadFilesToTree();

            RestorePlaylistFileDirectories(mainControl.fileControl.playlistFileControl);
            mainControl.fileControl.playlistFileControl.ViewModel.LoadFilesToTree();

            RestorePlaylistControl(mainControl.playlistControl);
            RestoreCurrentSong(MusicPlayer.Player, SongInfo.Instance);
            RestoreVolumeLevel(MusicPlayer.Player);
            RestoreCurrentTrackPosition(MusicPlayer.Player);
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
            List<string> plist = playlistControl.GetPlaylistsAsFiles();
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

        public static void StoreCurrentTrackPosition(MusicPlayer player)
        {
            if (player != null && player.CurrentSong != null)
                Properties.Settings.Default.TrackPosition = player.positionIndex;
        }

        public static void RestoreMusicFileDirectories(MusicFileControl musicFileControl)
        {
            foreach (string dir in Properties.Settings.Default.MusicFileDirectories)
                musicFileControl.ViewModel.fileLoader.AddDirectory(dir);
        }

        public static void RestorePlaylistFileDirectories(PlaylistFileControl playlistFileControl)
        {
            foreach (string dir in Properties.Settings.Default.PlaylistDirectories)
                playlistFileControl.ViewModel.fileLoader.AddDirectory(dir);
        }

        public static void RestorePlaylistControl(PlaylistControl playlistControl)
        {
            foreach (string s in Properties.Settings.Default.ManagedPlaylists)
                playlistControl.AddPlaylistFromFile(s);
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
            Song s = SongInfo.Instance.GetInfo(Properties.Settings.Default.CurrentSong);
            player.CurrentSong = s;
        }

        public static void RestoreVolumeLevel(MusicPlayer player)
        {
            if (player != null)
                return;
            player.Volume =Properties.Settings.Default.Volume;
        }

        public static void RestoreCurrentTrackPosition(MusicPlayer player)
        {
            if (player != null)
                return;
            player.positionIndex = Properties.Settings.Default.TrackPosition;
        }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
