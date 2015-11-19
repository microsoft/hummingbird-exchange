using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    public class UnifiedGroupAliasResponseBody
    {
        [XmlElement("ValidateUnifiedGroupAliasResponseMessage", Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages")]
        public UnifiedGroupAliasResponse Response { get; set; }
    }
}
