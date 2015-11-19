using System.Xml.Serialization;

namespace Hummingbird.Models
{
    [XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    [XmlRootAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types", IsNullable = false)]
    public class ServerVersionInfo
    {
        /// <remarks/>
        [XmlAttributeAttribute()]
        public int MajorVersion { get; set; }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public int MinorVersion { get; set; }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public int MajorBuildNumber { get; set; }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public int MinorBuildNumber { get; set; }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Version { get; set; }
    }
}
