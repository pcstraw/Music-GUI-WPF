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
            //manager = PlaylistFileManager.Instance;
            treeView.DataContext = this;
            ViewModel = new VMPlaylistFiles();
        }

        public VMPlaylistFiles ViewModel { get; set; }

        object selectSource;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BrowsePlaylistFileContext_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAndLoadDirectory("Select Playlist Directory");
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
            e.Handled = true; //doesn't stop event bubbling up
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
            }
        }
    }
}
