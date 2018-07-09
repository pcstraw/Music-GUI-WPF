using Glaxion.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Glaxion.ViewModel
{
    public class VMNode : INotifyPropertyChanged
    {
        public VMNode()
        {
            Nodes = new ObservableCollection<VMNode>();
            ID = globalID;
        }
        public VMNode(string text)
        {
            Name = text;
            Nodes = new ObservableCollection<VMNode>();
            // TreePath = text;
            ID = globalID;
        }

        public ObservableCollection<VMNode> Nodes { get; set; }

        //Test
        private readonly IDictionary<string, VMNode> _nodePaths =
            new Dictionary<string, VMNode>();

        public string Name { get; set; }
        public string FilePath { get; set; }
        public string TreePath;
        int ID;
        static int globalID;

        bool _expanded;
        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (value != _expanded)
                {
                    _expanded = value;
                    this.OnPropertyChanged();
                }
                // Expand all the way up to the root.
                if (_expanded && Parent != null)
                    Parent.Expanded = true;
            }
        }
        bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public VMNode Parent { get; set; }
        public string Text { get; internal set; }
        public bool IsFile { get; internal set; }

       // public Color ForeColor;
        //public Color BackColor;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddNode(VMNode node)
        {
            node.Parent = this;
            Nodes.Add(node);
        }

        public VMNode Clone()
        {
            VMNode node = new VMNode(Name);
            node.Name = Name;
            node.Expanded = Expanded;
            node.Selected = Selected;

            foreach (VMNode v in Nodes)
            {
                node.Nodes.Add(v.Clone());
            }
            return node;
        }

        //recursively build child nodes based on the filepath
        internal void BuildChildNodes(string directory, string fullPath, string subPath)
        {
            //remove everything before the directory in the path.  We could shift this responsibility to 
            //the root caller of this function instead, since it's only relevant to the initial fullpath
            string[] dir_splitter = new string[] { directory };
            string[] file_part = subPath.Split(dir_splitter,
                StringSplitOptions.RemoveEmptyEntries);

            //we always expect file_part.length to be 1, even if the functin is being called
            //recursively and subpath's directory has already been removed
            if (file_part.Length == 0)
            {
                tool.show(10, "Assert: we should always remain with one substring after removing directory");
                return;
            }
            if (file_part.Length != 1)
            {
                tool.show(10, "Assert: Expecting subPath.Split(Directory) to always return 1 string",
                    "", "Path:", "", fullPath,
                    "", "SubPath: ", subPath);
                tool.show(10, "Assert: file_part.Length", "", file_part.Length);
                foreach (string s in file_part)
                    tool.show(5, "Assert: filePart: ", s);
                return;
            }

            string substring = file_part[0];
            //split the string by file seperators
            string[] use_slashes = new string[] { @"\" };
            string[] parts = substring.Split(use_slashes,
                StringSplitOptions.RemoveEmptyEntries);
            //we should always have at least one entry
            if (parts.Length == 0)
            {
                tool.show(10, "Assert: Something might have gone wrong:  we're not expecting string.split to return an empty array",
                    "SubString:", substring,
                    "FilePart[0]", file_part[0],
                    "Subpath", subPath,
                    "FullPath", fullPath);
                return;
            }
            //if there is only one part left then we have found the file/last folder
            if (parts.Length == 1)
            {
                VMNode node = new VMNode();
                node.Name = parts[0];
                // TreePath = node.Name;
                node.FilePath = fullPath;
                node.IsFile = true;
                AddNode(node);
                return;
            }
            //this is the next consecutive folder in the branch
            string nextPath = parts[0];
            VMNode child;
            //Does this node alredy have a child with the given folder name/path?
            //If so use it, otherwise create a new node
            if (!_nodePaths.TryGetValue(nextPath, out child))
            {
                child = new VMNode();
                child.Name = nextPath;
                //construct the folder path
                string[] folder_splitter = new string[] { parts[1] };
                string[] folder_part = fullPath.Split(folder_splitter,
                    StringSplitOptions.RemoveEmptyEntries);
                //remove the trailer \
                child.FilePath = folder_part[0].Remove(folder_part[0].Count() - 1);
                // child.TreePath = nextPath;
                _nodePaths[nextPath] = child;
                AddNode(child);
            }
            //reconstruct the sub-folder path, skipping the folder/child we just created
            string newsubstring = "";
            for (int i = 1; i < parts.Length; i++)
            {
                newsubstring = string.Concat(newsubstring, @"\", parts[i]);
            }
            //continue building the tree structure from this child based on the newly shortened path
            child.BuildChildNodes(directory, fullPath, newsubstring);
        }

        //File nodes recusrively so that single file nodes come last
        internal void SortNodes()
        {
            if (Nodes.Count == 0)
                return;
            //collection.remove doesn't work for now so create a new collection instead
            ObservableCollection<VMNode> newCollection = new ObservableCollection<VMNode>();
            List<VMNode> fileNodes = new List<VMNode>();

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Nodes.Count > 0)
                    newCollection.Add(Nodes[i]);
                else
                    fileNodes.Add(Nodes[i]);
            }

            foreach (VMNode n in newCollection)
                n.SortNodes();

            foreach (VMNode n in fileNodes)
                newCollection.Add(n);

            Nodes = newCollection;
        }

        internal VMNode GetAncestor()
        {
            VMNode n = this;
            while (n.Parent != null)
            {
                n = n.Parent;
            }
            return n;
        }
    }
}
    
