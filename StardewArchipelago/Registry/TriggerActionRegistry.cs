using System;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewValley.Triggers;

namespace StardewArchipelago.Registry
{
    public class TriggerActionRegistry : IRegistry
    {
        private LogHandler _logger;

        public TriggerActionRegistry(LogHandler logger)
        {
            _logger = logger;
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, StardewLocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
        }

        public void RegisterOnModEntry()
        {
            try
            {
                TriggerActionManager.RegisterAction(TriggerActionProvider.TRAVELING_MERCHANT_PURCHASE, TravelingMerchantInjections.OnPurchasedRandomItem);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(TriggerActionRegistry)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }
    }
}
