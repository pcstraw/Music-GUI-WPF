using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Glaxion.ViewModel
{
    public class VMPlaylistManager
    {
        public VMPlaylistManager()
        {
            Playlists = new ObservableCollection<Playlist>();
        }

        public ObservableCollection<Playlist> Playlists;
        VMPlaylistFiles _playlistFileManager;
        VMMusicFiles _musicFileManager;

        internal void LinkFileManagers(VMPlaylistFiles playlistFilesManager, VMMusicFiles musicFileManager)
        {
            _playlistFileManager = playlistFilesManager;
            _musicFileManager = musicFileManager;
            _playlistFileManager.OpenPlaylist += Manager_OpenPlaylist;
            _musicFileManager.OpenPlaylist += Manager_OpenPlaylist;
        }

        //use enurmated with yield
        public List<string> GetPlaylistsAsFiles()
        {
            List<string> plist = new List<string>();
            foreach (Playlist p in Playlists)
                plist.Add(p.Filepath);
            return plist;
        }
        //move to viewmodel
        public void AddPlaylistFromFile(string file)
        {
            Playlist p = new Playlist(file, true);
            if (!p.failed)
                Playlists.Add(p);
        }
        public void BrowsePlaylistDialog()
        {
            List<string> l = tool.SelectFiles(false, true, "Select Playlist");

            if (l.Count == 0)
                return;
            foreach (string s in l)
            {
                AddPlaylistFromFile(s);
            }
        }

        private void Manager_OpenPlaylist(object sender, EventArgs args)
        {
            if (sender is string)
                AddPlaylistFromFile(sender as string);
        }

        public void RemoveSelectedPlaylists(System.Collections.IList range)
        {
            List<Playlist> plist = new List<Playlist>(range.Count);
            foreach (Playlist item in range)
                plist.Add(item);

            foreach (Playlist p in plist)
                Playlists.Remove(p);
        }
    }
}
