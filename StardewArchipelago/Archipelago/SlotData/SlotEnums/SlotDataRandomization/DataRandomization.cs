using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class DataRandomization
    {
        private RandomizedData _randomizedData;

        public Dictionary<string, RandomizedCropData> CropData => _randomizedData.Crops;
        public Dictionary<string, RandomizedFishData> FishData => _randomizedData.Fish;
        public Dictionary<string, RandomizedCropData> CropDataBySeedName { get; }

        public DataRandomization(string randomizedDataJson)
        {
            _randomizedData = JsonConvert.DeserializeObject<RandomizedData>(randomizedDataJson);
            _randomizedData.AssignNames();
            // CropDataBySeedName = CropData.Where(x => !string.IsNullOrWhiteSpace(x.Value.Seed)).ToDictionary(x => x.Value.Seed, x => x.Value);
        }
    }
}