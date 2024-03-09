using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Unlocks;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FishingRodInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool SkipEvent_BambooPole_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.BAMBOO_POLE)
                {
                    return true; // run original logic
                }

                SkipBambooPoleEventArchipelago(__instance);
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkipEvent_BambooPole_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_BambooPole_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                var festivalWinnersField = _modHelper.Reflection.GetField<HashSet<long>>(@event, "festivalWinners");
                if (@event.id != EventIds.BAMBOO_POLE || args.Length <= 1 || args[1].ToLower() != "rod")
                {
                    return true; // run original logic
                }

                OnCheckBambooPoleLocation();

                if (Game1.activeClickableMenu == null)
                    @event.CurrentCommand++;
                @event.CurrentCommand++;

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AwardFestivalPrize_BambooPole_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void SkipBambooPoleEventArchipelago(Event bambooPoleEvent)
        {
            if (bambooPoleEvent.playerControlSequence)
            {
                bambooPoleEvent.EndPlayerControlSequence();
            }

            Game1.playSound("drumkit6");

            var actorPositionsAfterMoveField = _modHelper.Reflection.GetField<Dictionary<string, Vector3>>(bambooPoleEvent, "actorPositionsAfterMove");
            actorPositionsAfterMoveField.GetValue().Clear();

            foreach (var actor in bambooPoleEvent.actors)
            {
                var ignoreStopAnimation = actor.Sprite.ignoreStopAnimation;
                actor.Sprite.ignoreStopAnimation = true;
                actor.Halt();
                actor.Sprite.ignoreStopAnimation = ignoreStopAnimation;
                bambooPoleEvent.resetDialogueIfNecessary(actor);
            }

            bambooPoleEvent.farmer.Halt();
            bambooPoleEvent.farmer.ignoreCollisions = false;
            Game1.exitActiveMenu();
            Game1.dialogueUp = false;
            Game1.dialogueTyping = false;
            Game1.pauseTime = 0.0f;

            OnCheckBambooPoleLocation();

            bambooPoleEvent.endBehaviors(new string[4]
            {
                "end",
                "position",
                "43",
                "36",
            }, Game1.currentLocation);
        }

        private static void OnCheckBambooPoleLocation()
        {
            _locationChecker.AddCheckedLocation("Bamboo Pole Cutscene");
        }
    }
}
