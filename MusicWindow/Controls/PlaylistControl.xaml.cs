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
            DataContext = listView.viewModel;
            vm = listView.viewModel as VMPlaylistManager;
            listView.UpdateItemSource();
        }
        

        internal VMPlaylistManager vm;
        internal FileControl mainFileControl;
        internal List<TrackControl> trackControls = new List<TrackControl>();

        public TrackControl currentTrackControl { get; internal set; }

        public void LinkControls(FileControl fileControl)
        {
            vm.LinkFileManagers(fileControl.playlistFileControl.ViewModel,fileControl.musicFileControl.ViewModel);
            mainFileControl = fileControl;
        }

        //move to view model
        private void BrowsePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            listView.BrowseForPlaylistDirectory();
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VMPlaylist p = ((ListViewItem)sender).Content as VMPlaylist;
            AddDockColumn(p);
        }

        private void OpenPlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (VMPlaylist p in listView.SelectedItems)
                AddDockColumn(p);
        }


        private void CreatePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            vm.CreateNewPlaylist();
        }


        //move to viewmodel
        private void RemovePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveSelectedPlaylists(listView.SelectedItems);
        }

        private void DeletePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            //e.Handled = true;
            vm.DeleteSelectedPlaylists(listView.SelectedItems);
        }
        
        #region Docking
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

        internal VMPlaylist FindPlaylist(string filePath)
        {
            foreach(VMPlaylist p in listView.Items)
            {
                if (p.Filepath == filePath)
                    return p;
            }
            return null;
        }

        public TrackControl AddDockColumn(VMPlaylist playlist)
        {
            TrackControl tc = new TrackControl(playlist);
            DockTrackControl(tc);
            return tc;
        }
        #endregion

        private void SaveAsPlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (VMPlaylist p in listView.SelectedItems)
            {
                p.playlist.SaveAs();
                p.Refresh();
            }
        }

        private void QuickSavePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            foreach (VMPlaylist p in listView.SelectedItems)
                p.playlist.Save();
        }

        private void FolderPlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            listView.OpenPlaylistFolders();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void listView_Drop(object sender, DragEventArgs e)
        {
            bool isListViewEx = e.Data.GetDataPresent(typeof(ListViewEx<VMSong>));
            if (!isListViewEx)
                return;

            List<string> itemsToMove = new List<string>();
            ListViewEx<VMSong>source = e.Data.GetData(typeof(ListViewEx<VMSong>)) as ListViewEx<VMSong>;
            int index = listView.CurrentIndex;//GetCurrentIndex(e.GetPosition);
                                     /*
                                     if(index >= Items.Count)
                                     {
                                         throw new Exception("Index should be valid with the range");
                                     }
                                     */
                                     
           // index = listView.GetCurrentIndex(e.GetPosition);

            listView.SelectedItems.Clear();
            listView._selItems.Clear();


            foreach (VMSong s in source._selItems)
                itemsToMove.Add(s.Filepath);

            listView.AddDataFromFiles(index, itemsToMove);
        }
    }
}
