using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Glaxion.Tools;
using Glaxion.Music;
using System.Diagnostics;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        public static MainControl Current;
        public Window window;
        IntPtr windowHandle;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                tool.debug("Starting MusicGUI");
                window = Window.GetWindow(this);
                windowHandle = new WindowInteropHelper(window).Handle;
                window.Title = "Music GUI";
                SaveSettings.RestoreMainControl(this);
                
                window.Closing += Window_Closing;
                playlistControl.LinkControls(fileControl);
                tool.debug("Music GUI Loaded");

                GetAllResources();
                if(!consoleCheckBox.IsChecked.Value)
                    tool.HideConsole();
          
                MusicPlayer.Player.PlayEvent += Player_PlayEvent;
                SetWindowTitleFromCurrentSong();

                if (Current == null)
                    Current = this;

               // Database.Instance.PopulateMusicFiles();
            }
        }

        internal void SetWindowTitleFromCurrentSong()
        {
            if(MusicPlayer.Player == null ||MusicPlayer.Player.CurrentSong != null)
                window.Title = MusicPlayer.Player.CurrentSong.Filepath;
        }

        private void Player_PlayEvent(object sender, EventArgs args)
        {
            SetWindowTitleFromCurrentSong();
        }

        private void GetAllResources()
        {
            string[] resourceNames = this.GetType().Assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                if (tool.IsImageFile(resourceName))
                {
                    string[] parts = resourceName.Split('.');
                    if (parts.Length > 1)
                    {
                        int last = parts.Length-1;
                        string fileName = string.Concat(parts[last - 1], ".", parts[last]);
                        string newFile = System.IO.Path.Combine("Resources", fileName);

                        if(!File.Exists(newFile))
                            CopyResource(resourceName, newFile);
                    }
                }
            }
        }
        
        private void CopyResource(string resourceName, string file)
        {
            using (Stream resource = GetType().Assembly
                                              .GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                    throw new ArgumentException("No such resource", resourceName);

                using (Stream output = File.OpenWrite(file))
                    resource.CopyTo(output);
            }
        }

        void ToggleConsole()
        {
            if (consoleCheckBox.IsChecked != null)
                tool.ToggleConsole(consoleCheckBox.IsChecked.Value);
            if(window.WindowState != WindowState.Maximized)
            tool.SetForegroundWindow(windowHandle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings.SaveMainControl(this);
            MusicPlayer.Player.Stop();
        }

        private void consoleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ToggleConsole();
        }

        private void consoleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleConsole();
        }

        private void AlbumArtControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
               // MusicPlayer.Player.TrackChangeEvent += Player_TrackChangeEvent;
                // MusicPlayer.Player.PlayEvent += Player_TrackChangeEvent;
                Song s = MusicPlayer.Player.CurrentSong;
                if (s == null)
                    s = new Song("empty");
                s.LoadAlbumArt();
                albumArtControl.infoControl.viewModel.SetSong(s);

                MusicPlayer.Player.TrackChangeEvent += Player_TrackChangeEvent;
            }
        }

        private void Player_TrackChangeEvent(object sender, EventArgs args)
        {
            Song s = sender as Song;
            if (s == null)
                return;

            albumArtControl.infoControl.viewModel.SetSong(s);
        }
    }
}
