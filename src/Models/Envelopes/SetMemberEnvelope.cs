using Hummingbird.Interfaces;
using Hummingbird.Models.EnvelopeAtoms;
using System.Xml.Serialization;

namespace Hummingbird.Models.Envelopes
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false, ElementName = "Envelope")]
    public class SetMemberEnvelope:IExchangeResponse
    {
        public Header Header { get; set; }
        public SetUnifiedGroupMembershipBody Body { get; set; }
    }
}
