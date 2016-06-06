using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hummingbird.Models
{
    public class AddMembersErrorDetails
    {
        public List<string> InvalidMembers { get; set; }

        public List<string> FailedMembers { get; set; }
    }
}