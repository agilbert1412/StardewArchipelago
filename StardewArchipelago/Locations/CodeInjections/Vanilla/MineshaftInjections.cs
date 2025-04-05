using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = xTile.Dimensions.Rectangle;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class MineshaftInjections
    {
        private const string TREASURE_LOCATION = "The Mines Floor {0} Treasure";
        private const string ELEVATOR_LOCATION = "Floor {0} Elevator";

        private static ILogger _logger;
        private static ModConfig _config;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Texture2D _miniArchipelagoIcon;

        public static void Initialize(LogHandler logger, IModHelper modHelper, ModConfig config, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _config = config;
            _archipelago = archipelago;
            _locationChecker = locationChecker;

            var desiredTextureName = ArchipelagoTextures.COLOR;
            _miniArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(logger, modHelper, 12, desiredTextureName);
        }

        public static bool CheckForAction_MineshaftChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value || Game1.mine == null || Game1.mine.mineLevel > 120)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.Items.Count <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                Game1.mine.chestConsumed();
                var obj = __instance.Items[0];
                __instance.Items[0] = null;
                __instance.Items.RemoveAt(0);
                __result = true;

                _locationChecker.AddCheckedLocation(string.Format(TREASURE_LOCATION, Game1.mine.mineLevel));

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForAction_MineshaftChest_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool AddLevelChests_Level120_Prefix(MineShaft __instance)
        {
            try
            {
                if (__instance.mineLevel != 120 || Game1.player.chestConsumedMineLevels.ContainsKey(120))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Game1.player.completeQuest("18");
                Game1.getSteamAchievement("Achievement_TheBottom");
                var chestPosition = new Vector2(9f, 9f);
                var items = new List<Item>();
                items.Add(new MeleeWeapon("8"));
                __instance.overlayObjects[chestPosition] = new Chest(items, chestPosition)
                {
                    Tint = Color.Pink,
                };

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddLevelChests_Level120_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void enterMine(int whatLevel)
        public static void EnterMine_SendElevatorCheck_PostFix(int whatLevel)
        {
            try
            {
                if (whatLevel < 5 || whatLevel > 120 || whatLevel % 5 != 0)
                {
                    return;
                }

                var progression = _archipelago.SlotData.ElevatorProgression;
                var currentMineshaft = Game1.player.currentLocation as MineShaft;
                var currentMineLevel = currentMineshaft?.mineLevel ?? 0;
                if (progression == ElevatorProgression.ProgressiveFromPreviousFloor && currentMineLevel != whatLevel - 1)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(string.Format(ELEVATOR_LOCATION, whatLevel));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(EnterMine_SendElevatorCheck_PostFix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_LoadElevatorMenu_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGet(action, 0, out var actionName, out _, name: "string actionType"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (actionName != "MineElevator")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CreateElevatorMenuIfUnlocked();
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_LoadElevatorMenu_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool CheckAction_LoadElevatorMenu_Prefix(MineShaft __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);

                if (tile == null || !who.IsLocalPlayer || tile.TileIndex != 112 || __instance.mineLevel > 120)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CreateElevatorMenuIfUnlocked();
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_LoadElevatorMenu_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CreateElevatorMenuIfUnlocked()
        {
            var numberOfMineElevatorReceived = _archipelago.GetReceivedItemCount(VanillaUnlockManager.PROGRESSIVE_MINE_ELEVATOR);
            var mineLevelUnlocked = numberOfMineElevatorReceived * 5;
            mineLevelUnlocked = Math.Min(120, Math.Max(0, mineLevelUnlocked));

            if (mineLevelUnlocked < 5)
            {
                Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_MineElevator_NotWorking")));
            }
            else
            {
                var previousMaxLevel = MineShaft.lowestLevelReached;
                MineShaft.lowestLevelReached = mineLevelUnlocked;
                Game1.activeClickableMenu = new MineElevatorMenu();
                MineShaft.lowestLevelReached = previousMaxLevel;
            }
        }

        // public override void draw(SpriteBatch b)
        public static void Draw_AddArchipelagoIndicators_Postfix(MineElevatorMenu __instance, SpriteBatch b)
        {
            try
            {
                if (!_config.ShowElevatorIndicators)
                {
                    return;
                }

                foreach (var button in __instance.elevators)
                {
                    var floor = Convert.ToInt32(button.name);
                    var elevatorLocation = string.Format(ELEVATOR_LOCATION, floor);
                    var treasureLocation = string.Format(TREASURE_LOCATION, floor);
                    var checkRemainingOnThatFloor = _locationChecker.IsLocationMissing(elevatorLocation) || _locationChecker.IsLocationMissing(treasureLocation);

                    if (!checkRemainingOnThatFloor)
                    {
                        continue;
                    }

                    var buttonLocation = new Vector2(button.bounds.X, button.bounds.Y);
                    var position = buttonLocation + new Vector2(12f, 12f);
                    var sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 12);
                    var color = Color.White;
                    var origin = new Vector2(8f, 8f);
                    b.Draw(_miniArchipelagoIcon, position, sourceRectangle, color, 0.0f, origin, 1f, SpriteEffects.None, 0.86f);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddArchipelagoIndicators_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
