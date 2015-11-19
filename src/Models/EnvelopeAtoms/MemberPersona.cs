using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Hummingbird.Models.EnvelopeAtoms
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.microsoft.com/exchange/services/2006/types")]
    public class MemberPersona
    {
        public MemberPersonaId PersonaId { get; set; }

        /// <remarks/>
        public string DisplayName { get; set; }

        /// <remarks/>
        public string Title { get; set; }

        /// <remarks/>
        public MemberPersonaEmail EmailAddress { get; set; }

        /// <remarks/>
        public string ImAddress { get; set; }

        /// <remarks/>
        public long RelevanceScore { get; set; }
    }
}
