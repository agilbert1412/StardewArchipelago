using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace StardewArchipelago.Json
{
    public class SmapiJsonLoader : IJsonLoader
    {
        private readonly IModHelper _helper;

        public SmapiJsonLoader(IModHelper helper)
        {
            _helper = helper;
        }

        public Dictionary<TKey, TValue> DeserializeFile<TKey, TValue>(string filePath)
        {
            return _helper.Data.ReadJsonFile<Dictionary<TKey, TValue>>(filePath);
        }

        public Dictionary<string, JObject> DeserializeFile(string filePath)
        {
            return DeserializeFile<string, JObject>(filePath);
        }
    }
}
