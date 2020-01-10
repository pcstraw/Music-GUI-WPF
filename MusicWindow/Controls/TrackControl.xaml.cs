using Glaxion.Music;
using Glaxion.Tools;
using Glaxion.ViewModel;
using MultiSelection;
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
            viewModel = listView.viewModel as VMTrackManager;
            DataContext = viewModel;
            listView.UpdateItemSource();
        }

        internal VMTrackManager viewModel;
        
        public TrackControl()
        {
            Construction();
        }

        public TrackControl(VMPlaylist playlist)
        {
            Construction();
            listView.ItemsSource = null;
            viewModel.SetPlaylist(playlist);
            listView.UpdateItemSource();
            playlistNameLabel.DataContext = playlist;
            
           // titleLabel.DataContext = infoControl.viewModel;
           // titleLabel.SetBinding(Label.ContentProperty, "Title");
        }
        
        private void PlaylistnameLabel_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ScrollToCurrentSong();
           // listView.ItemsSource = null;
            //viewModel.OpenPlaylistSelectDialog();
            //listView.UpdateItemSource();
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item == null)
                return;

            item.IsSelected = false;
            listView.SelectedItems.Remove(item);
            int index = listView.CurrentIndex;
            viewModel.PlaySong(index,true);
            ToggleUpdatePlayerButton();
            e.Handled = true;
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
            viewModel.UpdateCurrentPlaylist();
            playlistControlParent.RemoveFromPlaylistControl(this);
        }
        
        private void TrackControlCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TrackControlReloadButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ReloadPlaylistFromFile();
            listView.ItemsSource = null;
            listView.UpdateItemSource();
        }

        private void TrackControlSaveButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SavePlaylist();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.player.PlayEvent += Player_PlayEvent;
            viewModel.player.MusicUpdatedEvent += Player_MusicUpdatedEvent;
            ShowCurrentPlaying();
           // ToggleUpdatePlayerButton();
            ToggleSelectionMode(false);
            HideAllButtons(Visibility.Hidden);
        }

        private void Player_MusicUpdatedEvent(object sender, EventArgs args)
        {
            Playlist p = sender as Playlist;
            if (p == null)
                return;
            ToggleUpdatePlayerButton();

        }

        void ToggleUpdatePlayerButton()
        {
            bool visible = !viewModel.IsCurrentlyPlaying();
            if (!visible)
                HideUpdatePlayerButton();
            else
                ShowUpdatePlayerButton();
        }

        void ShowUpdatePlayerButton()
        {
            updatePlayerButton.Visibility = Visibility.Visible; 
        }

        void HideUpdatePlayerButton()
        {
            updatePlayerButton.Visibility = Visibility.Hidden;
        }

        void ScrollToCurrentSong()
        {
            Song song = viewModel.player.CurrentSong;
            if (song == null)
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

        void ShowCurrentPlaying()
        {
            if (playlistControlParent.currentTrackControl == this)
                return;

            if (viewModel.player.currentList == viewModel.CurrentList.playlist)
            {
                listView.ScrollIntoView(listView.Items[viewModel.player.CurrentTrackIndex]);
                return;
            }

            Song song = viewModel.player.CurrentSong;
            if (song == null)
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
         //   ShowCurrentPlaying();
          //  ToggleUpdatePlayerButton();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            viewModel.player.PlayEvent -= Player_PlayEvent;
            viewModel.player.MusicUpdatedEvent -= Player_MusicUpdatedEvent;
        }

        internal void ResetSearchState()
        {
            searchBox.Text = "Search";
        }

        private void searchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text == "Search")
                searchBox.Text = "";
        }
        
        List<VMSong> searchedSongs = new List<VMSong>();
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (viewModel == null)
                return;
     
            foreach(VMSong song in searchedSongs)
            {
                song.RestorePrevState();
            }
            searchedSongs.Clear();

            if (searchBox.Text.Length < 2)
                return;

            foreach (object song in listView.Items)
            {
                ListViewItem listViewItem = listView.GetItemFromData(song);
                if (listViewItem == null)
                    continue;

                VMSong s = song as VMSong;
                if(s.Title.ToLower().Contains(searchBox.Text.ToLower()))
                {
                    s.MakeSearchedState();
                    searchedSongs.Add(s);
                }
            }
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
        

        private void listView_GotFocus(object sender, RoutedEventArgs e)
        {
            if (playlistControlParent == null)
                return;
            playlistControlParent.currentTrackControl = this;
        }

        internal void FindTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            VMMusicFiles mf = playlistControlParent.vm.musicFileManager;
            ListViewItem item = listView.GetListViewItem(listView.PreContextIndex);

            if (item == null)
                return;

            VMSong vmsong = item.DataContext as VMSong;
            MusicFileControl mfc = playlistControlParent.mainFileControl.musicFileControl;
            playlistControlParent.mainFileControl.ShowMusicFiles();
            mfc.treeView.UnselectAll();
            mfc.ResetSearchState();
            mf.FindFile(vmsong.Filepath);
        }

        private void EditTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            foreach(VMSong song in listView.SelectedItems)
                tool.OpenVegas(song.Filepath);
        }
        
        private void listView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        private void SaveAsTracklistContext_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentList.playlist.SaveAs();
            viewModel.CurrentList.Refresh();
        }

        private void SendToTop_Click(object sender, RoutedEventArgs e)
        {
            listView.viewModel.MoveItems(0, listView._selItems);
            e.Handled = true;
        }

        private void SendToBottom_Click(object sender, RoutedEventArgs e)
        {
            listView.viewModel.MoveItems(listView.Items.Count-1, listView._selItems);
            e.Handled = true;
        }

        private void moveContextItem_MouseMove(object sender, MouseEventArgs e)
        {
          
        }

        private void moveContextItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                listView.SortCachedSelectedItems();
                int index = listView.PreContextIndex;
                listView.viewModel.MoveItems(index, listView._selItems);
            }
        }

        private void UpdatePathsContext_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CurrentList.playlist.UpdatePaths();
            viewModel.SetPlaylist(viewModel.CurrentList);
            listView.ItemsSource = null;
            listView.UpdateItemSource();
        }

        private void SelectAlbum_Click(object sender, RoutedEventArgs e)
        {
            listView.SelectHoveredAlbum();
        }

        private void SelectArtist_Click(object sender, RoutedEventArgs e)
        {
            listView.SelectHoveredArtist();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            listView.SelectHoveredFolder();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            listView.SelectEveryting();
        }

        private void SelectClear_Click(object sender, RoutedEventArgs e)
        {
            listView.ClearSelection();
        }

        private void ClearVisible_Click(object sender, RoutedEventArgs e)
        {
            listView.ClearVisible();
        }

        private void TrackControlUpdatePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateMusicPlayer();
            HideUpdatePlayerButton();
        }

        bool ctrlDown;

        private void listView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            /*
            if(e.Key == Key.LeftShift||e.Key == Key.RightShift)
            {
                listView.SelectionMode = SelectionMode.Extended;
            }
            */
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                // if (listView.SelectionMode == SelectionMode.Extended)
                //if(!listView.multi_select)
                if (!ctrlDown)
                {
                    switch (last_select_mode)
                    {
                        case SelectionMode.Multiple:
                                ToggleSelectionMode(false);
                                break;
                        case SelectionMode.Extended:
                            ToggleSelectionMode(true);
                            break;
                        default:
                            break;
                    }
                    ctrlDown = true;
                }
            }
        }

        SelectionMode last_select_mode;
        private void listView_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            /*
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                listView.SelectionMode = SelectionMode.Multiple;
            }
            */
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (ctrlDown)
                {
                    switch (last_select_mode)
                    {
                        case SelectionMode.Multiple:
                            ToggleSelectionMode(false);
                            break;
                        case SelectionMode.Extended:
                            ToggleSelectionMode(true);
                            break;
                        default:
                            break;
                    }
                    ctrlDown = false;
                }
                
            }
        }

        private void listView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //  if (ctrlDown)
            //      listView.ClearSelection();

            //dep
            if (listView.SelectionMode == SelectionMode.Extended)
            {
                ListViewItem item = sender as ListViewItem;
                if (item == null)
                    return;

                if (item.IsSelected)
                {
                    item.IsSelected = false;
                }
            }
        }
        
        void ToggleSelectionMode(bool enabled)
        {
            bool m = listView.ToggleMultiSelection(enabled);
            if (listView.SelectionMode == SelectionMode.Multiple)
            {
                MultiSelectButton.Content = "M";
            }
            if (listView.SelectionMode == SelectionMode.Extended)
            {
                MultiSelectButton.Content = "E";
            }
            last_select_mode = listView.SelectionMode;
        }

        private void MultiSelectButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSelectionMode(!listView.multi_select);
        }

        private void Item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            if (ctrlDown)
                return;
            
            if (listView.SelectionMode == SelectionMode.Extended)
            {
                ListViewItem item = sender as ListViewItem;
                if (item == null)
                    return;

                if (item.IsSelected)
                {
                    // VMItem i = item.DataContext as VMItem;
                    item.IsSelected = false;
                    listView.SelectedItems.Remove(item);
                    e.Handled = true;
                }
            }
            return;
        }
        
        private void EditTagsTracklistContext_Click(object sender, RoutedEventArgs e)
        {

            VMSong item = viewModel.Items[listView.PreContextIndex] as VMSong;
            if (item == null)
                return;

            AlbumArtControl id3control = new AlbumArtControl();
            id3control.infoControl.viewModel.SetSong(item.CurrentSong);
            Window w = AlbumArtControl.CreateWindowHostingUserControl(id3control,250,250);
            id3control.SetWindow(w);
            w.Owner = MainControl.Current.window;
            w.Show();
        }
        
        void HideAllButtons(Visibility visibility)
        {
            MultiSelectButton.Visibility = visibility;
            saveButton.Visibility = visibility;
            reloadButton.Visibility = visibility;
            closeButton.Visibility = visibility;

            if (visibility == Visibility.Visible)
            {
                ToggleUpdatePlayerButton();
                return;
            }
            else
                updatePlayerButton.Visibility = visibility;
        }
        

        private void TopPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            HideAllButtons(Visibility.Visible);
        }

        private void TopPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            double Y = e.GetPosition(this).Y;
            if (Y > 0 && Y < TopPanel.ActualHeight)
            {

            }
            else
                HideAllButtons(Visibility.Hidden);
        }
        
        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            double Y = e.GetPosition(this).Y;
            if (Y > 0 && Y < TopPanel.ActualHeight)
            {

            }else
                HideAllButtons(Visibility.Hidden);
        }

        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Length < 2)
                ResetSearchState();
        }

        int searchHighlightIndex = 0;
        private void searchBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (searchedSongs.Count == 0)
                return;

            if (searchHighlightIndex >= searchedSongs.Count)
                searchHighlightIndex = 0;
            listView.ScrollIntoView(searchedSongs[searchHighlightIndex]);
            searchHighlightIndex++;
        }
    }
}
