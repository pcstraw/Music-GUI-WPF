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
using Glaxion.Tools;

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
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                tool.debug("Starting MusicGUI");
                SaveSettings.RestoreMainControl(this);
                Window window = Window.GetWindow(this);
                window.Closing += Window_Closing;
                playlistControl.LinkControls(fileControl);
                tool.debug("Music GUI Loaded");
                tool.HideConsole();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings.SaveMainControl(this);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
