using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace Glaxion.ViewModel
{
    public class VMTrackManager : VMListView<VMSong>, INotifyPropertyChanged
    {
        public VMTrackManager()
        {
            CurrentList = new Playlist();
            player = MusicPlayer.Player;
            //Songs = new ObservableCollection<VMSong>();
        }

        public MusicPlayer player;
        public Playlist CurrentList { get; private set; }
       // public ObservableCollection<VMSongInfo> Songs;

        public string PlaylistNameLabel
        {
            get { return CurrentList.Name; }
            private set { CurrentList.Name = value; OnPropertyChanged(); } 
        }

        public void SetPlaylist(Playlist playlist)
        {
            if (playlist == null)
            {
                tool.show(3, "Playlist is null");
                return;
            }
            playlist.UpdatePaths();
            CurrentList = playlist;
            Items = null;
            Items = GetSongs(CurrentList.songs);
            PlaylistNameLabel = playlist.Name;
        }

        public ObservableCollection<VMSong> GetSongs(IEnumerable<Song> songs)
        {
            ObservableCollection<VMSong> return_list = new ObservableCollection<VMSong>();
            foreach (Song s in songs)
            {
               // this.InsertSong(0, new VMSong(s));
                 return_list.Add(new VMSong(s));
            }
            return return_list;
        }
        
        public Playlist OpenPlaylistSelectDialog()
        {
            List<string> l = tool.SelectFiles(false, false, "Select Playlist");
            if (l.Count != 1)
                return null;

            if (!tool.IsPlaylistFile(l[0]))
                return null;

            string path = l[0];
            Playlist p = new Playlist(path, true);
            SetPlaylist(p);
            return p;
        }

        public void PlaySong(Song song)
        {
            UpdateCurrentPlaylist();
            player.PlayPlaylist(CurrentList, song);
        }

        public List<VMSong> SearchVMSongs(string searchtText,string filter)
        {
            List<VMSong> result = new List<VMSong>();

            foreach(VMSong s in Items)
            {
                string test = s.GetType().GetProperty(filter).GetValue(s) as string;
                if (test.ToLower().Contains(searchtText))
                    result.Add(s);
            }
            return result;
        }

        internal List<string> ConvertToAudioFiles(string filePath)
        {
            List<string> result = new List<string>();
            if (tool.IsAudioFile(filePath))
            {
                result.Add(filePath);
                return result;
            }
            
            Playlist p = new Playlist(filePath, true);
            if(p.failed)
            {
                tool.show(3,"Unable to Make playlist: ", p.Filepath);
                return result;
            }
            foreach(Song s in p.songs)
            {
                result.Add(s.Filepath);
            }
            return result;
        }

        internal VMSong InsertSong(int index, VMSong song)
        {
            if (index < 0)
                index = 0;
            Items.Insert(index, song);
            return song;
        }

        internal VMSong InsertSongFromFile(int index, string filePath)
        {
            Song s = SongInfo.Instance.GetInfo(filePath);
            VMSong vmSong = new VMSong(s);
            Items.Insert(index,vmSong);
            return vmSong;
        }

        internal void InsertSongsFromFiles(int insertionIndex, List<string> files)
        {
            foreach(string file in files)
            {
                if (tool.IsAudioFile(file))
                {
                    VMSong vms = InsertSongFromFile(insertionIndex, file);
                    vms.IsSelected = true;
                }
                else
                {
                    List<string> results = ConvertToAudioFiles(file);
                    results.Reverse();
                    foreach(string s in results)
                    {
                        VMSong vms = InsertSongFromFile(insertionIndex, s);
                        vms.IsSelected = true;
                    }
                }
            }
        }

        internal void ReloadPlaylistFromFile()
        {
            CurrentList.ReadFile();
            SetPlaylist(CurrentList);
        }

        internal void UpdateCurrentPlaylist()
        {
            CurrentList.songs.Clear();
            foreach(VMSong s in Items)
            {
                if (s == null)
                    throw new Exception("Make sure there is always a song in each VMsong");
                CurrentList.songs.Add(s.CurrentSong);
            }
        }

        public void SavePlaylist()
        {
            UpdateCurrentPlaylist();
            CurrentList.Save();
        }

        internal void AddItems(int insertIndex, List<VMSong> items)
        {
            foreach(object o in items)
            {
                if(o is VMSong)
                {
                    VMSong original_vmsong = o as VMSong;
                    VMSong vms = this.InsertSong(insertIndex,new VMSong(original_vmsong.CurrentSong));
                    vms.IsSelected = true;
                }
                
                if(o is Playlist)
                {
                    Playlist p = o as Playlist;
                    if (p == CurrentList)
                        continue;

                    foreach (Song s in p.songs)
                    {
                        VMSong vms = this.InsertSong(insertIndex, new VMSong(s));
                        vms.IsSelected = true;
                        insertIndex++;
                    }
                }
            }
        }

        internal void OpenSelectedTrackFolders()
        {
            foreach(VMSong song in Items)
            {
                if (song.IsSelected)
                    tool.OpenFileDirectory(song.Filepath);
            }
        }
    }
}
