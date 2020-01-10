using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Glaxion.Music;
using Glaxion.Tools;
using MultiSelection;
using Glaxion.ViewModel;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for PlaylistFileControl.xaml
    /// </summary>
    public partial class PlaylistFileControl : INotifyPropertyChanged
    {
        public PlaylistFileControl()
        {
            InitializeComponent();
            DataContext = this;
            treeView.DataContext = this;
            ViewModel = new VMPlaylistFileTree();
        }

        object selectSource;
        bool Search_init;
        public VMPlaylistFileTree ViewModel { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BrowsePlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            List<FileDirectory> directories = ViewModel.SelectAndLoadDirectory("Select Playlist Directory");
            LoadPlaylistFiles(directories);
        }

        private void OpenPlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in treeView.SelectedItems)
            {
                VMNode v = i.DataContext as VMNode;
                ViewModel.RaiseOpenPlaylistEvent(v.FilePath);
            }
        }
  
        private void PlaylistFileNode_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != selectSource)
                return;
            MSTreeViewItem item = sender as MSTreeViewItem;
            VMNode s = item.DataContext as VMNode;
            e.Handled = true;
            ViewModel.RaiseOpenPlaylistEvent(s.FilePath);
        }
        
        private void PlaylistFileTreeItem_Selected(object sender, RoutedEventArgs e)
        {
            selectSource = sender;
            e.Handled = true; //doesn't seem stop event bubbling up
        }

        private void RemovePlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in treeView.SelectedItems)
            {
                VMNode node = i.DataContext as VMNode;
                if (node == null)
                    continue;
                //move to VMMusicFiles
                ViewModel.RemoveAncestorDirectory(node);
                Database.Instance.DeletePlaylistDirectory(node.FilePath);
            }
        }
        
        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Length == 0)
            {
                searchBox.Text = "Search";
                ViewModel.RestorecachedNodes();
                return;
            }
        }

        private void searchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text == "Search")
                searchBox.Text = "";
            if (!Search_init)
                Search_init = true;
        }

        private void searchBox_TextInput(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Search_init)
                return;
            if (searchBox.Text.Length > 2)
                ViewModel.SearchFiles(searchBox.Text);
        }

        private void FolderPlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            treeView.OpenSelectedFolders();
        }

        private void ReloadPlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylistFiles(ViewModel.fileLoader.Directories);
        }

        void LoadPlaylistFiles(List<FileDirectory> directories)
        {
            foreach (FileDirectory fd in directories)
            {
                List<string> list = tool.LoadFiles(fd.directory, ".m3u");
                fd.AddRange(list);
                Database.Instance.PopulatePlaylistFiles(fd);
            }
            ViewModel.LoadDirectoriesToTree();
        }
    }
}
