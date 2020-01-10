using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for MusicFileControl.xaml
    /// </summary>
    public partial class MusicFileControl : UserControl
    {
        public MusicFileControl()
        {
            InitializeComponent();
            DataContext = this;
            treeView.DataContext = this;
            ViewModel = new VMMusicFiles();
        }

        bool Search_init;
        public VMMusicFiles ViewModel { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void OpenFileAsPlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in treeView.SelectedItems)
            {
                VMNode v = i.DataContext as VMNode;
                ViewModel.RaiseOpenPlaylistEvent(v.FilePath);
            }
        }

        private void AddDirectoryFileContext_Click(object sender, RoutedEventArgs e)
        {
           List<FileDirectory> addedFolders = ViewModel.SelectAndLoadDirectory("Select Music Directory");
           LoadMusicFiles(addedFolders);
        }
        
        private void RemoveDirectoryContext_Click(object sender, RoutedEventArgs e)
        {
            foreach(var i in treeView.SelectedItems)
            {
                VMNode node = i.DataContext as VMNode;
                if (node == null)
                    continue;
                //move to VMMusicFiles
                ViewModel.RemoveAncestorDirectory(node);
                Database.Instance.DeleteMusicDirectory(node.FilePath);
            }
        }
        
        private void MSTreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null)
                tvi.BringIntoView();
        }
        
        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Length == 0)
                ResetSearchState();
        }

        internal void ResetSearchState()
        {
            searchBox.Text = "Search";
            ViewModel.RestorecachedNodes();
        }

        private void searchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text == "Search")
                searchBox.Text = "";
            if (!Search_init)
                Search_init = true;
        }

        private void searchBox_TextInput(object sender, TextChangedEventArgs e)
        {
            if (!Search_init)
                return;
            if (searchBox.Text.Length > 2)
                ViewModel.SearchFiles(searchBox.Text);
        }

        private void FolderDirectoryContext_Click(object sender, RoutedEventArgs e)
        {
            treeView.OpenSelectedFolders();
        }

        private void ReloadMusicFileContext_Click(object sender, RoutedEventArgs e)
        {
            LoadMusicFiles(ViewModel.fileLoader.Directories);
        }

        private void LoadMusicFiles(List<FileDirectory> directories)
        {
            foreach (FileDirectory fd in directories)
            {
                List<string> files = tool.LoadAudioFiles(fd.directory, SearchOption.AllDirectories);
                fd.AddRange(files);
                Database.Instance.PopulateMusicFiles(fd);
            }
            ViewModel.LoadDirectoriesToTree();
        }
    }
}
