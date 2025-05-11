using System;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Registry
{
    public class ChatCommandsRegistry : IRegistry
    {
        private LogHandler _logger;

        public ChatCommandsRegistry(LogHandler logger)
        {
            _logger = logger;
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, LocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
        }

        public void RegisterOnModEntry()
        {
            try
            {
                ChatCommands.Register("kaito", SimpleSecretsInjections.KaitoKidChatCommand, null, new[] { "kaitokid", "kaito kid", });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ChatCommandsRegistry)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }
    }
}
