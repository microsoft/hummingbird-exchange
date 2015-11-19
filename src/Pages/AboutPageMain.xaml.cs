using System.Diagnostics;
using System.Windows;

namespace Hummingbird.Pages
{
    /// <summary>
    /// Interaction logic for AboutPageMain.xaml
    /// </summary>
    public partial class AboutPageMain
    {
        public AboutPageMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Click handler for the button that handles developer contact.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnContact_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("mailto:dendeli@microsoft.com");
        }
    }
}
