using System;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class ReceivedItem
    {
        private static Random _random = new Random((int)Game1.uniqueIDForThisGame);

        public string LocationName { get; }
        public string ItemName { get; }
        public string PlayerName { get; }
        public long LocationId { get; }
        public long ItemId { get; }
        public long PlayerId { get; }

        public ReceivedItem(string locationName, string itemName, string playerName, long locationId, long itemId,
            long playerId)
        {
            LocationName = locationName;
            ItemName = itemName;
            PlayerName = playerName;
            LocationId = locationId;
            ItemId = itemId;
            PlayerId = playerId;

            // TODO: Makes StartInventory letters unique 
            if (LocationId < 0 && playerId == 0)
            {
                LocationId = _random.Next(-999999, -99);
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 21;
                hash = (hash * 13) + ItemId.GetHashCode();
                hash = (hash * 13) + LocationId.GetHashCode();
                hash = (hash * 13) + PlayerId.GetHashCode();
                hash = (hash * 13) + ItemName.GetHashCode();
                hash = (hash * 13) + LocationName.GetHashCode();
                hash = (hash * 13) + PlayerName.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is not ReceivedItem otherReceivedItem)
            {
                return false;
            }

            if (this.ItemId != otherReceivedItem.ItemId)
            {
                return false;
            }

            if (this.LocationId != otherReceivedItem.LocationId)
            {
                return false;
            }

            if (this.PlayerId != otherReceivedItem.PlayerId)
            {
                return false;
            }

            if (this.ItemName != otherReceivedItem.ItemName)
            {
                return false;
            }

            if (this.LocationName != otherReceivedItem.LocationName)
            {
                return false;
            }

            if (this.PlayerName != otherReceivedItem.PlayerName)
            {
                return false;
            }

            return true;
        }
    }
}
