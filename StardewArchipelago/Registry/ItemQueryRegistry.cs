using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley.Internal;

namespace StardewArchipelago.Registry
{
    public class ItemQueryRegistry : IRegistry
    {
        private LogHandler _logger;
        private IModHelper _modHelper;
        private StardewArchipelagoClient _archipelago;
        private StardewLocationChecker _locationChecker;
        private WeaponsManager _weaponsManager;

        public ItemQueryRegistry(LogHandler logger, IModHelper modHelper)
        {
            _logger = logger;
            _modHelper = modHelper;
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, StardewLocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _weaponsManager = weaponsManager;
        }

        public void RegisterOnModEntry()
        {
            try
            {
                ItemQueryResolver.Register(IDProvider.AP_LOCATION, PurchasableAPLocationQueryDelegate);
                ItemQueryResolver.Register(IDProvider.METAL_DETECTOR_ITEMS, TravelingMerchantInjections.CreateMetalDetectorItems);
                ItemQueryResolver.Register(IDProvider.TRAVELING_CART_DAILY_CHECK, TravelingMerchantInjections.CreateDailyCheck);
                ItemQueryResolver.Register(IDProvider.ARCHIPELAGO_EQUIPMENTS, AdventureGuildEquipmentsQueryDelegate);
                ItemQueryResolver.Register(IDProvider.TOOL_UPGRADES_CHEAPER, ToolShopStockModifier.ToolUpgradesCheaperQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ItemQueryRegistry)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }

        private IEnumerable<ItemQueryResult> PurchasableAPLocationQueryDelegate(string key, string arguments, ItemQueryContext context, bool avoidrepeat, HashSet<string> avoiditemids, Action<string, string> logerror)
        {
            return ObtainableArchipelagoLocation.Create(arguments, _logger, _modHelper, _archipelago, _locationChecker);
        }

        private IEnumerable<ItemQueryResult> AdventureGuildEquipmentsQueryDelegate(string key, string arguments, ItemQueryContext context, bool avoidrepeat, HashSet<string> avoiditemids, Action<string, string> logerror)
        {
            return _weaponsManager.GetEquipmentsForSale(arguments);
        }
    }
}
