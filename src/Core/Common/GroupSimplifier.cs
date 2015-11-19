using System.Collections.ObjectModel;
using Hummingbird.Models;
using Hummingbird.Models.EnvelopeAtoms;

namespace Hummingbird.Core.Common
{
    public class GroupSimplifier
    {
        public DistributionList SimplifyGroup(Member[] members, string groupName)
        {
            var list = new DistributionList {Name = groupName, Owner = "INVALID", Members = new ObservableCollection<string>()};

            foreach (var member in members)
            {
                list.Members.Add(member.Persona.EmailAddress.EmailAddress);
            }

            return list;
        }
    }
}