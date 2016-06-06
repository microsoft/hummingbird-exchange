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
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Win32;

namespace Hummingbird.Pages
{
    /// <summary>
    ///     Interaction logic for BulkAddMain.xaml
    /// </summary>
    public partial class BulkAddMain : UserControl
    {
        /// <summary>
        /// The maximum number of members to add in a single call to SetUnifiedGroupMembershipState.
        /// In testing, we found the server to *appear* to succeed but without actually adding any members if we used
        /// a larger number. Your mileage may vary.
        /// </summary>
        private const int AddMembersBatchSize = 225;

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

            // DL Owner is not required to also be a member of the DL, but group owners should also be members.
            string owner = DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Owner;
            if (!string.IsNullOrWhiteSpace(owner))
            {
                DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Members.Add(owner);
            }
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

                    Func<BulkAddMembersResult, bool> isResultSuccessful = r => r.StatusCode.Equals("noerror", StringComparison.InvariantCultureIgnoreCase);

                    int successfulMemberCount = membersAdded.Where(r => isResultSuccessful(r))
                                                            .Sum(r => r.MemberCount);
                    int failedMemberCount = membersAdded.Where(r => !isResultSuccessful(r))
                                                        .Sum(r => r.MemberCount);

                    if (failedMemberCount == 0)
                    {
                        string message = string.Format("Bulk add complete! Added {0} members.", successfulMemberCount);

                        ModernDialog.ShowMessage(
                            message,
                            "Hummingbird",
                            MessageBoxButton.OK);

                        TxtGroupAddress.Text = string.Empty;
                        TxtPath.Text = string.Empty;
                        DlGroupMigrationViewModel.Instance.BulkAddDistributionList = new DistributionList();
                    }
                    else if (successfulMemberCount == 0)
                    {
                        ModernDialog.ShowMessage(
                            "An error occurred and we couldn't complete the request. Please contact the developer.",
                            "Hummingbird",
                            MessageBoxButton.OK);
                    }
                    else // failed > 0 and successful > 0
                    {
                        var errorCodes = membersAdded.Where(r => !isResultSuccessful(r))
                            .Select(r => r.StatusCode)
                            .Distinct()
                            .JoinStrings(",");

                        string message = string.Format(
                            "Bulk add was partially successful. We added {0} members but {1} failed due to an error. Error codes: {2}",
                            successfulMemberCount,
                            failedMemberCount,
                            errorCodes);

                        ModernDialog.ShowMessage(
                            message,
                            "Hummingbird",
                            MessageBoxButton.OK);
                    }

                }
                catch (Exception)
                {
                    ModernDialog.ShowMessage(
                        "An error occurred and we couldn't complete the request. Please contact the developer.",
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

        private static async Task<BulkAddMembersResult[]> AddMembersToGroup(UserCredentials credentials, string alias,
            List<string> members)
        {
            var tasks = new List<Task<BulkAddMembersResult>>();

            for (int i = 0; i < members.Count; i += AddMembersBatchSize)
            {
                IEnumerable<string> membersForBatch = members.Skip(i).Take(AddMembersBatchSize);
                tasks.Add(Task.Run(() => AddMembersToGroupSingleBatch(credentials, alias, membersForBatch)));
            }
            
            return await Task.WhenAll(tasks.ToArray());
        }
        
        private static BulkAddMembersResult AddMembersToGroupSingleBatch(UserCredentials credentials, string alias,
            IEnumerable<string> members)
        {
            var memberList = members.ToList();
            var connector = new ExchangeConnector();

            IExchangeResponse result = connector.PerformExchangeRequest(credentials,
                AccountSettingsViewModel.Instance.ServerUrl, alias, ExchangeRequestType.AddMember, memberList);

            return new BulkAddMembersResult
            {
                StatusCode = result != null
                    ? ((SetMemberEnvelope) result).Body.SetUnifiedGroupMembershipResponseMessage.ResponseCode
                    : string.Empty,

                MemberCount = memberList.Count
            };
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

        private class BulkAddMembersResult
        {
            public string StatusCode { get; set; }
            public int MemberCount { get; set; }
        }
    }
}