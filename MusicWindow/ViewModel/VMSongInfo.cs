using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Glaxion.Music;

namespace Glaxion.ViewModel
{
    public class VMSong : VMItem
    {
        public VMSong()
        {
        }

        public VMSong(Song song)
        {
            SetSong(song);
        }

        Song _song;
        public Song CurrentSong {
            get { return _song; }
            set { SetSong(value); }
        }

        public void SetSong(Song song)
        {
            Title = song.Title;
            Artist = song.Artist;
            Album = song.Album;
            Year = song.Year;
            Genre = song.Genres[0];
            Picture = song.image;
            Filepath = song.Filepath;
            State = song.State;
            if (_song != null)
                _song.PropertyChanged -= _song_PropertyChanged;
            _song = song;
            //need to unsubscribe this when song changes
            _song.PropertyChanged += _song_PropertyChanged;
           // CurrentSong = song;
        }

        private void _song_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e is SongEventArgs))
                return;

            SongEventArgs se = e as SongEventArgs;
            if (se.PropertyName.ToLower() == "state")
                State = (SongState)se.PropertyValue;

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

        string _title;
        public string Title
        {
            get { return _title; }
            set {if (_title == value) return;
                _title = value;
                OnPropertyChanged(); }
        }

        string _album;
        public string Album
        {
            get { return _album; }
            set { if (_album == value) return;
                _album = value;
                OnPropertyChanged(); }
        }
        string _artist;
        public string Artist
        {
            get { return _artist; }
            set {if (_artist == value) return;
                _artist = value;
                OnPropertyChanged(); }
        }
        string _year;
        public string Year
        {
            get { return _year; }
            set { if (_year == value) return;
                _year = value;
                OnPropertyChanged(); }
        }
        string _genre;
        public string Genre
        {
            get { return _genre; }
            set { if (_genre == value) return;
                _genre = value;
                OnPropertyChanged(); }
        }
        Image _picture;
        public Image Picture
        {
            get { return _picture; }
            set {if (_picture == value) return;
                _picture = value;
                OnPropertyChanged(); }
        }

        SongState _state;
        
        public SongState State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged(); }
        }
        
        public void MakeSearchedState()
        {
            _song.MakeSearchedState();
           // CacheSongState();
           // State = SongState.Searched;
        }
        public void RestorePrevState()
        {
            _song.RestorePrevState();
        }

        
        public void ReloadTags()
        {
             _song.Reload();
            SetSong(_song);
        }

        public void SaveTags()
        {
            //we can move these checks to the set methods(for strings at least)
            if (_song.Title != Title)
                _song.SetTitle(Title);
            if (_song.Artist != Artist)
                _song.SetArtist(Artist);
            if (_song.Album != Album)
                _song.SetAlbum(Album);
            if (_song.Year != Year)
                _song.SetYear(Year);
            if (_song.Genres[0] != Genre)
                _song.SetGenre(Genre);
            if (_song.image != Picture)
                _song.SetPictureFromImage(Picture);
            _song.SaveInfo();
              //  ReloadAsync();
        }

        //call reload on song asynchronusly and wait
        //for the thread pool to finish loading
        public void ReloadAsync()
        {
            Task.Run(() => WaitForReload()).ConfigureAwait(false);
        }
        void WaitForReload()
        {
            _song.Reload();
            //wait until the song has loaded
            while (!_song.loaded)
            {
            }
            //is this check relevant? Better to stay safe
            if(_song.State != SongState.MissingTags)
                SetSong(_song);
        }
    }
}
