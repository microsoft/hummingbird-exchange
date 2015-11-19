using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hummingbird.Models.EnvelopeAtoms
{
    public class UnifiedGroupMembershipState
    {
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public GroupIdentity GroupIdentity { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Member", IsNullable = false)]
        public List<string> Members { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public string Action { get; set; }
    }
}
