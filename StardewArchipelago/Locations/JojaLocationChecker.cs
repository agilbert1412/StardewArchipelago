using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
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

        public void AddWalnutCheckedLocation(string locationName)
        {
            _locationsCheckedByJoja.Add(locationName);
            _locationChecker.AddWalnutCheckedLocation(locationName);
        }

        public void AddCheckedLocations(string[] locationNames)
        {
            _locationsCheckedByJoja.AddRange(locationNames);
            _locationChecker.AddCheckedLocations(locationNames);
        }

        public void AddCheckedLocation(string locationName)
        {
            _locationsCheckedByJoja.Add(locationName);
            _locationChecker.AddCheckedLocation(locationName);
        }

        public List<string> GetAllLocationsCheckedByJoja()
        {
            return _locationsCheckedByJoja.ToList();
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
    }
}
