using System.Collections.Generic;
using StardewValley;

namespace StardewArchipelago.Items.Traps.Shuffle
{
    public class ItemSlot
    {
        public IList<Item> Inventory { get; set; }
        public int SlotNumber { get; set; }

        public ItemSlot(IList<Item> inventory, int slotNumber)
        {
            Inventory = inventory;
            SlotNumber = slotNumber;
        }

        public void SetItem(Item item)
        {
            while (SlotNumber >= Inventory.Count)
            {
                Inventory.Add(null);
            }
            Inventory[SlotNumber] = item;
        }
    }
}
