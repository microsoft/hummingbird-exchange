using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    public class MemberPersonaId
    {
        [XmlAttribute()]
        public string Id { get; set; }
    }
}
