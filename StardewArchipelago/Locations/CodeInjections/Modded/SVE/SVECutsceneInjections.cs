using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
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
        private const int AURORA_EVENT = 658059254;
        private const int MORGAN_EVENT = 658078924;
        private const string RAILROAD_KEY = "Clint2Again";
        private const int RAILROAD_BOULDER_ID = 8050108;
        private const int IRIDIUM_BOMB_ID = 8050109;
        private const string LANCE_CHEST = "Lance's Diamond Wand";
        private static ShopMenu _lastShopMenuUpdated = null;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            AppendFakeClintBoulderSpecialOrderKey();
        }
        //Format is Map|NewEventID(Optional)|Name
        public static readonly Dictionary<string, string[]> SVE_Static_Events = new()
        {
            { $"FarmHouse|{AURORA_EVENT}|VineyardEvent", new[] {" 77759254/e 191393/e 1000037/j 138/t 600 630/H "}},
            { "AdventureGuild||GuildRunes", new[] { "908073/e 908072/z winter/t 600 1900/w sunny", "908073/e 908072/z winter/t 1910 2400/w sunny", "908073/e 908072/z winter/w rainy", "908073/e 908072/z spring/z summer/z fall" } }, // Unlocking Summit Rune
        };

        public static readonly Dictionary<string, string[]> SVE_OnWarped_Events = new()
        {
            // Contains the original event requirements to be edited
            { "IslandWest||LanceEvent", new[] { "65360191/f Lance 2000" } },
            { "FarmHouse||FarmRune", new[] { "908074/e 908072/t 600 630", "908074/t 600 2600" } },
            { "FarmHouse||OutpostRune", new[] { "908078/e 908072/t 600 2400" } },
            { "Backwoods||WizardRune", new[] { "908072/e 908071" } },
            { "Custom_AuroraVineyard||VineyardRune", new[] { "908075/e 908072", "908075/e 908072/f Apples 10" } },
            { "Custom_SpriteSpring2||SpringRunes", new[] { "908076/e 908072" } },
            { "Custom_JunimoWoods||JunimoRunes", new[] { "908077/e 908072" } },
            { "Town|Scarlett", new[] { "3691371" } },
            { "Custom_AdventurerSummit||RustyKey", new[] { "1090501/e 1000034/k 1090502/f MarlonFay 1250", "1090501/e 1000034/k 1090502/b 1"}},
            { "Farm||GroveInitialize", new[] {"908070/t 600 900"}},
            { $"Farm|{MORGAN_EVENT}|MorganEntry", new [] {"5978924/y 3/d Mon Tue/e 1724095", "5978924/y 3/d Mon Tue/e 1724095/t 600 2600"} },
            { "Farm|GuntherRustyKey", new[] {"103042015/e 295672/t 600 700/H"}}
        };

        private static readonly Dictionary<string, string> _claireScheduleWhenMovies = new(){
            {"Mon", "0 Custom_Claire_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Claire_Blink/2130 BusStop 12 8 0/2340 Custom_DayEnd_WarpRoom 3 3 0"},
            {"Wed", "0 Custom_Claire_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Claire_Blink/2130 BusStop 12 8 0/2340 Custom_DayEnd_WarpRoom 3 3 0"},
            {"Thu", "0 Custom_Claire_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Claire_Blink/2130 BusStop 12 8 0/2340 Custom_DayEnd_WarpRoom 3 3 0"},
            {"Fri", "0 Custom_Claire_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Claire_Blink/2130 BusStop 12 8 0/2340 Custom_DayEnd_WarpRoom 3 3 0"},
            {"Sun", "0 Custom_Claire_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Claire_Blink/2130 BusStop 12 8 0/2340 Custom_DayEnd_WarpRoom 3 3 0"},
            {"Sat", "0 Custom_Claire_WarpRoom 1 3 3"},
            {"spring", "0 Custom_Claire_WarpRoom 1 3 3"}
        };

        private static readonly Dictionary<string, string> _martinScheduleWhenMovies = new(){
            {"Tue", "0 Custom_Martin_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Martin_Blink/1530 BusStop 12 8 0/1740 Custom_DayEnd_WarpRoom 3 3 0"},
            {"Sat", "0 Custom_Martin_WarpRoom 1 3 3/650 Town 92 44 2/850 MovieTheater 7 5 2 Martin_Blink/2130 BusStop 12 8 0/2340 Custom_DayEnd_WarpRoom 3 3 0"},
            {"spring", "0 Custom_Martin_WarpRoom 1 3 3"}
        };

        private static readonly Dictionary<string, Dictionary<string, string>> characterToSchedule = new(){
            {"Claire", _claireScheduleWhenMovies},
            {"Martin", _martinScheduleWhenMovies}
        };

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

        // Railroad Boulder Special Order won't load if Iridium Bomb is sent early, so we duplicate it so the player gets it.
        private static void AppendFakeClintBoulderSpecialOrderKey()
        {
            var untimedSpecialOrdersType = AccessTools.TypeByName("HarmonyPatch_UntimedSpecialOrders");
            var specialOrderKeysField = _modHelper.Reflection.GetField<List<string>>(untimedSpecialOrdersType, "SpecialOrderKeys");
            var specialOrderKeys = specialOrderKeysField.GetValue();
            specialOrderKeys.Add("Clint2Again");
        }

        public static void ChangeScheduleForMovie()
        {
            if (_archipelago.GetReceivedItemCount("Progressive Movie Theater") >= 2)
            {
                string[] charactersToModify = {"Claire", "Martin"};
                foreach(var name in charactersToModify)
                {
                    var schedules = Game1.content.Load<Dictionary<string, string>>($"Characters\\Schedules\\{name}");
                    foreach (KeyValuePair<string, string> day in schedules)
                    {
                        if (characterToSchedule[name].Keys.Contains(day.Key))
                        {
                            schedules[day.Key] = characterToSchedule[name][day.Key];
                        }
                    }
                }              
            }
        }
    }
}