using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MusicWindow
{
    public class PlaylistManagerListView : ListViewEx<VMPlaylist>
    {
        public PlaylistManagerListView() :base()
        {
            playlistManager = new VMPlaylistManager();
            viewModel = playlistManager;
            Loaded += PlaylistManagerListView_Loaded;
        }

        private void PlaylistManagerListView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            playlistManager.HookPlayerEvents();
            playlistManager.UpdatePlaylistColours();
        }

        internal VMPlaylistManager playlistManager;

        internal void OpenPlaylistFolders()
        {
            foreach (VMPlaylist p in SelectedItems)
                tool.OpenFileDirectory(p.Filepath);
        }

        internal void BrowseForPlaylistDirectory()
        {
            string startPath = null;
            if (SelectedItems.Count > 0)
                startPath = (SelectedItems[0] as VMPlaylist).Filepath;
            playlistManager.BrowsePlaylistDialog(startPath);
        }

        #region required override methods
        internal override void AddDataFromFiles(int insertionIndex, List<string> files)
        {
            foreach (string file in files)
            {
                playlistManager.AddPlaylistFromFile(file);
            }
        }
        
        protected override void AddData(int insertIndex, List<VMPlaylist> items)
        {
            int count = Items.Count;
            foreach (object o in items)
            {
                if (o is VMPlaylist)
                {
                    playlistManager.AddPlaylistFromFile(count, o as Playlist);
                }
                if (o is VMSong)
                {
                    VMSong s = o as VMSong;
                    playlistManager.AddPlaylistFromFile(count, s.Filepath);
                }
            }
        }
        
        #endregion
    }
}
