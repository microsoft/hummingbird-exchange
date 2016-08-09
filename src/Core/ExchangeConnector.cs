using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Hummingbird.Interfaces;
using Hummingbird.Models;
using Hummingbird.Models.Envelopes;
using Hummingbird.Properties;
using Hummingbird.ViewModels;

namespace Hummingbird.Core
{
    internal class ExchangeConnector
    {
        /// <summary>
        ///     Get the owner of an external distribution list.
        /// </summary>
        /// <param name="dl">Distribution list name (alias) without the domain.</param>
        /// <param name="credentials">User credentials for service access.</param>
        /// <param name="isValidDl">Indicates if there exists a DL with the alias.</param>
        /// <returns></returns>
        public string GetExternalDistributionListOwner(string dl, UserCredentials credentials, out bool isValidDl)
        {
            var owner = new StringBuilder();
            isValidDl = false;

            var secureString = new SecureString();
            foreach (var c in credentials.Password)
                secureString.AppendChar(c);

            var credential = new PSCredential(credentials.Username, secureString);

            var localUri = new Uri(AccountSettingsViewModel.Instance.ServerUrl);
            var serverUrl = localUri.GetLeftPart(UriPartial.Authority);

            var connectionInfo =
                new WSManConnectionInfo(new Uri(string.Concat(serverUrl, "/powershell-liveid/")),
                    "Microsoft.Exchange", credential)
                {
                    AuthenticationMechanism = AuthenticationMechanism.Basic,
                    SkipCACheck = true,
                    SkipCNCheck = true,
                    MaximumConnectionRedirectionCount = 4
                };

            try
            {
                var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();

                Collection<PSObject> rsResultsresults = null;

                using (var plPileLine = runspace.CreatePipeline())
                {
                    var getGroupCommand = new Command(AppSetup.CmdletGetDistributionGroup);
                    getGroupCommand.Parameters.Add("Identity", dl);

                    plPileLine.Commands.Add(getGroupCommand);
                    rsResultsresults = plPileLine.Invoke();

                    if (rsResultsresults.Count == 1)
                    {
                        isValidDl = true;

                        var manager = rsResultsresults.First().Properties["ManagedBy"].Value;

                        var owners = manager.ToString().Split(' ');

                        owner.Append(owners[0]);
                        owner.Append('@');

                        plPileLine.Stop();
                    }
                    else
                    {
                        LoggingViewModel.Instance.Logger.Write(string.Concat("GetExternalDistributionListOwner ",
                            Environment.NewLine,
                            "INVALID DL", Environment.NewLine, dl));
                    }
                }

                using (var plPileLine = runspace.CreatePipeline())
                {
                    var getAutoritativeDomainCommand =
                        new Command(AppSetup.CmdletGetAuthoritativeDomain);

                    plPileLine.Commands.Add(getAutoritativeDomainCommand);
                    rsResultsresults = plPileLine.Invoke();

                    var filteredDomainResults =
                        rsResultsresults.Where(cr => ((string)cr.Properties["DomainType"].Value == "Authoritative"));

                    var authoritativeDomain = filteredDomainResults.First().Properties["DomainName"].Value;

                    owner.Append(authoritativeDomain);

                    plPileLine.Stop();
                }

                runspace.Close();
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("GetExternalDistributionListOwner ",
                    Environment.NewLine,
                    exception.Message, Environment.NewLine, exception.StackTrace, Environment.NewLine, dl));
            }

            return owner.ToString();
        }

