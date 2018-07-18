using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for TrackControl.xaml
    /// </summary>
    public partial class TrackControl : UserControl
    {
        private void Construction()
        {
            InitializeComponent();
            viewModel = new VMTrackManager();
            DataContext = viewModel;
            //listView.ItemsSource = viewModel.Songs;
            playlistNameLabel.DataContext = viewModel;
            listView.DropDataEvent += ListView_DropDataEvent;
            listView.MultiDropDataEvent += ListView_MultiDropDataEvent;
        }

        VMTrackManager viewModel;

        private void ListView_MultiDropDataEvent(int Index, List<object> Item)
        {
            listView.MultiDropData<Song>(Index, viewModel.Songs, Item);
        }

        private void ListView_DropDataEvent(int ReplaceIndex, object Item)
        {
            listView.DropData<Song>(ReplaceIndex, viewModel.Songs, Item);
        }

        public TrackControl()
        {
            Construction();
        }

        public TrackControl(Playlist playlist)
        {
            Construction();
            listView.ItemsSource = null;
            viewModel.SetPlaylist(playlist);
            listView.ItemsSource = viewModel.Songs;
        }
       // public Playlist CurrentList { get; private set; }
       // public ObservableCollection<Song> Songs { get; set; }
       /*
        public void SetPlaylist(Playlist playlist)
        {
            if(playlist == null)
            {
                tool.show(3, "Playlist is null");
                return;
            }
            playlist.UpdatePaths();
            CurrentList = playlist;

            Songs = playlist.songs;
            listView.ItemsSource = Songs;
            //TODO:  use proper binding
            playlistNameLabel.Content = playlist.Name; 
        }
        */
        private void PlaylistnameLabel_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            listView.ItemsSource = null;
            viewModel.OpenPlaylistSelectDialog();
            listView.ItemsSource = viewModel.Songs;
            /*
            List<string> l = tool.SelectFiles(false, false,"Select Playlist");
            if (l.Count != 1)
                return;

            if (!tool.IsPlaylistFile(l[0]))
                return;

            string path = l[0];
            Playlist p = new Playlist(path, true);
            SetPlaylist(p);
            */
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            Song s = item.Content as Song;
            viewModel.PlaySong(s);
        }

        private void DelesctAll_Click(object sender, RoutedEventArgs e)
        {
            listView.UnselectAll();
        }
        
        //dock controls
        internal ColumnDefinition controlColumn;
        internal ColumnDefinition splitColumn;
        internal GridSplitter split;
        internal Grid grid;
        internal PlaylistControl playlistControlParent;
        internal int ColumnIndex;
        internal int SplitterIndex;
        
        internal void SetDockingControls(ColumnDefinition trackColumn, ColumnDefinition splitterColumn, GridSplitter splitter, Grid ownerGrid)
        {
            controlColumn = trackColumn;
            splitColumn = splitterColumn;
            split = splitter;
            grid = ownerGrid;
        }
        
        internal void UndockControlFromParent()
        {
            grid.Children.Remove(this);
            grid.Children.Remove(split);

            grid.ColumnDefinitions.Remove(controlColumn);
            grid.ColumnDefinitions.Remove(splitColumn);
            playlistControlParent = null;
        }

        internal void Close()
        {
            playlistControlParent.RemoveFromPlaylistControl(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
