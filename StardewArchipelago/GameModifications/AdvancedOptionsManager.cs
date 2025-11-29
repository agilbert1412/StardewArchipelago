using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.Testing;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications
{
    public class AdvancedOptionsManager
    {
        private static ModEntry _modEntry;
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private readonly Harmony _harmony;
        private static StardewArchipelagoClient _archipelago;
        private static TesterFeatures _testerFeatures;

        public AdvancedOptionsManager(ModEntry modEntry, ILogger logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, TesterFeatures testerFeatures)
        {
            _modEntry = modEntry;
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
            _testerFeatures = testerFeatures;
        }

        public void InjectArchipelagoAdvancedOptions()
        {
            InjectAdvancedOptionsRemoval();
            InjectNewGameForcedSettings();
            InjectArchipelagoConnectionFields();
        }

        private void InjectAdvancedOptionsRemoval()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCustomization), "ResetComponents"),
                postfix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(ResetComponents_RemoveAdvancedOptionsButton_Postfix))
            );
        }

        private void InjectNewGameForcedSettings()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.loadForNewGame)),
                prefix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(LoadForNewGame_ForceSettings_Prefix))
            );
        }

        private void InjectArchipelagoConnectionFields()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.update)),
                postfix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(TitleMenuUpdate_ReplaceCharacterMenu_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCustomization), nameof(CharacterCustomization.canLeaveMenu)),
                prefix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(CanLeaveMenu_ConsiderNewFields_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCustomization), "optionButtonClick"),
                prefix: new HarmonyMethod(typeof(AdvancedOptionsManager), nameof(OptionButtonClick_OkConnectToAp_Prefix))
            );
        }

        // private void ResetComponents()
        public static void ResetComponents_RemoveAdvancedOptionsButton_Postfix(CharacterCustomization __instance)
        {
            try
            {
                if (__instance?.advancedOptionsButton == null)
                {
                    return;
                }

                __instance.advancedOptionsButton.visible = false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetComponents_RemoveAdvancedOptionsButton_Postfix)}:\n{ex}");
                return;
            }
        }

        public static bool LoadForNewGame_ForceSettings_Prefix(bool loadedGame = false)
        {
            try
            {
                if (!_archipelago.IsConnected)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                ForceGameSeedToArchipelagoProvidedSeed();
                ForceFarmTypeToArchipelagoProvidedFarm();
                Game1.bundleType = Game1.BundleType.Default;
                Game1.game1.SetNewGameOption<bool>("YearOneCompletable", false);
                Game1.startingCabins = _testerFeatures.Multiplayer.Value;
                Game1.cabinsSeparate = false;

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(LoadForNewGame_ForceSettings_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ForceGameSeedToArchipelagoProvidedSeed()
        {
            var trimmedSeed = _archipelago.SlotData.Seed.Trim();

            var result = int.Parse(trimmedSeed.Substring(0, Math.Min(9, trimmedSeed.Length)));
            Game1.startingGameSeed = (ulong)result;
        }

        private static void ForceFarmTypeToArchipelagoProvidedFarm()
        {
            var farmType = _archipelago.SlotData.FarmType;

            Game1.whichFarm = farmType.GetWhichFarm();
            Game1.whichModFarm = farmType.GetWhichModFarm();
            Game1.spawnMonstersAtNight = farmType.GetSpawnMonstersAtNight();
        }

        public static void TitleMenuUpdate_ReplaceCharacterMenu_Postfix(TitleMenu __instance, GameTime time)
        {
            try
            {
                if (!(TitleMenu.subMenu is CharacterCustomization characterMenu) || TitleMenu.subMenu is CharacterCustomizationArchipelago || characterMenu.source != CharacterCustomization.Source.NewGame)
                {
                    return;
                }

                var apCharacterMenu = new CharacterCustomizationArchipelago(characterMenu, _modHelper);
                TitleMenu.subMenu = apCharacterMenu;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TitleMenuUpdate_ReplaceCharacterMenu_Postfix)}:\n{ex}");
                return;
            }
        }

        public static bool CanLeaveMenu_ConsiderNewFields_Prefix(CharacterCustomization __instance, ref bool __result)
        {
            try
            {
                if (!(__instance is CharacterCustomizationArchipelago apInstance))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = Game1.player.Name.Length > 0 &&
                           Game1.player.farmName.Length > 0 &&
                           Game1.player.favoriteThing.Length > 0 &&
                           apInstance.IpAddressTextBox.Text.Length > 0 &&
                           apInstance.SlotNameTextBox.Text.Length > 0 &&
                           apInstance.IpIsFormattedCorrectly();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CanLeaveMenu_ConsiderNewFields_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool OptionButtonClick_OkConnectToAp_Prefix(CharacterCustomization __instance, string name)
        {
            try
            {
                if (!(__instance is CharacterCustomizationArchipelago apInstance) || name != "OK" || !__instance.canLeaveMenu())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!apInstance.TryParseIpAddress(out var ip, out var port))
                {
                    return false;
                }

                var result = _modEntry.ArchipelagoConnect(ip, port, apInstance.SlotNameTextBox.Text, apInstance.PasswordTextBox.Text);

                if (!result.Success)
                {
                    var currentMenu = TitleMenu.subMenu;
                    _logger.LogError($"Connection to Archipelago failed: {result.Message}");
                    TitleMenu.subMenu = new InformationDialog(result.Message, (_) => OnClickOkBehavior(currentMenu));
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD; // run original logic only if connected successfully
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OptionButtonClick_OkConnectToAp_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void OnClickOkBehavior(IClickableMenu previousMenu)
        {
            TitleMenu.subMenu = previousMenu;
        }
    }
}
