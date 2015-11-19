using System.Collections.ObjectModel;

namespace Hummingbird.Models
{
    public class DistributionList
    {
        public DistributionList()
        {
            Members = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Members { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
    }
}