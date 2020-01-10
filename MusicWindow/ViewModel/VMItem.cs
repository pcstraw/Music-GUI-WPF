using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Glaxion.ViewModel
{
    public class VMItem : INotifyPropertyChanged
    {
        public VMItem()
        {

        }

        public object Data;

        int _colourIndex;
        public int ColourIndex
        {
            get { return _colourIndex; }
            set { _colourIndex = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
