using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    public class MemberPersonaEmail
    {
        public string Name { get; set; }

        /// <remarks/>
        public string EmailAddress { get; set; }

        /// <remarks/>
        public string RoutingType { get; set; }

        /// <remarks/>
        public string MailboxType { get; set; }
    }
}
