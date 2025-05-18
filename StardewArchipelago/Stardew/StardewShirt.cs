using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class StardewShirt : StardewItem
    {
        public StardewShirt(string id, string name, string description, int price, string displayName)
            : base(id, name, price, displayName, description)
        {
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new Clothing(Id);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var shirt = PrepareForGivingToFarmer();
            farmer.addItemByMenuIfNecessaryElseHoldUp(shirt);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            throw new NotImplementedException();
            // return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveShirt, Id.ToString());
        }

        public override string GetQualifiedId()
        {
            return $"{QualifiedItemIds.SHIRT_QUALIFIER}{Id}";
        }
    }
}
