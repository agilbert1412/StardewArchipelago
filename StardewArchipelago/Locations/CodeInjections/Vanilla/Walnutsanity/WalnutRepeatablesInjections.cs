using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Locations;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutRepeatablesInjections
    {
        private const string WALNUT_FISHING_KEY = "IslandFishing";
        private const string WALNUT_FARMING_KEY = "IslandFarming";
        private const string WALNUT_MUSSEL_KEY = "MusselStone";
        private const string WALNUT_TIGER_SLIMES_KEY = "TigerSlimeNut";
        private const string WALNUT_VOLCANO_MINING_KEY = "VolcanoMining";
        private const string WALNUT_VOLCANO_MONSTER_KEY = "VolcanoMonsterDrop";
        private const string WALNUT_VOLCANO_BARREL_KEY = "VolcanoBarrel";
        private const string WALNUT_VOLCANO_COMMON_CHEST_KEY = "VolcanoNormalChest";
        private const string WALNUT_VOLCANO_RARE_CHEST_KEY = "VolcanoRareChest";
        private const string WALNUT_JOURNAL_SCRAP_4 = "Island_W_BuriedTreasureNut";
        private const string WALNUT_JOURNAL_SCRAP_6 = "Island_W_BuriedTreasureNut2";
        private const string WALNUT_JOURNAL_SCRAP_10 = "Island_N_BuriedTreasureNut";

        private const double WALNUT_BASE_CHANCE_FISHING = 0.15;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_FISHING = 0.75;

        private const double WALNUT_BASE_CHANCE_FARMING = 0.10;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_FARMING = 0.75;

        private const double WALNUT_BASE_CHANCE_MUSSEL = 0.15;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_MUSSEL = 0.75;

        private const double WALNUT_BASE_CHANCE_VOLCANO_MINING = 0.05;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_VOLCANO_MINING = 0.75;

        private const double WALNUT_BASE_CHANCE_VOLCANO_MONSTER = 0.10;
        private const double INFINITY_WALNUT_CHANCE_REDUCTION_VOLCANO_MONSTER = 0.75;


        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;

            InitializeReflectionCalls();
        }

        private static MethodInfo _gameLocationGetFishMethod;
        private static IntPtr _gameLocationGetFishMethodPointer;
        private static Dictionary<string, Func<float, string, int, Farmer, double, Vector2, string, Item>> _islandLocationBaseGetFish;

        private static void InitializeReflectionCalls()
        {
            _gameLocationGetFishMethod = typeof(GameLocation).GetMethod("getFish", BindingFlags.Instance | BindingFlags.Public);
            _gameLocationGetFishMethodPointer = _gameLocationGetFishMethod.MethodHandle.GetFunctionPointer();
            _islandLocationBaseGetFish = new Dictionary<string, Func<float, string, int, Farmer, double, Vector2, string, Item>>();
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

                var baseFishCallBack = () => CallBaseGetFish(__instance, millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
                __result = RollForRepeatableWalnutOrCheck(WALNUT_FISHING_KEY, $"{Prefix.WALNUTSANITY}Fishing Walnut", random, WALNUT_BASE_CHANCE_FISHING, INFINITY_WALNUT_CHANCE_REDUCTION_FISHING, baseFishCallBack);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetFish_RepeatableWalnut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static Item CallBaseGetFish(IslandLocation islandLocation, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who,
            double baitPotency, Vector2 bobberTile, string locationName)
        {
            // base.resetLocalState();
            var key = islandLocation.NameOrUniqueName ?? "";
            if (!_islandLocationBaseGetFish.ContainsKey(key))
            {
                var baseGetFish = (Func<float, string, int, Farmer, double, Vector2, string, Item>)Activator.CreateInstance(
                    typeof(Func<float, string, int, Farmer, double, Vector2, string, Item>),
                    islandLocation, _gameLocationGetFishMethodPointer);
                _islandLocationBaseGetFish.Add(key, baseGetFish);
            }
            return _islandLocationBaseGetFish[key](millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
        }

        // public override bool performUseAction(Vector2 tileLocation)
        public static bool PerformUseAction_RepeatableFarmingWalnut_Prefix(HoeDirt __instance, Vector2 tileLocation, ref bool __result)
        {
            try
            {
                if (__instance.crop == null || __instance.Location is not IslandLocation)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var harvestMethod = __instance.crop.GetHarvestMethod();
                if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isScythe() && Game1.player.CurrentTool.ItemId == "66")
                {
                    harvestMethod = HarvestMethod.Scythe;
                }

                if (harvestMethod != HarvestMethod.Grab || !__instance.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, __instance))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __instance.destroyCrop(false);
                __result = true;

                RollForRepeatableWalnutOrCheck(WALNUT_FARMING_KEY, $"{Prefix.WALNUTSANITY}Harvesting Walnut", __instance.Location, tileLocation.X, tileLocation.Y, Game1.random, WALNUT_BASE_CHANCE_FARMING, INFINITY_WALNUT_CHANCE_REDUCTION_FARMING);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformUseAction_RepeatableFarmingWalnut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
        public static bool PerformToolAction_RepeatableFarmingWalnut_Prefix(HoeDirt __instance, Tool t, int damage, Vector2 tileLocation, ref bool __result)
        {
            try
            {
                if (__instance?.crop == null || __instance.Location is not IslandLocation || t == null || !t.isScythe())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var harvestMethod = __instance.crop.GetHarvestMethod();
                if (harvestMethod != HarvestMethod.Scythe || !__instance.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, __instance, isForcedScytheHarvest: true))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.crop.indexOfHarvest.Value == "771" && t.hasEnchantmentOfType<HaymakerEnchantment>())
                {
                    for (var index = 0; index < 2; ++index)
                    {
                        Game1.createItemDebris(ItemRegistry.Create("(O)771"), new Vector2((float)((double)tileLocation.X * 64.0 + 32.0), (float)((double)tileLocation.Y * 64.0 + 32.0)), -1);
                    }
                }

                __instance.destroyCrop(true);

                if (__instance.crop == null && t.ItemId == "66" && __instance.Location.objects.ContainsKey(tileLocation) && __instance.Location.objects[tileLocation].isForage())
                {
                    var @object = __instance.Location.objects[tileLocation];
                    if (t.getLastFarmerToUse() != null && t.getLastFarmerToUse().professions.Contains(16))
                    {
                        @object.Quality = 4;
                    }
                    Game1.createItemDebris((Item)@object, new Vector2((float)((double)tileLocation.X * 64.0 + 32.0), (float)((double)tileLocation.Y * 64.0 + 32.0)), -1);
                    __instance.Location.objects.Remove(tileLocation);
                }

                __instance.shake((float)Math.PI / 32f, (float)Math.PI / 40f, (double)tileLocation.X * 64.0 < (double)Game1.player.Position.X);
                __result = false;

                RollForRepeatableWalnutOrCheck(WALNUT_FARMING_KEY, $"{Prefix.WALNUTSANITY}Harvesting Walnut", __instance.Location, tileLocation.X, tileLocation.Y, Game1.random, WALNUT_BASE_CHANCE_FARMING, INFINITY_WALNUT_CHANCE_REDUCTION_FARMING);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformToolAction_RepeatableFarmingWalnut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // protected virtual bool breakStone(string stoneId, int x, int y, Farmer who, Random random)
        public static bool BreakStone_RepeatableMusselWalnut_Prefix(GameLocation __instance, string stoneId, int x, int y, Farmer who, Random r, ref bool __result)
        {
            try
            {
                if (stoneId != ObjectIds.MUSSEL_NODE || __instance is not IslandLocation)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var farmerId = who != null ? who.UniqueMultiplayerID : 0L;
                Game1.createMultipleObjectDebris("(O)719", x, y, r.Next(2, 5), farmerId, __instance);

                if (who != null && __instance.HasUnlockedAreaSecretNotes(who) && r.NextDouble() < 3.0 / 400.0)
                {
                    var unseenSecretNote = __instance.tryToCreateUnseenSecretNote(who);
                    if (unseenSecretNote != null)
                    {
                        Game1.createItemDebris((Item)unseenSecretNote, new Vector2((float)x + 0.5f, (float)y + 0.75f) * 64f, Game1.player.FacingDirection, __instance);
                    }
                }
                who?.gainExperience(3, 5);
                __result = true;

                RollForRepeatableWalnutOrCheck(WALNUT_MUSSEL_KEY, $"{Prefix.WALNUTSANITY}Mussel Node Walnut", __instance, x, y, r, WALNUT_BASE_CHANCE_MUSSEL, INFINITY_WALNUT_CHANCE_REDUCTION_MUSSEL);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BreakStone_RepeatableMusselWalnut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void RequestLimitedNutDrops(string key, GameLocation location, int x, int y, int limit, int rewardAmount = 1)
        // Game1.player.team.RequestLimitedNutDrops("TigerSlimeNut", (GameLocation) this, x, y, 1);
        public static bool RequestLimitedNutDrops_TigerSlimesAndCreatesWalnuts_Prefix(FarmerTeam __instance, string key, GameLocation location, int x, int y, int limit, int rewardAmount)
        {
            try
            {
                if (__instance.limitedNutDrops.TryGetValue(key, out var numberAlreadyDropped) && numberAlreadyDropped >= limit)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (HandleRepeatableWalnuts(__instance, key, location, x, y, numberAlreadyDropped))
                {
                    return false;
                }

                if (HandleDigSpotWalnuts(__instance, key, location, x, y, numberAlreadyDropped))
                {
                    return false;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(RequestLimitedNutDrops_TigerSlimesAndCreatesWalnuts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool HandleRepeatableWalnuts(FarmerTeam __instance, string key, GameLocation location, int x, int y, int numberAlreadyDropped)
        {
            if (!_archipelago.SlotData.Walnutsanity.HasFlag(Archipelago.SlotData.SlotEnums.Walnutsanity.Repeatables))
            {
                return false;
            }

            if (key == WALNUT_TIGER_SLIMES_KEY)
            {
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Tiger Slime Walnut", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = numberAlreadyDropped + 1;
                return true;
            }

            if (key == WALNUT_VOLCANO_BARREL_KEY)
            {
                var newNumber = numberAlreadyDropped + 1;
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Volcano Crates Walnut {newNumber}", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = newNumber;
                return true;
            }

            if (key == WALNUT_VOLCANO_COMMON_CHEST_KEY)
            {
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Volcano Common Chest Walnut", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = numberAlreadyDropped + 1;
                return true;
            }

            if (key == WALNUT_VOLCANO_RARE_CHEST_KEY)
            {
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Volcano Rare Chest Walnut", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = numberAlreadyDropped + 1;
                return true;
            }

            return false;
        }

        private static bool HandleDigSpotWalnuts(FarmerTeam __instance, string key, GameLocation location, int x, int y, int numberAlreadyDropped)
        {
            if (!_archipelago.SlotData.Walnutsanity.HasFlag(Archipelago.SlotData.SlotEnums.Walnutsanity.DigSpots))
            {
                return false;
            }

            if (key == WALNUT_JOURNAL_SCRAP_4)
            {
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Journal Scrap #4", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = numberAlreadyDropped + 1;
                return true;
            }

            if (key == WALNUT_JOURNAL_SCRAP_6)
            {
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Journal Scrap #6", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = numberAlreadyDropped + 1;
                return true;
            }

            if (key == WALNUT_JOURNAL_SCRAP_10)
            {
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Journal Scrap #10", new Vector2(x, y), location);
                __instance.limitedNutDrops[key] = numberAlreadyDropped + 1;
                return true;
            }

            return false;
        }

        // protected virtual bool breakStone(string stoneId, int x, int y, Farmer who, Random random)
        public static bool BreakStone_RepeatableVolcanoStoneWalnut_Prefix(VolcanoDungeon __instance, string stoneId, int x, int y, Farmer who, Random r, ref bool __result)
        {
            try
            {
                if (who != null && stoneId is "845" or "846" or "847" && Game1.random.NextDouble() < 0.005)
                {
                    Game1.createObjectDebris(QualifiedItemIds.MUMMIFIED_BAT, x, y, who.UniqueMultiplayerID, __instance);
                }

                if (who != null)
                {
                    RollForRepeatableWalnutOrCheck(WALNUT_VOLCANO_MINING_KEY, $"{Prefix.WALNUTSANITY}Volcano Rocks Walnut", __instance, x, y, r, WALNUT_BASE_CHANCE_VOLCANO_MINING, INFINITY_WALNUT_CHANCE_REDUCTION_VOLCANO_MINING);
                }

                __result = CallBaseBreakStone(__instance, stoneId, x, y, who, r);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BreakStone_RepeatableVolcanoStoneWalnut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool CallBaseBreakStone(GameLocation gameLocation, string stoneId, int x, int y, Farmer who, Random r)
        {
            // base.breakStone(stoneId, x, y, who, r);
            var gameLocationBreakStoneMethod = typeof(GameLocation).GetMethod("breakStone", BindingFlags.Instance | BindingFlags.NonPublic);
            var functionPointer = gameLocationBreakStoneMethod.MethodHandle.GetFunctionPointer();
            var baseBreakStone = (Func<string, int, int, Farmer, Random, bool>)Activator.CreateInstance(
                typeof(Func<string, int, int, Farmer, Random, bool>),
                gameLocation, functionPointer);
            return baseBreakStone(stoneId, x, y, who, r);
        }

        // public override void monsterDrop(Monster monster, int x, int y, Farmer who)
        public static bool MonsterDrop_RepeatableVolcanoMonsterWalnut_Prefix(VolcanoDungeon __instance, Monster monster, int x, int y, Farmer who)
        {
            try
            {
                CallBaseMonsterDrop(__instance, monster, x, y, who);
                RollForRepeatableWalnutOrCheck(WALNUT_VOLCANO_MONSTER_KEY, $"{Prefix.WALNUTSANITY}Volcano Monsters Walnut", __instance, new Vector2(x, y), Game1.random, WALNUT_BASE_CHANCE_VOLCANO_MONSTER, INFINITY_WALNUT_CHANCE_REDUCTION_VOLCANO_MONSTER);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MonsterDrop_RepeatableVolcanoMonsterWalnut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CallBaseMonsterDrop(GameLocation gameLocation, Monster monster, int x, int y, Farmer who)
        {
            // public virtual void monsterDrop(Monster monster, int x, int y, Farmer who)
            var gameLocationMonsterDropMethod = typeof(GameLocation).GetMethod("monsterDrop", BindingFlags.Instance | BindingFlags.Public);
            var functionPointer = gameLocationMonsterDropMethod.MethodHandle.GetFunctionPointer();
            var baseMonsterDrop = (Action<Monster, int, int, Farmer>)Activator.CreateInstance(
                typeof(Action<Monster, int, int, Farmer>),
                gameLocation, functionPointer);
            baseMonsterDrop(monster, x, y, who);
        }

        private static void RollForRepeatableWalnutOrCheck(string walnutKey, string apLocationName, GameLocation gameLocation, int x, int y, Random random, double baseChance, double chanceReduction)
        {
            RollForRepeatableWalnutOrCheck(walnutKey, apLocationName, gameLocation, new Vector2(x, y) * 64f, random, baseChance, chanceReduction);
        }

        private static void RollForRepeatableWalnutOrCheck(string walnutKey, string apLocationName, GameLocation gameLocation, float x, float y, Random random, double baseChance, double chanceReduction)
        {
            RollForRepeatableWalnutOrCheck(walnutKey, apLocationName, gameLocation, new Vector2(x, y) * 64f, random, baseChance, chanceReduction);
        }

        private static void RollForRepeatableWalnutOrCheck(string walnutKey, string apLocationName, GameLocation gameLocation, Vector2 pixelOrigin, Random random, double baseChance, double chanceReduction, Func<Item> failCallback = null)
        {
            var itemToSpawn = RollForRepeatableWalnutOrCheck(walnutKey, apLocationName, random, baseChance, chanceReduction, failCallback);
            if (itemToSpawn != null)
            {
                CreateLocationDebris(itemToSpawn, pixelOrigin, gameLocation);
            }
        }

        private static Item RollForRepeatableWalnutOrCheck(string walnutKey, string apLocationName, Random random, double baseChance, double chanceReduction, Func<Item> failCallback)
        {
            var roll = random.NextDouble();
            var chanceRequired = baseChance;
            if (roll > chanceRequired)
            {
                return failCallback?.Invoke();
            }

            if (!Game1.player.team.limitedNutDrops.TryGetValue(walnutKey, out var numberWalnutsSoFar))
            {
                numberWalnutsSoFar = 0;
            }

            AddPotentiallyMissedChecks(apLocationName, Math.Min(5, numberWalnutsSoFar));

            if (numberWalnutsSoFar < 5)
            {
                numberWalnutsSoFar++;
                Game1.player.team.limitedNutDrops[walnutKey] = numberWalnutsSoFar;
                var itemToSpawnId = QualifiedItemIds.GOLDEN_WALNUT;
                if (_archipelago.SlotData.Walnutsanity.HasFlag(Archipelago.SlotData.SlotEnums.Walnutsanity.Repeatables))
                {
                    var location = $"{apLocationName} {numberWalnutsSoFar}";
                    itemToSpawnId = IDProvider.CreateApLocationItemId(location);
                }

                return ItemRegistry.Create(itemToSpawnId);
            }

            // We allow the player to get extra walnuts here, but each one is less likely than the last
            chanceRequired *= Math.Pow(chanceReduction, numberWalnutsSoFar - 4);
            if (roll > chanceRequired)
            {
                return failCallback?.Invoke();
            }

            Game1.player.team.limitedNutDrops[walnutKey] = numberWalnutsSoFar + 1;
            return ItemRegistry.Create(QualifiedItemIds.GOLDEN_WALNUT);
        }

        private static void AddPotentiallyMissedChecks(string apLocationName, int numberWalnutsSoFar)
        {
            for (var i = 1; i <= numberWalnutsSoFar; i++)
            {
                var location = $"{apLocationName} {i}";
                _locationChecker.AddCheckedLocation(location);
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
