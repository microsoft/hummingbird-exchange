using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hummingbird.Interfaces;

namespace Hummingbird.Models.Envelopes
{
    internal class EnteredMemberEnvelope : IExchangeResponse
    {
        public Header Header { get; set; }
    }
}
