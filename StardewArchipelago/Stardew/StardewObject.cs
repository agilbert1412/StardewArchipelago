﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewObject : StardewItem
    {
        public int Edibility { get; private set; }
        public string Type { get; private set; }
        public int Category { get; private set; }
        public string Misc1 { get; private set; }
        public string Misc2 { get; private set; }
        public string BuffDuration { get; private set; }

        public StardewObject(string id, string name, int sellPrice, int edibility, string type, int category, string displayName, string description, string misc1 = "", string misc2 = "", string buffDuration = "")
        : base(id, name, sellPrice, displayName, description)
        {
            Edibility = edibility;
            Type = type;
            Category = category;
            Misc1 = misc1;
            Misc2 = misc2;
            BuffDuration = buffDuration;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new Object(Id, amount);
        }

        public override Item PrepareForRecovery()
        {
            throw new System.NotImplementedException();
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
    }
}
