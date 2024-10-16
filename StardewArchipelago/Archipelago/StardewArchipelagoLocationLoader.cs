using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;

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
            var location = new ArchipelagoLocation(locationName, id);
            return location;
        }
    }
}
