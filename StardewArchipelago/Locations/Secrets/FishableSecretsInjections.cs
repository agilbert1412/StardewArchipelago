using System;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Secrets
{
    public class FishableSecretsInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void doneHoldingFish(Farmer who, bool endOfNight = false)
        public static void DoneHoldingFish_FishableSecret_Postfix(FishingRod __instance, Farmer who, bool endOfNight)
        {
            try
            {
                var locationsMap = SecretsLocationNames.FISHABLE_QUALIFIED_ITEM_IDS_TO_LOCATIONS;
                var id = __instance.whichFish.QualifiedItemId;
                if (locationsMap.ContainsKey(id))
                {
                    _locationChecker.AddCheckedLocation(locationsMap[id]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneHoldingFish_FishableSecret_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
