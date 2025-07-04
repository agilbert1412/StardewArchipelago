using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Quests
{
    public class DarkTalismanInjections
    {
        private const string DARK_TALISMAN_EVENT = "Dark Talisman";
        private const string DARK_TALISMAN_QUEST = $"Quest: {DARK_TALISMAN_EVENT}";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // protected override void resetLocalState()
        public static void ResetLocalState_PlayCutsceneIfConditionsAreMet_Postfix(Railroad __instance)
        {
            try
            {
                if (!Game1.player.hasRustyKey || Game1.currentLocation is not Railroad || Game1.eventUp || __instance.currentEvent != null || Game1.farmEvent != null || Game1.player.eventsSeen.Contains(EventIds.DARK_TALISMAN))
                {
                    return;
                }

                if (!__instance.TryGetLocationEvents(out _, out var locationEvents))
                {
                    return;
                }

                var darkTalismanEventKey = $"{EventIds.DARK_TALISMAN}/C";
                var darkTalismanEvent = new Event(locationEvents[darkTalismanEventKey], null, EventIds.DARK_TALISMAN, Game1.player);
                __instance.startEvent(darkTalismanEvent);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetLocalState_PlayCutsceneIfConditionsAreMet_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void setUpLocationSpecificFlair()
        public static bool SetUpLocationSpecificFlair_CreateBuglandChest_Prefix(GameLocation __instance)
        {
            try
            {
                if (!__instance.Name.Equals("BugLand") || __instance is not BugLand bugLand)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationMissing(DARK_TALISMAN_QUEST) && __instance.CanItemBePlacedHere(new Vector2(31f, 5f)))
                {
                    __instance.overlayObjects.Add(new Vector2(31f, 5f), new Chest(new List<Item>()
                    {
                        new SpecialItem(6),
                    }, new Vector2(31f, 5f))
                    {
                        Tint = Color.Gray,
                    });
                }
                foreach (var monster in __instance.characters)
                {
                    if (monster is Grub grub)
                    {
                        grub.setHard();
                    }
                    if (monster is Fly fly)
                    {
                        fly.setHard();
                    }
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetUpLocationSpecificFlair_CreateBuglandChest_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        public static bool CheckForAction_BuglandChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value || Game1.currentLocation is not BugLand)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.Items.Count <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                {
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                }
                else
                {
                    __instance.performOpenChest();
                }

                while (__instance.Items.Count > 0)
                {
                    __instance.Items[0] = null;
                    __instance.Items.RemoveAt(0);
                }

                __result = true;

                _locationChecker.AddCheckedLocation(DARK_TALISMAN_QUEST);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForAction_BuglandChest_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // private void performRemoveHenchman()
        public static void PerformRemoveHenchman_CheckGoblinProblemLocation_Postfix(NPC __instance)
        {
            try
            {
                _locationChecker.AddCheckedLocation($"Quest: Goblin Problem");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformRemoveHenchman_CheckGoblinProblemLocation_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void actionWhenReceived(Farmer who)
        public static void ActionWhenReceived_MagicInk_Postfix(SpecialItem __instance, Farmer who)
        {
            try
            {
                if (__instance.which.Value != 7)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation($"Quest: Magic Ink");
                who.hasMagicInk = _archipelago.HasReceivedItem("Magic Ink");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ActionWhenReceived_MagicInk_Postfix)}:\n{ex}");
                return;
            }
        }

    }
}
