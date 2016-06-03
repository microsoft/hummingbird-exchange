using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;
using Hummingbird.Core;
using Hummingbird.Core.Common;
using Hummingbird.Core.Internal;
using Hummingbird.Dialogs;
using Hummingbird.Interfaces;
using Hummingbird.Models;
using Hummingbird.Models.Envelopes;
using Hummingbird.ViewModels;
using Microsoft.Win32;

namespace Hummingbird.Pages
{
    /// <summary>
    ///     Interaction logic for DLGroupMigrationMain.xaml
    /// </summary>
    public partial class DlGroupMigrationMain
    {
        private readonly DispatcherTimer _timer;

        public DlGroupMigrationMain()
        {
            InitializeComponent();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += _timer_Tick;
        }

        private async void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            DlGroupMigrationViewModel.Instance.IsAliasValid = false;

            if (string.IsNullOrWhiteSpace(TxtGroupAlias.Text)) return;

            var credManager = new CredentialManager();
            var credentials = credManager.GetUserCredentials();

            if (credentials != null)
            {
                var aliasIsValid = await ValidateAlias(credentials, TxtGroupAlias.Text);

                if (aliasIsValid)
                {
                    DlGroupMigrationViewModel.Instance.IsAliasValid = true;
                }
            }
            else
            {
                ModernDialog.ShowMessage(
                    "There are no user credentials provided. Open SETTINGS and add your credentials.", "Hummingbird",
                    MessageBoxButton.OK);
            }
        }

