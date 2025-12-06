using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class EatConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public void doneEating()
        [HarmonyPriority(Priority.VeryHigh)] // High priority to stop all other eat patches
        public static bool DoneEating_DropOnTheFloor_Prefix(Farmer __instance)
        {
            try
            {
                var numberEatsanityPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.EATSANITY); // There are 273 total

                if (numberEatsanityPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }


                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.03, numberEatsanityPurchased, Game1.ticks))
                {
                    DoneEatingDropOnFloor(__instance);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }


                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneEating_DropOnTheFloor_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void DoneEatingDropOnFloor(Farmer farmer)
        {
            farmer.isEating = false;
            farmer.tempFoodItemTextureName.Value = (string)null;
            farmer.completelyStopAnimatingOrDoingAction();
            farmer.forceCanMove();
            if (farmer.mostRecentlyGrabbedItem == null || !farmer.IsLocalPlayer)
            {
                return;
            }

            var itemToEat = farmer.itemToEat as Object;
            farmer.mostRecentlyGrabbedItem = null;
            farmer.itemToEat = null;

            Game1.createItemDebris(itemToEat, Game1.player.Position, Game1.player.FacingDirection, Game1.player.currentLocation, 0);
            var emotes = new[] { "sad", "x", "pause", "blush", "angry", "surprised", "uh" };
            var emote = emotes[Game1.random.Next(emotes.Length)];
            Game1.player.performPlayerEmote(emote);
        }
    }
}
