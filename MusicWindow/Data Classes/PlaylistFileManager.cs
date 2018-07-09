using System;
using System.Collections.Generic;
using Glaxion.Tools;
using System.IO;
using System.Collections.ObjectModel;

namespace Glaxion.Music
{
    public class PlaylistFileManager : FileManager
    {
        private PlaylistFileManager() : base()
        {
        }

        public static PlaylistFileManager Instance { get { return Nested.instance; } }
        private class Nested
        {
            static Nested() { }//lazy singleton
            internal static readonly PlaylistFileManager instance = new PlaylistFileManager();
        }

        public override List<string> LoadFiles(string directory)
        {
            List<string> list = tool.LoadFiles(directory, ".m3u");
            Files.AddRange(list);
            return list;
        }

        /*
        public void LoadPlaylistDirectories()
        {
            Nodes.Clear();
           // ObservableCollection<VNode> nodes = new ObservableCollection<VNode>();
            int count = Directories.Count;
            for (int i = 0; i < count; i++)
            {
                VMNode d = LoadDirectoryToNode(Directories[i]);
                if (d == null)
                {
                    tool.show(3, "Directory:", "", Directories[i], "", "Doesn't Have Any Playlist Files");
                    Directories.RemoveAt(i);
                    continue;
                }
                Nodes.Add(d);
            }

            if (Nodes.Count == 1)
                Nodes[0].Expanded = true;
            else
            {
                foreach (VMNode t in Nodes)
                {
                    if (t.Name == Playlist.DefaultDirectory)
                        t.Expanded = true;
                }
            }
            // _view.PopulateTree(Nodes);
            CacheNodes();
        }
        */

        /*
    public VMNode LoadDirectoryToNode(string directory)
    {
        string[] files = Directory.GetFiles(directory);  //use enumerate files here?
        string[] dirs = Directory.GetDirectories(directory);
        VMNode rn = new VMNode(Path.GetFileName(directory));
       // rn.Name = directory;
        rn.FilePath = directory;
        bool hasPlaylist = false;

        foreach (string f in files)
        {
            if (!tool.IsPlaylistFile(f))
                continue;

            VMNode tn = new VMNode();
            tn.Name = Path.GetFileName(f);
            tn.FilePath = f;
            rn.AddNode(tn);
            //Playlist p = fileLoader.GetPlaylist(f, false);
            hasPlaylist = true;
           // if (!PlaylistFiles.ContainsKey(f))
           //     PlaylistFiles.Add(f, p.Name);
        }

        foreach (string d in dirs)
        {
            if (Path.HasExtension(d)) //could try and get parent dir if file
                continue;

            VMNode tn = LoadDirectoryToNode(d);
            if (tn == null)
                continue;

            tn.Name = d;
            if (tn.Nodes.Count > 0)
                rn.AddNode(tn);
        }
        if(!hasPlaylist)
            return null;
        return rn;
    }
    */

        /*
    public void DeleteSelectedFiles(bool AllowConfirmation)
    {
        bool deleted = false;
        foreach (VMNode node in SelectedNodes)
        {
            string path = node.FilePath;

            if (tool.IsPlaylistFile(path) && File.Exists(path))
            {
                if (AllowConfirmation)
                {
                    bool confirm = tool.askConfirmation(path);
                    if (!confirm)
                        continue;
                }
                File.Delete(path);
                string message = string.Concat("file deleted: ", path);
                tool.show(10, message);
                tool.debug(message);
                deleted = true;
            }
        }
        if (deleted)
            LoadPlaylistDirectories();
    }
    */

        /*
        private void DuplicatePlaylistFile(VMNode selectedNode)
        {
            if (selectedNode != null)
            {
                string path = selectedNode.FilePath;
                string s = tool.AppendFileName(path, "+");
                if (!File.Exists(s))
                    File.Copy(path, s);
                LoadPlaylistDirectories();
            }
        }
        */
    }
}