        private async void btnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtDlAlias.Text))
            {
                var credManager = new CredentialManager();
                var userCredentials = credManager.GetUserCredentials();

                if (userCredentials != null)
                {
                    // Disabling pieces of UI that might interfere.
                    DlGroupMigrationViewModel.Instance.BackupControlsEnabled = false;
                    DlGroupMigrationViewModel.Instance.RestoreControlsEnabled = false;

                    TxtBackupStatus.Text = "trying to identify alias...";

                    var adConnector = new ActiveDirectoryConnector();
                    var dl = new DistributionList {Name = TxtDlAlias.Text};
                    var owner = string.Empty;

                    if (!AccountSettingsViewModel.Instance.IsInternal)
                    {
                        // The user is external.
                        var connector = new ExchangeConnector();
                        owner = connector.GetExternalDistributionListOwner(TxtDlAlias.Text, userCredentials);
                    }
                    else
                    {
                        // The user is internal
                        owner = await adConnector.GetDistributionListOwner(TxtDlAlias.Text);
                    }

                    bool resolveMembers = !string.IsNullOrWhiteSpace(owner);
                    if (!resolveMembers)
                    {
                        var result =
                            ModernDialog.ShowMessage(
                                "The group owner could not be found. Would you like to attempt to retrieve members anyway? The backup will be missing the Owner property.",
                                "Hummingbird", MessageBoxButton.YesNo);

                        resolveMembers = result == MessageBoxResult.Yes;
                    }

                    if (resolveMembers)
                    {
                        dl.Owner = owner;
                        
                        TxtBackupStatus.Text = "getting members...";
                        
                        var members = await adConnector.GetDistributionListMembers(TxtDlAlias.Text);
                        if (members != null)
                        {
                            dl.Members = members;

                            var fsOperator = new FileSystemOperator();
                            var filePath = fsOperator.StoreDistributionListInformation(dl);

                            if (!string.IsNullOrWhiteSpace(filePath))
                            {
                                var result =
                                    ModernDialog.ShowMessage(
                                        "Distribution list backup created. Open Windows Explorer for its location?",
                                        "Hummingbird", MessageBoxButton.YesNo);

                                if (result == MessageBoxResult.Yes)
                                {
                                    Process.Start("explorer.exe", string.Concat("/select, ", filePath));
                                }
                            }
                            else
                            {
                                ModernDialog.ShowMessage(
                                    "We couldn't create the backup file for this distribution list.", "Hummingbird",
                                    MessageBoxButton.OK);
                            }
                        }
                        else
                        {
                            ModernDialog.ShowMessage(
                                "We couldn't get distribution list members. Check your credentials.", "Hummingbird",
                                MessageBoxButton.OK);
                        }
                    }

                    // Re-enable the pieces of UI that might interfere.
                    DlGroupMigrationViewModel.Instance.BackupControlsEnabled = true;
                    DlGroupMigrationViewModel.Instance.RestoreControlsEnabled = true;
                }
                else
                {
                    ModernDialog.ShowMessage(
                        "There are no user credentials provided. Open SETTINGS and add your credentials.", "Hummingbird",
                        MessageBoxButton.OK);
                }
            }
            else
            {
                ModernDialog.ShowMessage("Alias cannot be empty!", "Hummingbird", MessageBoxButton.OK);
            }
        }

        private async void BtnCreateGroup_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtGroupAlias.Text))
            {
                if (DlGroupMigrationViewModel.Instance.IsAliasValid)
                {
                    var credManager = new CredentialManager();
                    var credentials = credManager.GetUserCredentials();

                    if (credentials != null)
                    {
                        if (DlGroupMigrationViewModel.Instance.CurrentlyActiveDl == null ||
                            DlGroupMigrationViewModel.Instance.CurrentlyActiveDl.Members.Count == 0)
                        {
                            var dialogResult =
                                ModernDialog.ShowMessage("This will just create a group with no members. Proceed?",
                                    "Hummingbird", MessageBoxButton.YesNo);

                            if (dialogResult == MessageBoxResult.Yes)
                            {
                                DlGroupMigrationViewModel.Instance.BackupControlsEnabled = false;
                                DlGroupMigrationViewModel.Instance.RestoreControlsEnabled = false;
                                TxtBackupStatus.Text = "creating group...";

                                var groupCreated = await CreateGroup(credentials, TxtGroupAlias.Text);

                                if (!string.IsNullOrWhiteSpace(groupCreated))
                                {
                                    ModernDialog.ShowMessage("Group creation complete!", "Hummingbird",
                                        MessageBoxButton.OK);
                                }

                                DlGroupMigrationViewModel.Instance.BackupControlsEnabled = true;
                                DlGroupMigrationViewModel.Instance.RestoreControlsEnabled = true;
                            }
                        }
                        else
                        {
                            DlGroupMigrationViewModel.Instance.BackupControlsEnabled = false;
                            DlGroupMigrationViewModel.Instance.RestoreControlsEnabled = false;
                            TxtBackupStatus.Text = "creating group...";

                            var groupCreated = await CreateGroup(credentials, TxtGroupAlias.Text);

                            if (!string.IsNullOrEmpty(groupCreated))
                            {
                                TxtBackupStatus.Text = "adding members...";

                                var membersAdded = await AddMembersToGroup(credentials, groupCreated,
                                    DlGroupMigrationViewModel.Instance.CurrentlyActiveDl.Members.ToList());

                                if (membersAdded.ToLower() == "noerror")
                                {
                                    ModernDialog.ShowMessage("Group creation complete!", "Hummingbird",
                                        MessageBoxButton.OK);
                                }
                            }

                            DlGroupMigrationViewModel.Instance.BackupControlsEnabled = true;
                            DlGroupMigrationViewModel.Instance.RestoreControlsEnabled = true;
                        }
                    }
                    else
                    {
                        ModernDialog.ShowMessage("Looks like you don't have any credentials associated.", "Hummingbird",
                            MessageBoxButton.OK);
                    }
                }
                else
                {
                    ModernDialog.ShowMessage("Looks like an invalid alias or we can't reach the server at this time.",
                        "Hummingbird", MessageBoxButton.OK);
                }
            }
            else
            {
                ModernDialog.ShowMessage("Alias cannot be empty!", "Hummingbird", MessageBoxButton.OK);
            }

            DlGroupMigrationViewModel.Instance.CurrentlyActiveDl = null;
            DlGroupMigrationViewModel.Instance.IsAliasValid = false;
            TxtPath.Text = string.Empty;
        }

        private static async Task<string> CreateGroup(UserCredentials credentials, string alias)
        {
            IExchangeResponse result = null;

            await Task.Run(() =>
            {
                var connector = new ExchangeConnector();

                result = connector.PerformExchangeRequest(credentials,
                    AccountSettingsViewModel.Instance.ServerUrl, alias, ExchangeRequestType.CreateGroup);
            });


            if (result != null)
            {
                if (
                    ((GroupCreationEnvelope) result).Body.CreateUnifiedGroupResponseMessage.GroupIdentity.Type.ToLower() ==
                    "smtpaddress")
                {
                    return ((GroupCreationEnvelope) result).Body.CreateUnifiedGroupResponseMessage.GroupIdentity.Value;
                }
                return string.Empty;
            }
            return string.Empty;
        }

        private static async Task<string> AddMembersToGroup(UserCredentials credentials, string alias,
            List<string> members)
        {
            IExchangeResponse result = null;

            await Task.Run(() =>
            {
                var connector = new ExchangeConnector();

                result = connector.PerformExchangeRequest(credentials,
                    AccountSettingsViewModel.Instance.ServerUrl, alias, ExchangeRequestType.AddMember, members);
            });

            return result != null
                ? ((SetMemberEnvelope) result).Body.SetUnifiedGroupMembershipResponseMessage.ResponseCode
                : string.Empty;
        }

        private void TxtGroupAlias_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        private static async Task<bool> ValidateAlias(UserCredentials credentials, string alias)
        {
            IExchangeResponse result = null;

            await Task.Run(() =>
            {
                var connector = new ExchangeConnector();

                result = connector.PerformExchangeRequest(credentials,
                    AccountSettingsViewModel.Instance.ServerUrl, alias, ExchangeRequestType.ValidateAlias);
            });

            return result != null && ((AliasValidationEnvelope) result).Body.Response.IsAliasUnique;
        }

        private void BtnViewMembers_OnClick(object sender, RoutedEventArgs e)
        {
            var viewMembersDialog = new ViewMembersDialog();
            viewMembersDialog.ShowDialog();
        }

        private void BtnOpenDlFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "XML DL Files (*.xmldl) | *.xmldl"};
            var result = openFileDialog.ShowDialog();

            if (result != true) return;

            var path = openFileDialog.FileName;
            TxtPath.Text = path;

            var fileOperator = new FileSystemOperator();

            DlGroupMigrationViewModel.Instance.CurrentlyActiveDl = fileOperator.GetDistributionListInformation(path);
            DlGroupMigrationViewModel.Instance.CurrentlyActiveDl.Members.Add(
                DlGroupMigrationViewModel.Instance.CurrentlyActiveDl.Owner);
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            var credManager = new CredentialManager();
            var creds = credManager.GetUserCredentials();

            IExchangeResponse result = null;

            await Task.Run(() =>
            {
                var connector = new ExchangeConnector();

                result = connector.PerformExchangeRequest(creds,
                    AccountSettingsViewModel.Instance.ServerUrl, string.Empty, ExchangeRequestType.DeleteGroup, null);
            });
        }
    }
}