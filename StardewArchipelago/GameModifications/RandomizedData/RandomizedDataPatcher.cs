using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class RandomizedDataPatcher
    {
        private readonly LogHandler _logger;
        private readonly Harmony _harmony;
        private readonly IModHelper _helper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly DataRandomization _dataRandomization;
        private readonly StardewItemManager _stardewItemManager;
        private readonly FishDataModifier _fishDataModifier;

        public RandomizedDataPatcher(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _harmony = harmony;
            _helper = modHelper;
            _archipelago = archipelago;
            _dataRandomization = _archipelago.SlotData.DataRandomization;
            _stardewItemManager = stardewItemManager;
            _fishDataModifier = new FishDataModifier(_logger, _helper, _archipelago, _dataRandomization);
        }

        public void PatchAllRandomizedData()
        {
            _helper.Events.Content.AssetRequested += _fishDataModifier.OnFishDataRequested;
        }

        public void CleanRandomizedDataEvents()
        {
            _helper.Events.Content.AssetRequested -= _fishDataModifier.OnFishDataRequested;
        }
    }

}
