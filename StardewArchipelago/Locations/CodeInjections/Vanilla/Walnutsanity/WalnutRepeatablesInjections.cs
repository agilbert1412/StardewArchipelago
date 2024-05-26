using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Minigames;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutRepeatablesInjections
    {
        private const string WALNUT_FISHING_KEY = "IslandFishing";
        private const double INFINITY_WALNUT_CHANCE_REDUCTION = 0.8;

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
                var chanceRequired = 0.15;

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
                chanceRequired *= Math.Pow(INFINITY_WALNUT_CHANCE_REDUCTION, numberWalnutsFishedSoFar);
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
    }
}
