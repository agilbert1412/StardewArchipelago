using System;
using System.Linq;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CropsanityInjections
    {
        private static readonly string[] _cropsanityExceptions =
        {
            ObjectIds.WEEDS, ObjectIds.SPRING_ONION, ObjectIds.ANCIENT_FRUIT, ObjectIds.FIBER, ObjectIds.QI_FRUIT,
        };

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
        }

        // public virtual bool harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null)
        public static void Harvest_CheckCropsanityLocation_Postfix(Crop __instance, int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester, ref bool __result)
        {
            try
            {
                if (!__result && !__instance.fullyGrown.Value || __instance.indexOfHarvest.Value == null)
                {
                    return;
                }

                var itemId = __instance.indexOfHarvest.Value;

                if (itemId == ObjectIds.SUNFLOWER_SEEDS)
                {
                    itemId = ObjectIds.SUNFLOWER; // Sunflower instead of sunflower seeds
                }

                if (!_itemManager.ObjectExistsById(itemId))
                {
                    _logger.LogError($"Unrecognized Cropsanity Crop: [{itemId}]");
                    return;
                }

                var item = _itemManager.GetObjectById(itemId);
                var apLocation = $"Harvest {item.Name}";

                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
                else if (!_cropsanityExceptions.Contains(itemId))
                {
                    _logger.LogError($"Unrecognized Cropsanity Location: {item.Name} [{itemId}]");
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Harvest_CheckCropsanityLocation_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
        public static bool Shake_CheckCropsanityFruitTreeLocation_Prefix(FruitTree __instance, Vector2 tileLocation, bool doEvenIfStillShaking)
        {
            try
            {
                if (!__instance.fruit.Any())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var fruit = __instance.fruit.First();
                var apLocation = $"Harvest {fruit.Name}";

                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
                else
                {
                    _logger.LogError($"Unrecognized Cropsanity Tree Fruit Location: {fruit.Name} [{fruit.ItemId}]");
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Shake_CheckCropsanityFruitTreeLocation_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
