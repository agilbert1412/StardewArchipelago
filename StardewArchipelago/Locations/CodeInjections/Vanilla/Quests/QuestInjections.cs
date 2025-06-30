using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework.Content;
using StardewArchipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Locations.Secrets;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Quests
{
    public static class QuestInjections
    {
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
            StoryQuestInjections.Initialize(logger, helper, archipelago, locationChecker);
            HelpWantedQuestInjections.Initialize(logger, helper, archipelago, locationChecker);
            OtherQuestInjections.Initialize(logger, helper, archipelago, locationChecker);
            // SecretNotesInjections.Initialize(logger, helper, archipelago, locationChecker);
        }

        // public virtual void questComplete()
        public static bool QuestComplete_LocationInsteadOfReward_Prefix(Quest __instance)
        {
            try
            {
                if (__instance.completed.Value)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                bool runOriginal;
                if (OtherQuestInjections.TryHandleQuestComplete(__instance, out runOriginal))
                {
                    return runOriginal;
                }

                if (HelpWantedQuestInjections.TryHandleQuestComplete(__instance, out runOriginal))
                {
                    return runOriginal;
                }

                if (SecretNotesInjections.TryHandleQuestComplete(__instance, out runOriginal))
                {
                    return runOriginal;
                }

                _logger.LogInfo($"Trying to handle quest complete for '{__instance.GetName()}'");
                if (StoryQuestInjections.TryHandleQuestComplete(__instance, out runOriginal))
                {
                    return runOriginal;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(QuestComplete_LocationInsteadOfReward_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static void OriginalQuestCompleteCode(Quest __instance)
        {
            __instance.completed.Value = true;
            Game1.player.currentLocation?.customQuestCompleteBehavior(__instance.id.Value);
            if (__instance.nextQuests.Count > 0)
            {
                foreach (var nextQuest in __instance.nextQuests.Where(x => !string.IsNullOrEmpty(x) && x != "-1"))
                {
                    Game1.player.addQuest(nextQuest);
                }

                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
            }

            Game1.player.questLog.Remove(__instance);
            Game1.playSound("questcomplete");
            if (__instance.id.Value == "126")
            {
                Game1.player.mailReceived.Add("emilyFiber");
                Game1.player.activeDialogueEvents.Add("emilyFiber", 2);
            }

            Game1.dayTimeMoneyBox.questsDirty = true;
            Game1.player.autoGenerateActiveDialogueEvent("questComplete_" + __instance.id.Value);
        }
    }
}
