using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;

namespace StardewArchipelago.Archipelago
{
    public class StardewDatapackageCache : DataPackageCache
    {
        public StardewDatapackageCache(string snakeCaseGameName, params string[] pathsToFolder) : base(snakeCaseGameName, pathsToFolder)
        {
        }

        public StardewDatapackageCache(IJsonLoader jsonLoader, string snakeCaseGameName, params string[] pathsToFolder) : base(jsonLoader, snakeCaseGameName, pathsToFolder)
        {
        }

        public StardewDatapackageCache(IArchipelagoLoader<ArchipelagoItem> itemLoader, IArchipelagoLoader<ArchipelagoLocation> locationLoader, string snakeCaseGameName, params string[] pathsToFolder) : base(itemLoader, locationLoader, snakeCaseGameName, pathsToFolder)
        {
        }

        public IEnumerable<StardewArchipelagoLocation> GetAllLocations()
        {
            return _locationCacheById.Values.Cast<StardewArchipelagoLocation>();
        }

        public StardewArchipelagoLocation GetLocation(string locationName)
        {
            return (StardewArchipelagoLocation)_locationCacheByName[locationName];
        }
    }
}
