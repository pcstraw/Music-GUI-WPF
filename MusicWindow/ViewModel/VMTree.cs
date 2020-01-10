using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Glaxion.Music;
using System.IO;

namespace Glaxion.ViewModel
{
    public class VMTree : INotifyPropertyChanged
    {
        public VMTree()
        {
            Nodes = new ObservableCollection<VMNode>();
            _searchedNodes = new ObservableCollection<VMNode>();
        }

        ObservableCollection<VMNode> _nodes;
        public ObservableCollection<VMNode> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; OnPropertyChanged(); }
        }

        ObservableCollection<VMNode> cachedNodes = new ObservableCollection<VMNode>();
        public ObservableCollection<VMNode> SelectedNodes { get; set; }
        
        public FileManager fileLoader;
        public string searchText;
        bool _useCahcedNodes;
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event OpenPlaylistEventHandler OpenPlaylist;
        public void RaiseOpenPlaylistEvent(string filePath)
        {
            OpenPlaylist?.Invoke(filePath, EventArgs.Empty);
        }
        
        //dep.  Switch to the FileDirectory version below
        private VMNode PopulateNodesFromDirectory(string directory)
        {
            VMNode root = new VMNode();
            root.Name = directory;
            root.FilePath = directory;
            List<string> files = fileLoader.LoadFiles(directory);
           // List<string> files = tool.LoadAudioFiles(directory, SearchOption.AllDirectories);
            if (files.Count == 0)
                return null;

            foreach (string f in files)
                root.BuildChildNodes(directory, f, f);

            root.SortNodes();
            return root;
        }

        private VMNode PopulateNodesFromDirectory(FileDirectory fileDirectory)
        {
            VMNode root = new VMNode();
            root.Name = fileDirectory.directory;
            root.FilePath = fileDirectory.directory;
            List<string> files = fileLoader.LoadFiles(fileDirectory.directory);
            //List<string> files = tool.LoadAudioFiles(fileDirectory.directory, SearchOption.AllDirectories);
            if (files.Count == 0)
                return null;

            fileDirectory.Clear();
            fileDirectory.AddRange(files);

            foreach (string f in fileDirectory)
                root.BuildChildNodes(fileDirectory.directory, f, f);

            root.SortNodes();
            return root;
        }

        private VMNode PopulateNodesFromFileDirectory(FileDirectory fileDirectory)
        {
            VMNode root = new VMNode();
            root.Name = fileDirectory.directory;
            root.FilePath = fileDirectory.directory;
            if (fileDirectory.Count == 0)
                return null;
            
            foreach (string f in fileDirectory)
                root.BuildChildNodes(fileDirectory.directory, f, f);

            root.SortNodes();
            return root;
        }

        public void LoadFilesToTree()
        {
            cachedNodes.Clear();
            foreach (FileDirectory fileDirectory in fileLoader.Directories)
            {
                VMNode root = PopulateNodesFromDirectory(fileDirectory);
                if (root == null)
                    continue;
                cachedNodes.Add(root);
            }
            if (cachedNodes.Count == 0)
                return;

            cachedNodes[0].Expanded = true;
            Nodes = cachedNodes;
        }

        public void LoadDirectoriesToTree()
        {
            cachedNodes.Clear();
            foreach (FileDirectory fileDirectory in fileLoader.Directories)
            {
                VMNode root = PopulateNodesFromFileDirectory(fileDirectory);
                if (root == null)
                    continue;
                cachedNodes.Add(root);
            }
            if (cachedNodes.Count == 0)
                return;

            cachedNodes[0].Expanded = true;
            Nodes = cachedNodes;
        }

        //pass the title from for the dialog box from the caller
        public virtual List<FileDirectory> SelectAndLoadDirectory(string dialogBrowserTile)
        {
            List<string> folders = tool.SelectFiles(true, true, dialogBrowserTile);
            bool directoryAdded = false;
            List<FileDirectory> addedFolders = new List<FileDirectory>();

            foreach (string folder in folders)
            {
                FileDirectory fd = AddDirectory(folder);
                if (fd != null)
                {
                    addedFolders.Add(fd);
                    directoryAdded = true;
                }
            }
            if (directoryAdded)
            {
                // LoadFilesToTree();
                LoadDirectoriesToTree();
                Nodes = cachedNodes;
            }
            return addedFolders;
        }

        internal void RestorecachedNodes()
        {
            Nodes = cachedNodes;
        }

        public virtual FileDirectory AddDirectory(string directoryPath)
        {
            return fileLoader.AddDirectory(directoryPath);
        }

        ObservableCollection<VMNode> _searchedNodes;
        public ObservableCollection<VMNode> SearchFiles(string text)
        {
            _searchedNodes.Clear();
            foreach (string s in fileLoader.Files)
            {
                if(s.ToLower().Contains(text.ToLower()))
                {
                    VMNode newNode = VMNode.Create(s);
                    _searchedNodes.Add(newNode);
                }
            }

            Nodes = _searchedNodes;
            return _searchedNodes;
        }

        public VMNode SearchTreeView(VMNode tree, string text)
        {
            if (tree.Name == text) //?
                return tree;
            if (tree.Nodes.Count > 0)
            {
                foreach (VMNode t in tree.Nodes)
                {
                    if (t.FilePath == text)
                        return t;

                    if (t.Nodes.Count > 0)
                    {
                        VMNode tn = SearchTreeView(t, text);
                        if (tn != null)
                            return tn;
                    }
                }
            }
            return null;
        }

        public virtual VMNode FindFile(string path)
        {
            string s = path;
            VMNode t = null;
            if (Nodes.Count > 0)
            {
                foreach (VMNode directory in Nodes)
                {
                    t = SearchTreeView(directory, s);
                    if (t != null)
                    {
                        t.Selected = true;
                        t.Expanded = true;
                        return t;
                    }
                }
            }
            return t;
        }

        public void RemoveAncestorDirectory(VMNode node)
        {
            VMNode ancestor = node.GetAncestor();
            if (ancestor.Parent == null)
            {
                string dir = ancestor.FilePath;
                Nodes.Remove(ancestor);
                fileLoader.RemoveDirectory(dir);
            }
        }

        static void IterateTreeNodes(ObservableCollection<VMNode> originalNode, ObservableCollection<VMNode> rootNode)
        {
            foreach (VMNode childNode in originalNode)
            {
                VMNode newNode = childNode.Clone();
                IterateTreeNodes(childNode.Nodes, newNode.Nodes);
                rootNode.Add(newNode);
            }
        }
        /*
        public void CacheNodes()
        {
            cachedNodes.Clear();
            IterateTreeNodes(Nodes, cachedNodes);
        }
        */
    }
}