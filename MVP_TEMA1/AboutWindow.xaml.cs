using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;

namespace MVP_TEMA1
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {            
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
