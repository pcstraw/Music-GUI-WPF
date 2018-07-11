using Glaxion.Music;
using System;
using System.Collections.Generic;
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

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for MusicInfoControl.xaml
    /// </summary>
    public partial class MusicInfoControl : UserControl
    {
        public MusicInfoControl()
        {
            InitializeComponent();
            InitEntries();
        }

        private void InitEntries()
        {
            propertyControl1.SetEntry("Title","");
            propertyControl2.SetEntry("Artist","");
            propertyControl3.SetEntry("Album","");
            propertyControl4.SetEntry("Year","");
            propertyControl5.SetEntry("Genre","");
        }

        private void SetEntryValues(Song s)
        {
            propertyControl1.SetEntryValue(s.Title);
            propertyControl2.SetEntryValue(s.Artist);
            propertyControl3.SetEntryValue(s.Album);
            propertyControl4.SetEntryValue(s.Year);
            propertyControl5.SetEntryValue(s.Genres[0]);
        }
        
        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            if (!(sender is Song))
                return;
            Song s = sender as Song;
            SetEntryValues(s);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Player.TrackChangeEvent += Player_TrackChangeEvent;
        }
    }
}
