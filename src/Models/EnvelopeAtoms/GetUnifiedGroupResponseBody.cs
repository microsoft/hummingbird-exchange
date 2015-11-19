using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class GetUnifiedGroupResponseBody
    {
        [XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages")]
        public GetUnifiedGroupResponse GetUnifiedGroupMembersResponseMessage { get; set; }
    }
}
