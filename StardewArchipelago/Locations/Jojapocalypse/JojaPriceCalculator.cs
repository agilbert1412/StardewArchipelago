﻿
using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojaPriceCalculator
    {
        private ILogger _logger;
        private StardewArchipelagoClient _archipelago;
        private StardewLocationChecker _locationChecker;
        private int _totalLocationsInSlot;
        private double _currentPriceMultiplier;

        public JojaPriceCalculator(ILogger logger, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _totalLocationsInSlot = _locationChecker.GetAllLocations().Count();
            _currentPriceMultiplier = 1.0;
        }

        public int GetNextItemPrice()
        {
            var missingLocationsCount = _locationChecker.GetAllMissingLocations().Count;
            var doneLocationsCount = _totalLocationsInSlot - missingLocationsCount;

            if (_archipelago.SlotData.Jojapocalypse.PricingPattern == JojapocalypsePricingPattern.Exponential)
            {
                return GetNextItemPriceExponential(doneLocationsCount);
            }
            if (_archipelago.SlotData.Jojapocalypse.PricingPattern == JojapocalypsePricingPattern.Linear)
            {
                return GetNextItemPriceLinear(doneLocationsCount);
            }

            return _archipelago.SlotData.Jojapocalypse.StartPrice;
        }

        private int GetNextItemPriceExponential(int doneLocationsCount)
        {
            var ratio = (double)doneLocationsCount / (_totalLocationsInSlot - 1);
            var priceNext = _archipelago.SlotData.Jojapocalypse.StartPrice * Math.Pow((double)_archipelago.SlotData.Jojapocalypse.EndPrice / _archipelago.SlotData.Jojapocalypse.StartPrice, ratio);
            var priceMultiplied = Math.Min(_archipelago.SlotData.Jojapocalypse.EndPrice, priceNext * _currentPriceMultiplier);
            return (int)Math.Round(priceMultiplied);
        }

        private int GetNextItemPriceLinear(int doneLocationsCount)
        {
            var priceRange = _archipelago.SlotData.Jojapocalypse.EndPrice - _archipelago.SlotData.Jojapocalypse.StartPrice;
            var priceIncreasePerLocation = (double)priceRange / (_totalLocationsInSlot - 1);
            var priceNext = _archipelago.SlotData.Jojapocalypse.StartPrice + (doneLocationsCount * priceIncreasePerLocation);
            var priceMultiplied = Math.Min(_archipelago.SlotData.Jojapocalypse.EndPrice, priceNext * _currentPriceMultiplier);
            return (int)Math.Round(priceMultiplied);
        }

        public void SetPriceMultiplier(double priceMultiplier)
        {
            _currentPriceMultiplier = priceMultiplier;
        }
    }
}
