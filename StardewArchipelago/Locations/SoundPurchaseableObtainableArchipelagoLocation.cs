using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Locations.InGameLocations;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations
{
    internal class SoundPurchaseableObtainableArchipelagoLocation : ObtainableArchipelagoLocation
    {
        private string _sound;

        public SoundPurchaseableObtainableArchipelagoLocation(string locationName, ILogger logger, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, string sound) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints, sound)
        {
        }

        public SoundPurchaseableObtainableArchipelagoLocation(string locationDisplayName, string locationName, ILogger logger, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, string sound) : base(locationDisplayName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints)
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
