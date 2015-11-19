using System.Xml.Serialization;

namespace Hummingbird.Models
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    [XmlRootAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types", IsNullable = false, ElementName = "RequestServerVersion")]
    public class RequestServerVersion
    {
        [XmlAttributeAttribute()]
        public string Version { get; set; }
    }
}
