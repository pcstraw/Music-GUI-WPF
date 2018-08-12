using Glaxion.ViewModel;
using System.Collections.Generic;
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

        protected override void AddDataFromFiles(int inertionIndex, List<string> files)
        {
            trackManager.InsertSongsFromFiles(inertionIndex, files);
        }

        protected override void AddData(int insertIndex, List<VMSong> items)
        {
            trackManager.AddItems(insertIndex, items);
        }
    }
}
