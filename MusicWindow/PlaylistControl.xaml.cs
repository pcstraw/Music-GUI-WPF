using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Glaxion.Music;
using Glaxion.Tools;

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
            Playlists = new ObservableCollection<Playlist>();
            DataContext = this;
            listView.ItemsSource = Playlists;
        }
        ObservableCollection<Playlist> Playlists;

        public delegate void OpenPlaylistEventHandler(object sender, Playlist playlist);
        public event OpenPlaylistEventHandler OpenPlaylistEvent;
        protected void On_OpenPlaylist(object sender, Playlist playlist)
        {
        }

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

        private void BrowsePlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            List<string> l = tool.SelectFiles(false, true);

            if (l.Count == 0)
                return;
            foreach(string s in l)
            {
                if(tool.IsPlaylistFile(s))
                {
                    Playlist p = new Playlist(s, true);
                    Playlists.Add(p);
                }
            }
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Playlist p = ((ListViewItem)sender).Content as Playlist;
            AddDockColumn(p);
           //OpenPlaylistEvent(this, ((ListViewItem)sender).Content as Playlist);
        }

        private void OpenPlaylistContext_Click(object sender, RoutedEventArgs e)
        {
            if (OpenPlaylistEvent == null)
                return;
            foreach (Playlist p in listView.SelectedItems)
            {
                AddDockColumn(p);
            }
        }

        public void AddDockColumn(Playlist playlist)
        {
            GridSplitter gs = new GridSplitter();
            gs.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
            gs.ResizeDirection = GridResizeDirection.Columns;
            gs.VerticalAlignment = VerticalAlignment.Stretch;
            gs.HorizontalAlignment = HorizontalAlignment.Stretch;
            gs.Width = 5;
            gs.Background = new SolidColorBrush(Colors.Black);

            ColumnDefinition subcolumn2 = new ColumnDefinition();
            subcolumn2.Width = GridLength.Auto;
            maingrid.ColumnDefinitions.Add(subcolumn2);
            Grid.SetColumn(gs, maingrid.ColumnDefinitions.Count-1);
            maingrid.Children.Add(gs);

            TrackControl c = new TrackControl(playlist);
            c.VerticalAlignment = VerticalAlignment.Stretch;
            c.HorizontalAlignment = HorizontalAlignment.Stretch;
            c.Width = Double.NaN;
            c.Height = Double.NaN;

            Grid.SetColumn(c, maingrid.ColumnDefinitions.Count);
            maingrid.Children.Add(c);
            ColumnDefinition subcolumn = new ColumnDefinition();
            maingrid.ColumnDefinitions.Add(subcolumn);
        }
    }
}
