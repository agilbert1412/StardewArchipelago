using StardewArchipelago.Archipelago;
using System.Collections.Generic;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoStateDto
    {
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        public Dictionary<long, int> ItemsReceived { get; set; }
        public List<string> LocationsChecked { get; set; }
        public Dictionary<string, ScoutedLocation> LocationsScouted { get; set; }

        public ArchipelagoStateDto()
        {
            ItemsReceived = new Dictionary<long, int>();
            LocationsChecked = new List<string>();
            LocationsScouted = new Dictionary<string, ScoutedLocation>();
        }
    }
}
