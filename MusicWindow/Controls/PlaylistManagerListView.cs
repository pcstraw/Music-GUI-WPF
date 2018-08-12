using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MusicWindow
{
    public class PlaylistManagerListView : ListViewEx<Playlist>
    {
        public PlaylistManagerListView() :base()
        {
            playlistManager = new VMPlaylistManager();
            viewModel = playlistManager;
        }
        internal VMPlaylistManager playlistManager;

        internal void OpenPlaylistFolders()
        {
            foreach (Playlist p in SelectedItems)
                tool.OpenFileDirectory(p.Filepath);
        }

        internal void BrowseForPlaylistDirectory()
        {
            string startPath = null;
            if (SelectedItems.Count > 0)
                startPath = (SelectedItems[0] as Playlist).Filepath;
            playlistManager.BrowsePlaylistDialog(startPath);
        }

        #region required override methods
        protected override void AddDataFromFiles(int insertionIndex, List<string> files)
        {
            foreach (string file in files)
            {
                playlistManager.AddPlaylistFromFile(file);
            }
        }
        
        protected override void AddData(int insertIndex, List<Playlist> items)
        {
            int count = Items.Count;
            foreach (object o in items)
            {
                if (o is Playlist)
                {
                    playlistManager.AddPlaylistFromFile(count, o as Playlist);
                }
                if (o is Song)
                {
                    Song s = o as Song;
                    playlistManager.AddPlaylistFromFile(count, s.Filepath);
                }
            }
        }
        
        #endregion
    }
}