        /// <summary>
        ///     Core method for performing EWS calls against the Exchange service.
        /// </summary>
        /// <param name="credentials">User credentials.</param>
        /// <param name="exchangeUrl">URL to the EWS endpoint.</param>
        /// <param name="param">Additional parameters that work with the specific request.</param>
        /// <param name="requestType">Type of the EWS request.</param>
        /// <param name="additionalParam">List of additional parameters needed for a request.</param>
        /// <returns></returns>
        public IExchangeResponse PerformExchangeRequest(UserCredentials credentials, string exchangeUrl, string param,
            ExchangeRequestType requestType, IEnumerable<string> additionalParam = null)
        {
            var postContent = string.Empty;
            Type type = null;

            var authHeaderContent = GetBasicAuthFormat(credentials);
            var request = (HttpWebRequest) WebRequest.Create(exchangeUrl);
            request.Method = WebRequestMethods.Http.Post;
            request.Headers.Add(HttpRequestHeader.Authorization, string.Concat("Basic ", authHeaderContent));
            request.Headers.Add(HttpRequestHeader.UserAgent, "Humming Bird");
            request.ContentType = "text/xml";

            switch (requestType)
            {
                case ExchangeRequestType.ValidateAlias:
                    {
                        LoggingViewModel.Instance.Logger.Write(string.Concat("ExchangeConnector:ValidateAlias ", param));

                        postContent = Resources.AliasValidationPOST.Replace(AppSetup.GroupIdFiller, param);
                        type = typeof (AliasValidationEnvelope);
                        break;
                    }
                case ExchangeRequestType.CreateGroup:
                    {
                        LoggingViewModel.Instance.Logger.Write(string.Concat("ExchangeConnector:CreateGroup ", param));

                        postContent = Resources.GroupCreationPOST.Replace(AppSetup.GroupIdFiller, param);
                        if (additionalParam != null && additionalParam.Count() == 1)
                        {
                            postContent = postContent.Replace(AppSetup.OwnerSmtpFiller, additionalParam.First());
                        }
                        type = typeof (GroupCreationEnvelope);
                        break;
                    }
                case ExchangeRequestType.GetUnifiedGroupDetails:
                    {
                        LoggingViewModel.Instance.Logger.Write(string.Concat("ExchangeConnector:GetGroupDetails ", param));

                        postContent = Resources.GetUnifiedGroupsPOST.Replace(AppSetup.GroupSmtpFiller, param);
                        type = typeof (GetUnifiedGroupEnvelope);

                        break;
                    }
                case ExchangeRequestType.DeleteGroup:
                    {
                        postContent = Resources.DeleteGroupPOST;
                        type = null;

                        break;
                    }
                case ExchangeRequestType.AddMember:
                    {
                        LoggingViewModel.Instance.Logger.Write(string.Concat("ExchangeConnector:AddMember ", param));

                        if (additionalParam != null)
                        {
                            using (
                                var addMemberEnvelopeStream =
                                    new MemoryStream(Encoding.UTF8.GetBytes(Resources.SetUnifiedGroupMembersPOST)))
                            {
                                using (var reader = new StreamReader(addMemberEnvelopeStream))
                                {
                                    var serializer = new XmlSerializer(typeof (AddMemberEnvelope));
                                    var deserializedContent = (AddMemberEnvelope) serializer.Deserialize(reader);

                                    deserializedContent.Body.SetUnifiedGroupMembershipState.Members =
                                        new List<string>(additionalParam);
                                    deserializedContent.Body.SetUnifiedGroupMembershipState.GroupIdentity.Value = param;

                                    var ns = new XmlSerializerNamespaces();
                                    ns.Add("s", "http://schemas.xmlsoap.org/soap/envelope/");

                                    var xmlWriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true };

                                    using (var textWriter = new StringWriter())
                                    {
                                        using (var writer = XmlWriter.Create(textWriter, xmlWriterSettings))
                                        {
                                            serializer.Serialize(writer, deserializedContent, ns);

                                            postContent = textWriter.ToString();

                                            LoggingViewModel.Instance.Logger.Write(
                                                string.Concat("ExchangeConnector:AddMemberPostContent ", postContent));

                                            type = typeof (SetMemberEnvelope);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException("additionalParam",
                                Resources.ExchangeRequestAddMembersWithoutMembers);
                        }
                        break;
                    }
            }

            var processedPostContent = Encoding.UTF8.GetBytes(postContent);
            request.ContentLength = processedPostContent.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(processedPostContent, 0, processedPostContent.Length);
            }

            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var serializer = new XmlSerializer(type);

                            var translatedResponse = (IExchangeResponse) serializer.Deserialize(reader);
                            return translatedResponse;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("ExchangeConnector:Error ", exception.Message,
                    Environment.NewLine,
                    exception.StackTrace));
            }

            return null;
        }

        /// <summary>
        ///     Gets the concatenated username:password string required for the REST request.
        /// </summary>
        /// <param name="credentials">User credentials.</param>
        /// <returns></returns>
        private static string GetBasicAuthFormat(UserCredentials credentials)
        {
            return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    string.Concat(credentials.Username, ":", credentials.Password)));
        }
    }
}