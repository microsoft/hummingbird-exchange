using System.Collections.Generic;

namespace Hummingbird.Models
{
    public class AddMembersErrorDetails
    {
        public List<string> InvalidMembers { get; set; }

        public List<string> FailedMembers { get; set; }
    }
}