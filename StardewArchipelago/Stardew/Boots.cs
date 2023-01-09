using StardewValley;

namespace StardewArchipelago.Stardew
{
    public class Boots : StardewItem
    {
        public int AddedDefense { get; }
        public int AddedImmunity { get; }
        public int ColorIndex { get; }

        public Boots(int id, string name, int sellPrice, string description, int addedDefense, int addedImmunity, int colorIndex, string displayName)
        : base(id, name, sellPrice, displayName, description)
        {
            AddedDefense = addedDefense;
            AddedImmunity = addedImmunity;
            ColorIndex = colorIndex;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new StardewValley.Objects.Boots(Id);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var boots = PrepareForGivingToFarmer();
            farmer.addItemByMenuIfNecessaryElseHoldUp(boots);
        }
    }
}
