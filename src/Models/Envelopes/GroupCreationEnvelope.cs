using System.Xml.Serialization;
using Hummingbird.Interfaces;
using Hummingbird.Models.EnvelopeAtoms;

namespace Hummingbird.Models.Envelopes
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false, ElementName = "Envelope")]
    public class GroupCreationEnvelope : IExchangeResponse
    {

        public Header Header { get; set; }

        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public GroupCreationResponseBody Body { get; set; }
    }
}
