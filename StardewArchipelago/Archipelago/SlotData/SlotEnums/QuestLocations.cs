using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Constants.Vanilla;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    public class QuestLocations
    {
        private readonly int _value;
        public bool StoryQuestsEnabled => _value >= 0;
        public int HelpWantedNumber => Math.Max(0, _value);

        public Dictionary<string, string> HelpWantedQuests { get; set; }

        public Dictionary<string, string> GetRemainingHelpWanteds(LocationChecker locationChecker)
        {
            return HelpWantedQuests.Where(x => locationChecker.IsLocationMissing(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }

        internal QuestLocations(int value, Dictionary<string, string> helpWanteds)
        {
            _value = value;
            HelpWantedQuests = helpWanteds;
        }

        public QuestType GetQuestType(string locationName)
        {
            if (!locationName.StartsWith(DailyQuest.HELP_WANTED_PREFIX))
            {
                throw new ArgumentException($"The Location `{locationName}` is not a Help Wanted Quest");
            }

            if (locationName.Contains(DailyQuest.ITEM_DELIVERY))
            {
                return QuestType.ItemDelivery;
            }

            if (locationName.Contains(DailyQuest.GATHERING))
            {
                return QuestType.ResourceCollection;
            }

            if (locationName.Contains(DailyQuest.SLAY_MONSTERS))
            {
                return QuestType.SlayMonsters;
            }

            if (locationName.Contains(DailyQuest.FISHING))
            {
                return QuestType.Fishing;
            }

            if (locationName.Contains(DailyQuest.HELLO))
            {
                return QuestType.Socialize;
            }

            throw new ArgumentException($"The Location `{locationName}` is not a recognized type of Help Wanted Quest");
        }

        public (string nameInfo, string extraInfo) GetExtraInfo(string locationName)
        {
            return GetExtraInfo(GetQuestType(locationName), locationName);
        }

        public (string nameInfo, string extraInfo) GetExtraInfo(QuestType questType, string locationName)
        {
            var prefix = DailyQuest.HELP_WANTED_PREFIX;
            prefix += questType switch
            {
                QuestType.ItemDelivery => $"{DailyQuest.ITEM_DELIVERY} ",
                QuestType.SlayMonsters => $"{DailyQuest.SLAY_MONSTERS} ",
                QuestType.ResourceCollection => $"{DailyQuest.GATHERING} ",
                QuestType.Fishing => $"{DailyQuest.FISHING} ",
                QuestType.Socialize => $"{DailyQuest.HELLO} ",
                _ => throw new ArgumentOutOfRangeException(nameof(questType), questType, null)
            };

            var nameInfo = locationName.Substring(prefix.Length);
            var extraInfo = HelpWantedQuests[locationName];
            return (nameInfo, extraInfo);
        }
    }
}