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
using Glaxion.ViewModel;
using Glaxion.Tools;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicWindow
{
    /// <summary>
    /// Interaction logic for MusicInfoControl.xaml
    /// </summary>
    public partial class MusicInfoControl : UserControl , INotifyPropertyChanged
    {
        public MusicInfoControl()
        {
            InitializeComponent();
            InitEntries();
        }


        List<PropertyControl> entryControls;
        internal VMSong viewModel;

        bool _sto;
        public bool ShowTagOptions
        {
            get { return _sto; }
            set { _sto = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       

        private void InitEntries()
        {
            entryControls = new List<PropertyControl>();
            
            entryControls.Add(propertyControl1);
            entryControls.Add(propertyControl2);
            entryControls.Add(propertyControl3);
            entryControls.Add(propertyControl4);
            entryControls.Add(propertyControl5);

            viewModel = new VMSong();
            SetPropertyControlBinding(propertyControl1, nameof(viewModel.Title));
            SetPropertyControlBinding(propertyControl2, nameof(viewModel.Artist));
            SetPropertyControlBinding(propertyControl3, nameof(viewModel.Album));
            SetPropertyControlBinding(propertyControl4, nameof(viewModel.Year));
            SetPropertyControlBinding(propertyControl5, nameof(viewModel.Genre));
        }
        
        private void SetEntryValues(Song s)
        {
            foreach (PropertyControl pc in entryControls)
                pc._textBox.Width = double.NaN;

            viewModel.SetSong(s);
           // viewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.UpdateLayout();
            SetMaxWidth();
        }

        void SetPropertyControlBinding(PropertyControl propertyControl, string PropertyName)
        {
            propertyControl.propertyLabel.Content = PropertyName;
            propertyControl.textBox.Text = "";
            var binding = new Binding();
            binding.Mode = BindingMode.TwoWay;
            binding.Source = viewModel;
            binding.Path = new PropertyPath(PropertyName);
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            propertyControl.textBox.SetBinding(TextBox.TextProperty, binding);
            propertyControl.PreviewKeyUp += PropertyControl_PreviewKeyUp;
        }
       
        private void PropertyControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ShowTagOptions = true;
        }

        private void SetMaxWidth()
        {
            double _max = 0;
            foreach(PropertyControl pc in entryControls)
            {
                double _width = pc._textBox.ActualWidth;
                if (_width > _max)
                    _max = _width;
            }
            
            foreach(PropertyControl pc in entryControls)
            {
                pc._textBox.Width = _max;
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
