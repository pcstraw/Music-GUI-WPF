using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for SwapControl.xaml
    /// </summary>
    public partial class FileControl : UserControl
    {
        public FileControl()
        {
            InitializeComponent();
        }

        public void SwapControls()
        {
            int mfrow = Grid.GetRow(musicFileControl);
            int pfrow = Grid.GetRow(playlistFileControl);

            Grid.SetRow(musicFileControl, pfrow);
            Grid.SetRow(playlistFileControl, mfrow);
        }

        public void ShowMusicFiles()
        {
            Grid.SetRow(musicFileControl, 1);
            Grid.SetRow(playlistFileControl, 3);
            isMusicFilesVisible = true;
            isPlaylistsVisible = false;
        }

        public void ShowPlaylistFiles()
        {
            Grid.SetRow(musicFileControl, 3);
            Grid.SetRow(playlistFileControl, 1);
            isMusicFilesVisible = false;
            isPlaylistsVisible = true;
        }

        bool isPlaylistsVisible;
        bool isMusicFilesVisible;

        public void CollapseSplitter()
        {
            splitterGrid.RowDefinitions[3].Height = new GridLength(0, GridUnitType.Star);
        }

        private void MusicFileTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            if(isMusicFilesVisible)
            {
                CollapseSplitter();
                return;
            }
            ShowMusicFiles();
        }

        private void PlaylistFileTab_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            if (isPlaylistsVisible)
            {
                CollapseSplitter();
                return;
            }
            ShowPlaylistFiles();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CollapseSplitter();
        }
    }
}
