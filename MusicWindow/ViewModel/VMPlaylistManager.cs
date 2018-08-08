using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Glaxion.ViewModel
{
    public class VMPlaylistManager : VMListView<Playlist>
    {
        public VMPlaylistManager()
        {
        }
        
        VMPlaylistFileTree _playlistFileManager;
        VMMusicFiles _musicFileManager;

        internal void LinkFileManagers(VMPlaylistFileTree playlistFilesManager, VMMusicFiles musicFileManager)
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
            foreach (Playlist p in Items)
                plist.Add(p.Filepath);
            return plist;
        }

        public void AddPlaylistFromFile(string file)
        {
            Playlist p = new Playlist(file, true);
            if (!p.failed)
                Items.Add(p);
            p.UpdatePaths();
        }

        internal void AddPlaylist(Playlist p)
        {
            Items.Add(p);
            p.UpdatePaths();
        }

        public void AddPlaylistFromFile(int index,string file)
        {
            Playlist p = new Playlist(file,true);
            Items.Insert(index,p);
            p.UpdatePaths();
        }

        public void AddPlaylistFromFile(int index,Playlist p)
        {
            if(p== null)
            {
                tool.show(5, "Warning, playlist is null");
                return;
            }
            Items.Insert(index,p);
            p.UpdatePaths();
        }

        public Playlist CreateNewPlaylist()
        {
            Playlist p = new Playlist("New Playlist");
            if (!p.OpenFile())
                return p;
            //if(!File.Exists(p.Filepath))
            //    p.SaveAs();
            AddPlaylist(p);
            return p;
        }

        public void BrowsePlaylistDialog(string startPath)
        {
            if (!Directory.Exists(startPath))
                startPath = Playlist.DefaultDirectory;
            string[] l = Playlist.OpenBrowserDialog(startPath);
        
            if (l == null)
                return;

            foreach (string s in l)
            {
                AddPlaylistFromFile(s);
            }
        }

        internal void DeleteSelectedPlaylists(IList selectedItems)
        {
            List<Playlist> removeItems = new List<Playlist>();
            foreach(Playlist p in selectedItems)
            {
                string path = p.Filepath;
                tool.DeleteFile(path, true, true);
                removeItems.Add(p);
            }

            foreach(Playlist p in removeItems)
                Items.Remove(p);

            removeItems.Clear();
        }

        private void RemovePlaylist(Playlist p)
        {
            Items.Remove(p);
        }

        public void DeletePlaylist(Playlist p)
        {
            Items.Remove(p);
            tool.DeleteAsync(p.Filepath);
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
                Items.Remove(p);
        }
    }
}
