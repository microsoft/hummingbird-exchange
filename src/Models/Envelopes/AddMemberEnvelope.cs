using Hummingbird.Interfaces;
using System.Xml.Serialization;
using Hummingbird.Models.EnvelopeAtoms;

namespace Hummingbird.Models.Envelopes
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false, ElementName = "Envelope")]
    public class AddMemberEnvelope
    {
        public RequestHeader Header { get; set; }
        public UnifiedGroupMembershipStateBody Body { get; set; }
    }
}
