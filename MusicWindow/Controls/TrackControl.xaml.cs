using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            vm = listView.viewModel as VMTrackManager;
            DataContext = vm;
            playlistNameLabel.DataContext = vm;
            listView.UpdateItemSource();
        }

        VMTrackManager vm;

        public TrackControl()
        {
            Construction();
        }

        public TrackControl(Playlist playlist)
        {
            Construction();
            listView.ItemsSource = null;
            vm.SetPlaylist(playlist);
            listView.UpdateItemSource();
            
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
            vm.OpenPlaylistSelectDialog();
            listView.UpdateItemSource();
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
            VMSong s = item.Content as VMSong;
            vm.PlaySong(s.CurrentSong);
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
            vm.UpdateCurrentPlaylist();
            playlistControlParent.RemoveFromPlaylistControl(this);
        }
        
        private void TrackControlCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TrackControlReloadButton_Click(object sender, RoutedEventArgs e)
        {
            vm.ReloadPlaylistFromFile();
            listView.ItemsSource = null;
            listView.UpdateItemSource();
        }

        private void TrackControlSaveButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SavePlaylist();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm.player.PlayEvent += Player_PlayEvent;
            ShowCurrentPlaying();
        }

        void ShowCurrentPlaying()
        {
            Song song = vm.player.CurrentSong;
            if (song == null)
                return;

            if (listView.IsFocused)
                return;
            //revise in light of using vmsong
            foreach (VMSong s in listView.Items)
            {
                if (s.CurrentSong == song)
                {
                    listView.ScrollIntoView(s);
                    return;
                }
            }
        }

        private void Player_PlayEvent(object sender, EventArgs args)
        {
            ShowCurrentPlaying();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            vm.player.PlayEvent -= Player_PlayEvent;
        }

        Brush defaultColor;
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vm == null)
                return;
            if (defaultColor != listView.Background)
                defaultColor = listView.Background;

            
            if (searchBox.Text.Length ==0)
            {
                foreach (object song in listView.Items)
                {
                    ListViewItem listViewItem = listView.GetItemFromData(song);
                    if (listViewItem == null)
                        continue;
                    listViewItem.Background = defaultColor;
                }
                return;
            }
            /*
            List<Song> result = viewModel.SearchSongs(searchBox.Text, "Title");
            
            foreach (object song in listView.Items)
            {
                ListViewItem listViewItem = listView.GetItemFromData(song);
                if (listViewItem == null)
                    continue;
                listViewItem.Background = defaultColor;
                foreach (Song s in result)
                {
                    if (s == song)
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromRgb(255, 200, 200));
                    }
                }
            }
            listView.UpdateDefaultStyle();
            */
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           // listView._selItems.Clear();
        }

        private void RemoveTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            listView.RemoveSelectedItems();
        }

        private void DeleteTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            tool.Show(5, "Not implemented");
        }

        private void FolderTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            listView.trackManager.OpenSelectedTrackFolders();
        }

        private void MoveTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            listView.SortCachedSelectedItems();
            listView.viewModel.MoveItems(listView.CurrentIndex, listView._selItems);
            listView.highlightItem.RestoreBackground();
        }
    }
}
