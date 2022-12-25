using System.Collections.Generic;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoStateDto
    {
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        public List<long> ItemsReceived { get; set; }
        public List<long> LocationsChecked { get; set; }

        public ArchipelagoStateDto()
        {
            ItemsReceived = new List<long>();
            LocationsChecked = new List<long>();
        }
    }
}
