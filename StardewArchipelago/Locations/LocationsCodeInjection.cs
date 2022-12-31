using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewArchipelago.Locations
{
    internal class LocationsCodeInjection
    {
        private const string BACKPACK_UPGRADE_LEVEL_KEY = "Backpack_Upgrade_Level_Key";
        private const string PICKAXE_UPGRADE_LEVEL_KEY = "Pickaxe_Upgrade_Level_Key";
        private const string AXE_UPGRADE_LEVEL_KEY = "Axe_Upgrade_Level_Key";
        private const string HOE_UPGRADE_LEVEL_KEY = "Hoe_Upgrade_Level_Key";
        private const string WATERINGCAN_UPGRADE_LEVEL_KEY = "WateringCan_Upgrade_Level_Key";
        private const string TRASHCAN_UPGRADE_LEVEL_KEY = "TrashCan_Upgrade_Level_Key";
        private const string GOT_GOLDEN_SCYTHE_KEY = "Got_GoldenScythe_Key";

        public const string RECEIVED_TRAINING_ROD_KEY = "Got_TrainingRod_Key";
        public const string RECEIVED_BAMBOO_POLE_KEY = "Got_BambooPole_Key";
        public const string RECEIVED_FIBERGLASS_ROD_KEY = "Got_FiberglassRod_Key";
        public const string RECEIVED_IRIDIUM_ROD_KEY = "Got_IridiumRod_Key";

        private const string PURCHASED_TRAINING_ROD_KEY = "Purchased_TrainingRod_Key";
        private const string PURCHASED_FIBERGLASS_ROD_KEY = "Purchased_FiberglassRod_Key";
        private const string PURCHASED_IRIDIUM_ROD_KEY = "Purchased_IridiumRod_Key";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static BundleReader _bundleReader;
        private static Action<string> _addCheckedLocation;

        public LocationsCodeInjection(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, Action<string> addCheckedLocation)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _bundleReader = bundleReader;
            _addCheckedLocation = addCheckedLocation;
        }

        public void DoAreaCompleteReward(int whichArea)
        {
            var AreaAPLocationName = "";
            switch ((Area)whichArea)
            {
                case Area.Pantry:
                    AreaAPLocationName = "Complete Pantry";
                    break;
                case Area.CraftsRoom:
                    AreaAPLocationName = "Complete Crafts Room";
                    break;
                case Area.FishTank:
                    AreaAPLocationName = "Complete Fish Tank";
                    break;
                case Area.BoilerRoom:
                    AreaAPLocationName = "Complete Boiler Room";
                    break;
                case Area.Vault:
                    AreaAPLocationName = "Complete Vault";
                    break;
                case Area.Bulletin:
                    AreaAPLocationName = "Complete Bulletin Board";
                    break;
            }
            _addCheckedLocation(AreaAPLocationName);
        }

        public static void CheckForRewards_PostFix(JunimoNoteMenu __instance)
        {
            try
            {
                var bundleStates = _bundleReader.ReadCurrentBundleStates();
                var completedBundleNames = bundleStates.Where(x => x.IsCompleted).Select(x => x.RelatedBundle.BundleName + " Bundle");
                foreach (var completedBundleName in completedBundleNames)
                {
                    _addCheckedLocation(completedBundleName);    
                }

                var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
                var bundleRewardsDictionary = communityCenter.bundleRewards;
                foreach (var bundleRewardKey in bundleRewardsDictionary.Keys)
                {
                    bundleRewardsDictionary[bundleRewardKey] = false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForRewards_PostFix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool AnswerDialogueAction_BackPackPurchase_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Backpack_Purchase")
                {
                    return true; // run original logic
                }

                __result = true;
                var modData = Game1.getFarm().modData;
                InitializeModDataValue(BACKPACK_UPGRADE_LEVEL_KEY, "0");

                if (Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] == "0" && Game1.player.Money >= 2000)
                {
                    Game1.player.Money -= 2000;
                    modData[BACKPACK_UPGRADE_LEVEL_KEY] = "1";
                    _addCheckedLocation("Large Pack");
                    return false; // don't run original logic
                }

                if (Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] == "1" && Game1.player.Money >= 10000)
                {
                    Game1.player.Money -= 10000;
                    modData[BACKPACK_UPGRADE_LEVEL_KEY] = "2";
                    _addCheckedLocation("Deluxe Pack");
                    return false; // don't run original logic
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_BackPackPurchase_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AnswerDialogueAction_ToolUpgrade_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Blacksmith_Upgrade")
                {
                    return true; // run original logic
                }

                __result = true;
                var modData = Game1.getFarm().modData;
                InitializeToolUpgradeModDataValues();

                var farmer = Game1.player;
                var utilityPriceForToolMethod = _modHelper.Reflection.GetMethod(typeof(Utility), "priceForToolUpgradeLevel");
                var indexOfExtraMaterialForToolMethod = _modHelper.Reflection.GetMethod(typeof(Utility), "indexOfExtraMaterialForToolUpgrade");

                var blacksmithUpgradeStock = new Dictionary<ISalable, int[]>();
                AddToolUpgradeToStock(modData, blacksmithUpgradeStock, AXE_UPGRADE_LEVEL_KEY, () => new Axe(), utilityPriceForToolMethod, indexOfExtraMaterialForToolMethod);
                AddToolUpgradeToStock(modData, blacksmithUpgradeStock, WATERINGCAN_UPGRADE_LEVEL_KEY, () => new WateringCan(), utilityPriceForToolMethod, indexOfExtraMaterialForToolMethod);
                AddToolUpgradeToStock(modData, blacksmithUpgradeStock, PICKAXE_UPGRADE_LEVEL_KEY, () => new Pickaxe(), utilityPriceForToolMethod, indexOfExtraMaterialForToolMethod);
                AddToolUpgradeToStock(modData, blacksmithUpgradeStock, HOE_UPGRADE_LEVEL_KEY, () => new Hoe(), utilityPriceForToolMethod, indexOfExtraMaterialForToolMethod);
                AddTrashCanUpgradeToStock(modData, blacksmithUpgradeStock, utilityPriceForToolMethod, indexOfExtraMaterialForToolMethod);

                Game1.activeClickableMenu = new ShopMenu(blacksmithUpgradeStock, who: "ClintUpgrade");

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_ToolUpgrade_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void AddToolUpgradeToStock(ModDataDictionary modData, Dictionary<ISalable, int[]> blacksmithUpgradeStock,
            string toolUpgradeKey, Func<Tool> toolCreationFunction,
            IReflectedMethod utilityPriceForToolMethod, IReflectedMethod indexOfExtraMaterialForToolMethod)
        {
            var currentToolLevel = int.Parse(modData[toolUpgradeKey]);
            if (currentToolLevel >= 4)
            {
                return;
            }

            var newTool = toolCreationFunction();
            newTool.UpgradeLevel = currentToolLevel + 1;

            blacksmithUpgradeStock.Add(newTool, new int[3]
            {
                utilityPriceForToolMethod.Invoke<int>(newTool.UpgradeLevel),
                1,
                indexOfExtraMaterialForToolMethod.Invoke<int>(newTool.UpgradeLevel),
            });
        }

        private static void AddTrashCanUpgradeToStock(ModDataDictionary modData, Dictionary<ISalable, int[]> blacksmithUpgradeStock,
            IReflectedMethod utilityPriceForToolMethod, IReflectedMethod indexOfExtraMaterialForToolMethod)
        {
            var currentTrashCanLevel = int.Parse(modData[TRASHCAN_UPGRADE_LEVEL_KEY]);
            if (currentTrashCanLevel >= 4)
            {
                return;
            }
            
            var newUpgradeLevel = currentTrashCanLevel + 1;
            Tool newTool = new GenericTool("Trash Can",
                Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description",
                    ((newUpgradeLevel * 15).ToString() ?? "")), newUpgradeLevel,
                13 + currentTrashCanLevel, 13 + currentTrashCanLevel);
            newTool.upgradeLevel.Value = newUpgradeLevel;

            blacksmithUpgradeStock.Add(newTool, new int[3]
            {
                utilityPriceForToolMethod.Invoke<int>(newTool.UpgradeLevel) / 2,
                1,
                indexOfExtraMaterialForToolMethod.Invoke<int>(newTool.UpgradeLevel),
            });
        }

        public static bool ActionWhenPurchased_ToolUpgrade_Prefix(Tool __instance, ref bool __result)
        {
            try
            {
                var modData = Game1.getFarm().modData;
                InitializeToolUpgradeModDataValues();

                switch (__instance)
                {
                    case Axe _:
                        IncrementModDataValue(AXE_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[AXE_UPGRADE_LEVEL_KEY])} Axe Upgrade");
                        break;
                    case Pickaxe _:
                        IncrementModDataValue(PICKAXE_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[PICKAXE_UPGRADE_LEVEL_KEY])} Pickaxe Upgrade");
                        break;
                    case Hoe _:
                        IncrementModDataValue(HOE_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[HOE_UPGRADE_LEVEL_KEY])} Hoe Upgrade");
                        break;
                    case WateringCan _:
                        IncrementModDataValue(WATERINGCAN_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[WATERINGCAN_UPGRADE_LEVEL_KEY])} Watering Can Upgrade");
                        break;
                    case GenericTool _:
                        IncrementModDataValue(TRASHCAN_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[TRASHCAN_UPGRADE_LEVEL_KEY])} Trash Can Upgrade");
                        break;
                    default:
                        return true; // run original logic
                }

                Game1.playSound("parry");
                Game1.exitActiveMenu();
                Game1.drawDialogue(Game1.getCharacterFromName("Clint"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"));
                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ActionWhenPurchased_ToolUpgrade_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static string GetMetalNameForTier(string s)
        {
            var value = int.Parse(s);
            switch (value)
            {
                case 0:
                    return "Basic";
                case 1:
                    return "Copper";
                case 2:
                    return "Iron";
                case 3:
                    return "Gold";
                case 4:
                    return "Iridium";
                default:
                    throw new ArgumentException($"Tier {value} is not a value upgrade level for a tool");
            }
        }

        public static bool PerformAction_BuyBackpack_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var actionParts = action.Split(' ');
                var actionName = actionParts[0];
                if (actionName == "BuyBackpack")
                {
                    BuyBackPackArchipelago(__instance, out __result);
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_BuyBackpack_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool PerformAction_GoldenScythe_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var actionParts = action.Split(' ');
                var actionName = actionParts[0];
                if (actionName == "GoldenScythe")
                {
                    __result = true;
                    var modData = Game1.getFarm().modData;
                    InitializeModDataValue(GOT_GOLDEN_SCYTHE_KEY, "0");

                    if (modData[GOT_GOLDEN_SCYTHE_KEY] == "0")
                    {
                        Game1.playSound("parry");
                        __instance.setMapTileIndex(29, 4, 245, "Front");
                        __instance.setMapTileIndex(30, 4, 246, "Front");
                        __instance.setMapTileIndex(29, 5, 261, "Front");
                        __instance.setMapTileIndex(30, 5, 262, "Front");
                        __instance.setMapTileIndex(29, 6, 277, "Buildings");
                        __instance.setMapTileIndex(30, 56, 278, "Buildings");
                        _addCheckedLocation("Grim Reaper statue");
                        modData[GOT_GOLDEN_SCYTHE_KEY] = "1";
                        return false; // don't run original logic
                    }

                    Game1.changeMusicTrack("none");
                    __instance.performTouchAction("MagicWarp Mine 67 10", Game1.player.getStandingPosition());
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_GoldenScythe_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void BuyBackPackArchipelago(GameLocation __instance, out bool __result)
        {
            __result = true;

            var modData = Game1.getFarm().modData;
            InitializeModDataValue(BACKPACK_UPGRADE_LEVEL_KEY, "0");

            var responsePurchaseLevel1 = new Response("Purchase",
                Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
            var responsePurchaseLevel2 = new Response("Purchase",
                Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
            var responseDontPurchase = new Response("Not",
                Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
            if (modData[BACKPACK_UPGRADE_LEVEL_KEY] == "0")
            {
                __instance.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"),
                    new Response[2]
                    {
                        responsePurchaseLevel1,
                        responseDontPurchase
                    }, "Backpack");
            }
            else if (modData[BACKPACK_UPGRADE_LEVEL_KEY] == "1")
            {
                __instance.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"),
                    new Response[2]
                    {
                        responsePurchaseLevel2,
                        responseDontPurchase
                    }, "Backpack");
            }
        }

        public static bool CheckForAction_MineshaftChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value || Game1.mine == null)
                {
                    return true; // run original logic
                }

                if (__instance.items.Count <= 0)
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                Game1.mine.chestConsumed();
                var obj = __instance.items[0];
                __instance.items[0] = null;
                __instance.items.RemoveAt(0);
                __result = true;
                
                _addCheckedLocation($"The Mines Floor {Game1.mine.mineLevel} Treasure");

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_MineshaftChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AddLevelChests_Level120_Prefix(MineShaft __instance)
        {
            try
            {
                if (__instance.mineLevel != 120 || Game1.player.chestConsumedMineLevels.ContainsKey(120))
                {
                    return true; // run original logic
                }

                Game1.player.completeQuest(18);
                Game1.getSteamAchievement("Achievement_TheBottom");
                var chestPosition = new Vector2(9f, 9f);
                List<Item> items = new List<Item>();
                items.Add(new MeleeWeapon(8));
                __instance.overlayObjects[chestPosition] = new Chest(0, items, chestPosition)
                {
                    Tint = Color.Pink
                };

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_MineshaftChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool SkipEvent_BambooPole_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != 739330)
                {
                    return true; // run original logic
                }

                SkipBambooPoleEventArchipelago(__instance);
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkipEvent_BambooPole_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AwardFestivalPrize_BambooPole_Prefix(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                var festivalWinnersField = _modHelper.Reflection.GetField<HashSet<long>>(__instance, "festivalWinners");
                if (__instance.id != 739330 || festivalWinnersField.GetValue().Contains(Game1.player.UniqueMultiplayerID) || split.Length <= 1 || split[1].ToLower() != "rod")
                {
                    return true; // run original logic
                }
                
                OnCheckBambooPoleLocation();

                if (Game1.activeClickableMenu == null)
                    __instance.CurrentCommand++;
                __instance.CurrentCommand++;

                SkipBambooPoleEventArchipelago(__instance);
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AwardFestivalPrize_BambooPole_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void SkipBambooPoleEventArchipelago(Event __instance)
        {
            if (__instance.playerControlSequence)
            {
                __instance.EndPlayerControlSequence();
            }

            Game1.playSound("drumkit6");

            var actorPositionsAfterMoveField = _modHelper.Reflection.GetField<Dictionary<string, Vector3>>(__instance, "actorPositionsAfterMove");
            actorPositionsAfterMoveField.GetValue().Clear();

            foreach (var actor in __instance.actors)
            {
                var ignoreStopAnimation = actor.Sprite.ignoreStopAnimation;
                actor.Sprite.ignoreStopAnimation = true;
                actor.Halt();
                actor.Sprite.ignoreStopAnimation = ignoreStopAnimation;
                __instance.resetDialogueIfNecessary(actor);
            }

            __instance.farmer.Halt();
            __instance.farmer.ignoreCollisions = false;
            Game1.exitActiveMenu();
            Game1.dialogueUp = false;
            Game1.dialogueTyping = false;
            Game1.pauseTime = 0.0f;

            OnCheckBambooPoleLocation();

            __instance.endBehaviors(new string[4]
            {
                "end",
                "position",
                "43",
                "36"
            }, Game1.currentLocation);
        }

        public static bool GetFishShopStock_Prefix(Farmer who, ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                InitializeFishingRodsModDataValues();

                var fishShopStock = new Dictionary<ISalable, int[]>();
                AddFishingObjects(fishShopStock);
                AddFishingToolsAPLocations(fishShopStock);
                AddFishingTools(fishShopStock);
                AddFishingFurniture(fishShopStock);
                AddItemsFromPlayerToSell(fishShopStock);
                __result = fishShopStock;

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkipEvent_BambooPole_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void AddFishingObjects(Dictionary<ISalable, int[]> fishShopStock)
        {
            fishShopStock.Add(new StardewValley.Object(TROUT_SOUP_ID, 1), new[] { 250, int.MaxValue });
            if (Game1.player.fishingLevel.Value >= 2)
            {
                fishShopStock.Add(new StardewValley.Object(BAIT_ID, 1), new[] { 5, int.MaxValue });
            }

            if (Game1.player.fishingLevel.Value >= 3)
            {
                fishShopStock.Add(new StardewValley.Object(CRAB_POT_ID, 1), new[] { 1500, int.MaxValue });
            }

            if (Game1.player.fishingLevel.Value >= 6)
            {
                fishShopStock.Add(new StardewValley.Object(SPINNER_ID, 1), new[] { 500, int.MaxValue });
                fishShopStock.Add(new StardewValley.Object(TRAP_BOBBER_ID, 1), new[] { 500, int.MaxValue });
                fishShopStock.Add(new StardewValley.Object(LEAD_BOBBER_ID, 1), new[] { 200, int.MaxValue });
            }

            if (Game1.player.fishingLevel.Value >= 7)
            {
                fishShopStock.Add(new StardewValley.Object(TREASURE_HUNTER_ID, 1), new[] { 750, int.MaxValue });
                fishShopStock.Add(new StardewValley.Object(CORK_BOBBER_ID, 1), new[] { 750, int.MaxValue });
            }

            if (Game1.player.fishingLevel.Value >= 8)
            {
                fishShopStock.Add(new StardewValley.Object(BARBED_HOOK_ID, 1), new[] { 1000, int.MaxValue });
                fishShopStock.Add(new StardewValley.Object(DRESSED_SPINNER_ID, 1), new[] { 1000, int.MaxValue });
            }

            if (Game1.player.fishingLevel.Value >= 9)
            {
                fishShopStock.Add(new StardewValley.Object(MAGNET_ID, 1), new[] { 1000, int.MaxValue });
            }
        }

        private static void AddFishingToolsAPLocations(Dictionary<ISalable, int[]> fishShopStock)
        {
            var modData = Game1.getFarm().modData;
            if (modData[PURCHASED_TRAINING_ROD_KEY] == "0")
            {
                var trainingRod = new FishingRod(1);
                fishShopStock.Add(new PurchaseableArchipelagoLocation("Archipelago Check: Training Rod", Game1.toolSpriteSheet, trainingRod.IndexOfMenuItemView, OnPurchaseTrainingRodLocation), new[] { 25, 1 });
            }
            if (Game1.player.fishingLevel.Value >= 2 && modData[PURCHASED_FIBERGLASS_ROD_KEY] == "0")
            {
                var fiberglassRod = new FishingRod(2);
                fishShopStock.Add(new PurchaseableArchipelagoLocation("Archipelago Check: Fiberglass Rod", Game1.toolSpriteSheet, fiberglassRod.IndexOfMenuItemView, OnPurchaseFiberglassRodLocation), new[] { 1800, 1 });
            }
            if (Game1.player.fishingLevel.Value >= 6 && modData[PURCHASED_IRIDIUM_ROD_KEY] == "0")
            {
                var iridiumRod = new FishingRod(3);
                fishShopStock.Add(new PurchaseableArchipelagoLocation("Archipelago Check: Iridium Rod", Game1.toolSpriteSheet, iridiumRod.IndexOfMenuItemView, OnPurchaseIridiumRodLocation), new[] { 7500, 1 });
            }
        }

        private static void AddFishingTools(Dictionary<ISalable, int[]> fishShopStock)
        {
            var modData = Game1.getFarm().modData;
            if (modData[RECEIVED_TRAINING_ROD_KEY] == "1")
            {
                fishShopStock.Add(new FishingRod(1), new[] { 25, int.MaxValue });
            }

            if (modData[RECEIVED_BAMBOO_POLE_KEY] == "1")
            {
                fishShopStock.Add(new FishingRod(0), new[] { 500, int.MaxValue });
            }

            if (modData[RECEIVED_FIBERGLASS_ROD_KEY] == "1")
            {
                fishShopStock.Add(new FishingRod(2), new[] { 1800, int.MaxValue });
            }

            if (modData[RECEIVED_IRIDIUM_ROD_KEY] == "1")
            {
                fishShopStock.Add(new FishingRod(3), new[] { 7500, int.MaxValue });
            }

            if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
            {
                fishShopStock.Add(new Pan(), new[] { 2500, int.MaxValue });
            }
        }

        private static void AddFishingFurniture(Dictionary<ISalable, int[]> fishShopStock)
        {
            fishShopStock.Add(new FishTankFurniture(2304, Vector2.Zero), new[] { 2000, int.MaxValue });
            fishShopStock.Add(new FishTankFurniture(2322, Vector2.Zero), new[] { 500, int.MaxValue });
            if (Game1.player.mailReceived.Contains("WillyTropicalFish"))
            {
                fishShopStock.Add(new FishTankFurniture(2312, Vector2.Zero), new[] { 5000, int.MaxValue });
            }

            fishShopStock.Add(new BedFurniture(2502, Vector2.Zero), new[] { 25000, int.MaxValue });
        }

        private static void AddItemsFromPlayerToSell(Dictionary<ISalable, int[]> fishShopStock)
        {
            var locationFromName = Game1.getLocationFromName("FishShop");
            if (locationFromName is not ShopLocation fishShop) return;
            foreach (var key in fishShop.itemsFromPlayerToSell)
            {
                if (key.Stack <= 0) continue;
                var num = key.salePrice();
                if (key is StardewValley.Object)
                {
                    num = (key as StardewValley.Object).sellToStorePrice();
                }
                fishShopStock.Add(key, new[] { num, key.Stack });
            }
        }

        private static void OnCheckBambooPoleLocation()
        {
            _addCheckedLocation("Bamboo Pole Cutscene");
        }

        private static void OnPurchaseTrainingRodLocation()
        {
            _addCheckedLocation("Purchase Training Rod");
            SetToOneModDataValue(PURCHASED_TRAINING_ROD_KEY);
        }

        private static void OnPurchaseFiberglassRodLocation()
        {
            _addCheckedLocation("Purchase Fiberglass Rod");
            SetToOneModDataValue(PURCHASED_FIBERGLASS_ROD_KEY);
        }

        private static void OnPurchaseIridiumRodLocation()
        {
            _addCheckedLocation("Purchase Iridium Rod");
            SetToOneModDataValue(PURCHASED_IRIDIUM_ROD_KEY);
        }

        private static void InitializeToolUpgradeModDataValues()
        {
            InitializeModDataValue(PICKAXE_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(AXE_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(HOE_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(WATERINGCAN_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(TRASHCAN_UPGRADE_LEVEL_KEY, "0");
        }

        private static void InitializeFishingRodsModDataValues()
        {
            InitializeModDataValue(RECEIVED_TRAINING_ROD_KEY, "0");
            InitializeModDataValue(RECEIVED_BAMBOO_POLE_KEY, "0");
            InitializeModDataValue(RECEIVED_FIBERGLASS_ROD_KEY, "0");
            InitializeModDataValue(RECEIVED_IRIDIUM_ROD_KEY, "0");

            InitializeModDataValue(PURCHASED_TRAINING_ROD_KEY, "0");
            InitializeModDataValue(PURCHASED_FIBERGLASS_ROD_KEY, "0");
            InitializeModDataValue(PURCHASED_IRIDIUM_ROD_KEY, "0");
        }

        private static void InitializeModDataValue(string key, string defaultValue)
        {
            var modData = Game1.getFarm().modData;
            if (!modData.ContainsKey(key))
            {
                modData.Add(key, defaultValue);
            }
        }

        public static void SetToOneModDataValue(string key)
        {
            var modData = Game1.getFarm().modData;
            modData[key] = "1";
        }

        private static void IncrementModDataValue(string key, int increment = 1)
        {
            var modData = Game1.getFarm().modData;
            modData[key] = (int.Parse(modData[key]) + increment).ToString();
        }

        private const int TROUT_SOUP_ID = 219;
        private const int BAIT_ID = 685;
        private const int CRAB_POT_ID = 710;
        private const int SPINNER_ID = 686;
        private const int TRAP_BOBBER_ID = 694;
        private const int LEAD_BOBBER_ID = 692;
        private const int TREASURE_HUNTER_ID = 693;
        private const int CORK_BOBBER_ID = 695;
        private const int BARBED_HOOK_ID = 691;
        private const int DRESSED_SPINNER_ID = 687;
        private const int MAGNET_ID = 703;
    }
}
