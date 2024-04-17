﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewHat : StardewItem
    {
        public string SkipHairDraw { get; }
        public bool IgnoreHairstyleOffset { get; }

        public StardewHat(string id, string name, string description, string skipHairDraw, bool ignoreHairstyleOffset, string displayName)
        : base(id, name, 0, displayName, description)
        {
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new StardewValley.Objects.Hat(Id);
        }

        public override Item PrepareForRecovery()
        {
            throw new System.NotImplementedException();
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var boots = PrepareForGivingToFarmer();
            farmer.addItemByMenuIfNecessaryElseHoldUp(boots);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveHat, Id.ToString());
        }
    }
}
