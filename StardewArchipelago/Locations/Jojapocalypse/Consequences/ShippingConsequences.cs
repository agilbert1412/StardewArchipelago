using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class ShippingConsequences
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

        public static double AdjustProfitMargin(double profitMargin)
        {
            try
            {
                if (_archipelago.SlotData.Jojapocalypse.Jojapocalypse == JojapocalypseSetting.Disabled)
                {
                    return profitMargin;
                }

                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.SHIPSANITY);
                if (numberPurchased <= 0)
                {
                    return profitMargin;
                }

                var profitMultiplierPerPurchase = 0.99;
                if (_archipelago.SlotData.Shipsanity == Shipsanity.Everything)
                {
                    profitMultiplierPerPurchase = 0.997;
                }

                var newProfitMargin = profitMargin * Math.Pow(profitMultiplierPerPurchase, numberPurchased);
                return newProfitMargin;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AdjustProfitMargin)}:\n{ex}");
                return profitMargin;
            }
        }
    }
}
