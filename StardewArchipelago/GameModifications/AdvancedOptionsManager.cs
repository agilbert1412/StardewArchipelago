using System;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications
{
    public class AdvancedOptionsManager
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private Harmony _harmony;
        private static ArchipelagoClient _archipelago;

        public AdvancedOptionsManager(IMonitor monitor, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
        }

        public void InjectArchipelagoAdvancedOptions()
        {
            InjectAdvancedOptionsRemoval();
            InjectNewGameForcedSettings();
        }

        private void InjectAdvancedOptionsRemoval()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCustomization), "setUpPositions"),
                postfix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(AdvancedOptionsManager.SetUpPositions_RemoveAdvancedOptionsButton_Postfix))
            );
        }

        private void InjectNewGameForcedSettings()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.loadForNewGame)),
                prefix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(AdvancedOptionsManager.LoadForNewGame_ForceSettings_Prefix))
            );
        }

        public static void SetUpPositions_RemoveAdvancedOptionsButton_Postfix(CharacterCustomization __instance)
        {
            try
            {
                __instance.advancedOptionsButton.visible = false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetUpPositions_RemoveAdvancedOptionsButton_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static bool LoadForNewGame_ForceSettings_Prefix(bool loadedGame = false)
        {
            try
            {
                if (!_archipelago.IsConnected)
                {
                    return true; // run original logic
                }

                ForceGameSeedToArchipelagoProvidedSeed();
                Game1.bundleType = Game1.BundleType.Default;
                Game1.game1.SetNewGameOption<bool>("YearOneCompletable", true);

                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(LoadForNewGame_ForceSettings_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void ForceGameSeedToArchipelagoProvidedSeed()
        {
            var trimmedSeed = _archipelago.SlotData.Seed.Trim();
            
            int result = int.Parse(trimmedSeed.Substring(0, 9));
            Game1.startingGameSeed = (ulong)result;
        }
    }
}
