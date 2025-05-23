﻿using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Locations.InGameLocations;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Logging;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations
{
    internal class SoundPurchaseableObtainableArchipelagoLocation : ObtainableArchipelagoLocation
    {
        private readonly string _sound;

        public SoundPurchaseableObtainableArchipelagoLocation(string locationName, LogHandler logger, IModHelper modHelper, LocationChecker locationChecker, StardewArchipelagoClient archipelago, Hint[] myActiveHints, string sound) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints, sound)
        {
        }

        public SoundPurchaseableObtainableArchipelagoLocation(string locationDisplayName, string locationName, LogHandler logger, IModHelper modHelper, LocationChecker locationChecker, StardewArchipelagoClient archipelago, Hint[] myActiveHints, string sound) : base(locationDisplayName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints)
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
