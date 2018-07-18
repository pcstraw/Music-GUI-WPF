using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for PlaylistControl.xaml
    /// </summary>
    public partial class PlaylistControl : UserControl
    {
        public PlaylistControl()
        {
            InitializeComponent();
            // playlistManager = new VMPlaylistManager();
            // Playlists = new ObservableCollection<Playlist>();
            viewModel = new VMPlaylistManager();
            DataContext = viewModel;
            listView.ItemsSource = viewModel.Playlists;
        }

        internal VMPlaylistManager viewModel;
        internal List<TrackControl> trackControls = new List<TrackControl>();


        public void LinkControls(FileControl fileControl)
        {
            viewModel.LinkFileManagers(fileControl.playlistFileControl.ViewModel,fileControl.musicFileControl.ViewModel);
            //_fileControl = fileControl;
           // _playlistFileControl = fileControl.playlistFileControl;
            //_musicFileControl = fileControl.musicFileControl;
            //_playlistFileControl.ViewModel.OpenPlaylist += Manager_OpenPlaylist;
           // _musicFileControl.ViewModel.OpenPlaylist += Manager_OpenPlaylist;
        }

       // ObservableCollection<Playlist> Playlists;
       // PlaylistFileControl _playlistFileControl;
        //MusicFileControl _musicFileControl;
        //FileControl _fileControl;
        /*
        public List<string> GetPlaylistsAsFiles()
        {
            List<string> plist = new List<string>();
            foreach(Playlist p in Playlists)
                plist.Add(p.Filepath);
            return plist;
        }
        */
        //dep
        public Playlist DockPlaylist(Playlist playlist)
        {
            ColumnDefinition c = new ColumnDefinition();
            GridLength gl = new GridLength(10.00);
            c.Width = gl;
            maingrid.ColumnDefinitions.Add(c);

            GridSplitter splitter = new GridSplitter()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                Background = new SolidColorBrush(Colors.Green),
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft
            };

            maingrid.Children.Add(splitter);
            Grid.SetRow(splitter, 2);

            return playlist;
        }

        //move to view model
        private void BrowsePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            viewModel.BrowsePlaylistDialog();
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Playlist p = ((ListViewItem)sender).Content as Playlist;
            AddDockColumn(p);
           //OpenPlaylistEvent(this, ((ListViewItem)sender).Content as Playlist);
        }

        private void OpenPlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (Playlist p in listView.SelectedItems)
            {
                AddDockColumn(p);
            }
        }
        
        internal void DockTrackControl(TrackControl tc)
        {
            //if one of the docked controls has been resized then we need to reset the default
            //column back to star, otherwise newly docked controls will be super narrow
            maingrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            
            //create grid splitter
            GridSplitter gs = new GridSplitter();
            gs.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
            gs.ResizeDirection = GridResizeDirection.Columns;
            gs.VerticalAlignment = VerticalAlignment.Stretch;
            gs.HorizontalAlignment = HorizontalAlignment.Stretch;
            gs.Width = 5;
            gs.Background = new SolidColorBrush(Colors.PaleGoldenrod);
            //gridsplitter needs it's own column
            ColumnDefinition splitterColumn = new ColumnDefinition();
            splitterColumn.Width = GridLength.Auto;
            //later on we will need to  link the control to a column number
            //columndef.count means it will add a column to the far right
            //store in the trackcontrol because we will need it for reorrdering
            //the controls after a trackcontrol is removed
            //the splitter's column comes first
            tc.SplitterIndex = maingrid.ColumnDefinitions.Count;
            maingrid.ColumnDefinitions.Add(splitterColumn);
            //set trackcontrol size behaviour
            tc.VerticalAlignment = VerticalAlignment.Stretch;
            tc.HorizontalAlignment = HorizontalAlignment.Stretch;
            tc.Width = Double.NaN;
            tc.Height = Double.NaN;
            //column for the track control itself
            ColumnDefinition subcolumn = new ColumnDefinition();
            //As above, use the column.def count as the index
            tc.ColumnIndex = maingrid.ColumnDefinitions.Count;
            maingrid.ColumnDefinitions.Add(subcolumn);
            //we need to add the controls to the grid as well
            maingrid.Children.Add(gs);
            maingrid.Children.Add(tc);
            //trackcontrol needs to know about it's containing parent for removal
            tc.playlistControlParent = this;
            //so that a docked track control and clean itself up
            tc.SetDockingControls(subcolumn, splitterColumn, gs, maingrid);
            //maintain a list of docked track controls
            trackControls.Add(tc);
            //now we can set the column index for the splitter and track control
            SetColumnsPosition(tc);

        }

        internal void SetColumnsPosition(TrackControl trackControl)
        {
            Grid.SetColumn(trackControl.split, trackControl.SplitterIndex);
            Grid.SetColumn(trackControl, trackControl.ColumnIndex);
            //for debugging
            /*
            trackControl.ToolTip = trackControl.ColumnIndex;
            trackControl.split.ToolTip = trackControl.SplitterIndex;
            */
        }
        //remove track control
        internal void RemoveFromPlaylistControl(TrackControl trackControl)
        {
            //reorder the controls if the removed control is not last in order
            List<TrackControl> reorderedControls = new List<TrackControl>();
            for (int i = 0; i < trackControls.Count; i++)
            {
                TrackControl tc = trackControls[i];
                if (tc != trackControl)
                {
                    if (tc.ColumnIndex > trackControl.ColumnIndex)
                    {
                        tc.ColumnIndex -= 2;
                        tc.SplitterIndex -= 2;
                        reorderedControls.Add(tc);
                    }
                }
            }
            trackControl.UndockControlFromParent();
            trackControls.Remove(trackControl);
            foreach (TrackControl tc in reorderedControls)
            {
                SetColumnsPosition(tc);
            }
        }

        public void AddDockColumn(Playlist playlist)
        {
            TrackControl tc = new TrackControl(playlist);
            DockTrackControl(tc);
        }
        //move to viewmodel
        private void RemovePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveSelectedPlaylists(listView.SelectedItems);
        }
    }
}
