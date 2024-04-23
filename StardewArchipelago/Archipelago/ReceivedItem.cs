namespace StardewArchipelago.Archipelago
{
    public class ReceivedItem
    {
        public string LocationName { get; }
        public string ItemName { get; }
        public string PlayerName { get; }
        public long LocationId { get; }
        public long ItemId { get; }
        public long PlayerId { get; }
        public int UniqueId { get; }

        public ReceivedItem(string locationName, string itemName, string playerName, long locationId, long itemId,
            long playerId, int uniqueId)
        {
            LocationName = locationName;
            ItemName = itemName;
            PlayerName = playerName;
            LocationId = locationId;
            ItemId = itemId;
            PlayerId = playerId;
            UniqueId = uniqueId;
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
                hash = (hash * 13) + UniqueId.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is not ReceivedItem otherReceivedItem)
            {
                return false;
            }

            if (ItemId != otherReceivedItem.ItemId)
            {
                return false;
            }

            if (LocationId != otherReceivedItem.LocationId)
            {
                return false;
            }

            if (PlayerId != otherReceivedItem.PlayerId)
            {
                return false;
            }

            if (ItemName != otherReceivedItem.ItemName)
            {
                return false;
            }

            if (LocationName != otherReceivedItem.LocationName)
            {
                return false;
            }

            if (PlayerName != otherReceivedItem.PlayerName)
            {
                return false;
            }

            if (UniqueId != otherReceivedItem.UniqueId)
            {
                return false;
            }

            return true;
        }
    }
}
