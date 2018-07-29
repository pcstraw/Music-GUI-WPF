using Glaxion.Tools;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Glaxion.Music
{
    public class DirectoryCollection : Collection<string>
    {
        public DirectoryCollection() : base()
        {

        }
    }

    public class FileManager
    {
        public FileManager()
        {
            Directories = new DirectoryCollection();
            Files = new List<string>();
        }
       
        public DirectoryCollection Directories { get; set; }
        public List<string> Files { get; set; }

        public virtual List<string> LoadFiles(string directory)
        {
            //todo:  change to generic enumrate files
            return tool.LoadAudioFiles(directory, SearchOption.AllDirectories);
        }
        
        public virtual bool AddDirectory(string folder)
        {
            if (!Directory.Exists(folder))
            {
                if (folder.ToLower() == "default")
                    return false;
                tool.show(3, "Directory: ", "", folder, "", " Does not exist");
                return false;
            }
            if (Directories.Contains(folder))
            {
                tool.show(3, "Directory: ", "", folder, "", "Already Added");
                return false;
            }
            Directories.Add(folder);
            return true;
        }

        public void SavePlaylistDirectories()
        {
            throw new NotImplementedException("Need to save property list but data classes cannot access Application.Settings.  Should this function even be here?");
           // tool.SetPropertyList(Playlist.Directories, Properties.Settings.Default.PlaylistDirectories);
            //Properties.Settings.Default.Save();
        }

        public void SaveDirectories()
        {
            throw new NotImplementedException("Need to save property list but data classes cannot access Application.Settings.  Should this function even be here?");
            //tool.SetPropertyList(MusicDirectories, Properties.Settings.Default.MusicDirectories);
            // foreach (string s in Properties.Settings.Default.MusicDirectories)
            //    tool.debug(s);
            // Properties.Settings.Default.Save();
        }
    }
}
