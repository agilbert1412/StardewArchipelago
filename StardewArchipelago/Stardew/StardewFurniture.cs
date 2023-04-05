using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class StardewFurniture : StardewItem
    {
        public string Type { get; }
        public string TilesheetSize { get; }
        public string BoundingBoxSize { get; }
        public string Rotations { get; }
        public string PlacementRestriction { get; }

        public StardewFurniture(int id, string name, string type, string tilesheetSize, string boundingBoxSize, string rotations, string price, string displayName, string placementRestriction): base(id, name, 0, displayName, "")
        {
            Type = type;
            TilesheetSize = tilesheetSize;
            BoundingBoxSize = boundingBoxSize;
            Rotations = rotations;
            PlacementRestriction = placementRestriction;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new StardewValley.Objects.Furniture(Id, Vector2.Zero);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var furniture = PrepareForGivingToFarmer();
            farmer.addItemByMenuIfNecessaryElseHoldUp(furniture);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveFurniture, Id.ToString());
        }
    }
}
