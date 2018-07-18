using Glaxion.Music;
using Glaxion.Tools;
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
            entryControls = new List<PropertyControl>();
            propertyControl1.SetEntry("Title","");
            propertyControl2.SetEntry("Artist","");
            propertyControl3.SetEntry("Album","");
            propertyControl4.SetEntry("Year","");
            propertyControl5.SetEntry("Genre","");

            entryControls.Add(propertyControl1);
            entryControls.Add(propertyControl2);
            entryControls.Add(propertyControl3);
            entryControls.Add(propertyControl4);
            entryControls.Add(propertyControl5);
        }

        List<PropertyControl> entryControls;
        
        private void SetEntryValues(Song s)
        {
            foreach (PropertyControl pc in entryControls)
                pc.textBox.Width = double.NaN;

            propertyControl1.SetEntryValue(s.Title);
            propertyControl2.SetEntryValue(s.Artist);
            propertyControl3.SetEntryValue(s.Album);
            propertyControl4.SetEntryValue(s.Year);
            propertyControl5.SetEntryValue(s.Genres[0]);

            this.UpdateLayout();
            SetMaxWidth();
        }
        
        private void SetMaxWidth()
        {
            double _max = 0;
            foreach(PropertyControl pc in entryControls)
            {
                double _width = pc.textBox.ActualWidth;
                if (_width > _max)
                    _max = _width;
            }
            
            foreach(PropertyControl pc in entryControls)
            {
                pc.textBox.Width = _max;
            }
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
