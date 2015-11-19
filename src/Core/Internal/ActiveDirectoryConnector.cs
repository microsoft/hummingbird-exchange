using System;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hummingbird.ViewModels;
using Microsoft.Exchange.WebServices.Data;
using Task = System.Threading.Tasks.Task;

namespace Hummingbird.Core.Internal
{
    internal class ActiveDirectoryConnector
    {
        public ActiveDirectoryConnector()
        {
            Service = new ExchangeService(ExchangeVersion.Exchange2013);
        }

        private ObservableCollection<string> Members { get; set; }
        private ExchangeService Service { get; }

        /// <summary>
        ///     Gets the owner of a distribution list on the local Microsoft domain.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        internal async Task<string> GetDistributionListOwner(string alias)
        {
            var result = string.Empty;

            await Task.Run(() =>
            {
                try
                {
                    var context = new PrincipalContext(ContextType.Domain, AppSetup.ActiveLookupDomain);
                    LoggingViewModel.Instance.Logger.Write(string.Concat("AD:Context ", context.ConnectedServer, "\n",
                        context.Name, "\n", context.UserName));

                    var distributionList = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, alias);
                    LoggingViewModel.Instance.Logger.Write(string.Concat("AD:DL ", distributionList.DisplayName, "\n",
                        distributionList.UserPrincipalName, "\n", distributionList.Guid));

                    var entry = (DirectoryEntry) distributionList.GetUnderlyingObject();
                    var propertyValueCollection = entry.Properties["managedBy"];
                    var userIdentity = propertyValueCollection.Value.ToString();

                    var regex = new Regex(string.Concat("DC=(.*),DC=", AppSetup.ActiveDomainControllerPrefix));
                    var match = regex.Match(userIdentity);

                    var domain = AppSetup.ActiveLookupDomain;
                    if (match.Success)
                    {
                        domain = match.Groups[1].Value;
                    }
                    context = new PrincipalContext(ContextType.Domain, domain);
                    var contextualName = UserPrincipal.FindByIdentity(context, userIdentity);
                    if (contextualName != null)
                    {
                        result = contextualName.UserPrincipalName;
                    }
                }
                catch (Exception ex)
                {
                    LoggingViewModel.Instance.Logger.Write(string.Concat("AD:Failure ", ex.Message, "\n",
                        ex.StackTrace, "\n", ex.Source));
                }
            });

            return result;
        }

        /// <summary>
        ///     Returns the full list of distribution list members.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        internal async Task<ObservableCollection<string>> GetDistributionListMembers(string alias)
        {
            Members = null;

            var credManager = new CredentialManager();
            var credentials = credManager.GetUserCredentials();

            LoggingViewModel.Instance.Logger.Write(string.Concat("GetDistributionListMembers with ",
                credentials.Username));

            await Task.Run(() =>
            {
                try
                {
                    Service.Credentials = new WebCredentials(credentials.Username, credentials.Password);
                    Service.AutodiscoverUrl(credentials.Username, RedirectionUrlValidationCallback);

                    var nameResolutions = Service.ResolveName(alias, ResolveNameSearchLocation.DirectoryOnly, true);
                    var resolvedName =
                        nameResolutions.First(
                            x => x.Mailbox.Address.ToLower().StartsWith(string.Concat(alias.ToLower(), "@")));

                    LoggingViewModel.Instance.Logger.Write(string.Concat("Starting DL in query: ",
                        resolvedName.Mailbox));

                    TryExpandDistributionList(resolvedName.Mailbox);
                }
                catch (Exception ex)
                {
                    LoggingViewModel.Instance.Logger.Write(
                        string.Concat("Could not perform distribution list lookup because of ", ex.Message,
                            Environment.NewLine, ex.StackTrace));
                }
            });

            return Members;
        }

        /// <summary>
        ///     Expands the distribution list into a set of user emails.
        /// </summary>
        private void TryExpandDistributionList(EmailAddress address)
        {
            if (Members == null)
            {
                Members = new ObservableCollection<string>();
            }

            if (address.MailboxType == MailboxType.PublicGroup)
            {
                var groupResults = Service.ExpandGroup(address);

                foreach (var email in groupResults)
                {
                    LoggingViewModel.Instance.Logger.Write(string.Concat("TryExpanDistributionList ", email));
                    TryExpandDistributionList(email);
                }
            }
            else
            {
                if (!Members.Contains(address.Address))
                {
                    Members.Add(address.Address);
                }
            }
        }

        /// <summary>
        ///     Used to return the autodiscover status.
        /// </summary>
        /// <param name="redirectionUrl">Redirection URL for the call.</param>
        /// <returns></returns>
        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            var result = false;

            var redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }
    }
}