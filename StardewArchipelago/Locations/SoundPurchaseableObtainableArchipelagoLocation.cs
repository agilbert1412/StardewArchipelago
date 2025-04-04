using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Locations.InGameLocations;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations
{
    internal class SoundPurchaseableObtainableArchipelagoLocation : ObtainableArchipelagoLocation
    {
        private readonly string _sound;

        public SoundPurchaseableObtainableArchipelagoLocation(string locationName, LogHandler logger, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, bool autoHint, string sound) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints, autoHint, sound)
        {
        }

        public SoundPurchaseableObtainableArchipelagoLocation(string locationDisplayName, string locationName, LogHandler logger, IModHelper modHelper, LocationChecker locationChecker, ArchipelagoClient archipelago, Hint[] myActiveHints, bool autoHint, string sound) : base(locationDisplayName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints, autoHint)
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
