namespace StardewArchipelago.Archipelago.Gifting
{
    internal class ReceivedGift
    {
        public string ItemName { get; }
        public int SenderSlot { get; }
        public string SenderName { get; }

        public ReceivedGift(string itemName, int senderSlot, string senderName)
        {
            ItemName = itemName;
            SenderSlot = senderSlot;
            SenderName = senderName;
        }

        public override bool Equals(object obj)
        {
            if (obj is not ReceivedGift otherGift)
            {
                return false;
            }

            return ItemName.Equals(otherGift.ItemName) && SenderSlot == otherGift.SenderSlot && SenderName.Equals(otherGift.SenderName);
        }

        public override int GetHashCode()
        {
            return ItemName.GetHashCode() ^ SenderSlot ^ SenderName.GetHashCode();
        }

        public static bool operator ==(ReceivedGift obj1, ReceivedGift obj2)
        {
            if (obj1 is null && obj2 is null)
            {
                return true;
            }

            if (obj1 is null || obj2 is null)
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ReceivedGift obj1, ReceivedGift obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
