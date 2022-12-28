using StardewArchipelago.Archipelago;
using System.Collections.Generic;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoStateDto
    {
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        public Dictionary<long, int> ItemsReceived { get; set; }
        public List<long> LocationsChecked { get; set; }

        public ArchipelagoStateDto()
        {
            ItemsReceived = new Dictionary<long, int>();
            LocationsChecked = new List<long>();
        }
    }
}
