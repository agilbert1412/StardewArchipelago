using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.CodeInjections.Modded.SVE
{
    public class SVECutsceneInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private const string RAILROAD_KEY = "Clint2Again";
        private const int AURORA_EVENT = 658059254;
        private const int MORGAN_EVENT = 658078924;
        private const int RAILROAD_BOULDER_ID = 8050108;
        private const int IRIDIUM_BOMB_ID = 8050109;
        private const string LANCE_CHEST = "Lance's Diamond Wand";
        private const string DUMMY_EVENT_DATA = "continue/-200 -200 0/Lewis -10 -10 0/pause 50/end";
        private static ShopMenu _lastShopMenuUpdated = null;

        private static readonly Dictionary<string, string[]> WarpKeys = new()
        {
            // Contains the original event requirements to be edited
            { "IslandWest", new[] { "65360191/f Lance 2000" } }, // Lance giving you the Fable Reef Warp
            { "FarmHouseFarmRune", new[] { "908074/e 908072/t 600 630" } }, // Unlocking the Farm Rune
            { "FarmHouseOutpostRune", new[] { "908078/e 908072/t 600 2400" } }, // Unlocking Galmoran Outpost
            { "Backwoods", new[] { "908072/e 908071" } }, // Unlocking Wizard Rune
            { "AdventureGuild", new[] { "908073/e 908072/z winter/t 600 1900/w sunny", "908073/e 908072/z winter/t 1910 2400/w sunny", "908073/e 908072/z winter/w rainy", "908073/e 908072/z spring/z summer/z fall" } }, // Unlocking Summit Rune
            { "Custom_AuroraVineyard", new[] { "908075/e 908072", "908075/e 908072/f Apples 10" } }, // Unlocking Vineyard Rune
            { "Custom_SpriteSpring2", new[] { "908076/e 908072" } }, // Unlocking Springs Rune
            { "Custom_JunimoWoods", new[] { "908077/e 908072" } }, // Unlocking Junimo Rune
            { "Town", new[] { "3691371" } }, // The initializer for Scarlett to be a villager.
            // Event where Marlon gives the player a second Rusty Key.
            { "Custom_AdventurerSummit", new[] { "1090501/e 1000034/k 1090502/f MarlonFay 1250", "1090501/e 1000034/k 1090502/b 1"}}
        };

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            InitializeSVEEvents();
        }
        
        public static bool CheckForAction_LanceChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value)
                {
                    return true; // run original logic
                }

                if (__instance.items.Count is <= 0 or > 1 || __instance.items.First().Name != "Diamond Wand")
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                var obj = __instance.items[0];
                __instance.items[0] = null;
                __instance.items.RemoveAt(0);
                __result = true;

                _locationChecker.AddCheckedLocation(LANCE_CHEST);

                return false; // don't run original logic

            }

            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_LanceChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void ResetLocalState_PlayRailroadBoulderCutsceneIfConditionsAreMet_Postfix(GameLocation __instance)
        {
            try
            {
                var bombBeforeQuest = Game1.player.eventsSeen.Contains(RAILROAD_BOULDER_ID) && Game1.player.eventsSeen.Contains(IRIDIUM_BOMB_ID);
                if (!Game1.player.hasSkullKey || !bombBeforeQuest)
                {
                    return;
                }

                // Add a fake Special Order for Clint's boulder destruction because the real one gets removed by SVE when the actual boulder is removed
                var railroadBoulderOrder = SpecialOrder.GetSpecialOrder("Clint2", null);
                railroadBoulderOrder.questKey.Value = RAILROAD_KEY;
                Game1.player.team.specialOrders.Add(railroadBoulderOrder);

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ResetLocalState_PlayRailroadBoulderCutsceneIfConditionsAreMet_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void InitializeSVEEvents()
        {
            ReplaceViewableCutscenes();
            ReplaceLockedCutscenes();
            AppendFakeClintBoulderSpecialOrderKey();
        }

        // Some events we want to see, so this will let them play out, so we make up AP event scene requirements.
        public static void ReplaceViewableCutscenes()
        {
            var farmEvents = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Farm");
            var farmhouseEvents = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse");
            var oldApplesRule = "77759254/e 191393/e 1000037/j 138/t 600 630/H";
            var newApplesRule = $"77759254/e {AURORA_EVENT}"; //Only shows up if AP event plays
            var oldMorganRule = "5978924/y 3/d Mon Tue/e 1724095";
            var newMorganRule = $"5978924/e {MORGAN_EVENT}"; //Only shows up if AP event plays
            var morganEvent = farmEvents[oldMorganRule];
            var applesEvent = farmhouseEvents[oldApplesRule];
            farmEvents.Remove(oldMorganRule);
            farmhouseEvents.Remove(oldApplesRule);
            farmEvents[newMorganRule] = morganEvent;
            farmhouseEvents[newApplesRule] = applesEvent;
        }

        // Otherwise, we should just lock the events out of being seen at all and simply toggled by eventSeen.
        public static void ReplaceLockedCutscenes()
        {
            foreach (var (eventMapName, eventKeys) in WarpKeys)
            {
                var mapName = eventMapName;
                if (mapName.Contains("FarmHouse"))
                {
                    mapName = "FarmHouse";
                }

                if (mapName.Contains("Custom_AdventurerSummit"))
                {
                    mapName = "Custom_AdventurerSummit";
                }

                var currentMapEventData = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + mapName);
                foreach (var eventKey in eventKeys)
                {
                    var eventID = eventKey.Split("/")[0];
                    // Sometimes, CP has not added the cutscene to the event list yet due to its dynamic adding methods...
                    if (!currentMapEventData.ContainsKey(eventKey))
                    {
                        // ... so just add a dummy event if that's the case to make sure its locked and move on to the next event.
                        currentMapEventData[eventKey] = DUMMY_EVENT_DATA;
                    }

                    // Everything exists now, so just make the requirement for the event itself.
                    var newEventKey = eventKey + "/e " + eventID;
                    var eventData = currentMapEventData[eventKey];
                    currentMapEventData.Remove(eventKey);
                    currentMapEventData[newEventKey] = eventData;
                }
            }
        }

        // Railroad Boulder Special Order won't load if Iridium Bomb is sent early, so we duplicate it so the player gets it.
        private static void AppendFakeClintBoulderSpecialOrderKey()
        {
            var untimedSpecialOrdersType = AccessTools.TypeByName("HarmonyPatch_UntimedSpecialOrders");
            var specialOrderKeysField = _modHelper.Reflection.GetField<List<string>>(untimedSpecialOrdersType, "SpecialOrderKeys");
            var specialOrderKeys = specialOrderKeysField.GetValue();
            specialOrderKeys.Add("Clint2Again");
        }
    }
}