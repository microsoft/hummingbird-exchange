using FirstFloor.ModernUI.Windows.Controls;
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
using FirstFloor.ModernUI.Windows.Media;
using Hummingbird.ViewModels;

namespace Hummingbird.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewMembersDialog.xaml
    /// </summary>
    public partial class ViewMembersDialog : ModernDialog
    {
        public ViewMembersDialog()
        {
            InitializeComponent();

            // define the dialog buttons

            Button removeButton = new Button() {Content = "remove"};
            removeButton.Click += removeButton_Click;
            this.Buttons = new Button[] { removeButton, this.CloseButton };
        }

        /// <summary>
        /// Click handler for the button that removes a member from the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoreGrid.SelectedItem != null)
            {
                DlGroupMigrationViewModel.Instance.CurrentlyActiveDl.Members.Remove(CoreGrid.SelectedItem.ToString());
            }
        }
    }
}
