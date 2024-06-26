﻿using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public abstract class StardewRecipe
    {
        public string ItemName { get; protected set; }
        public Dictionary<string, int> Ingredients { get; private set; }
        public string YieldItemId { get; private set; }
        public int YieldItemAmount { get; private set; }
        public string UnlockConditions { get; private set; }
        public string DisplayName { get; private set; }

        protected StardewRecipe(string itemName, Dictionary<string, int> ingredients, string yieldItemId, int yieldItemAmount, string unlockConditions, string displayName)
        {
            ItemName = itemName;
            Ingredients = ingredients;
            YieldItemId = yieldItemId;
            YieldItemAmount = yieldItemAmount;
            UnlockConditions = unlockConditions;
            DisplayName = displayName;
        }

        public abstract void TeachToFarmer(Farmer farmer);

        public abstract LetterAttachment GetAsLetter(ReceivedItem receivedItem);
    }
}
