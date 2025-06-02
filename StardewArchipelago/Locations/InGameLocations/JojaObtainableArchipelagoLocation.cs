using System;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewArchipelago.Logging;
using StardewModdingAPI;

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
    }
}
