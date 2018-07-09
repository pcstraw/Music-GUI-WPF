using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Glaxion.Music;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for PlaylistFileControl.xaml
    /// </summary>
    public partial class PlaylistFileControl : INotifyPropertyChanged, ITreeView
    {
        public PlaylistFileControl()
        {
            InitializeComponent();
            DataContext = this;
            manager = new PlaylistFileManager(this);
          //  treeView.ItemsSource = manager.Nodes;
        }

        public PlaylistFileManager manager;

        private List<VNode> m_folders;
        public List<VNode> Folders
        {
            get { return m_folders; }
            set
            {
                m_folders = value;
                NotifiyPropertyChanged("Folders");
            }
        }

        void NotifiyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BrowsePlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            manager.SelectAndLoadDirectory();
        }

        private void OpenPlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {

        }

        //convert virtual tree node into WPF treeView format
        //call recursively
        public static TreeViewItem GetTreeNode(VNode node)
        {
            TreeViewItem t = new TreeViewItem();
            t.Header = node.Text;
            if(node.Expand)
                t.ExpandSubtree();
            foreach (VNode vn in node.Nodes)
                t.Items.Add(GetTreeNode(vn));

            return t;
        }

        //call via the managers tree interface to update the treeview
        public void PopulateTree(List<VNode> tree)
        {
            
            treeView.Items.Clear();
            foreach(VNode n in tree)
            {
                TreeViewItem rootItem = GetTreeNode(n);
                treeView.Items.Add(rootItem);
            }
        }

        private void RemovePlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            //foreach()
        }
    }
}
