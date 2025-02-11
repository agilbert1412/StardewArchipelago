﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Tools;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = xTile.Dimensions.Rectangle;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class IsolatedEventInjections
    {
        public const string OLD_MASTER_CANNOLI_AP_LOCATION = "Old Master Cannoli";
        public const string BEACH_BRIDGE_AP_LOCATION = "Beach Bridge Repair";
        public const string GALAXY_SWORD_SHRINE_AP_LOCATION = "Galaxy Sword Shrine";
        public const string RUSTY_SWORD_AP_LOCATION = "The Mines Entrance Cutscene";
        public const string POT_OF_GOLD_AP_LOCATION = "Pot Of Gold";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool CheckAction_OldMasterCanolli_Prefix(Woods __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tile = __instance.map.GetLayer("Buildings")
                    .PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                if (tile == null || !who.IsLocalPlayer || (tile.TileIndex != 1140 && tile.TileIndex != 1141) || !__instance.hasUnlockedStatue.Value)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.hasUnlockedStatue.Value && !__instance.localPlayerHasFoundStardrop() && who.freeSpotsInInventory() > 0)
                {
                    _locationChecker.AddCheckedLocation(OLD_MASTER_CANNOLI_AP_LOCATION);
                    if (!Game1.player.mailReceived.Contains("CF_Statue"))
                    {
                        Game1.player.mailReceived.Add("CF_Statue");
                    }
                }
                __result = true;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_OldMasterCanolli_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool AnswerDialogueAction_BeachBridge_Prefix(Beach __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "BeachBridge_Yes")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Game1.player.Items.ReduceId("388", 300);
                _locationChecker.AddCheckedLocation(BEACH_BRIDGE_AP_LOCATION);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_BeachBridge_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool CheckAction_BeachBridge_Prefix(Beach __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (tileLocation.X != 58 || tileLocation.Y != 13)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = true;
                if (_locationChecker.IsLocationChecked(BEACH_BRIDGE_AP_LOCATION))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (who.Items.ContainsId("388", 300))
                {
                    __instance.createQuestionDialogue(
                        Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Question"),
                        __instance.createYesNoResponses(), "BeachBridge");
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Beach_FixBridge_Hint"));
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_BeachBridge_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool? _realBridgeFixed = null;

        // public static void fixBridge(GameLocation location)

        public static bool FixBridge_DontFixDuringDraw_Prefix(GameLocation location)
        {
            try
            {
                if (_realBridgeFixed != null)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_archipelago.HasReceivedItem(VanillaUnlockManager.BEACH_BRIDGE))
                {
                    return true;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FixBridge_DontFixDuringDraw_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void draw(SpriteBatch b)

        public static bool Draw_BeachBridgeQuestionMark_Prefix(Beach __instance, SpriteBatch b)
        {
            try
            {
                if (_realBridgeFixed == null)
                {
                    _realBridgeFixed = __instance.bridgeFixed.Value;
                }

                __instance.bridgeFixed.Value = _locationChecker.IsLocationChecked(BEACH_BRIDGE_AP_LOCATION);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_BeachBridgeQuestionMark_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void draw(SpriteBatch b)

        public static void Draw_BeachBridgeQuestionMark_Postfix(Beach __instance, SpriteBatch b)
        {
            try
            {
                if (_realBridgeFixed != null)
                {
                    __instance.bridgeFixed.Value = _realBridgeFixed.Value;
                }

                _realBridgeFixed = null;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_BeachBridgeQuestionMark_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual void performTouchAction(string[] action, Vector2 playerStandingPosition)
        public static bool PerformTouchAction_GalaxySwordShrine_Prefix(GameLocation __instance, string[] action, Vector2 playerStandingPosition)
        {
            try
            {
                var actionFirstWord = action[0];
                if (Game1.eventUp || actionFirstWord != "legendarySword" || _locationChecker.IsLocationChecked(GALAXY_SWORD_SHRINE_AP_LOCATION))
                {
                    return actionFirstWord != "legendarySword"; // run original logic only if it's something other than the shrine
                }

                if (Game1.player.ActiveObject?.QualifiedItemId == QualifiedItemIds.PRISMATIC_SHARD)
                {
                    Game1.player.Halt();
                    Game1.player.faceDirection(2);
                    Game1.player.showCarrying();
                    Game1.player.jitterStrength = 1f;

                    Game1.pauseThenDoFunction(7000, CheckGalaxySwordApLocation);

                    Game1.changeMusicTrack("none", music_context: MusicContext.Event);
                    __instance.playSound("crit");
                    Game1.screenGlowOnce(new Color(30, 0, 150), true, 0.01f, 0.999f);
                    DelayedAction.playSoundAfterDelay("stardrop", 1500);
                    Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000));
                    Game1.afterDialogues += () => Game1.stopMusicTrack(MusicContext.Event);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __instance.localSound("SpringBirds");
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformTouchAction_GalaxySwordShrine_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckGalaxySwordApLocation()
        {
            Game1.flashAlpha = 1f;
            Game1.player.holdUpItemThenMessage(new MeleeWeapon("4"));
            Game1.player.reduceActiveItemByOne();
            _locationChecker.AddCheckedLocation(GALAXY_SWORD_SHRINE_AP_LOCATION);

            // Game1.player.mailReceived.Contains("galaxySword")
            // GameLocation.getGalaxySword

            Game1.player.jitterStrength = 0.0f;
            Game1.screenGlowHold = false;
        }

        public static bool SkipEvent_RustySword_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.RUSTY_SWORD)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                EventInjections.BaseSkipEvent(__instance, () => _locationChecker.AddCheckedLocation(RUSTY_SWORD_AP_LOCATION));

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_RustySword_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_RustySword_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.RUSTY_SWORD)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (Game1.activeClickableMenu == null)
                {
                    @event.CurrentCommand++;
                }
                @event.CurrentCommand++;

                _locationChecker.AddCheckedLocation(RUSTY_SWORD_AP_LOCATION);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_RustySword_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        public static bool CheckForAction_PotOfGold_Prefix(Object __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (__instance.isTemporarilyInvisible || justCheckingForActivity || __instance.QualifiedItemId != QualifiedItemIds.POT_OF_GOLD)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationMissing(POT_OF_GOLD_AP_LOCATION))
                {
                    Game1.playSound("hammer");
                    Game1.playSound("moneyDial");
                    __instance.Location.removeObject(__instance.TileLocation, false);
                    Utility.addDirtPuffs(__instance.Location, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, 1, 1, 3);
                    Utility.addStarsAndSpirals(__instance.Location, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, 1, 1, 100, 30, Color.White);
                    _locationChecker.AddCheckedLocation(POT_OF_GOLD_AP_LOCATION);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForAction_PotOfGold_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
