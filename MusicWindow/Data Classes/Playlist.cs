using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using Glaxion.Tools;

namespace Glaxion.Music
{
    public delegate void OpenPlaylistEventHandler(object sender, EventArgs args);

    public class Playlist
    {
        public Playlist()
        {
            debugSave = true;
        }

        //Any class capable of opening an playlist should use this event handler
        // public event OpenPlaylistEventHandler OpenPlaylistEvent;

      //  string Filepath;
        public string Filepath { get; set ; }
       // string Name;
        public string Name { get ;  set ; }
        public string ext = ".m3u";
        public int trackIndex;
        public bool dirty;
        public bool debugSave;
        public bool failed;
        //public List<string> tracks = new List<string>();
        public ObservableCollection<Song> songs = new ObservableCollection<Song>();
        //public static string DefaultPlaylistDirectory;
        static string _defaultDir;
        public static string DefaultDirectory
        {
            get
            {
                if (!Directory.Exists(_defaultDir))
                {
                    _defaultDir = Path.Combine("Output", "tmp");
                    if (!Directory.Exists(_defaultDir))
                        Directory.CreateDirectory(_defaultDir);
                }
                return _defaultDir;
            }
            set
            {
                _defaultDir = value;
            }
        }

        public string SelectDefaultDirectory()
        {
            List<string> list = tool.SelectFiles(true, false,"Select Default Playlist Directory");
            if (list.Count == 1)
                return DefaultDirectory = list[0];
            else
                return null;
        }
        
        //use this in future to get default directory
        /*
        public static string FindDefaultDirectory()
        {
            string dd = DefaultPlaylistDirectory;
            if (Directory.Exists(dd))
                return dd;
            else
            {
                DefaultPlaylistDirectory = tool.BrowseForDirectory(true, true,"Select Main Playlist Directory");
                if (DefaultPlaylistDirectory == null)
                    return null;
                Glaxion.Music.Properties.Settings.Default.DefaultPlaylistDirectory = DefaultPlaylistDirectory;
                return DefaultPlaylistDirectory;
            }
        }
        */

        public object Clone()
        {
            Playlist p = new Playlist();
            p.Name = Name;
            p.Filepath = Filepath;
            foreach (Song s in songs)
            {
                p.songs.Add(s);
            }
            p.ext = ext;
            return p;
        }

        public string UpdateFilePath()
        {
            string dir = Path.GetDirectoryName(Filepath);
            Filepath = Path.Combine(dir, Name + ext);
            return Filepath;
        }
        
        public void UpdateName(string newName)
        {
            Name = newName;
            UpdateFilePath();
            /*
            name = newName;
            string oldDir = Path.GetDirectoryName(path);
            string newPath = oldDir + @"\" + newName + ext;
            path = newPath;
            */
        }
        
        public Playlist(string filePath, bool readFile)
        {
            debugSave = true;
            Filepath = filePath;
            if (tool.IsPlaylistFile(filePath))
            {
                if(readFile)
                    ReadFile();
            }else
            {
                if(tool.IsAudioFile(filePath))
                {
                    filePath = Path.GetDirectoryName(filePath);
                }
                if (Directory.Exists(filePath))
                {
                    Filepath = filePath;
                    if (readFile)
                    {
                        List<string> tracks = tool.LoadAudioFiles(filePath, SearchOption.TopDirectoryOnly);
                        GetSongs(tracks);
                    }
                }else
                {
                    failed = true;
                    return;
                }
                if (DefaultDirectory != null)
                {
                    Filepath = Path.Combine(DefaultDirectory,filePath + ext);
                }
            }
            GetName(filePath);
        }

        private void GetName(string file)
        {
            Name = Path.GetFileNameWithoutExtension(file);
        }

        private void GetSongs(List<string> tracks)
        {
            songs.Clear();
            foreach(string s in tracks)
            {
                Song song = SongInfo.Instance.GetInfo(s);
                songs.Add(song);
            }
        }

