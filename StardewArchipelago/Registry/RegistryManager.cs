using System;
using System.Collections.Generic;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Registry
{
    public class RegistryManager : IRegistry
    {
        private LogHandler _logger;
        private List<IRegistry> _registryManagers;

        public RegistryManager(LogHandler logger, IModHelper modHelper, ModEntry mod)
        {
            _logger = logger;
            _registryManagers = new List<IRegistry>();
            _registryManagers.Add(new ItemRegistry(_logger));
            _registryManagers.Add(new ItemQueryRegistry(_logger, modHelper));
            _registryManagers.Add(new GameStateQueryRegistry(_logger));
            _registryManagers.Add(new ChatCommandsRegistry(_logger));
            _registryManagers.Add(new ConsoleCommandsRegistry(_logger, modHelper, mod));
            _registryManagers.Add(new TriggerActionRegistry(_logger));
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, LocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
            try
            {
                foreach (var registryManager in _registryManagers)
                {
                    registryManager.Initialize(archipelago, stardewItemManager, locationChecker, _giftHandler, weaponsManager, state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(RegistryManager)} failed during {nameof(Initialize)}: {ex}");
            }
        }

        public void RegisterOnModEntry()
        {
            try
            {
                foreach (var registryManager in _registryManagers)
                {
                    registryManager.RegisterOnModEntry();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(RegistryManager)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }
    }
}
