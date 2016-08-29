using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            var openFileDialog = new OpenFileDialog { Filter = "XML DL Files (*.xmldl) | *.xmldl" };
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

                    BulkAddMembersResult[] addMembersResults = await AddMembersToGroup(credentials, TxtGroupAddress.Text,
                        DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Members.ToList());
                    DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Name = TxtGroupAddress.Text;

                    int totalMembersCount = DlGroupMigrationViewModel.Instance.BulkAddDistributionList.Members.Count;
                    int failedMemberCount = addMembersResults.Where(r => r.FailedMembers != null).Sum(r => r.FailedMembers.Count);
                    int invalidMemberCount = addMembersResults.Where(r => r.InvalidMembers != null).Sum(r => r.InvalidMembers.Count);
                    int successfulMembersCount = totalMembersCount - (invalidMemberCount + failedMemberCount);

                    if (addMembersResults.All(x => x.StatusCode.ToLower() == "noerror"))
                    {
                        string message = string.Format("The bulk add was successful. {0} members were added.", totalMembersCount);

                        ModernDialog.ShowMessage(
                            message,
                            "Hummingbird",
                            MessageBoxButton.OK);

                        TxtGroupAddress.Text = string.Empty;
                        TxtPath.Text = string.Empty;
                        DlGroupMigrationViewModel.Instance.BulkAddDistributionList = new DistributionList();
                    }
                    else
                    {
                        StringBuilder message = new StringBuilder();

                        if (successfulMembersCount > 0)
                        {
                            message.Append("The bulk add was partially successful.");
                            message.Append(string.Format("\nNumber of members added successfully: {0}.", successfulMembersCount));
                        }

                        if (failedMemberCount > 0)
                        {
                            message.Append(string.Format("\nNumber of members not added: {0}.", failedMemberCount));
                        }

                        if (invalidMemberCount > 0)
                        {
                            message.Append(string.Format("\nNumber of invalid members: {0}.", invalidMemberCount));
                        }

                        ModernDialog.ShowMessage(
                            message.ToString(),
                            "Hummingbird",
                            MessageBoxButton.OK);

                        var fsOperator = new FileSystemOperator();
                        AddMembersErrorDetails error = new AddMembersErrorDetails
                        {
                            FailedMembers = new List<string>(),
                            InvalidMembers = new List<string>()
                        };
                        foreach (var list in addMembersResults)
                        {
                            if (list.FailedMembers != null) { error.FailedMembers.AddRange(list.FailedMembers); }
                            if (list.InvalidMembers != null) { error.InvalidMembers.AddRange(list.InvalidMembers); }
                        }

                        var filePath = fsOperator.StoreDistributionListFailures(DlGroupMigrationViewModel.Instance.BulkAddDistributionList, "BulkAdd", error);

                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            var result =
                                ModernDialog.ShowMessage(
                                    "A list of failures was created. Do you want to open File Explorer to find its location?",
                                    "Hummingbird", MessageBoxButton.YesNo);

                            if (result == MessageBoxResult.Yes)
                            {
                                Process.Start("explorer.exe", string.Concat("/select, ", filePath));
                            }
                        }

                    }

                }
                catch (Exception)
                {
                    ModernDialog.ShowMessage(
                        "Members couldn't be added from the backup file. The alias may be invalid or there might be a problem connecting to the server. Please try again later.",
                        "Hummingbird",
                        MessageBoxButton.OK);
                }
            }
            else
            {
                ModernDialog.ShowMessage(
                    "Make sure you've added both the DL backup file name and the group address (SMTP address).",
                    "Hummingbird", MessageBoxButton.OK);
            }

            DlGroupMigrationViewModel.Instance.BulkAddControlsEnabled = true;
        }

        private static async Task<BulkAddMembersResult[]> AddMembersToGroup(UserCredentials credentials, string alias,
            List<string> members)
        {
            var tasks = new List<Task<BulkAddMembersResult>>();
            var results = new List<BulkAddMembersResult>();

            for (int i = 0; i < members.Count; i += AddMembersBatchSize)
            {
                IEnumerable<string> membersForBatch = members.Skip(i).Take(AddMembersBatchSize);
                var task = Task.Run(() => AddMembersToGroupSingleBatch(credentials, alias, membersForBatch));
                results.Add(await task);
            }

            return results.ToArray();
        }

        private static BulkAddMembersResult AddMembersToGroupSingleBatch(UserCredentials credentials, string alias,
            IEnumerable<string> members)
        {
            var memberList = members.ToList();
            var connector = new ExchangeConnector();

            IExchangeResponse result = connector.PerformExchangeRequest(credentials,
                AccountSettingsViewModel.Instance.ServerUrl, alias, ExchangeRequestType.AddMember, memberList);

            var addMemberResult = new BulkAddMembersResult
            {
                StatusCode = result != null
                    ? ((SetMemberEnvelope)result).Body.SetUnifiedGroupMembershipResponseMessage.ResponseCode
                    : string.Empty,

                MemberCount = memberList.Count
            };

            if (addMemberResult.StatusCode.ToLower() != "noerror" && addMemberResult.StatusCode.ToLower() != "errorinvalidid")
            {
                addMemberResult.FailedMembers = memberList;
            }
            else if (result != null)
            {
                addMemberResult.FailedMembers = ((SetMemberEnvelope)result).Body.SetUnifiedGroupMembershipResponseMessage.FailedMembers;

                addMemberResult.InvalidMembers = ((SetMemberEnvelope)result).Body.SetUnifiedGroupMembershipResponseMessage.InvalidMembers;
            }

            return addMemberResult;
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
            public List<string> FailedMembers { get; set; }
            public List<string> InvalidMembers { get; set; }
        }
    }
}