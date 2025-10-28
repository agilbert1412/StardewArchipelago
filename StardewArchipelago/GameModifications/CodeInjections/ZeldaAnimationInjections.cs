using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class ZeldaAnimationInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public void holdUpItemThenMessage(Item item, int countAdded, bool showMessage = true)
        public static bool HoldUpItemThenMessage_SkipBasedOnConfig_Prefix(Farmer __instance, Item item, int countAdded, bool showMessage)
        {
            try
            {
                if (Game1.isFestival())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // We skip this whole method when skipping hold up animations is true
                if (!ModEntry.Instance.Config.SkipHoldUpAnimations || FoolManager.ShouldDoZeldaPrank())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }


                __instance.CanMove = true;
                __instance.freezePause = 0;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HoldUpItemThenMessage_SkipBasedOnConfig_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public Item addItemToInventory(Item item, int position)
        public static void AddItemToInventory_Position_PrankDay_Postfix(Farmer __instance, Item item, int position, Item __result)
        {
            try
            {
                if (!FoolManager.ShouldDoZeldaPrank())
                {
                    return;
                }

                DoPrankZeldaAnimation(__instance, item, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddItemToInventory_Position_PrankDay_Postfix)}:\n{ex}");
                return;
            }
        }

        // public Item addItemToInventory(Item item, List<Item> affected_items_list)
        public static void AddItemToInventory_AffectedItems_PrankDay_Postfix(Farmer __instance, Item item, List<Item> affected_items_list, Item __result)
        {
            try
            {
                if (!FoolManager.ShouldDoZeldaPrank())
                {
                    return;
                }

                DoPrankZeldaAnimation(__instance, item, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddItemToInventory_AffectedItems_PrankDay_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DoPrankZeldaAnimation(Farmer farmer, Item item, bool showMessage)
        {
            if (Game1.random.NextDouble() > 0.05)
            {
                return;
            }

            farmer.completelyStopAnimatingOrDoingAction();
            if (showMessage)
            {
                DelayedAction.playSoundAfterDelay("getNewSpecialItem", 750);
            }
            farmer.faceDirection(2);
            farmer.freezePause = 4000;
            farmer.FarmerSprite.animateOnce(new[]
            {
                new(57, 0),
                new(57, 2500, false, false, who => Farmer.showHoldingItem(who, item)),
                showMessage ?
                    new FarmerSprite.AnimationFrame((short)farmer.FarmerSprite.CurrentFrame, 500, false, false, who => Farmer.showReceiveNewItemMessage(who, item, item.Stack), true) :
                    new FarmerSprite.AnimationFrame((short)farmer.FarmerSprite.CurrentFrame, 500, false, false),
            });
            farmer.mostRecentlyGrabbedItem = item;
            farmer.canMove = false;

            if (Game1.random.NextDouble() < 0.2)
            {
                Game1.chatBox.addMessage("April's Fool! use !!fool to disable", Color.Gold);
            }
        }
    }
}
