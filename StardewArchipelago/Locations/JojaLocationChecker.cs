using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Locations;
using StardewValley;
using StardewValley.Extensions;

namespace StardewArchipelago.Locations
{
    public class JojaLocationChecker : ILocationChecker
    {
        private StardewArchipelagoClient _archipelago;
        private StardewLocationChecker _locationChecker;
        private HashSet<string> _locationsCheckedByJoja;
        private Dictionary<string, int> _checkedLocationsByTag;

        public JojaLocationChecker(StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, List<string> locationsAlreadyCheckedByJoja)
        {
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _locationsCheckedByJoja = new HashSet<string>(locationsAlreadyCheckedByJoja);
            _checkedLocationsByTag = new Dictionary<string, int>();
        }

        public void AddCheckedLocations(string[] locationNames)
        {
            _locationsCheckedByJoja.AddRange(locationNames);
            _locationChecker.RememberCheckedLocations(locationNames);
            _locationChecker.SendAllLocationChecks();
        }

        public void AddCheckedLocation(string locationName)
        {
            _locationsCheckedByJoja.Add(locationName);
            _locationChecker.RememberCheckedLocation(locationName);
            _locationChecker.SendAllLocationChecks();
        }

        public List<string> GetAllLocationsCheckedByJoja()
        {
            return _locationsCheckedByJoja.ToList();
        }

        public double GetPercentCheckedLocationsByJoja()
        {
            var countByJoja = _locationsCheckedByJoja.Count;
            var totalLocations = _locationChecker.GetAllLocations().Count();
            return (double)countByJoja / totalLocations;
        }

        public int CountCheckedLocationsWithTag(string locationTag)
        {
            if (!_checkedLocationsByTag.ContainsKey(locationTag))
            {
                _checkedLocationsByTag.Add(locationTag, _locationsCheckedByJoja.Count(x => _archipelago.DataPackageCache.GetLocation(x).LocationTags.Contains(locationTag)));
            }
            return _checkedLocationsByTag[locationTag];
        }

        public void RecountTags()
        {
            _checkedLocationsByTag.Clear();
        }

        public bool HasCheckedLocation(string locationName)
        {
            return _locationsCheckedByJoja.Contains(locationName);
        }

        public string GetTodayRandomOfferLocation()
        {
            var random = Utility.CreateDaySaveRandom();
            var missing = _locationChecker.GetAllMissingLocationNames().ToArray();
            var index = random.Next(missing.Length);
            return missing[index];
        }
    }
}
