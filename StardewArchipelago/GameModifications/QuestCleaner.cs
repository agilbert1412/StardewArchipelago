﻿using StardewArchipelago.Locations;
using StardewValley;
using StardewValley.Quests;

namespace StardewArchipelago.GameModifications
{
    public class QuestCleaner
    {
        private StardewLocationChecker _locationChecker;

        private const string INITIATION_ID_SECOND_PART = "16";
        private const string RAT_PROBLEM_ID = "26";
        private const string MEET_THE_WIZARD_ID = "1";
        private const string RAISING_ANIMALS_ID = "7";

        public QuestCleaner(StardewLocationChecker locationChecker)
        {
            _locationChecker = locationChecker;
        }

        public void CleanQuests(Farmer player)
        {
            CleanInitiation();
            foreach (var quest in player.questLog)
            {
                CleanRatProblem(player, quest);
                CleanRaisingAnimals(quest);
            }
        }

        private void CleanInitiation()
        {
            if (Game1.player.mailReceived.Contains("guildMember"))
            {
                return;
            }

            Game1.player.mailReceived.Add("guildMember");
        }

        private void CleanRatProblem(Farmer player, Quest quest)
        {
            if (quest.id.Value != RAT_PROBLEM_ID)
            {
                return;
            }

            if (_locationChecker.IsLocationMissing("Quest: Rat Problem"))
            {
                return;
            }

            quest.questComplete();
        }

        private void CleanRaisingAnimals(Quest quest)
        {
            if (quest.id.Value != RAISING_ANIMALS_ID)
            {
                return;
            }

            var farm = Game1.getFarm();
            var hasAnyCoop = farm.isBuildingConstructed("Coop") || farm.isBuildingConstructed("Big Coop") || farm.isBuildingConstructed("Deluxe Coop");

            if (!hasAnyCoop)
            {
                return;
            }

            quest.questComplete();
        }
    }
}
