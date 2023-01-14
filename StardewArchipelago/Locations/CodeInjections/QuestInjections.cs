using System;
using System.Linq;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class QuestInjections
    {
        private const string QUEST_AP_LOCATION_PATTERN = "Quest #{0} ({1})";

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool QuestComplete_LocationInsteadOfReward_Prefix(Quest __instance)
        {
            try
            {
                if (__instance.completed.Value)
                {
                    return true; // run original logic
                }

                if (__instance.dailyQuest.Value ||
                    __instance.questType.Value == (int)QuestType.Fishing)
                {
                    ++Game1.stats.QuestsCompleted;
                }

                __instance.completed.Value = true;
                if (__instance.nextQuests.Count > 0)
                {
                    foreach (var nextQuest in __instance.nextQuests.Where(nextQuest => nextQuest > 0))
                    {
                        Game1.player.questLog.Add(Quest.getQuestFromId(nextQuest));
                    }

                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
                }

                Game1.player.questLog.Remove(__instance);
                Game1.playSound("questcomplete");
                if (__instance.id.Value == 126)
                {
                    Game1.player.mailReceived.Add("emilyFiber");
                    Game1.player.activeDialogueEvents.Add("emilyFiber", 2);
                }
                Game1.dayTimeMoneyBox.questsDirty = true;

                var apLocationName =
                    string.Format(QUEST_AP_LOCATION_PATTERN, __instance.id.Value, __instance.GetName());
                _locationChecker.AddCheckedLocation(apLocationName);

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(QuestComplete_LocationInsteadOfReward_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }

    public enum QuestType
    {
        Basic = 1,
        Crafting = 2,
        ItemDelivery = 3,
        Monster = 4,
        SlayMonster = 4,
        Socialize = 5,
        Location = 6,
        Fishing = 7,
        Building = 8,
        ItemHarvest = 9,
        LostItem = 9,
        SecretLostItem = 9,
        ResourceCollection = 10,
    }
}
