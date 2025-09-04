using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Stardew
{
    public class StardewMannequin : StardewItem
    {
        public bool Cursed { get; }

        public StardewMannequin(string id, string name, bool cursed, int price, string displayName, string description) : base(id, name, price, displayName, description)
        {
            Cursed = cursed;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new Mannequin(Id);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var furniture = PrepareForGivingToFarmer();
            farmer.addItemByMenuIfNecessaryElseHoldUp(furniture);
        }

        public override LetterAttachment GetAsLetter(ReceivedItem receivedItem, int amount = 1)
        {
            throw new NotImplementedException();
            //  return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveFurniture, Id.ToString());
        }

        public override string GetQualifiedId()
        {
            return $"{QualifiedItemIds.MANNEQUIN_QUALIFIER}{Id}";
        }
    }
}
