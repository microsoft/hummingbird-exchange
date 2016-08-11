namespace Hummingbird.Core
{
    public static class AppSetup
    {
        // Active Directory lookup settings
        // While publishing an EXE for use by Microsoft, this value has to be ActiveLookupDomain:REDMOND ActiveDomainControllerPrefix:corp
        public const string ActiveLookupDomain = "CHANGE_DOMAIN";
        public const string ActiveDomainControllerPrefix = "CHANGE_CONTROLLER_PREFIX";

        // Settings location
        public const string CredentialContainerName = "container";
        public const string SettingsContainerName = "Settings";
        public const string SettingsFileName = "hbset.xml";

        // Group-related strings used to replace content in pre-built
        // request files.
        public const string GroupIdFiller = "##GROUPID##";
        public const string GroupSmtpFiller = "##GROUP_SMTP_ADDRESS##";
        public const string OwnerSmtpFiller = "##OWNER_SMTP_ADDRESS##";

        // Commandlets used to execute queries against a connected O365 PS session.
        public const string CmdletGetDistributionGroup = "Get-DistributionGroup";
        public static string CmdletGetAuthoritativeDomain = "Get-AcceptedDomain";
    }
}