using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using Hummingbird.Core;
using Hummingbird.Core.Common;
using Hummingbird.Interfaces;
using Hummingbird.Models;
using Hummingbird.Models.Envelopes;
using Hummingbird.ViewModels;
using Microsoft.Win32;

namespace Hummingbird.Pages
{
    /// <summary>
    ///     Interaction logic for BulkAddMain.xaml
    /// </summary>
    public partial class BulkAddMain : UserControl
    {
        public BulkAddMain()
        {
            InitializeComponent();
        }

        private void BtnOpenDlFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "XML DL Files (*.xmldl) | *.xmldl"};
            var result = openFileDialog.ShowDialog();

            if (result != true) return;

            var path = openFileDialog.FileName;
            TxtPath.Text = path;

            var fileOperator = new FileSystemOperator();

            DlGroupMigrationViewModel.Instance.BulkAddDistributionList =
                fileOperator.GetDistributionListInformation(path);
            DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Members.Add(
                DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Owner);
        }

        private async void BtnAddMembers_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtPath.Text) && !string.IsNullOrWhiteSpace(TxtGroupAddress.Text))
            {
                DlGroupMigrationViewModel.Instance.BulkAddControlsEnabled = false;

                try
                {
                    var credManager = new CredentialManager();
                    var credentials = credManager.GetUserCredentials();

                    var membersAdded = await AddMembersToGroup(credentials, TxtGroupAddress.Text,
                        DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Members.ToList());

                    if (membersAdded.ToLower() == "noerror")
                    {
                        ModernDialog.ShowMessage("Bulk add complete!", "Hummingbird",
                            MessageBoxButton.OK);

                        TxtGroupAddress.Text = string.Empty;
                        TxtPath.Text = string.Empty;
                        DlGroupMigrationViewModel.Instance.BulkAddDistributionList = new DistributionList();
                    }
                }
                catch (Exception)
                {
                    ModernDialog.ShowMessage(
                        "An error occured and we couldn't complete the request. Please contact the developer.",
                        "Hummingbird",
                        MessageBoxButton.OK);
                }
            }
            else
            {
                ModernDialog.ShowMessage(
                    "Missing key pieces of information. Check that you have both the members and group address entered.",
                    "Hummingbird", MessageBoxButton.OK);
            }

            DlGroupMigrationViewModel.Instance.BulkAddControlsEnabled = true;
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

        private async void BtnPerformGroupGrab_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtBackupGroupBox.Text))
            {
                DlGroupMigrationViewModel.Instance.BulkAddControlsEnabled = false;

                try
                {
                    var credManager = new CredentialManager();
                    var credentials = credManager.GetUserCredentials();

                    var gugResults = await GetGroupInformation(credentials, TxtBackupGroupBox.Text);

                    GetUnifiedGroupEnvelope gugEnvelope = (GetUnifiedGroupEnvelope) gugResults;

                    GroupSimplifier simplifier = new GroupSimplifier();
                    var dl =
                        simplifier.SimplifyGroup(gugEnvelope.Body.GetUnifiedGroupMembersResponseMessage.MembersInfo.Members,
                            TxtBackupGroupBox.Text);

                    var fsOperator = new FileSystemOperator();
                    var filePath = fsOperator.StoreDistributionListInformation(dl);

                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        var result =
                            ModernDialog.ShowMessage(
                                "Group backup created. Open Windows Explorer for its location?",
                                "Hummingbird", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start("explorer.exe", string.Concat("/select, ", filePath));
                        }
                    }
                    else
                    {
                        ModernDialog.ShowMessage(
                            "We couldn't create the backup file for this group.", "Hummingbird",
                            MessageBoxButton.OK);
                    }
                    //if (membersAdded.ToLower() == "noerror")
                    //{
                    //    ModernDialog.ShowMessage("Bulk add complete!", "Hummingbird",
                    //        MessageBoxButton.OK);

                    //    TxtGroupAddress.Text = string.Empty;
                    //    TxtPath.Text = string.Empty;
                    //    DlGroupMigrationViewModel.Instance.BulkAddDistributionList = new DistributionList();
                    //}
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(
                        "An error occured and we couldn't complete the request. Please contact the developer.",
                        "Hummingbird",
                        MessageBoxButton.OK);
                }
            }
            else
            {
                ModernDialog.ShowMessage(
                    "Missing key pieces of information. Check that you have both the members and group address entered.",
                    "Hummingbird", MessageBoxButton.OK);
            }

            DlGroupMigrationViewModel.Instance.BulkAddControlsEnabled = true;
        }

        private async Task<object> GetGroupInformation(UserCredentials credentials, string groupQualifiedName)
        {
            IExchangeResponse result = null;

            await Task.Run(() =>
            {
                var connector = new ExchangeConnector();

                result = connector.PerformExchangeRequest(credentials,
                    AccountSettingsViewModel.Instance.ServerUrl, groupQualifiedName, ExchangeRequestType.GetUnifiedGroupDetails);
            });

            return (GetUnifiedGroupEnvelope) result;
        }
    }
}