using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    [XmlRootAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types", IsNullable = false)]
    public class GroupIdentity
    {
        /// <remarks/>
        public string Type { get; set; }

        /// <remarks/>
        public string Value { get; set; }
    }
}
