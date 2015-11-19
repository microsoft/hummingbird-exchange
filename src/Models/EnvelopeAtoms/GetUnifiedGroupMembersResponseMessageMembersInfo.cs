using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    public class GetUnifiedGroupMembersResponseMessageMembersInfo
    {
        [XmlElement(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        public int TotalCount { get; set; }

        /// <remarks/>
        [XmlArrayAttribute(Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
        [XmlArrayItemAttribute("Member", IsNullable = false)]
        public Member[] Members { get; set; }
    }
}
