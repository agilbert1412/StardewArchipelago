﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewObject : StardewItem
    {
        public int Edibility { get; private set; }
        public string Type { get; private set; }
        public int Category { get; private set; }
        public bool IsFlavorable { get; private set; }

        public StardewObject(string id, string name, int sellPrice, int edibility, string type, int category, string displayName, string description, bool isFlavorable = false)
        : base(id, name, sellPrice, displayName, description)
        {
            Edibility = edibility;
            Type = type;
            Category = category;
            IsFlavorable = isFlavorable;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new Object(Id, amount);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var item = PrepareForGivingToFarmer(amount);
            farmer.addItemByMenuIfNecessary(item);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            return new LetterItemAttachment(receivedItem, this, amount);
        }

        public override string GetQualifiedId()
        {
            return $"{QualifiedItemIds.OBJECT_QUALIFIER}{Id}";
        }
    }
}
