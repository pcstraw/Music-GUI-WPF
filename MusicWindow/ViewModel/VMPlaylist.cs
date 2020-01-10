using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glaxion.Music;

namespace Glaxion.ViewModel
{
    public class VMPlaylist : VMItem
    {
        public VMPlaylist(Playlist p)
        {
            playlist = p;
        }

        public void Refresh()
        {
            SetPlaylist(_playlist);
        }

        Playlist _playlist;
        public Playlist playlist
        {
            get { return _playlist; }
            set
            {
                //call set playlist here
                if (_playlist == value) return;
                _playlist = value;
                SetPlaylist(_playlist);
                OnPropertyChanged();
            }
        }

        void SetPlaylist(Playlist p)
        {
            Filepath = p.Filepath;
            Name = p.Name;
        }

        public void UpdatePlaylistState(MusicPlayer player)
        {
            if (player.CurrentSong == null)
                return;

            ColourIndex = 0;
            
            foreach(Song s in playlist.songs)
            {
                if(s.Filepath == player.CurrentSong.Filepath)
                {
                    if(player.currentList == playlist)
                    {
                        ColourIndex = 1;
                        return;
                    }
                    ColourIndex = 2;
                }
            }
        }

        string _Filepath;
        public string Filepath
        {
            get { return _Filepath; }
            set
            {
                if (_Filepath == value) return;
                _Filepath = value;
                OnPropertyChanged();
            }
        }

        string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }
    }
}
