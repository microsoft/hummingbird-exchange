using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages")]
    [XmlRootAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages", IsNullable = false)]
    public class GroupCreationResponse
    {
        public string ResponseCode { get; set; }

        [XmlElement(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public GroupIdentity GroupIdentity { get; set; }

        [XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public string ErrorCode { get; set; }

        public string LegacyDN { get; set; }

        public string MailboxDatabase { get; set; }

        [XmlAttributeAttribute()]
        public string ResponseClass { get; set; }
    }
}
