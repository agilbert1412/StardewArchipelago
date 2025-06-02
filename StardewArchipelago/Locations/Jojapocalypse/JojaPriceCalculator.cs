
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojaPriceCalculator
    {
        private const int START_PRICE = 10;
        private const int END_PRICE = 100000;
        private static readonly Dictionary<ItemClassification, double> CLASSIFICATION_MULTIPLIERS = new()
        {
            { ItemClassification.Progression, 1 },
            { ItemClassification.Useful, 0.4 },
            { ItemClassification.Trap, 0.1 },
            { ItemClassification.Filler, 0.2 },
        };

        private ILogger _logger;
        private StardewLocationChecker _locationChecker;
        private int _totalLocationsInSlot;
        private bool _useExponentialPricing = true;

        public JojaPriceCalculator(ILogger logger, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
            _totalLocationsInSlot = _locationChecker.GetAllLocations().Count();
        }

        public int GetNextItemPrice()
        {
            var missingLocationsCount = _locationChecker.GetAllMissingLocations().Count;
            var doneLocationsCount = _totalLocationsInSlot - missingLocationsCount;

            if (_useExponentialPricing)
            {
                return GetNextItemPriceExponential(doneLocationsCount);
            }
            else
            {
                return GetNextItemPriceLinear(doneLocationsCount);
            }
        }

        private int GetNextItemPriceExponential(int doneLocationsCount)
        {
            var ratio = (double)doneLocationsCount / (_totalLocationsInSlot - 1);
            var priceNext = START_PRICE * Math.Pow((double)END_PRICE / START_PRICE, ratio);
            return (int)Math.Round(priceNext);
        }

        private int GetNextItemPriceLinear(int doneLocationsCount)
        {
            var priceRange = END_PRICE - START_PRICE;
            var priceIncreasePerLocation = (double)priceRange / (_totalLocationsInSlot - 1);
            var priceNext = START_PRICE + (doneLocationsCount * priceIncreasePerLocation);
            return (int)Math.Round(priceNext);
        }
    }
}
