using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations
{
    internal class SoundPurchaseableArchipelagoLocation : PurchaseableArchipelagoLocation
    {
        private string _sound;

        public SoundPurchaseableArchipelagoLocation(string locationName, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, string sound) : this(locationName, locationName, modHelper, locationChecker, archipelago, myActiveHints, sound)
        {
        }

        public SoundPurchaseableArchipelagoLocation(string locationDisplayName, string locationName, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, string sound) : base(locationDisplayName, locationName, modHelper, locationChecker, archipelago, myActiveHints)
        {
            _sound = sound;
        }

        public override bool actionWhenPurchased(string shopId)
        {
            Game1.playSound(_sound);
            return base.actionWhenPurchased(shopId);
        }
    }
}
