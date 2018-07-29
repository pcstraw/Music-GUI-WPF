using System.Collections.Generic;
using System.ComponentModel;
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
            this.Loaded += MusicFileControl_Loaded;
        }

        //for testing.  Delete afterwards
        private void MusicFileControl_Loaded(object sender, RoutedEventArgs e)
        {
           // tool.ShowConsole();
        }

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
           ViewModel.SelectAndLoadDirectory("Select Music Directory");
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
            }
        }

        private void MusicFileTreeViewItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void MusicFileNode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
