﻿using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class QuestLogInjections
    {
        private const string ARCHAEOLOGY_QUEST_ID = "23";
        private const string ARCHAEOLOGY_QUEST_NAME = "Archaeology";

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public QuestLog()
        public static void Constructor_MakeQuestsNonCancellable_Postfix(QuestLog __instance)
        {
            try
            {
                foreach (var quest in Game1.player.questLog)
                {
                    quest.canBeCancelled.Value = false;
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Constructor_MakeQuestsNonCancellable_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public void foundArtifact(string itemId, int number)
        public static void FoundArtifact_StartArchaeologyIfMissed_Postfix(Farmer __instance, string itemId, int number)
        {
            try
            {
                if (itemId == "102" || _locationChecker.IsLocationChecked(ARCHAEOLOGY_QUEST_NAME) || __instance.hasQuest(ARCHAEOLOGY_QUEST_ID))
                {
                    return;
                }

                __instance.addQuest(ARCHAEOLOGY_QUEST_ID);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(FoundArtifact_StartArchaeologyIfMissed_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
