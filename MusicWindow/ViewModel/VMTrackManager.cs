using Glaxion.Music;
using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Glaxion.ViewModel
{
    public class VMTrackManager : INotifyPropertyChanged
    {
        public VMTrackManager()
        {
            Songs = new ObservableCollection<Song>();
            CurrentList = new Playlist();
            player = MusicPlayer.Player;
        }

        public MusicPlayer player;
        public Playlist CurrentList { get; private set; }
        ObservableCollection<Song> _songs;
        public ObservableCollection<Song> Songs
        {
            get { return _songs; }
            set { _songs = value; OnPropertyChanged(); }
        }
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
            Songs = null;
            Songs = playlist.songs;
            PlaylistNameLabel = playlist.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            player.PlayPlaylist(CurrentList, song);
        }

        public List<Song> SearchSongs(string searchtText,string filter)
        {
            List<Song> result = new List<Song>();

            foreach(Song s in Songs)
            {
                string test = s.GetType().GetProperty(filter).GetValue(s) as string;
                if (test.ToLower().Contains(searchtText))
                    result.Add(s);
            }
            return result;
        }
    }
}
