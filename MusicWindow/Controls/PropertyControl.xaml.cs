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
    /// Interaction logic for PropertyControl.xaml
    /// </summary>
    public partial class PropertyControl : UserControl
    {
        public PropertyControl()
        {
            InitializeComponent();
        }
        
        public void SetEntry(string entryName,string entryValue)
        {
            propertyLabel.Content = entryName;
            textBox.Text = entryValue;
        }

        public void SetLabel(string LabelName)
        {
            propertyLabel.Content = LabelName;
        }

        public void SetEntryValue(string entryValue)
        {
            textBox.Text = entryValue;
        }

        public string GetLabel()
        {
            return propertyLabel.Content.ToString();
        }

        public string GetValue()
        {
            return textBox.Text;
        }
    }
}
