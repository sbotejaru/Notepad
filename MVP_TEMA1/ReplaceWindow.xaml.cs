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

namespace MVP_TEMA1
{
    /// <summary>
    /// Interaction logic for ReplaceWindow.xaml
    /// </summary>
    public partial class ReplaceWindow : Window
    {
        public ReplaceWindow(MainVM mvvm)
        {
            InitializeComponent();
            DataContext = mvvm;
        }
    }
}
