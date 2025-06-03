
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
        private ILogger _logger;
        private StardewLocationChecker _locationChecker;
        private int _totalLocationsInSlot;

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

            if (JojapocalypseConfigs.UseExponentialPricing)
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
            var priceNext = JojapocalypseConfigs.StartPrice * Math.Pow((double)JojapocalypseConfigs.EndPrice / JojapocalypseConfigs.StartPrice, ratio);
            return (int)Math.Round(priceNext);
        }

        private int GetNextItemPriceLinear(int doneLocationsCount)
        {
            var priceRange = JojapocalypseConfigs.EndPrice - JojapocalypseConfigs.StartPrice;
            var priceIncreasePerLocation = (double)priceRange / (_totalLocationsInSlot - 1);
            var priceNext = JojapocalypseConfigs.StartPrice + (doneLocationsCount * priceIncreasePerLocation);
            return (int)Math.Round(priceNext);
        }
    }
}
