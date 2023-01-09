using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class ToolInjections
    {
        private const string PICKAXE_UPGRADE_LEVEL_KEY = "Pickaxe_Upgrade_Level_Key";
        private const string AXE_UPGRADE_LEVEL_KEY = "Axe_Upgrade_Level_Key";
        private const string HOE_UPGRADE_LEVEL_KEY = "Hoe_Upgrade_Level_Key";
        private const string WATERINGCAN_UPGRADE_LEVEL_KEY = "WateringCan_Upgrade_Level_Key";
        private const string TRASHCAN_UPGRADE_LEVEL_KEY = "TrashCan_Upgrade_Level_Key";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static LocationChecker _locationChecker;
        private static ModPersistence _modPersistence;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _locationChecker = locationChecker;
            _modPersistence = new ModPersistence();
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
                    (newUpgradeLevel * 15).ToString() ?? ""), newUpgradeLevel,
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
                        _modPersistence.IncrementModDataValue(AXE_UPGRADE_LEVEL_KEY);
                        _locationChecker.AddCheckedLocation($"{GetMetalNameForTier(modData[AXE_UPGRADE_LEVEL_KEY])} Axe Upgrade");
                        break;
                    case Pickaxe _:
                        _modPersistence.IncrementModDataValue(PICKAXE_UPGRADE_LEVEL_KEY);
                        _locationChecker.AddCheckedLocation($"{GetMetalNameForTier(modData[PICKAXE_UPGRADE_LEVEL_KEY])} Pickaxe Upgrade");
                        break;
                    case Hoe _:
                        _modPersistence.IncrementModDataValue(HOE_UPGRADE_LEVEL_KEY);
                        _locationChecker.AddCheckedLocation($"{GetMetalNameForTier(modData[HOE_UPGRADE_LEVEL_KEY])} Hoe Upgrade");
                        break;
                    case WateringCan _:
                        _modPersistence.IncrementModDataValue(WATERINGCAN_UPGRADE_LEVEL_KEY);
                        _locationChecker.AddCheckedLocation($"{GetMetalNameForTier(modData[WATERINGCAN_UPGRADE_LEVEL_KEY])} Watering Can Upgrade");
                        break;
                    case GenericTool _:
                        _modPersistence.IncrementModDataValue(TRASHCAN_UPGRADE_LEVEL_KEY);
                        _locationChecker.AddCheckedLocation($"{GetMetalNameForTier(modData[TRASHCAN_UPGRADE_LEVEL_KEY])} Trash Can Upgrade");
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

        private static void InitializeToolUpgradeModDataValues()
        {
            _modPersistence.InitializeModDataValue(PICKAXE_UPGRADE_LEVEL_KEY, "0");
            _modPersistence.InitializeModDataValue(AXE_UPGRADE_LEVEL_KEY, "0");
            _modPersistence.InitializeModDataValue(HOE_UPGRADE_LEVEL_KEY, "0");
            _modPersistence.InitializeModDataValue(WATERINGCAN_UPGRADE_LEVEL_KEY, "0");
            _modPersistence.InitializeModDataValue(TRASHCAN_UPGRADE_LEVEL_KEY, "0");
        }
    }
}
