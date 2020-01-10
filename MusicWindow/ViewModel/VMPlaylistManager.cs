using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Glaxion.ViewModel
{
    public class VMPlaylistManager : VMListView<VMPlaylist>
    {
        public VMPlaylistManager()
        {
           
        }
        
        internal VMPlaylistFileTree playlistFileManager;
        internal VMMusicFiles musicFileManager;
        MusicPlayer Player;

        internal void LinkFileManagers(VMPlaylistFileTree playlistFilesManager, VMMusicFiles musicFileManager)
        {
            playlistFileManager = playlistFilesManager;
            this.musicFileManager = musicFileManager;
            playlistFileManager.OpenPlaylist += Manager_OpenPlaylist;
            this.musicFileManager.OpenPlaylist += Manager_OpenPlaylist;
            
        }

        internal void HookPlayerEvents()
        {
            Player = MusicPlayer.Player;
            Player.TrackChangeEvent += Player_TrackChangeEvent;
            //Player.PlayEvent += Player_PlayEvent;
            Player.ResumeEvent += Player_ResumeEvent;
        }

        private void Player_ResumeEvent(object sender, EventArgs args)
        {
            UpdatePlaylistColours();
        }

        private void Player_PlayEvent(object sender, EventArgs args)
        {
            UpdatePlaylistColours();
        }

        internal void UpdatePlaylistColours()
        {
            foreach (VMPlaylist p in Items)
            {
                p.UpdatePlaylistState(Player);
            }
        }

        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            UpdatePlaylistColours();
        }

        //use enurmated with yield
        public List<string> GetPlaylistsAsFiles()
        {
            List<string> plist = new List<string>();
            foreach (VMPlaylist p in Items)
                plist.Add(p.Filepath);
            return plist;
        }

        VMPlaylist GetVMPlaylist(string file, bool readfile)
        {
            return GetVMPlaylist(new Playlist(file,readfile));
        }

        VMPlaylist GetVMPlaylist(Playlist p)
        {
            return new VMPlaylist(p);
        }

        public void AddPlaylistFromFile(string file)
        {
            VMPlaylist p = GetVMPlaylist(file,true);
            if (!p.playlist.failed)
                Items.Add(p);
            p.playlist.UpdatePaths();
        }

        internal void AddPlaylist(Playlist p)
        {
            VMPlaylist v_p = GetVMPlaylist(p);
            Items.Add(v_p);
            p.UpdatePaths();
        }

        public void AddPlaylistFromFile(int index,string file)
        {
            VMPlaylist p = GetVMPlaylist(file,true);
            Items.Insert(index,p);
            p.playlist.UpdatePaths();
        }

        public void AddPlaylistFromFile(int index,Playlist p)
        {
            if(p== null)
            {
                tool.show(5, "Warning, playlist is null");
                return;
            }
            VMPlaylist v_p = GetVMPlaylist(p);
            Items.Insert(index,v_p);
            v_p.playlist.UpdatePaths();
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
            List<VMPlaylist> removeItems = new List<VMPlaylist>();
            foreach(VMPlaylist p in selectedItems)
            {
                string path = p.playlist.Filepath;
                tool.DeleteFile(path, true, true);
                removeItems.Add(p);
            }

            foreach(VMPlaylist p in removeItems)
                Items.Remove(p);

            removeItems.Clear();
        }

        private void RemovePlaylist(VMPlaylist p)
        {
            Items.Remove(p);
        }

        public void DeletePlaylist(VMPlaylist p)
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
            List<VMPlaylist> plist = new List<VMPlaylist>(range.Count);
            foreach (VMPlaylist item in range)
                plist.Add(item);

            foreach (VMPlaylist p in plist)
                Items.Remove(p);
        }
    }
}
