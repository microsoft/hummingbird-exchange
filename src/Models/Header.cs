using System.Xml.Serialization;

namespace Hummingbird.Models
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Header
    {
        [XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public ServerVersionInfo ServerVersionInfo { get; set; }
    }
}
