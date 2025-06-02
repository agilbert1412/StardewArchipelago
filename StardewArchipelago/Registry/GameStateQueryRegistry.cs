using System;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Logging;
using StardewValley;
using StardewValley.Delegates;
using System.Linq;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Locations;

namespace StardewArchipelago.Registry
{
    public class GameStateQueryRegistry : IRegistry
    {
        private LogHandler _logger;
        private StardewArchipelagoClient _archipelago;

        public GameStateQueryRegistry(LogHandler logger)
        {
            _logger = logger;
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, StardewLocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
            _archipelago = archipelago;
        }

        public void RegisterOnModEntry()
        {
            try
            {
                GameStateQuery.Register(GameStateCondition.HAS_RECEIVED_ITEM, HasReceivedItemQueryDelegate);
                GameStateQuery.Register(GameStateCondition.HAS_STOCK_SIZE, TravelingMerchantInjections.HasStockSizeQueryDelegate);
                GameStateQuery.Register(GameStateCondition.FOUND_ARTIFACT, ArtifactsFoundQueryDelegate);
                GameStateQuery.Register(GameStateCondition.FOUND_MINERAL, MineralsFoundQueryDelegate);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(GameStateQueryRegistry)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }

        private bool HasReceivedItemQueryDelegate(string[] query, GameStateQueryContext context)
        {
            if (!query.Any())
            {
                return false;
            }

            var amount = int.Parse(query[1]);
            var itemName = string.Join(' ', query.Skip(2));
            return _archipelago.GetReceivedItemCount(itemName) >= amount;
        }

        private bool ArtifactsFoundQueryDelegate(string[] query, GameStateQueryContext context)
        {
            if (!query.Any())
            {
                return false;
            }
            var archaeologyFound = Game1.player.archaeologyFound.Keys;
            var requestedArtifact = query[1];
            if (!archaeologyFound.Contains(requestedArtifact))
            {
                return false;
            }
            return true;
        }

        private bool MineralsFoundQueryDelegate(string[] query, GameStateQueryContext context)
        {
            if (!query.Any())
            {
                return false;
            }
            var mineralsFound = Game1.player.mineralsFound.Keys;
            var requestedMineral = query[1];
            if (!mineralsFound.Contains(requestedMineral))
            {
                return false;
            }
            return true;
        }
    }
}
