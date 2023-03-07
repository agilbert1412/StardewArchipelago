using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago
{
    public class DataPackageCache
    {
        private Dictionary<string, ArchipelagoItem> _itemCacheByName { get; }
        private Dictionary<long, ArchipelagoItem> _itemCacheById { get; }
        private Dictionary<string, ArchipelagoLocation> _locationCacheByName { get; }
        private Dictionary<long, ArchipelagoLocation> _locationCacheById { get; }

        public DataPackageCache(IModHelper helper)
        {
            var items = ArchipelagoItem.LoadItems(helper);
            var locations = ArchipelagoLocation.LoadLocations(helper);

            _itemCacheByName = new Dictionary<string, ArchipelagoItem>();
            _itemCacheById = new Dictionary<long, ArchipelagoItem>();
            _locationCacheByName = new Dictionary<string, ArchipelagoLocation>();
            _locationCacheById = new Dictionary<long, ArchipelagoLocation>();

            foreach (var archipelagoItem in items)
            {
                _itemCacheByName.Add(archipelagoItem.Name, archipelagoItem);
                _itemCacheById.Add(archipelagoItem.Id, archipelagoItem);
            }

            foreach (var archipelagoLocation in locations)
            {
                _locationCacheByName.Add(archipelagoLocation.Name, archipelagoLocation);
                _locationCacheById.Add(archipelagoLocation.Id, archipelagoLocation);
            }
        }

        public string GetLocalItemName(long itemId)
        {
            return _itemCacheById[itemId].Name;
        }

        public long GetLocalItemId(string itemName)
        {
            return _itemCacheByName[itemName].Id;
        }

        public string GetLocalLocationName(long locationId)
        {
            return _locationCacheById[locationId].Name;
        }

        public long GetLocalLocationId(string locationName)
        {
            return _locationCacheByName[locationName].Id;
        }
    }
}
