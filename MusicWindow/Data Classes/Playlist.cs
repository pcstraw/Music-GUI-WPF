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

        string path;
        public string Filepath { get { return path;} set { path = value; } }
        string name;
        public string Name { get { return name; } set { name = value; } }
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
                return _defaultDir;
            }
            private set
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
            p.name = name;
            p.path = path;
            foreach (Song s in songs)
            {
                p.songs.Add(s);
            }
            p.ext = ext;
            return p;
        }

        public string UpdateFilePath()
        {
            string dir = Path.GetDirectoryName(path);
            path = dir + @"\" + name + ext;
            return path;
        }
        
        public void UpdateName(string newName)
        {
            name = newName;
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
            path = filePath;
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
                if (readFile && Directory.Exists(filePath))
                {
                    List<string> tracks = tool.LoadAudioFiles(filePath, SearchOption.TopDirectoryOnly);
                    GetSongs(tracks);
                }
                if (DefaultDirectory != null)
                {
                    path = DefaultDirectory + @"\" + filePath + ext;
                }
            }
            GetName(filePath);
        }

        private void GetName(string file)
        {
            name = Path.GetFileNameWithoutExtension(file);
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
                sr = new StreamReader(path);
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
                Log.Message(string.Concat("Could not find file: ", path));
            }
            catch (IOException)
            {
                Log.Message(string.Concat("File I/O Exception: ", path));
            }
            catch (OutOfMemoryException)
            {
                Log.Message(string.Concat("Out of Memory while loading playlist file:  ", path));
            }
            finally
            {
                if (sr != null) sr.Dispose();
            }
        }

        public void GetFromFile(string fullpath)
        {
            path = fullpath;
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
            path = fullpath;
            GetName(fullpath);
            return WriteToFile(false);
        }

        public string GetDirectory()
        {
            return Path.GetDirectoryName(path);
        }
        
        //replaces the current path to the playlist
        //with the globally set default playlist directory
        public string GetDefaultPlaylistPath()
        {
            if(Path.HasExtension(name))
            {

                name = "error name";
                tool.Show(name);
            }
            path = DefaultDirectory + @"\"+name + ext;
            return path;
        }

        public void SetDefaultPath()
        {
            path = GetDefaultPlaylistPath();
        }

        public List<string> GetTrackPaths()
        {
            List<string> l = new List<string>(songs.Count);
            foreach(Song s in songs)
            {
                l.Add(s.path);
            }
            return l;
        }
       
        public bool WriteToFile(bool append)
        {
            List<string> tracks = GetTrackPaths();
            if (path == null || append)
            {
                SaveFileDialog od = new SaveFileDialog();
                if (Directory.Exists(DefaultDirectory))
                {
                    od.InitialDirectory = DefaultDirectory;
                }

                od.FileName = name + ext;
                if (od.ShowDialog() == DialogResult.OK)
                {
                    path = od.FileName;
                    name = Path.GetFileNameWithoutExtension(path);
                    File.WriteAllLines(path, tracks);
                    tool.debug("Saved File: ", path);
                    if(debugSave)
                        tool.show(1, path, "", "Save Successfull");
                    return true;
                }else
                {
                    if (debugSave)
                        tool.show(1, path, "", "Failed to Save");
                    return false;
                }
            }
            if (File.Exists(path))
            {
                File.WriteAllLines(path, tracks);
                tool.debug("Saved File: ", path);
                if (debugSave)
                    tool.show(1, path, "", "Save Successfull");
                return true;
            }
            else
            {
                if (path == null)
                {
                    if (debugSave)
                        tool.show(1, path, "", "Failed to Save");
                    return false;
                }

                if(!Directory.Exists(path))
                {
                    //string dir = FinbdDefaultDirectory();
                    string tmp = @"Output\tmp\";
                    Directory.CreateDirectory(tmp);
                    path = tmp + @"\" + name + ext;
                }

                FileStream fs = File.Create(path);
                fs.Close();
                File.WriteAllLines(path, tracks);
                tool.debug("Created File: ", path);
                if (debugSave)
                    tool.show(1, path, "", "Save Successfull");
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
                if (File.Exists(songs[i].path))
                    continue;
                string t = songs[i].name;
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
