using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Constants;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;
using System;
using System.Linq;

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

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, StardewLocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state,
            TrapManager trapManager)
        {
            _archipelago = archipelago;
        }

        public void RegisterOnModEntry()
        {
            try
            {
                GameStateQuery.Register(GameStateCondition.HAS_RECEIVED_ITEM, HasReceivedItemQueryDelegate);
                GameStateQuery.Register(GameStateCondition.HAS_RECEIVED_ITEM_EXACT_AMOUNT, HasReceivedItemExactAmountQueryDelegate);
                GameStateQuery.Register(GameStateCondition.HAS_STOCK_SIZE, TravelingMerchantInjections.HasStockSizeQueryDelegate);
                GameStateQuery.Register(GameStateCondition.FOUND_ARTIFACT, ArtifactsFoundQueryDelegate);
                GameStateQuery.Register(GameStateCondition.FOUND_MINERAL, MineralsFoundQueryDelegate);
                GameStateQuery.Register(GameStateCondition.CURRENT_MINE_FLOOR, CurrentMineFloorQueryDelegate);
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
            var receivedCount = _archipelago.GetReceivedItemCount(itemName);
            return receivedCount >= amount;
        }

        private bool HasReceivedItemExactAmountQueryDelegate(string[] query, GameStateQueryContext context)
        {
            if (!query.Any())
            {
                return false;
            }

            var amount = int.Parse(query[1]);
            var itemName = string.Join(' ', query.Skip(2));
            var receivedCount = _archipelago.GetReceivedItemCount(itemName);
            return receivedCount == amount;
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

        private bool CurrentMineFloorQueryDelegate(string[] query, GameStateQueryContext context)
        {
            if (!query.Any())
            {
                return false;
            }

            if (!int.TryParse(query[1], out var requestedFloor))
            {
                return false;
            }

            if (Game1.player.currentLocation is not MineShaft currentMines)
            {
                return false;
            }

            return currentMines.mineLevel == requestedFloor;
        }
    }
}
