using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Locations;

namespace StardewArchipelago.Items.Unlocks
{
    public class EquipmentUnlockManager : IUnlockManager
    {
        public const string PROGRESSIVE_WEAPON = "Progressive Weapon";
        public const string PROGRESSIVE_SWORD = "Progressive Sword";
        public const string PROGRESSIVE_CLUB = "Progressive Club";
        public const string PROGRESSIVE_DAGGER = "Progressive Dagger";
        public const string PROGRESSIVE_BOOTS = "Progressive Footwear";
        public const string PROGRESSIVE_SLINGSHOT = "Progressive Slingshot";

        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public EquipmentUnlockManager(ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public void RegisterUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            RegisterEquipment(unlocks);
        }
        
        private void RegisterEquipment(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            unlocks.Add(PROGRESSIVE_WEAPON, SendProgressiveWeaponLetter);
            unlocks.Add(PROGRESSIVE_SWORD, SendProgressiveSwordLetter);
            unlocks.Add(PROGRESSIVE_CLUB, SendProgressiveClubLetter);
            unlocks.Add(PROGRESSIVE_DAGGER, SendProgressiveDaggerLetter);
            unlocks.Add(PROGRESSIVE_BOOTS, SendProgressiveBootsLetter);
            unlocks.Add(PROGRESSIVE_SLINGSHOT, SendProgressiveSlingshotLetter);
        }

        private LetterActionAttachment SendProgressiveWeaponLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveWeapon);
        }

        private LetterActionAttachment SendProgressiveSwordLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveSword);
        }

        private LetterActionAttachment SendProgressiveClubLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveClub);
        }

        private LetterActionAttachment SendProgressiveDaggerLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveDagger);
        }

        private LetterActionAttachment SendProgressiveBootsLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveProgressiveBoots);
        }

        private LetterActionAttachment SendProgressiveSlingshotLetter(ReceivedItem receivedItem)
        {
            return new LetterActionAttachment(receivedItem, LetterActionsKeys.GiveProgressiveSlingshot);
        }
    }
}
