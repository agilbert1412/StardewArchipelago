﻿using System;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public static class NewSkillsPageInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private const int BEAR_KNOWLEDGE_EVENT_ID = 2120303;
        private const string BEAR_KNOWLEDGE = "Bear's Knowledge";

        private static bool? _hasTrulySeenBearKnowledgeEvent = null;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public NewSkillsPage(int x, int y, int width, int height)
        public static bool NewSkillsPageCtor_BearKnowledgeEvent_Prefix(int x, int y, int width, int height)
        {
            try
            {
                var hasSeenEvent = Game1.player.eventsSeen.Contains(BEAR_KNOWLEDGE_EVENT_ID);
                var hasBearKnowledge = _archipelago.HasReceivedItem(BEAR_KNOWLEDGE);
                if (hasSeenEvent == hasBearKnowledge)
                {
                    return true; // run original logic
                }

                if (_hasTrulySeenBearKnowledgeEvent == null)
                {
                    _hasTrulySeenBearKnowledgeEvent = hasSeenEvent;
                }

                if (hasBearKnowledge)
                {
                    Game1.player.eventsSeen.Add(BEAR_KNOWLEDGE_EVENT_ID);
                }
                else
                {
                    Game1.player.eventsSeen.Remove(BEAR_KNOWLEDGE_EVENT_ID);
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewSkillsPageCtor_BearKnowledgeEvent_Prefix)}:\n{ex}", LogLevel.Error); return true; // run original logic

            }
        }

        // public NewSkillsPage(int x, int y, int width, int height)
        public static void NewSkillsPageCtor_BearKnowledgeEvent_Postfix(int x, int y, int width, int height)
        {
            try
            {
                if (_hasTrulySeenBearKnowledgeEvent == null)
                {
                    return;
                }

                var hasSeenEvent = Game1.player.eventsSeen.Contains(BEAR_KNOWLEDGE_EVENT_ID);
                if (hasSeenEvent == _hasTrulySeenBearKnowledgeEvent)
                {
                    _hasTrulySeenBearKnowledgeEvent = null;
                    return;
                }

                if (hasSeenEvent)
                {
                    Game1.player.eventsSeen.Remove(BEAR_KNOWLEDGE_EVENT_ID);
                }
                else
                {
                    Game1.player.eventsSeen.Add(BEAR_KNOWLEDGE_EVENT_ID);
                }

                _hasTrulySeenBearKnowledgeEvent = null;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewSkillsPageCtor_BearKnowledgeEvent_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
