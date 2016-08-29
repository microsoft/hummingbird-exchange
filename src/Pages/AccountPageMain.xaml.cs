using System;
using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using Hummingbird.Core;
using Hummingbird.ViewModels;

namespace Hummingbird.Pages
{
    /// <summary>
    ///     Interaction logic for AccountPageMain.xaml
    /// </summary>
    public partial class AccountPageMain
    {
        public AccountPageMain()
        {
            InitializeComponent();
            AccountSettingsViewModel.Instance.ServerUrl = @"https://outlook.office365.com/EWS/Exchange.asmx";
        }

        private void btnSaveCredentials_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtUsername.Text) && !string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                var credManager = new CredentialManager();
                var credentialsStored = credManager.StoreUserCredentials(TxtUsername.Text, TxtPassword.Password);

                ModernDialog.ShowMessage(
                    !credentialsStored
                        ? "We couldn't store your credentials at this time."
                        : "Credentials stored successfully.", "Hummingbird", MessageBoxButton.OK);
            }
            else
            {
                ModernDialog.ShowMessage("Both the username and the password are required.", "Hummingbird",
                    MessageBoxButton.OK);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            var credManager = new CredentialManager();
            var credentials = credManager.GetUserCredentials();

            if (credentials != null)
            {
                TxtUsername.Text = credentials.Username;
                TxtPassword.Password = credentials.Password;
            }

            base.OnInitialized(e);
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(AccountSettingsViewModel.Instance.ServerUrl))
            {
                try
                {
                    var settingsManager = new SettingsManager();
                    settingsManager.StoreUserSettings(AccountSettingsViewModel.Instance);

                    ModernDialog.ShowMessage("Settings successfully saved.", "Hummingbird", MessageBoxButton.OK);
                }
                catch
                {
                    ModernDialog.ShowMessage("Settings could not be saved at this time.", "Hummingbird",
                        MessageBoxButton.OK);
                }
            }
            else
            {
                ModernDialog.ShowMessage("Server URL cannot be empty.", "Hummingbird", MessageBoxButton.OK);
            }
        }
    }
}