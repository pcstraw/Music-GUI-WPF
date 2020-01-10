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

    public class FileDirectory : List<string>
    {
        public string directory;
        public FileDirectory() : base()
        {
        }

        public FileDirectory(string directoryName) : base()
        {
            directory = directoryName;
        }
    }

    public class FileManager
    {
        public FileManager()
        {
            Directories = new List<FileDirectory>();
            Files = new List<string>();
        }
       
        public List<FileDirectory> Directories { get; set; }
        public List<string> Files { get; set; }

        //dep
        public virtual List<string> LoadFiles(string directory)
        {
            //todo:  change to generic enumrate files
            return tool.LoadAudioFiles(directory, SearchOption.AllDirectories);
        }
        
        public virtual FileDirectory AddDirectory(string folder)
        {
            if (!Directory.Exists(folder))
            {
                if (folder.ToLower() == "default")
                    return null;
                tool.show(3, "Directory: ", "", folder, "", " Does not exist");
                return null;
            }
            if (HasDirectory(folder))
            {
                tool.show(3, "Directory: ", "", folder, "", "Already Added");
                //todo: can return the already added fileDirectory
                return null;
            }
            FileDirectory fd = new FileDirectory(folder);
            Directories.Add(fd);
            return fd;
        }

        public bool HasDirectory(string directory)
        {
            foreach(FileDirectory fileDirectory in Directories)
                if (fileDirectory.directory == directory)
                    return true;
            return false;
        }

        public bool RemoveDirectory(string directory)
        {
            foreach (FileDirectory fileDirectory in Directories)
            {
                if (fileDirectory.directory == directory)
                {
                    Directories.Remove(fileDirectory);
                    return true;
                }
            }
            return false;
        }
    }
}
