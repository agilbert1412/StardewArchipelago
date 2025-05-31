using System;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Logging;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Stardew.NameMapping;
using StardewValley.GameData.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class EatInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static NameSimplifier _nameSimplifier;
        private static CompoundNameMapper _nameMapper;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, NameSimplifier nameSimplifier)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _nameSimplifier = nameSimplifier;
            _nameMapper = new CompoundNameMapper(archipelago.SlotData);
        }

        // public void doneEating()
        public static void DoneEating_EatingPatches_Postfix(Farmer __instance)
        {
            try
            {
                FarmerInjections.DoneEatingFavoriteThingKaito(__instance);
                DifficultSecretsInjections.DoneEatingStardropSecret(__instance);
                DoneEatingEatsanityLocation(__instance);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneEating_EatingPatches_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DoneEatingEatsanityLocation(Farmer farmer)
        {
            if (_archipelago.SlotData.Eatsanity == Eatsanity.None)
            {
                return;
            }

            var eatenItem = farmer.itemToEat as Object;
            if (!Game1.objectData.ContainsKey(eatenItem.ItemId))
            {
                return;
            }

            var objectData = Game1.objectData[eatenItem.ItemId];

            var name = _nameSimplifier.GetSimplifiedName(eatenItem);
            name = _nameMapper.GetEnglishName(name); // For the Name vs Display Name discrepencies in Mods.

            var apLocation = objectData.IsDrink ? $"Drink {name}" : $"Eat {name}";
            if (_locationChecker.LocationExists(apLocation))
            {
                _locationChecker.AddCheckedLocation(apLocation);
            }
            else
            {
                _logger.LogError($"Unrecognized Eatsanity Location: {name} [{eatenItem.QualifiedItemId}]");
            }
        }
    }
}
