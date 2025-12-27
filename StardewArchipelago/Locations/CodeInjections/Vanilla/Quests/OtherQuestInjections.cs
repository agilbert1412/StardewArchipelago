using System;
using StardewArchipelago.Locations.Festival;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework.Content;
using StardewArchipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Quests
{
    public static class OtherQuestInjections
    {
        private const string DESERT_FESTIVAL_FISHING_QUEST_ID = "98765";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ContentManager _englishContentManager;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _englishContentManager = new ContentManager(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);
        }
        public static bool TryHandleQuestComplete(Quest quest, out bool runOriginal)
        {
            runOriginal = MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            if (quest.id.Value == null || !quest.id.Value.Equals(DESERT_FESTIVAL_FISHING_QUEST_ID, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            // This one is a quest for some reason
            _locationChecker.AddCheckedLocation(FestivalLocationNames.WILLYS_CHALLENGE);
            QuestInjections.OriginalQuestCompleteCode(quest);
            return true;

        }
    }
}
