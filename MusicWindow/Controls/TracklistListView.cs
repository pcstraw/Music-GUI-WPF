using Glaxion.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace MusicWindow
{
    public class TracklistListView : ListViewEx<VMSong>
    {
        public TracklistListView() :base()
        {
            trackManager = new VMTrackManager();
            viewModel = trackManager;
        }

        internal VMTrackManager trackManager;

        internal override void AddDataFromFiles(int inertionIndex, List<string> files)
        {
            trackManager.InsertSongsFromFiles(inertionIndex, files);
        }

        protected override void AddData(int insertIndex, List<VMSong> items)
        {
            trackManager.AddItems(insertIndex, items);
        }

        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            base.OnContextMenuClosing(e);
          //  base.InvalidateVisual();
        }

        internal void SelectHoveredAlbum()
        {
            if (PreContextIndex < 0)
                return;

            VMSong currentsong = Items[PreContextIndex] as VMSong;
            if (currentsong == null)
                return;

           // SelectedItems.Clear();
           
            foreach(VMSong song in Items)
            {
                if (currentsong.Album == song.Album)
                    song.IsSelected = true;
            }
        }

        internal void SelectHoveredArtist()
        {
            if (PreContextIndex < 0)
                return;

            VMSong currentsong = Items[PreContextIndex] as VMSong;
            if (currentsong == null)
                return;

          //  SelectedItems.Clear();

            foreach (VMSong song in Items)
            {
                if (currentsong.Artist== song.Artist)
                    song.IsSelected = true;
            }
        }

        internal void SelectHoveredFolder()
        {
            if (PreContextIndex < 0)
                return;
            VMSong currentsong = Items[PreContextIndex] as VMSong;
            if (currentsong == null)
                return;

          //  SelectedItems.Clear();
            string dir = Path.GetDirectoryName(currentsong.Filepath);
            foreach (VMSong song in Items)
            {
                if (dir == Path.GetDirectoryName(song.Filepath))
                    song.IsSelected = true;
            }
        }

        internal void ClearVisible()
        {
            SelectedItems.Clear();
        }

        internal void ClearSelection()
        {
            SelectedItems.Clear();
            foreach (VMSong song in Items)
                song.IsSelected = false;

        }

        internal void SelectEveryting()
        {
            foreach(VMSong song in Items)
                song.IsSelected = true;
        }

        internal bool multi_select;
        /*
        internal bool ToggleMultiSelection(bool EnableMultiSelect)
        {
            multi_select = EnableMultiSelect;
            ToggleMultiSelection();
            return EnableMultiSelect;
        }
        */

        internal bool ToggleMultiSelection(bool EnableMultiSelect)
        {
            multi_select = EnableMultiSelect;
            if(multi_select)
            {
                SelectionMode = SelectionMode.Multiple;
                return multi_select;
            }else{
                SelectionMode = SelectionMode.Extended;
                return multi_select;
            }
        }
    }
}
