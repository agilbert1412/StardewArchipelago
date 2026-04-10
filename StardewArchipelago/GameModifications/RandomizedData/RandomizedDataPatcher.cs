using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Logging;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

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
        private readonly ObjectDataModifier _objectDataModifier;

        public RandomizedDataPatcher(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _harmony = harmony;
            _helper = modHelper;
            _archipelago = archipelago;
            _dataRandomization = _archipelago.SlotData.DataRandomization;
            _stardewItemManager = stardewItemManager;
            _fishDataModifier = new FishDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            _objectDataModifier = new ObjectDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            RandomizedFishDataInjections.Initialize(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
        }

        public void PatchAllRandomizedData()
        {
            _helper.Events.Content.AssetRequested += _fishDataModifier.OnFishDataRequested;
            _helper.Events.Content.AssetRequested += _fishDataModifier.OnLocationsDataRequested;
            _helper.Events.Content.AssetRequested += _objectDataModifier.OnObjectDataRequested;
            _helper.GameContent.InvalidateCache("Data/Fish");
            _helper.GameContent.InvalidateCache("Data/Locations");
            _helper.GameContent.InvalidateCache("Data/Objects");

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.getFish)),
                postfix: new HarmonyMethod(typeof(RandomizedFishDataInjections), nameof(RandomizedFishDataInjections.GetFish_CorrectMinesFish_Postfix))
            );
        }

        public void CleanRandomizedDataEvents()
        {
            _helper.Events.Content.AssetRequested -= _fishDataModifier.OnFishDataRequested;
            _helper.Events.Content.AssetRequested -= _fishDataModifier.OnLocationsDataRequested;
            _helper.Events.Content.AssetRequested -= _objectDataModifier.OnObjectDataRequested;
        }
    }

}
