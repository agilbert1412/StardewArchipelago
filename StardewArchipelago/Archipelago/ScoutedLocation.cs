using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Archipelago
{
    public class ScoutedLocation
    {
        private const string UNKNOWN_AP_ITEM = "Item for another world in this Archipelago";

        public string LocationName { get; private set; }
        public string ItemName { get; private set; }
        public string PlayerName { get; private set; }
        public long LocationId { get; private set; }
        public long ItemId { get; private set; }
        public long PlayerId { get; private set; }

        public ScoutedLocation(string locationName, string itemName, string playerName, long locationId, long itemId,
            long playerId)
        {
            LocationName = locationName;
            ItemName = itemName;
            PlayerName = playerName;
            LocationId = locationId;
            ItemId = itemId;
            PlayerId = playerId;
        }

        public override string ToString()
        {
            return $"{PlayerName}'s {ItemName}";
        }

        public static string GenericItemName()
        {
            return UNKNOWN_AP_ITEM;
        }
    }
}
