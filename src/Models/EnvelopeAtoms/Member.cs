using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    public class Member
    {
        public MemberPersona Persona { get; set; }

        /// <remarks/>
        public bool IsOwner { get; set; }
    }
}
