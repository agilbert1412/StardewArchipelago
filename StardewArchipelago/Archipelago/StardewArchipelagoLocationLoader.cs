using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;
using StardewArchipelago.Archipelago.ApworldData;

namespace StardewArchipelago.Archipelago
{
    public class StardewArchipelagoLocationLoader : ArchipelagoLocationLoader
    {
        public StardewArchipelagoLocationLoader(IJsonLoader jsonLoader) : base(jsonLoader)
        {
        }

        public override ArchipelagoLocation Load(string locationName, JToken locationJson)
        {
            var id = locationJson["code"].Value<long>();
            var tags = locationJson["tags"].Values<string>().ToHashSet();
            var contentPacks = locationJson["content_packs"].Values<string>().ToHashSet();
            var location = new StardewArchipelagoLocation(locationName, id, tags, contentPacks);
            return location;
        }
    }
}
