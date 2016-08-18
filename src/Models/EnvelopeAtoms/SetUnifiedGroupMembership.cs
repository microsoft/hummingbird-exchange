using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages")]
    [XmlRootAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/messages", IsNullable = false)]
    public class SetUnifiedGroupMembership
    {
        public string ResponseCode { get; set; }

        [XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public string GroupActionResultType { get; set; }

        [XmlAttributeAttribute()]
        public string ResponseClass { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("Member", IsNullable = true)]
        public List<string> InvalidMembers { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("Member", IsNullable = true)]
        public List<string> FailedMembers { get; set; }
    }
}