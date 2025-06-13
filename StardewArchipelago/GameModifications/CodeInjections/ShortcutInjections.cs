using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using xTile.Dimensions;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class ShortcutInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_OpenMountainShortcuts_Postfix(Mountain __instance, bool force)
        {
            try
            {
                OpenMountainShortcuts(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_OpenMountainShortcuts_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_OpenForestShortcuts_Postfix(Forest __instance, bool force)
        {
            try
            {
                OpenForestShortcuts(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_OpenForestShortcuts_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_OpenBeachShortcuts_Postfix(Beach __instance, bool force)
        {
            try
            {
                OpenBeachShortcuts(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_OpenBeachShortcuts_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_OpenBackwoodsShortcuts_Postfix(GameLocation __instance, bool force)
        {
            try
            {
                if (__instance.Name != "Backwoods")
                {
                    return;
                }

                OpenBackwoodsShortcuts(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_OpenBackwoodsShortcuts_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void OpenTownShortcuts(Town town)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            OpenTownToMountainShortcut(town);
            OpenJojamartToMountainShortcut(town);
            OpenTownToTidePoolsShortcut(town);
        }

        public static void OpenMountainShortcuts(Mountain mountain)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            if (!_archipelago.HasReceivedItem("Mountain Shortcuts"))
            {
                return;
            }

            mountain.ApplyMapOverride("Mountain_Shortcuts");
            mountain.waterTiles[81, 37] = false;
            mountain.waterTiles[82, 37] = false;
            mountain.waterTiles[83, 37] = false;
            mountain.waterTiles[84, 37] = false;
            mountain.waterTiles[85, 37] = false;
            mountain.waterTiles[85, 38] = false;
            mountain.waterTiles[85, 39] = false;
            mountain.waterTiles[85, 40] = false;
        }

        public static void OpenForestShortcuts(Forest forest)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            if (!_archipelago.HasReceivedItem("Forest To Beach Shortcut"))
            {
                return;
            }

            // private bool hasShownCCUpgrade;
            var hasShownCCUpgradeField = _modHelper.Reflection.GetField<bool>(forest, "hasShownCCUpgrade");
            if (hasShownCCUpgradeField.GetValue())
            {
                return;
            }
            forest.removeTile(119, 36, "Buildings");
            LargeTerrainFeature largeTerrainFeature1 = null;
            foreach (var largeTerrainFeature2 in forest.largeTerrainFeatures)
            {
                if (largeTerrainFeature2.Tile == new Vector2(119f, 35f))
                {
                    largeTerrainFeature1 = largeTerrainFeature2;
                    break;
                }
            }
            if (largeTerrainFeature1 != null)
            {
                forest.largeTerrainFeatures.Remove(largeTerrainFeature1);
            }
            hasShownCCUpgradeField.SetValue(true);
            forest.warps.Add(new Warp(120, 35, "Beach", 0, 6, false));
            forest.warps.Add(new Warp(120, 36, "Beach", 0, 6, false));
        }

        public static void OpenBeachShortcuts(Beach beach)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            // private bool hasShownCCUpgrade;
            //var hasShownCCUpgradeField = _modHelper.Reflection.GetField<bool>(beach, "hasShownCCUpgrade");
            //if (hasShownCCUpgradeField.GetValue())
            //{
            //    return;
            //}
            //hasShownCCUpgradeField.SetValue(true);
            OpenBeachToForestShortcut(beach);
            OpenTidePoolsToTownShortcut(beach);
        }

        private static void OpenBeachToForestShortcut(Beach beach)
        {
            if (!_archipelago.HasReceivedItem("Forest To Beach Shortcut"))
            {
                return;
            }

            beach.warps.Add(new Warp(-1, 4, "Forest", 119, 35, false));
            beach.warps.Add(new Warp(-1, 5, "Forest", 119, 35, false));
            beach.warps.Add(new Warp(-1, 6, "Forest", 119, 36, false));
            beach.warps.Add(new Warp(-1, 7, "Forest", 119, 36, false));
            for (var x = 0; x < 5; ++x)
            {
                for (var y = 4; y < 7; ++y)
                {
                    beach.removeTile(x, y, "Buildings");
                }
            }
            beach.removeTile(7, 6, "Buildings");
            beach.removeTile(5, 6, "Buildings");
            beach.removeTile(6, 6, "Buildings");
            beach.setMapTile(3, 7, 107, "Back", "untitled tile sheet");
        }

        private static void OpenTidePoolsToTownShortcut(Beach beach)
        {
            if (!_archipelago.HasReceivedItem("Town To Tide Pools Shortcut"))
            {
                return;
            }

            beach.removeTile(67, 5, "Buildings");
            beach.removeTile(67, 4, "Buildings");
            beach.removeTile(67, 3, "Buildings");
            beach.removeTile(67, 2, "Buildings");
            beach.removeTile(67, 1, "Buildings");
            beach.removeTile(67, 0, "Buildings");
            beach.removeTile(66, 3, "Buildings");
            beach.removeTile(68, 3, "Buildings");
        }

        public static void OpenBackwoodsShortcuts(GameLocation backwoods)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            if (!_archipelago.HasReceivedItem("Tunnel To Backwoods Shortcut"))
            {
                return;
            }

            // protected HashSet<string> _appliedMapOverrides;
            var _appliedMapOverridesField = _modHelper.Reflection.GetField<HashSet<string>>(backwoods, "_appliedMapOverrides");
            if (_appliedMapOverridesField.GetValue().Contains("Backwoods_Staircase"))
            {
                return;
            }

            backwoods.ApplyMapOverride("Backwoods_Staircase");
            LargeTerrainFeature largeTerrainFeature1 = null;
            foreach (var largeTerrainFeature2 in backwoods.largeTerrainFeatures)
            {
                if (largeTerrainFeature2.Tile == new Vector2(37f, 16f))
                {
                    largeTerrainFeature1 = largeTerrainFeature2;
                    break;
                }
            }
            if (largeTerrainFeature1 != null)
            {
                backwoods.largeTerrainFeatures.Remove(largeTerrainFeature1);
            }
        }

        private static void OpenTownToMountainShortcut(Town town)
        {
            if (!_archipelago.HasReceivedItem("Mountain Shortcuts"))
            {
                return;
            }

            town.removeTile(90, 2, "Buildings");
            town.removeTile(90, 1, "Front");
            town.removeTile(90, 1, "Buildings");
            town.removeTile(90, 0, "Buildings");
            town.setMapTile(89, 1, 360, "Front", "Landscape");
            town.setMapTile(89, 2, 385, "Buildings", "Landscape");
            town.setMapTile(89, 1, 436, "Buildings", "Landscape");
            town.setMapTile(89, 0, 411, "Buildings", "Landscape");
        }

        private static void OpenJojamartToMountainShortcut(Town town)
        {
            if (!_archipelago.HasReceivedItem("Mountain Shortcuts"))
            {
                return;
            }

            town.removeTile(98, 4, "Buildings");
            town.removeTile(98, 3, "Buildings");
            town.removeTile(98, 2, "Buildings");
            town.removeTile(98, 1, "Buildings");
            town.removeTile(98, 0, "Buildings");
            town.setMapTile(98, 4, 12, "Back", "v16_landscape2");
            town.setMapTile(98, 3, 509, "Back", "Landscape");
            town.setMapTile(98, 2, 217, "Back", "Landscape");
            town.setMapTile(97, 3, 1683, "Buildings", "Landscape");
            town.setMapTile(97, 3, 509, "Back", "Landscape");
            town.setMapTile(97, 2, 1658, "Buildings", "Landscape");
            town.setMapTile(97, 2, 217, "Back", "Landscape");
            town.setMapTile(98, 2, 1659, "AlwaysFront", "Landscape");
        }

        private static void OpenTownToTidePoolsShortcut(Town town)
        {
            if (!_archipelago.HasReceivedItem("Town To Tide Pools Shortcut"))
            {
                return;
            }

            town.removeTile(92, 104, "Buildings");
            town.removeTile(93, 104, "Buildings");
            town.removeTile(94, 104, "Buildings");
            town.removeTile(92, 105, "Buildings");
            town.removeTile(93, 105, "Buildings");
            town.removeTile(94, 105, "Buildings");
            town.removeTile(93, 106, "Buildings");
            town.removeTile(94, 106, "Buildings");
            town.removeTile(92, 103, "Front");
            town.removeTile(93, 103, "Front");
            town.removeTile(94, 103, "Front");
        }
    }
}
