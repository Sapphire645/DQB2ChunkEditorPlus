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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DQB2ChunkEditor.Windows
{
    public partial class MassSelect : Window
    {
        public string BegC => BCName.Text;
        public string EndC => ECName.Text;
        public string BegY => BYName.Text;
        public string EndY => EYName.Text;
        public MassSelect()
        {
            InitializeComponent();
        }
        private void OkButton_OnClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(ButtonOk != null)
            {
                if (!ushort.TryParse(BegC, out var BEGC) || !ushort.TryParse(EndC, out var ENDC) || !ushort.TryParse(BegY, out var BEGY) || !ushort.TryParse(EndY, out var ENDY))
                {
                    ButtonOk.Visibility = Visibility.Collapsed;
                }
                else
                { ButtonOk.Visibility = Visibility.Visible; }
            }

        }
    }
}
