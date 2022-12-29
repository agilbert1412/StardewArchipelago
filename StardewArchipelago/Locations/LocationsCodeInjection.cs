using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    AreaAPLocationName = "Complete Bulletin";
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
                InitializeModDataValue(modData, BACKPACK_UPGRADE_LEVEL_KEY, "0");

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
                InitializeToolUpgradeModDataValues(modData);

                var farmer = Game1.player;
                var utilityPriceForToolMethod = _modHelper.Reflection.GetMethod(typeof(Utility), "priceForToolUpgradeLevel");
                var indexOfExtraMaterialForToolMethod = _modHelper.Reflection.GetMethod(typeof(Utility), "indexOfExtraMaterialForToolUpgrade");

                Dictionary<ISalable, int[]> blacksmithUpgradeStock = new Dictionary<ISalable, int[]>();
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

            string name = "";
            switch (currentTrashCanLevel + 1)
            {
                case 1:
                    name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14299",
                        Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
                    break;
                case 2:
                    name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14300",
                        Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
                    break;
                case 3:
                    name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14301",
                        Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
                    break;
                case 4:
                    name = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14302",
                        Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
                    break;
            }

            Tool newTool = new GenericTool(name,
                Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description",
                    (((currentTrashCanLevel + 1) * 15).ToString() ?? "")), currentTrashCanLevel + 1,
                13 + currentTrashCanLevel, 13 + currentTrashCanLevel);

            blacksmithUpgradeStock.Add(newTool, new int[3]
            {
                utilityPriceForToolMethod.Invoke<int>(newTool.UpgradeLevel),
                1,
                indexOfExtraMaterialForToolMethod.Invoke<int>(newTool.UpgradeLevel),
            });
        }

        public static bool ActionWhenPurchased_ToolUpgrade_Prefix(Tool __instance, ref bool __result)
        {
            try
            {
                var modData = Game1.getFarm().modData;
                InitializeToolUpgradeModDataValues(modData);

                switch (__instance)
                {
                    case Axe _:
                        IncrementModDataValue(modData, AXE_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[AXE_UPGRADE_LEVEL_KEY])} Axe Upgrade");
                        break;
                    case Pickaxe _:
                        IncrementModDataValue(modData, PICKAXE_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[PICKAXE_UPGRADE_LEVEL_KEY])} Pickaxe Upgrade");
                        break;
                    case Hoe _:
                        IncrementModDataValue(modData, HOE_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[HOE_UPGRADE_LEVEL_KEY])} Hoe Upgrade");
                        break;
                    case WateringCan _:
                        IncrementModDataValue(modData, WATERINGCAN_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[WATERINGCAN_UPGRADE_LEVEL_KEY])} Watering Can Upgrade");
                        break;
                    case GenericTool _:
                        IncrementModDataValue(modData, TRASHCAN_UPGRADE_LEVEL_KEY);
                        _addCheckedLocation($"{GetMetalNameForTier(modData[WATERINGCAN_UPGRADE_LEVEL_KEY])} Trash Can Upgrade");
                        break;
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

        public static bool PerformAction_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true;
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
                _monitor.Log($"Failed in {nameof(PerformAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void BuyBackPackArchipelago(GameLocation __instance, out bool __result)
        {
            __result = true;

            var modData = Game1.getFarm().modData;
            InitializeModDataValue(modData, BACKPACK_UPGRADE_LEVEL_KEY, "0");

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

        public static bool CheckForAction_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
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
                _monitor.Log($"Failed in {nameof(CheckForAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void InitializeToolUpgradeModDataValues(ModDataDictionary modData)
        {
            InitializeModDataValue(modData, PICKAXE_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(modData, AXE_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(modData, HOE_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(modData, WATERINGCAN_UPGRADE_LEVEL_KEY, "0");
            InitializeModDataValue(modData, TRASHCAN_UPGRADE_LEVEL_KEY, "0");
        }

        private static void InitializeModDataValue(ModDataDictionary modData, string key, string defaultValue)
        {
            if (!modData.ContainsKey(key))
            {
                modData.Add(key, defaultValue);
            }
        }

        private static void IncrementModDataValue(ModDataDictionary modData, string key, int increment = 1)
        {
            modData[key] = (int.Parse(modData[key]) + increment).ToString();
        }
    }
}
