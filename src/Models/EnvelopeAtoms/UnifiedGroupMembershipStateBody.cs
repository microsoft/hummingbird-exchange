using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class UnifiedGroupMembershipStateBody
    {
        [XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages")]
        public UnifiedGroupMembershipState SetUnifiedGroupMembershipState { get; set; }
    }
}
