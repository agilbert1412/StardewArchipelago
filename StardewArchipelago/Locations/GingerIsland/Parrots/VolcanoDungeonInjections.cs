﻿using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.GingerIsland.Parrots
{
    public class VolcanoDungeonInjections
    {
        private const string AP_VOLCANO_BRIDGE_PARROT = "Volcano Bridge";
        private const string AP_VOLCANO_EXIT_SHORTCUT_PARROT = "Volcano Exit Shortcut";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual void GenerateContents(bool use_level_level_as_layout = false)
        public static void GenerateContents_ReplaceParrots_Postfix(VolcanoDungeon __instance, bool use_level_level_as_layout)
        {
            try
            {
                if (!Game1.IsMasterGame)
                {
                    return;
                }

                if (__instance.level.Value == 0)
                {
                    __instance.parrotUpgradePerches.Clear();
                    AddVolcanoBridgeParrot(__instance);
                }
                else if (__instance.level.Value == 5)
                {
                    __instance.parrotUpgradePerches.Clear();
                    AddVolcanoExitShortcutParrot(__instance);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GenerateContents_ReplaceParrots_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void AddVolcanoBridgeParrot(IslandLocation volcano)
        {
            volcano.parrotUpgradePerches.Add(new ParrotUpgradePerchArchipelago(AP_VOLCANO_BRIDGE_PARROT, _archipelago, volcano,
                new Point(27, 39),
                new Rectangle(28, 34, 5, 4),
                5,
                PurchaseVolcanoBridgeParrot,
                IsVolcanoBridgeParrotPurchased,
                "VolcanoBridge",
                "reachedCaldera, Island_Turtle"));
        }

        private static void PurchaseVolcanoBridgeParrot()
        {
            _locationChecker.AddCheckedLocation(AP_VOLCANO_BRIDGE_PARROT);
        }

        private static bool IsVolcanoBridgeParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_VOLCANO_BRIDGE_PARROT);
        }

        private static void AddVolcanoExitShortcutParrot(IslandLocation volcano)
        {
            var shortcutOutPositionField =
                _modHelper.Reflection.GetField<Point>(typeof(VolcanoDungeon), "shortcutOutPosition");
            var shortcutOutPosition = shortcutOutPositionField.GetValue();
            volcano.parrotUpgradePerches.Add(new ParrotUpgradePerchArchipelago(AP_VOLCANO_EXIT_SHORTCUT_PARROT, _archipelago, volcano,
                new Point(shortcutOutPosition.X + 1, shortcutOutPosition.Y),
                new Rectangle(shortcutOutPosition.X - 1, shortcutOutPosition.Y - 1, 3, 3),
                5,
                PurchaseVolcanoExitShortcutParrot,
                IsVolcanoExitShortcutParrotPurchased,
                "VolcanoShortcutOut",
                "Island_Turtle"));
        }

        private static void PurchaseVolcanoExitShortcutParrot()
        {
            _locationChecker.AddCheckedLocation(AP_VOLCANO_EXIT_SHORTCUT_PARROT);
        }

        private static bool IsVolcanoExitShortcutParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_VOLCANO_EXIT_SHORTCUT_PARROT);
        }
    }
}
