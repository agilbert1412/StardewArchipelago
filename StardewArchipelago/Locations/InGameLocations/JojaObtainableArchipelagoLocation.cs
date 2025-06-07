using System;
using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Quests;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.InGameLocations
{
    internal class JojaObtainableArchipelagoLocation : ObtainableArchipelagoLocation
    {
        private JojaLocationChecker _jojaLocationChecker;
        private JojaPriceCalculator _jojaPriceCalculator;

        public JojaObtainableArchipelagoLocation(string locationName, LogHandler logger, IModHelper modHelper, JojaLocationChecker locationChecker, StardewArchipelagoClient archipelago, JojaPriceCalculator jojaPriceCalculator) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, jojaPriceCalculator)
        {
        }

        public JojaObtainableArchipelagoLocation(string locationDisplayName, string locationName, LogHandler logger, IModHelper modHelper, JojaLocationChecker locationChecker, StardewArchipelagoClient archipelago, JojaPriceCalculator jojaPriceCalculator) : base(locationDisplayName, locationName, logger, modHelper, locationChecker, archipelago, Array.Empty<Hint>(), false)
        {
            _jojaLocationChecker = locationChecker;
            _jojaPriceCalculator = jojaPriceCalculator;
        }

        public override int salePrice(bool ignoreProfitMargins = false)
        {
            return _jojaPriceCalculator.GetNextItemPrice();
        }

        public override bool actionWhenPurchased(string shopId)
        {
            ArchipelagoJunimoNoteMenu.CompleteBundleIfExists(LocationName);
            CompleteQuestIfExists();
            return base.actionWhenPurchased(shopId);
        }

        private void CompleteQuestIfExists()
        {
            var idToComplete = "";
            foreach (var activeQuest in Game1.player.questLog)
            {
                var questName = activeQuest.GetName();
                var englishQuestName = StoryQuestInjections.GetQuestEnglishName(activeQuest.id.Value, questName);
                if (!LocationName.Equals(englishQuestName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                idToComplete = activeQuest.id.Value;
                break;
            }

            if (!string.IsNullOrWhiteSpace(idToComplete))
            {
                Game1.player.completeQuest(idToComplete);
            }
        }
    }
}
