using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class SpecialOrderConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        public static double AdjustSpecialOrderAmountMultiplier(double amountMultiplier)
        {
            try
            {
                if (_archipelago.SlotData.Jojapocalypse.Jojapocalypse == JojapocalypseSetting.Disabled)
                {
                    return amountMultiplier;
                }

                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.SPECIAL_ORDER_BOARD) +
                                      _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.SPECIAL_ORDER_QI);
                if (numberPurchased <= 0)
                {
                    return amountMultiplier;
                }

                var newMultiplier = amountMultiplier * (1 + (0.1 * numberPurchased));

                return newMultiplier;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AdjustSpecialOrderAmountMultiplier)}:\n{ex}");
                return amountMultiplier;
            }
        }
    }
}
