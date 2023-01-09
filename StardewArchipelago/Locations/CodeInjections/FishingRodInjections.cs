using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.CodeInjections
{
    public class FishingRodInjections
    {
        public const string RECEIVED_FISHING_ROD_LEVEL_KEY = "FishingRod_Received_Level_Key";

        private const string PURCHASED_TRAINING_ROD_KEY = "Purchased_TrainingRod_Key";
        private const string PURCHASED_FIBERGLASS_ROD_KEY = "Purchased_FiberglassRod_Key";
        private const string PURCHASED_IRIDIUM_ROD_KEY = "Purchased_IridiumRod_Key";

        private const string PURCHASE_TRAINING_ROD_AP_LOCATION_NAME = "Purchase Training Rod";
        private const string PURCHASE_FIBERGLASS_ROD_AP_LOCATION_NAME = "Purchase Fiberglass Rod";
        private const string PURCHASE_IRIDIUM_ROD_AP_LOCATION_NAME = "Purchase Iridium Rod";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static Action<string> _addCheckedLocation;
        private static ModPersistence _modPersistence;

        public FishingRodInjections(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, Action<string> addCheckedLocation)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _addCheckedLocation = addCheckedLocation;
            _modPersistence = new ModPersistence();
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
            var toolSpriteSheet = Game1.toolSpriteSheet;
            if (modData[PURCHASED_TRAINING_ROD_KEY] == "0")
            {
                var trainingRod = new FishingRod(1);
                var indexOfMenuItemView = trainingRod.IndexOfMenuItemView;
                var trainingRodAPlocation = new PurchaseableArchipelagoLocation("Training Rod", PURCHASE_TRAINING_ROD_AP_LOCATION_NAME,
                    toolSpriteSheet, indexOfMenuItemView, OnPurchaseTrainingRodLocation, _archipelago);
                fishShopStock.Add(trainingRodAPlocation, new[] { 25, 1 });
            }
            if (Game1.player.fishingLevel.Value >= 2 && modData[PURCHASED_FIBERGLASS_ROD_KEY] == "0")
            {
                var fiberglassRod = new FishingRod(2);
                var indexOfMenuItemView = fiberglassRod.IndexOfMenuItemView;
                var fiberglassRodAPlocation = new PurchaseableArchipelagoLocation("Fiberglass Rod", PURCHASE_FIBERGLASS_ROD_AP_LOCATION_NAME,
                    toolSpriteSheet, indexOfMenuItemView, OnPurchaseFiberglassRodLocation, _archipelago);
                fishShopStock.Add(fiberglassRodAPlocation, new[] { 1800, 1 });
            }
            if (Game1.player.fishingLevel.Value >= 6 && modData[PURCHASED_IRIDIUM_ROD_KEY] == "0")
            {
                var iridiumRod = new FishingRod(3);
                var indexOfMenuItemView = iridiumRod.IndexOfMenuItemView;
                var iridiumRodAPLocation = new PurchaseableArchipelagoLocation("Iridium Rod", PURCHASE_IRIDIUM_ROD_AP_LOCATION_NAME,
                    toolSpriteSheet, indexOfMenuItemView, OnPurchaseIridiumRodLocation, _archipelago);
                fishShopStock.Add(iridiumRodAPLocation, new[] { 7500, 1 });
            }
        }

        private static void AddFishingTools(Dictionary<ISalable, int[]> fishShopStock)
        {
            var modData = Game1.getFarm().modData;
            var receivedFishingRodLevel = int.Parse(modData[RECEIVED_FISHING_ROD_LEVEL_KEY]);
            if (receivedFishingRodLevel >= 1)
            {
                var trainingRod = new FishingRod(1);
                fishShopStock.Add(trainingRod, new[] { 25, int.MaxValue });
            }

            if (receivedFishingRodLevel >= 2)
            {
                var bambooPole = new FishingRod(0);
                fishShopStock.Add(bambooPole, new[] { 500, int.MaxValue });
            }

            if (receivedFishingRodLevel >= 3)
            {
                var fiberglassRod = new FishingRod(2);
                fishShopStock.Add(fiberglassRod, new[] { 1800, int.MaxValue });
            }

            if (receivedFishingRodLevel >= 4)
            {
                var iridiumRod = new FishingRod(3);
                fishShopStock.Add(iridiumRod, new[] { 7500, int.MaxValue });
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
            _addCheckedLocation(PURCHASE_TRAINING_ROD_AP_LOCATION_NAME);
            _modPersistence.SetToOneModDataValue(PURCHASED_TRAINING_ROD_KEY);
        }

        private static void OnPurchaseFiberglassRodLocation()
        {
            _addCheckedLocation(PURCHASE_FIBERGLASS_ROD_AP_LOCATION_NAME);
            _modPersistence.SetToOneModDataValue(PURCHASED_FIBERGLASS_ROD_KEY);
        }

        private static void OnPurchaseIridiumRodLocation()
        {
            _addCheckedLocation(PURCHASE_IRIDIUM_ROD_AP_LOCATION_NAME);
            _modPersistence.SetToOneModDataValue(PURCHASED_IRIDIUM_ROD_KEY);
        }

        public static void InitializeFishingRodsModDataValues()
        {
            _modPersistence.InitializeModDataValue(RECEIVED_FISHING_ROD_LEVEL_KEY, "0");

            _modPersistence.InitializeModDataValue(PURCHASED_TRAINING_ROD_KEY, "0");
            _modPersistence.InitializeModDataValue(PURCHASED_FIBERGLASS_ROD_KEY, "0");
            _modPersistence.InitializeModDataValue(PURCHASED_IRIDIUM_ROD_KEY, "0");
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
