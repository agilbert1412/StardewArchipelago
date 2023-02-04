using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Stardew
{
    public class BigCraftable : StardewItem
    {
        public int Edibility { get; private set; }
        public string ObjectType { get; private set; }
        public string Category { get; private set; }
        public bool Outdoors { get; private set; }
        public bool Indoors { get; private set; }
        public int Fragility { get; private set; }

        public BigCraftable(int id, string name, int sellPrice, int edibility, string objectType, string category, string description, bool outdoors, bool indoors, int fragility, string displayName)
        : base(id, name, sellPrice, displayName, description)
        {
            Edibility = edibility;
            ObjectType = objectType;
            Category = category;
            Outdoors = outdoors;
            Indoors = indoors;
            Fragility = fragility;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            var bigCraftable = new Object(Vector2.Zero, Id);
            bigCraftable.Stack = amount;
            return bigCraftable;
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            throw new NotImplementedException();
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            throw new NotImplementedException();
        }
    }
}
