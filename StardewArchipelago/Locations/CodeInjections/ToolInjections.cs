using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class ToolInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool AnswerDialogueAction_ToolUpgrade_Prefix(GameLocation __instance, string questionAndAnswer,
            string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Blacksmith_Upgrade")
                {
                    return true; // run original logic
                }

                __result = true;

                var utilityPriceForToolMethod =
                    _modHelper.Reflection.GetMethod(typeof(Utility), "priceForToolUpgradeLevel");
                var indexOfExtraMaterialForToolMethod =
                    _modHelper.Reflection.GetMethod(typeof(Utility), "indexOfExtraMaterialForToolUpgrade");

                var blacksmithUpgradeStock = new Dictionary<ISalable, int[]>();
                AddToolUpgradeToStock("Axe", blacksmithUpgradeStock, () => new Axe(), utilityPriceForToolMethod,
                    indexOfExtraMaterialForToolMethod);
                AddToolUpgradeToStock("Watering Can", blacksmithUpgradeStock, () => new WateringCan(),
                    utilityPriceForToolMethod, indexOfExtraMaterialForToolMethod);
                AddToolUpgradeToStock("Pickaxe", blacksmithUpgradeStock, () => new Pickaxe(), utilityPriceForToolMethod,
                    indexOfExtraMaterialForToolMethod);
                AddToolUpgradeToStock("Hoe", blacksmithUpgradeStock, () => new Hoe(), utilityPriceForToolMethod,
                    indexOfExtraMaterialForToolMethod);
                AddTrashCanUpgradeToStock(blacksmithUpgradeStock, utilityPriceForToolMethod,
                    indexOfExtraMaterialForToolMethod);

                Game1.activeClickableMenu = new ShopMenu(blacksmithUpgradeStock, who: "ClintUpgrade");

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_ToolUpgrade_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void AddToolUpgradeToStock(string toolGenericName,
            Dictionary<ISalable, int[]> blacksmithUpgradeStock, Func<Tool> toolCreationFunction,
            IReflectedMethod utilityPriceForToolMethod, IReflectedMethod indexOfExtraMaterialForToolMethod)
        {
            for (var upgradeLevel = 1; upgradeLevel < 5; upgradeLevel++)
            {
                if (!ShouldShowToolUpgradeInShop(toolGenericName, upgradeLevel))
                {
                    continue;
                }

                var newTool = toolCreationFunction();
                newTool.UpgradeLevel = upgradeLevel;

                blacksmithUpgradeStock.Add(newTool, new int[3]
                {
                    utilityPriceForToolMethod.Invoke<int>(upgradeLevel),
                    1,
                    indexOfExtraMaterialForToolMethod.Invoke<int>(upgradeLevel),
                });
            }
        }

        private static void AddTrashCanUpgradeToStock(Dictionary<ISalable, int[]> blacksmithUpgradeStock,
            IReflectedMethod utilityPriceForToolMethod, IReflectedMethod indexOfExtraMaterialForToolMethod)
        {
            for (var upgradeLevel = 1; upgradeLevel < 5; upgradeLevel++)
            {
                if (!ShouldShowToolUpgradeInShop("Trash Can", upgradeLevel))
                {
                    continue;
                }

                Tool newTool = new GenericTool("Trash Can",
                    Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description",
                        (upgradeLevel * 15).ToString() ?? ""), upgradeLevel,
                    12 + upgradeLevel, 12 + upgradeLevel);
                newTool.upgradeLevel.Value = upgradeLevel;

                blacksmithUpgradeStock.Add(newTool, new int[3]
                {
                    utilityPriceForToolMethod.Invoke<int>(upgradeLevel) / 2,
                    1,
                    indexOfExtraMaterialForToolMethod.Invoke<int>(upgradeLevel),
                });
            }
        }

        private static bool ShouldShowToolUpgradeInShop(string toolGenericName, int upgradeLevel)
        {
            var metalName = GetMetalNameForTier(upgradeLevel);
            if (_locationChecker.IsLocationChecked($"{metalName} {toolGenericName} Upgrade"))
            {
                return false;
            }

            var progressiveToolItemName = $"{UnlockManager.PROGRESSIVE_TOOL_AP_PREFIX}{toolGenericName}";
            var receivedToolsOfThatType = _archipelago.GetReceivedItemCount(progressiveToolItemName);
            return receivedToolsOfThatType >= upgradeLevel - 1;
        }

        public static bool ActionWhenPurchased_ToolUpgrade_Prefix(Tool __instance, ref bool __result)
        {
            try
            {
                var metalName = GetMetalNameForTier(__instance.UpgradeLevel);

                switch (__instance)
                {
                    case Axe _:
                        _locationChecker.AddCheckedLocation($"{metalName} Axe Upgrade");
                        break;
                    case Pickaxe _:
                        _locationChecker.AddCheckedLocation($"{metalName} Pickaxe Upgrade");
                        break;
                    case Hoe _:
                        _locationChecker.AddCheckedLocation($"{metalName} Hoe Upgrade");
                        break;
                    case WateringCan _:
                        _locationChecker.AddCheckedLocation($"{metalName} Watering Can Upgrade");
                        break;
                    case GenericTool _:
                        _locationChecker.AddCheckedLocation($"{metalName} Trash Can Upgrade");
                        break;
                    default:
                        return true; // run original logic
                }

                Game1.playSound("parry");
                Game1.exitActiveMenu();
                Game1.drawDialogue(Game1.getCharacterFromName("Clint"),
                    Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"));
                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ActionWhenPurchased_ToolUpgrade_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static string GetMetalNameForTier(int upgradeLevel)
        {
            switch (upgradeLevel)
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
                    throw new ArgumentException($"Tier {upgradeLevel} is not a value upgrade level for a tool");
            }
        }
    }
}
