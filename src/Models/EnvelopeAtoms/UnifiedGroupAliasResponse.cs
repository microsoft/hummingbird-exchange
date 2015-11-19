using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    public class UnifiedGroupAliasResponse
    {
        [XmlElement("ResponseCode")]
        public string ResponseCode { get; set; }
        [XmlElement("IsAliasUnique")]
        public bool IsAliasUnique { get; set; }
        [XmlElement("Alias")]
        public string Alias { get; set; }
        [XmlElement("SmtpAddress")]
        public string SmtpAddress { get; set; }
        [XmlAttributeAttribute("ResponseClass")]
        public string ResponseClass { get; set; }
    }
}
