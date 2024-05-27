using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.TerrainFeatures;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutRepeatablesInjections
    {
        private const string WALNUT_FISHING_KEY = "IslandFishing";
        private const string WALNUT_FARMING_KEY = "IslandFarming";

        private const double WALNUT_BASE_CHANCE_FISHING = 0.25;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_FISHING = 0.75;
        private const double WALNUT_BASE_CHANCE_FARMING = 0.10;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_FARMING = 0.75;

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile,
        //string locationName = null)
        public static bool GetFish_RepeatableWalnut_Prefix(IslandLocation __instance, float millisecondsAfterNibble, string bait, int waterDepth,
            Farmer who, double baitPotency, Vector2 bobberTile, string locationName, ref Item __result)
        {
            try
            {
                double seedA = Game1.stats.DaysPlayed;
                double seedB = Game1.stats.TimesFished;
                double seedC = Game1.uniqueIDForThisGame;
                var random = Utility.CreateRandom(seedA, seedB, seedC);
                var roll = random.NextDouble();
                var chanceRequired = WALNUT_BASE_CHANCE_FISHING;

                if (!Game1.player.team.limitedNutDrops.TryGetValue(WALNUT_FISHING_KEY, out var numberWalnutsFishedSoFar))
                {
                    numberWalnutsFishedSoFar = 0;
                }

                if (numberWalnutsFishedSoFar < 5)
                {
                    if (roll > chanceRequired)
                    {
                        __result = CallBaseGetFish(__instance, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
                        return false; // don't run original logic
                    }

                    numberWalnutsFishedSoFar++;
                    Game1.player.team.limitedNutDrops[WALNUT_FISHING_KEY] = numberWalnutsFishedSoFar;
                    var itemToSpawnId = QualifiedItemIds.GOLDEN_WALNUT;
                    if (_archipelago.SlotData.Walnutsanity.HasFlag(Archipelago.Walnutsanity.Repeatables))
                    {
                        var location = $"Fishing Walnut {numberWalnutsFishedSoFar}";
                        itemToSpawnId = IDProvider.CreateApLocationItemId(location);
                    }

                    __result = ItemRegistry.Create(itemToSpawnId);
                    return false; // don't run original logic
                }

                // We allow the player to get extra walnuts here, but each one is less likely than the last
                chanceRequired *= Math.Pow(INFINITY_WALNUT_CHANCE_REDUCTION_FISHING, numberWalnutsFishedSoFar);
                if (roll > chanceRequired)
                {
                    __result = CallBaseGetFish(__instance, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
                    return false; // don't run original logic
                }
                
                Game1.player.team.limitedNutDrops[WALNUT_FISHING_KEY] = numberWalnutsFishedSoFar + 1;
                __result = ItemRegistry.Create(QualifiedItemIds.GOLDEN_WALNUT);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetFish_RepeatableWalnut_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static Item CallBaseGetFish(IslandLocation islandLocation, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who,
            double baitPotency, Vector2 bobberTile, string locationName)
        {
            // base.resetLocalState();
            var gameLocationGetFishMethod = typeof(GameLocation).GetMethod("getFish", BindingFlags.Instance | BindingFlags.NonPublic);
            var functionPointer = gameLocationGetFishMethod.MethodHandle.GetFunctionPointer();
            var baseGetFish = (Func<float, string, int, Farmer, double, Vector2, string, Item>)Activator.CreateInstance(
                typeof(Func<float, string, int, Farmer, double, Vector2, string, Item>),
                islandLocation, functionPointer);
            return baseGetFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
        }

        // public override bool performUseAction(Vector2 tileLocation)
        public static bool PerformUseAction_RepeatableFarmingWalnut_Prefix(HoeDirt __instance, Vector2 tileLocation, ref bool __result)
        {
            try
            {
                if (__instance.crop == null || __instance.Location is not IslandLocation)
                {
                    return true; // run original logic
                }

                var harvestMethod = __instance.crop.GetHarvestMethod();
                if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isScythe() && Game1.player.CurrentTool.ItemId == "66")
                {
                    harvestMethod = HarvestMethod.Scythe;
                }

                if (harvestMethod != HarvestMethod.Grab || !__instance.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, __instance))
                {
                    return true; // run original logic
                }

                __instance.destroyCrop(false);
                __result = true;
                var roll = Game1.random.NextDouble();
                var chanceRequired = WALNUT_BASE_CHANCE_FARMING;

                if (!Game1.player.team.limitedNutDrops.TryGetValue(WALNUT_FARMING_KEY, out var numberWalnutsFarmedSoFar))
                {
                    numberWalnutsFarmedSoFar = 0;
                }

                if (numberWalnutsFarmedSoFar < 5)
                {
                    if (roll > chanceRequired)
                    {
                        return false; // don't run original logic
                    }

                    numberWalnutsFarmedSoFar++;
                    Game1.player.team.limitedNutDrops[WALNUT_FARMING_KEY] = numberWalnutsFarmedSoFar;
                    var itemToSpawnId = QualifiedItemIds.GOLDEN_WALNUT;
                    if (_archipelago.SlotData.Walnutsanity.HasFlag(Archipelago.Walnutsanity.Repeatables))
                    {
                        var location = $"Harvesting Walnut {numberWalnutsFarmedSoFar}";
                        itemToSpawnId = IDProvider.CreateApLocationItemId(location);
                    }
                    
                    CreateLocationDebris(ItemRegistry.Create(itemToSpawnId), new Vector2(tileLocation.X, tileLocation.Y) * 64f, __instance.Location);
                    return false; // don't run original logic
                }

                // We allow the player to get extra walnuts here, but each one is less likely than the last
                chanceRequired *= Math.Pow(INFINITY_WALNUT_CHANCE_REDUCTION_FARMING, numberWalnutsFarmedSoFar);
                if (roll > chanceRequired)
                {
                    return false; // don't run original logic
                }

                Game1.player.team.limitedNutDrops[WALNUT_FARMING_KEY] = numberWalnutsFarmedSoFar + 1;
                CreateLocationDebris(ItemRegistry.Create(QualifiedItemIds.GOLDEN_WALNUT), new Vector2(tileLocation.X, tileLocation.Y) * 64f, __instance.Location);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformUseAction_RepeatableFarmingWalnut_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void CreateLocationDebris(string locationName, Vector2 pixelOrigin, GameLocation gameLocation, int direction = 0, int groundLevel = 0)
        {
            CreateLocationDebris(CreateLocationItem(locationName), pixelOrigin, gameLocation, direction, groundLevel);
        }

        private static void CreateLocationDebris(Item item, Vector2 pixelOrigin, GameLocation gameLocation, int direction = 0, int groundLevel = 0)
        {
            Game1.createItemDebris(item, pixelOrigin, direction, gameLocation, groundLevel);
        }

        private static Item CreateLocationItem(string locationName)
        {
            var itemId = IDProvider.CreateApLocationItemId(locationName);
            return ItemRegistry.Create(itemId);
        }
    }
}
