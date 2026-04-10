using Newtonsoft.Json;
using System.Collections.Generic;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class DataRandomization
    {
        private RandomizedData _randomizedData;

        public Dictionary<string, RandomizedCropData> CropData => _randomizedData.Crops;
        public Dictionary<string, RandomizedFishData> FishData => _randomizedData.Fish;

        public DataRandomization(string randomizedDataJson)
        {
            _randomizedData = JsonConvert.DeserializeObject<RandomizedData>(randomizedDataJson);
            _randomizedData.AssignNames();
        }
    }
}