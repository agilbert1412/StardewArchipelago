using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Service;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewArchipelago.GameModifications.Testing;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class StardewArchipelagoClient : ArchipelagoClient
    {
        private readonly IModHelper _modHelper;
        private readonly IManifest _manifest;
        private readonly Harmony _harmony;
        private readonly TesterFeatures _testerFeatures;
        private IDataStorageWrapper<BigInteger> _bigIntegerDataStorage;
        private DeathManager _deathManager;

        public override string GameName => "Stardew Valley";
        public override string ModName => _manifest.Name;
        public override string ModVersion => _manifest.Version.ToString();

        public SlotData SlotData => (SlotData)_slotData;
        public IGiftingService GiftingService { get; private set; }

        public StardewArchipelagoClient(ILogger logger, IModHelper modHelper, IManifest manifest, Harmony harmony, Action itemReceivedFunction, IJsonLoader jsonLoader, TesterFeatures testerFeatures) :
            base(logger, new DataPackageCache(new ArchipelagoItemLoader(jsonLoader), new StardewArchipelagoLocationLoader(jsonLoader), "stardew_valley", "IdTables"), itemReceivedFunction)
        {
            _modHelper = modHelper;
            _manifest = manifest;
            _harmony = harmony;
            _testerFeatures = testerFeatures;
        }

        protected override void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            _slotData = new SlotData(slotName, slotDataFields, Logger, _testerFeatures);
        }

        public override bool ConnectToMultiworld(ArchipelagoConnectionInfo connectionInfo, out string errorMessage)
        {
            if (!base.ConnectToMultiworld(connectionInfo, out errorMessage))
            {
                return false;
            }
            
            if (!SlotData.Mods.IsModStateCorrect(_modHelper, out errorMessage))
            {
                DisconnectPermanently();
                return false;
            }

            if (!SlotData.Mods.IsExtraRequirementsStateCorrect(_modHelper, out errorMessage))
            {
                DisconnectPermanently();
                return false;
            }

            return true;
        }

        protected override void InitializeAfterConnection()
        {
            base.InitializeAfterConnection();
            _bigIntegerDataStorage = new BigIntegerDataStorageWrapper(Logger, GetSession());
            GiftingService = new GiftingService(GetSession());
        }

        protected override void InitializeDeathLink()
        {
            if (_deathManager == null)
            {
                _deathManager = new DeathManager(Logger, _modHelper, _harmony, this);
                _deathManager.HookIntoDeathlinkEvents();
            }

            base.InitializeDeathLink();
        }

        protected override void OnMessageReceived(LogMessage message)
        {
            var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
            Logger.LogInfo(fullMessage);

            fullMessage = fullMessage.TurnHeartsIntoStardewHearts();

            switch (message)
            {
                case ChatLogMessage chatMessage:
                {
                    if (!ModEntry.Instance.Config.EnableChatMessages)
                    {
                        return;
                    }

                    var color = chatMessage.Player.Name.GetAsBrightColor();
                    Game1.chatBox?.addMessage(fullMessage, color);
                    return;
                }
                case ItemSendLogMessage itemSendLogMessage:
                {
                    if (ModEntry.Instance.Config.DisplayItemsInChat == ChatItemsFilter.None)
                    {
                        return;
                    }

                    if (ModEntry.Instance.Config.DisplayItemsInChat == ChatItemsFilter.RelatedToMe && !itemSendLogMessage.IsRelatedToActivePlayer)
                    {
                        return;
                    }

                    var color = Color.Gold;

                    Game1.chatBox?.addMessage(fullMessage, color);
                    return;
                }
                case GoalLogMessage:
                {
                    var color = Color.Green;
                    Game1.chatBox?.addMessage(fullMessage, color);
                    return;
                }
                case JoinLogMessage:
                case LeaveLogMessage:
                case TagsChangedLogMessage:
                {
                    if (!ModEntry.Instance.Config.EnableConnectionMessages)
                    {
                        return;
                    }

                    var color = Color.Gray;
                    Game1.chatBox?.addMessage(fullMessage, color);
                    return;
                }
                case CommandResultLogMessage:
                case not null:
                {
                    var color = Color.Gray;
                    Game1.chatBox?.addMessage(fullMessage, color);
                    return;
                }
            }
        }

        protected override void KillPlayerDeathLink(DeathLink deathlink)
        {
            DeathManager.ReceiveDeathLink();
            var deathLinkMessage = $"You have been killed by {deathlink.Source} ({deathlink.Cause})";
            Game1.chatBox?.addInfoMessage(deathLinkMessage);
        }

        public override void DisconnectAndCleanup()
        {
            base.DisconnectAndCleanup();
            _bigIntegerDataStorage = null;
        }

        protected override void OnError()
        {
            base.OnError();
            Game1.chatBox?.addMessage("Connection to Archipelago lost. The game will try reconnecting later. Check the SMAPI console for details", Color.Red);
        }

        protected override void OnReconnectFailure()
        {
            base.OnReconnectFailure();
            Game1.chatBox?.addMessage("Reconnection attempt failed", Color.Red);
        }

        protected override void OnReconnectSuccess()
        {
            base.OnReconnectSuccess();
            Game1.chatBox?.addMessage("Reconnection attempt successful!", Color.Green);
        }

        public void SetBigIntegerDataStorage(Scope scope, string key, BigInteger value)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            _bigIntegerDataStorage.Set(scope, key, value);
        }

        public BigInteger? ReadBigIntegerFromDataStorage(Scope scope, string key)
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            return _bigIntegerDataStorage.Read(scope, key);
        }

        public async Task<BigInteger?> ReadBigIntegerFromDataStorageAsync(Scope scope, string key)
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            return await _bigIntegerDataStorage.ReadAsync(scope, key);
        }

        public bool AddBigIntegerDataStorage(Scope scope, string key, BigInteger amount)
        {
            if (!MakeSureConnected())
            {
                return false;
            }

            return _bigIntegerDataStorage.Add(scope, key, amount);
        }

        public bool SubtractBigIntegerDataStorage(Scope scope, string key, BigInteger amount, bool dontGoBelowZero)
        {
            if (!MakeSureConnected())
            {
                return false;
            }

            return _bigIntegerDataStorage.Subtract(scope, key, amount, dontGoBelowZero);
        }

        public bool MultiplyBigIntegerDataStorage(Scope scope, string key, int multiple)
        {
            if (!MakeSureConnected())
            {
                return false;
            }

            return _bigIntegerDataStorage.Multiply(scope, key, multiple);
        }

        public bool DivideBigIntegerDataStorage(Scope scope, string key, int divisor)
        {
            if (!MakeSureConnected())
            {
                return false;
            }

            if (divisor != 2)
            {
                throw new NotImplementedException($"Can't divide DataStorage by {divisor} yet.");
            }

            return _bigIntegerDataStorage.DivideByTwo(scope, key);
        }
    }

}
