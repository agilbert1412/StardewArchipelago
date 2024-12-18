﻿using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class StardewRing : StardewObject
    {
        public StardewRing(string id, string name, int sellPrice, int edibility, string type, int category, string displayName, string description, string misc1 = "", string misc2 = "", string buffDuration = "")
            : base(id, name, sellPrice, edibility, type, category, displayName, description, misc1, misc2, buffDuration)
        {
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new Ring(Id);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveRing, Id.ToString());
        }
    }
}
