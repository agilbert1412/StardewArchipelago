﻿using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewValley;
using StardewValley.Events;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class WorldChangeEventInjections
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public bool setUp()
        public static bool SetUp_MakeSureEventsAreNotDuplicated_Prefix(WorldChangeEvent __instance, ref bool __result)
        {
            try
            {
                var eventToBeAdded = "";
                switch (__instance.whichEvent.Value)
                {
                    case 1:
                        eventToBeAdded = "cc_Greenhouse";
                        break;
                    case 2:
                    case 3:
                        eventToBeAdded = "cc_Minecart";
                        break;
                    case 4:
                    case 5:
                        eventToBeAdded = "cc_Bridge";
                        break;
                    case 6:
                    case 7:
                        eventToBeAdded = "cc_Bus";
                        break;
                    case 8:
                    case 9:
                        eventToBeAdded = "cc_Boulder";
                        break;
                    case 10:
                    case 11:
                        eventToBeAdded = "movieTheater";
                        break;
                    default:
                        return true; // run original logic
                }

                if (Game1.player.activeDialogueEvents.ContainsKey(eventToBeAdded))
                {
                    Game1.player.activeDialogueEvents.Remove(eventToBeAdded);
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetUp_MakeSureEventsAreNotDuplicated_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
