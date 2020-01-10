using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Glaxion.Music
{
    public class SongInfo
    {
        private SongInfo() //lazy singleton
        {
        }
        public static SongInfo Instance { get { return Nested.instance; } }
        private class Nested
        {
            static Nested() { }
            internal static readonly SongInfo instance = new SongInfo();
        }
        public List<string> artists = new List<string>();
        public List<string> albums = new List<string>();
        public List<string> years = new List<string>();
        public List<int> genreIDs = new List<int>();
        public List<string> genres = new List<string>();
        public Dictionary<string, Song> songs = new Dictionary<string, Song>(); //Dispose?

        //used to update file paths based on file name
        public List<Song> SearchThroughNames(string searchText)
        {
            List<Song> ls = new List<Song>();
            tool.debugWarning("Warning:  TODO: Lock fileLoader.MusicFiles before using it for searching");
            foreach (KeyValuePair<string, Song> kv in songs)
            {
                if (kv.Value.Name == searchText)
                    ls.Add(kv.Value);
            }
            return ls;
        }

        public Song SearchByName(string searchText)
        {
            foreach(string s in MusicFileManager.Instance.Files)
            {
                string name = Path.GetFileNameWithoutExtension(s);
                if (name == searchText)
                    return GetInfo(s);
            }
            return null;
        }

        public void AddSong(string path)
        {
            if (songs.ContainsKey(path))
                return;
            songs[path] = GetInfo(path);
        }
        
        //TODO:  Handle path being null and read tag failer
        public Song GetInfo(string path)
        {
            if (!tool.IsAudioFile(path))
            {
                Song s = new Song(path);
                s.Title = path;
                return s;
            }
            if (songs.ContainsKey(path))
            {
                //if the id3 info has been modified then should be set to false, reload the id3 tag
                // if (!songs[path].loaded)
                //songs[path].ReadID3Async();
                return songs[path];
            }
            Song ti = new Song(path);
            ti.ReadID3Info();
            AddInfo(ti);
            //Task.Run(() => ti.ReadID3Info());
            
            //ti.ReadID3Async();
            return ti;
        }

        public void AddInfo(Song info)
        {
            if (info == null)
                return;
            if (songs.ContainsKey(info.Filepath))
                return;
            songs.Add(info.Filepath, info);
            AddAlbum(info.Album);
            AddArtist(info.Artist);
            AddYear(info.Year);
            AddGenre(info.Genres[0]);
        }

        public string AddAlbum(string albumName)
        {
            if (!tool.StringCheck(albumName))
                return albumName;
            if (!albums.Contains(albumName))
                albums.Add(albumName);
            return albumName;
        }

        public string AddArtist(string artistName)
        {
            if (!tool.StringCheck(artistName))
                return artistName;
            if (!artists.Contains(artistName))
                artists.Add(artistName);
            return artistName;
        }

        public string AddYear(string year)
        {
            if (!tool.StringCheck(year))
                return year;
            if (!years.Contains(year))
                years.Add(year);
            return year;
        }

        public string AddGenre(string genre)
        {
            if (!tool.StringCheck(genre))
                return genre;
            if (!genres.Contains(genre))
                genres.Add(genre);
            return genre;
        }
        public int AddGenreID(int genreID)
        {
            if (genreID == 0)
                return 0;
            if (!genreIDs.Contains(genreID))
                genreIDs.Add(genreID);
            return genreID;
        }
    }
}