        public void ReadFile()
        {
            StreamReader sr = null;
            List<string> tracks = new List<string>();
            failed = true;
            try
            {
                sr = new StreamReader(Filepath);
                songs.Clear();
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    tracks.Add(line);
                }
                GetSongs(tracks);
                failed = false;
                sr.Close();
            }
            catch (FileNotFoundException)
            {
                Log.Message(string.Concat("Could not find file: ", Filepath));
            }
            catch (IOException)
            {
                Log.Message(string.Concat("File I/O Exception: ", Filepath));
            }
            catch (OutOfMemoryException)
            {
                Log.Message(string.Concat("Out of Memory while loading playlist file:  ", Filepath));
            }
            finally
            {
                if (sr != null) sr.Dispose();
            }
        }

        public void GetFromFile(string fullpath)
        {
            Filepath = fullpath;
            GetName(fullpath);
            ReadFile();
        }

        public bool Save()
        {
            return WriteToFile(false);
        }

        public bool SaveAs()
        {
            return WriteToFile(true);
        }

        public bool SaveTo(string fullpath)
        {
            Filepath = fullpath;
            GetName(fullpath);
            return WriteToFile(false);
        }

        public string GetDirectory()
        {
            return Path.GetDirectoryName(Filepath);
        }
        
        //replaces the current path to the playlist
        //with the globally set default playlist directory

        public void SetDefaultPath()
        {
            if (Path.HasExtension(Name))
            {
                Name = "error name";
                tool.Show(Name);
            }

            Filepath = Path.Combine(DefaultDirectory, Name + ext);
        }

        public List<string> GetTrackPaths()
        {
            List<string> l = new List<string>(songs.Count);
            foreach(Song s in songs)
            {
                l.Add(s.Filepath);
            }
            return l;
        }
       
        public bool WriteToFile(bool append)
        {
            List<string> tracks = GetTrackPaths();
            if (Filepath == null || append)
            {
                SaveFileDialog od = new SaveFileDialog();
                if (Directory.Exists(DefaultDirectory))
                {
                    od.InitialDirectory = DefaultDirectory;
                }

                od.FileName = Path.Combine(Name,ext);
                if (od.ShowDialog() == DialogResult.OK)
                {
                    Filepath = od.FileName;
                    Name = Path.GetFileNameWithoutExtension(Filepath);
                    File.WriteAllLines(Filepath, tracks);
                    tool.debug("Saved File: ", Filepath);
                    if(debugSave)
                        tool.show(1, Filepath, "", "Save Successfull");
                    return true;
                }else
                {
                    if (debugSave)
                        tool.show(1, Filepath, "", "Failed to Save");
                    return false;
                }
            }
            if (File.Exists(Filepath))
            {
                File.WriteAllLines(Filepath, tracks);
                tool.debug("Saved File: ", Filepath);
                if (debugSave)
                    tool.show(1, Filepath, "", "Save Successfull");
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(Filepath)|| string.IsNullOrWhiteSpace(Filepath))
                {
                    if (debugSave)
                        tool.show(1, Filepath, "", "Failed to Save");
                    return false;
                }

                if (!File.Exists(Filepath))
                    SetDefaultPath();

                FileStream fs = File.Create(Filepath);
                fs.Close();
                File.WriteAllLines(Filepath, tracks);
                tool.debug("Created File: ", Filepath);
                if (debugSave)
                    tool.show(1, Filepath, "", "Save Successfull");
                return true;
            }
        }
        
        public void UpdatePaths()
        {
            //List<string> updated = new List<string>();
            for(int i=0; i<songs.Count; i++)
            {
                //need to remove illegal characters
                if (songs[i] == null)
                    continue;

                string t = Path.GetFileNameWithoutExtension(songs[i].Filepath);
                Song s = SongInfo.Instance.SearchForFile(t);
                if (s == null)
                    continue;
                songs.RemoveAt(i);
                songs.Insert(i, s);
                //TODO handle case where multiple files are found
            }
        }

        public static List<string> SelectPlaylistFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = true;
            List<string> ls = new List<string>();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string s in ofd.FileNames)
                {
                    ls.Add(s);
                }
                return ls;
            }
            return ls;
        }
    }
}
