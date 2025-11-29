using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewValley.Buildings;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class BuildingConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public static Building CreateInstanceFromId(string typeId, Vector2 tile)
        public static void CreateInstanceFromId_AddConstructionDays_Postfix(string typeId, Vector2 tile, ref Building __result)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.BUILDING_BLUEPRINT);
                if (numberPurchased <= 0 || __result == null)
                {
                    return;
                }

                if (__result.daysOfConstructionLeft.Value > 0)
                {
                    __result.daysOfConstructionLeft.Value += numberPurchased;
                }

                if (__result.daysUntilUpgrade.Value > 0)
                {
                    __result.daysUntilUpgrade.Value += numberPurchased;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CreateInstanceFromId_AddConstructionDays_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
