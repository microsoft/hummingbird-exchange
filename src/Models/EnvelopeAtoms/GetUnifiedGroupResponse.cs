using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    public class GetUnifiedGroupResponse
    {
        public string ResponseCode { get; set; }

        /// <remarks/>
        public GetUnifiedGroupMembersResponseMessageMembersInfo MembersInfo { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string ResponseClass { get; set; }
    }
}
