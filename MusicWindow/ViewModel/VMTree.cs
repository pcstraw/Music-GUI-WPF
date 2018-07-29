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
        
        private VMNode PopulateNodesFromDirectory(string directory)
        {
            VMNode root = new VMNode();
            root.Name = directory;
            root.FilePath = directory;
            List<string> files = fileLoader.LoadFiles(directory);
            if (files.Count == 0)
                return null;

            foreach (string f in files)
                root.BuildChildNodes(directory, f, f);

            root.SortNodes();
            return root;
        }

        public void LoadFilesToTree()
        {
            cachedNodes.Clear();
            foreach (string dir in fileLoader.Directories)
            {
                VMNode root = PopulateNodesFromDirectory(dir);
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
        public virtual void SelectAndLoadDirectory(string dialogBrowserTile)
        {
            List<string> folders = tool.SelectFiles(true, true, dialogBrowserTile);
            bool directoryAdded = false;

            foreach (string folder in folders)
                directoryAdded = AddDirectory(folder);

            if (directoryAdded)
            {
                LoadFilesToTree();
                Nodes = cachedNodes;
            }
        }

        public virtual bool AddDirectory(string directoryPath)
        {
            return fileLoader.AddDirectory(directoryPath);
        }

        public VMNode SearchTreeView(VMNode tree, string text)
        {
            if (tree.Name == text) //?
                return tree;
            if (tree.Nodes.Count > 0)
            {
                foreach (VMNode t in tree.Nodes)
                {
                    if (t.Name == text)
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

        public virtual void FindFile(string path)
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
                        return;
                    }
                }
            }
        }

        public void RemoveAncestorDirectory(VMNode node)
        {
            VMNode ancestor = node.GetAncestor();
            if (ancestor.Parent == null)
            {
                string dir = ancestor.FilePath;
                Nodes.Remove(ancestor);
                fileLoader.Directories.Remove(dir);
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

        public void CacheNodes()
        {
            cachedNodes.Clear();
            IterateTreeNodes(Nodes, cachedNodes);
        }

        public List<VMNode> CopyTree()
        {
            List<VMNode> copied = new List<VMNode>();
            foreach (VMNode n in Nodes)
            {
                copied.Add(n.Clone());
            }
            return copied;
        }

        public void SearchForText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (!_useCahcedNodes)
                {
                    //_view.PopulateTree(cachedNodes);
                    _useCahcedNodes = true;
                    //TODO: restore last opened nodes
                    if (Nodes.Count > 0)
                    {
                        Nodes[0].Expanded = true;
                    }
                }
                return;
            }
            ObservableCollection<VMNode> results = new ObservableCollection<VMNode>();
            SearchTree(text, cachedNodes, results);
            if (results.Count > 0)
            {
                //_view.PopulateTree(results);
                _useCahcedNodes = false;
            }
        }

        //use ref for results to show we are modifying the list
        public void SearchTree(string text, ObservableCollection<VMNode> nodes, ObservableCollection<VMNode> results)
        {
            foreach (VMNode t in nodes)
            {
                if (t.Name.ToLower().Contains(text.ToLower()))
                {
                    results.Add(t.Clone());
                }
                SearchTree(text, t.Nodes, results);
            }
        }

        public void SetTree(ObservableCollection<VMNode> nodes, string SearchText)
        {
            foreach (VMNode n in nodes)
            {
                Nodes.Add(n);
                if (tool.StringCheck(SearchText))
                    SearchForText(SearchText);
            }
            //_view.PopulateTree(Nodes);
        }

        public void SetTree(VMNode node, string searchText)
        {
            //probably check if the node is a valid file?
            if (node == null)
                return;
            Nodes.Add(node);
            if (tool.StringCheck(searchText))
                SearchForText(searchText);
            //_view.PopulateTree(Nodes);
        }
    }
}