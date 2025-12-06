using System;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Quests;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.InGameLocations
{
    internal class JojaObtainableArchipelagoLocation : ObtainableArchipelagoLocation
    {
        private JojaLocationChecker _jojaLocationChecker;
        private JojaPriceCalculator _jojaPriceCalculator;
        private int _minPrice;
        private int _maxPrice;

        public JojaObtainableArchipelagoLocation(string locationName, LogHandler logger, IModHelper modHelper, JojaLocationChecker locationChecker, StardewArchipelagoClient archipelago, JojaPriceCalculator jojaPriceCalculator) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, jojaPriceCalculator)
        {
        }

        public JojaObtainableArchipelagoLocation(string locationDisplayName, string locationName, LogHandler logger, IModHelper modHelper, JojaLocationChecker locationChecker, StardewArchipelagoClient archipelago, JojaPriceCalculator jojaPriceCalculator) : base(locationDisplayName, locationName, logger, modHelper, locationChecker, archipelago, Array.Empty<Hint>(), false)
        {
            _jojaLocationChecker = locationChecker;
            _jojaPriceCalculator = jojaPriceCalculator;
            _minPrice = archipelago.SlotData.Jojapocalypse.StartPrice;
            _maxPrice = archipelago.SlotData.Jojapocalypse.EndPrice;
        }

        protected override Texture2D GetCorrectTexture(LogHandler logger, IModHelper modHelper, ScoutedLocation scoutedLocation, StardewArchipelagoClient archipelago, Hint relatedHint)
        {
            return ArchipelagoTextures.GetArchipelagoLogo(48, ArchipelagoTextures.JOJA);
        }

        public override int salePrice(bool ignoreProfitMargins = false)
        {
            const double priceVariance = 0.5;
            var currentPrice = _jojaPriceCalculator.GetNextItemPrice();
            var random = Utility.CreateDaySaveRandom(QualifiedItemId.GetHashCode());
            var randomValue = (random.NextDouble() + (1-priceVariance)) * (priceVariance*2);
            var price = (int)Math.Round(currentPrice * randomValue);
            return price;
        }

        public override bool actionWhenPurchased(string shopId)
        {
            var result = base.actionWhenPurchased(shopId);
            ArchipelagoJunimoNoteMenu.CompleteBundleIfExists(LocationName);
            CompleteQuestIfExists();
            return result;
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
