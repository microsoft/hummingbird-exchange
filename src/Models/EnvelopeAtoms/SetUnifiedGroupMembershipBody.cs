using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class SetUnifiedGroupMembershipBody
    {
        [XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages", ElementName = "SetUnifiedGroupMembershipStateResponseMessage")]
        public SetUnifiedGroupMembership SetUnifiedGroupMembershipResponseMessage { get; set; }
    }
}
