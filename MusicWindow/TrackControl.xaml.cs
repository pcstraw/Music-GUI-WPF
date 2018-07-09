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
using System.Xml;
using Glaxion.Music;
using Glaxion.Tools;

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
            DataContext = this;
        }
        
        public TrackControl()
        {
            Construction();
        }

        public TrackControl(Playlist playlist)
        {
            Construction();
            SetPlaylist(playlist);
        }
        
        public Playlist CurrentList { get; private set; }
        public ObservableCollection<Song> Songs { get; set; }

        public void SetPlaylist(Playlist playlist)
        {
            if(playlist == null)
            {
                tool.show(3, "Playlist is null");
                return;
            }
            CurrentList = playlist;
            Songs = playlist.songs;
            listView.ItemsSource = Songs;
            //TODO:  use proper binding
            playlistNameLabel.Content = playlist.Name; 
        }

        private void PlaylistnameLabel_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            List<string> l = tool.SelectFiles(false, false);
            if (l.Count != 1)
                return;

            if (!tool.IsPlaylistFile(l[0]))
                return;

            string path = l[0];
            Playlist p = new Playlist(path, true);
            SetPlaylist(p);
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            Song s = item.Content as Song;
            MusicPlayer.Player.Play(s);
        }
    }
}
