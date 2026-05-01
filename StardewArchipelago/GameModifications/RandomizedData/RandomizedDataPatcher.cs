using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Logging;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mods;

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
        private readonly CropDataModifier _cropDataModifier;
        private readonly FishDataModifier _fishDataModifier;
        private readonly FestivalDataModifier _festivalDataModifier;
        private readonly ObjectDataModifier _objectDataModifier;
        private readonly ShopEntriesDataModifier _shopDataModifier;

        public RandomizedDataPatcher(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _harmony = harmony;
            _helper = modHelper;
            _archipelago = archipelago;
            _dataRandomization = _archipelago.SlotData.DataRandomization;
            _stardewItemManager = stardewItemManager;
            _cropDataModifier = new CropDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            _fishDataModifier = new FishDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            _festivalDataModifier = new FestivalDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            _objectDataModifier = new ObjectDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            _shopDataModifier = new ShopEntriesDataModifier(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
            RandomizedFishDataInjections.Initialize(_logger, _helper, _archipelago, _stardewItemManager, _dataRandomization);
        }

        public void PatchAllRandomizedData()
        {
            _helper.Events.Content.AssetRequested += _cropDataModifier.OnCropDataRequested;
            _helper.Events.Content.AssetRequested += _fishDataModifier.OnFishDataRequested;
            _helper.Events.Content.AssetRequested += _fishDataModifier.OnLocationsDataRequested;
            _helper.Events.Content.AssetRequested += _objectDataModifier.OnObjectDataRequested;
            _helper.Events.Content.AssetRequested += _cropDataModifier.OnObjectDataRequested;
            _helper.GameContent.InvalidateCache("Data/Crops");
            _helper.GameContent.InvalidateCache("Data/Fish");
            _helper.GameContent.InvalidateCache("Data/Locations");
            _helper.GameContent.InvalidateCache("Data/Objects");

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.getFish)),
                postfix: new HarmonyMethod(typeof(RandomizedFishDataInjections), nameof(RandomizedFishDataInjections.GetFish_CorrectMinesFish_Postfix))
            );

            PatchRandomizedFestivalsData();
            PatchRandomizedShopsData();
        }

        private void PatchRandomizedFestivalsData()
        {
            if (_dataRandomization.FestivalData == null || _dataRandomization.FestivalData.Count <= 0)
            {
                return;
            }

            _helper.Events.Content.AssetRequested += _festivalDataModifier.OnFestivalDatesDataRequested;
            //_helper.Events.Content.AssetRequested += _festivalDataModifier.OnPassiveFestivalsDataRequested;

            _helper.GameContent.InvalidateCache("Data/Festivals/FestivalDates");
            _helper.GameContent.InvalidateCache("Data/PassiveFestivals");

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.tryToLoadFestivalData)),
                prefix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.TryToLoadFestivalData_LoadOriginalDataForNewDate_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.tryToLoadFestival)),
                prefix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.TryToLoadFestival_ConsiderModifiedFestivals_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getStartTimeOfFestival)),
                prefix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.GetStartTimeOfFestival_LoadOriginalDataForNewDate_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Billboard), nameof(Billboard.GetEventsForDay)),
                prefix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.GetEventsForDay_ConsiderModifiedFestivals_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.warpFarmer), new []{typeof(LocationRequest), typeof(int), typeof(int), typeof(int) }),
                prefix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.WarpFarmer_ConsiderModifiedFestivals_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(ModHooks), nameof(ModHooks.OnGame1_PerformTenMinuteClockUpdate)),
                prefix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.OnGame1_PerformTenMinuteClockUpdate_ConsiderModifiedFestivals_Prefix)),
                postfix: new HarmonyMethod(typeof(FestivalDataModifier), nameof(FestivalDataModifier.OnGame1_PerformTenMinuteClockUpdate_ConsiderModifiedFestivals_Postfix))
            );
        }

        private void PatchRandomizedShopsData()
        {
            if (!AreShopsDataRandomized())
            {
                return;
            }

            _helper.Events.Content.AssetRequested += _shopDataModifier.OnShopsDataRequested;
            _helper.GameContent.InvalidateCache("Data/Shops");
        }

        public void CleanRandomizedDataEvents()
        {
            _helper.Events.Content.AssetRequested -= _cropDataModifier.OnCropDataRequested;
            _helper.Events.Content.AssetRequested -= _cropDataModifier.OnObjectDataRequested;
            _helper.Events.Content.AssetRequested -= _fishDataModifier.OnFishDataRequested;
            _helper.Events.Content.AssetRequested -= _fishDataModifier.OnLocationsDataRequested;
            _helper.Events.Content.AssetRequested -= _objectDataModifier.OnObjectDataRequested;
            CleanRandomizedFestivalDataEvents();
            CleanRandomizedShopDataEvents();
        }

        private void CleanRandomizedFestivalDataEvents()
        {
            if (_dataRandomization.FestivalData == null || _dataRandomization.FestivalData.Count <= 0)
            {
                return;
            }

            _helper.Events.Content.AssetRequested -= _festivalDataModifier.OnFestivalDatesDataRequested;
            //_helper.Events.Content.AssetRequested -= _festivalDataModifier.OnPassiveFestivalsDataRequested;
        }

        private void CleanRandomizedShopDataEvents()
        {
            if (!AreShopsDataRandomized())
            {
                return;
            }

            _helper.Events.Content.AssetRequested -= _shopDataModifier.OnShopsDataRequested;
        }

        private bool AreShopsDataRandomized()
        {
            return _dataRandomization.ShopsData != null && _dataRandomization.ShopsData.Any() && _dataRandomization.ShopsData.SelectMany(x => x.Value).Any();
        }
    }

}
