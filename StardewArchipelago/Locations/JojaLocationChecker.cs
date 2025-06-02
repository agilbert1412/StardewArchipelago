using System.Collections.Generic;
using System.Linq;
using StardewValley.Extensions;

namespace StardewArchipelago.Locations
{
    public class JojaLocationChecker : ILocationChecker
    {
        private StardewLocationChecker _locationChecker;
        private HashSet<string> _locationsCheckedByJoja;

        public JojaLocationChecker(StardewLocationChecker locationChecker, List<string> locationsAlreadyCheckedByJoja)
        {
            _locationChecker = locationChecker;
            _locationsCheckedByJoja = new HashSet<string>(locationsAlreadyCheckedByJoja);
        }

        public void AddWalnutCheckedLocation(string locationName)
        {
            _locationChecker.AddWalnutCheckedLocation(locationName);
            _locationsCheckedByJoja.Add(locationName);
        }

        public void AddCheckedLocations(string[] locationNames)
        {
            _locationChecker.AddCheckedLocations(locationNames);
            _locationsCheckedByJoja.AddRange(locationNames);
        }

        public void AddCheckedLocation(string locationName)
        {
            _locationChecker.AddCheckedLocation(locationName);
            _locationsCheckedByJoja.Add(locationName);
        }

        public List<string> GetAllLocationsCheckedByJoja()
        {
            return _locationsCheckedByJoja.ToList();
        }
    }
}
