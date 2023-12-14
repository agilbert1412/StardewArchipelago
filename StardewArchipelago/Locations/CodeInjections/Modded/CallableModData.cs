using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewValley;
using StardewArchipelago.Locations.CodeInjections.Modded.SVE;
using StardewArchipelago.GameModifications.CodeInjections;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class CallableModData
    {
        private static readonly Dictionary<string, string[]> Base_Static_Events = new()
        {

        };

        private static readonly Dictionary<string, string[]> Total_Static_Events = new()
        {

        };

        private static readonly Dictionary<string, string[]> Base_OnWarped_Events = new()
        {

        };

        public static Dictionary<string, string[]> Total_OnWarped_Events = new()
        {

        };

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

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


        public void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            GenerateEventKeys();
            ReplaceCutscenes(Total_Static_Events);
            ReplaceCutscenes(Total_OnWarped_Events);
            GuntherInitializer();
            ChangeScheduleForMovie();
            
        }

        private static void GenerateEventKeys()
        {
            foreach (var (eventMapName, eventKeys) in Base_OnWarped_Events)
            {
                Total_OnWarped_Events[eventMapName] = eventKeys;
            }

            foreach (var (eventMapName, eventKeys) in Base_Static_Events)
            {
                Total_Static_Events[eventMapName] = eventKeys;
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                foreach (var (eventMapName, eventKeys) in SVECutsceneInjections.SVE_OnWarped_Events)
                {
                    Total_OnWarped_Events[eventMapName] = eventKeys;
                }

                foreach (var (eventMapName, eventKeys) in SVECutsceneInjections.SVE_Static_Events)
                {
                    Total_Static_Events[eventMapName] = eventKeys;
                }
            }
            
        }

        // These events only require to load at initialization due to lack of "When" requirements from CP.
        public void ReplaceCutscenes(Dictionary<string, string[]> events)
        {
            foreach (var (eventMapName, eventKeys) in events)
            {
                var mapKey = eventMapName.Split("|");
                var mapName = mapKey[0];
                var eventID = mapKey[1];
                var currentMapEventData = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + mapName);
                foreach (var eventKey in eventKeys)
                {

                    var newEventKey = "";
                    if (eventID == "")
                        eventID = eventKey.Split("/")[0];
                    // If CP does not add the event yet, continue.
                    if (!currentMapEventData.ContainsKey(eventKey))
                    {
                        continue;
                    }

                    // Append self-reference as requirement, or AP oriented event key.
                    newEventKey = eventKey + "/e " + eventID;
                    var eventData = currentMapEventData[eventKey];
                    currentMapEventData.Remove(eventKey);
                    currentMapEventData[newEventKey] = eventData;
                }
            }
        }

        public void ChangeScheduleForMovie()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SVE) && !(_archipelago.GetReceivedItemCount(TheaterInjections.MOVIE_THEATER_ITEM) < 2))
            {
                return;
            }
                foreach(var name in characterToSchedule.Keys)
                {
                    var loaded_schedules = Game1.content.Load<Dictionary<string, string>>($"Characters\\Schedules\\{name}");
                    var modified_schedules = characterToSchedule[name];
                    foreach (var day in modified_schedules)
                    {
                        loaded_schedules[day.Key] = characterToSchedule[name][day.Key];
                    }
                }              
        }

        public void GuntherInitializer()
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE) && !Game1.player.mailReceived.Contains("GuntherUnlocked"))
            {
                Game1.player.mailReceived.Add("GuntherUnlocked");
                Game1.player.eventsSeen.Add(103042015); // Gunther says hi
                Game1.player.eventsSeen.Add(1579125); // Marlon entering sewer immediately after; just annoying to see day one
            }
        }
    }
}