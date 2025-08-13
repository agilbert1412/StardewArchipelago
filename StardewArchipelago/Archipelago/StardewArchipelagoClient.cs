using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Service;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ConnectionResults;
using StardewArchipelago.Extensions;
using StardewArchipelago.GameModifications.MoveLink;
using StardewArchipelago.GameModifications.Testing;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using Color = Microsoft.Xna.Framework.Color;

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

        public SlotData.SlotData SlotData => (SlotData.SlotData)_slotData;
        public IGiftingService GiftingService { get; private set; }

        private List<string> _messagesToIgnore;

        public StardewArchipelagoClient(ILogger logger, IModHelper modHelper, IManifest manifest, Harmony harmony, Action itemReceivedFunction, IJsonLoader jsonLoader, TesterFeatures testerFeatures) :
            base(logger, new StardewDatapackageCache(new ArchipelagoItemLoader(jsonLoader), new StardewArchipelagoLocationLoader(jsonLoader), "stardew_valley", "IdTables"), itemReceivedFunction)
        {
            _modHelper = modHelper;
            _manifest = manifest;
            _harmony = harmony;
            _testerFeatures = testerFeatures;
            _messagesToIgnore = new List<string>();

            // PrintLocationTags();
        }

        private void PrintLocationTags()
        {
            var tags = new HashSet<string>();
            foreach (var location in DataPackageCache.GetAllLocations())
            {
                tags.AddRange(location.LocationTags);
            }
            var text = "";
            foreach (var tag in tags)
            {
                text += $"public const string {tag} = \"{tag}\";{Environment.NewLine}";
            }
            File.WriteAllText("AllLocationTags.cs",text);
        }

        protected override void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            _slotData = new SlotData.SlotData(slotName, slotDataFields, Logger, _testerFeatures);
        }

        public override ConnectionResult ConnectToMultiworld(ArchipelagoConnectionInfo connectionInfo)
        {
            var baseResult = base.ConnectToMultiworld(connectionInfo);
            if (!baseResult.Success)
            {
                return baseResult;
            }
            
            if (!SlotData.Mods.IsModStateCorrect(_modHelper, out var missingMods))
            {
                DisconnectPermanently();
                return new MissingModsConnectionResult(missingMods);
            }

            if (!SlotData.Mods.IsExtraRequirementsStateCorrect(_modHelper, out var missingModRequirements))
            {
                DisconnectPermanently();
                return new MissingModRequirementsConnectionResult(missingModRequirements);
            }

            if (!SlotData.Mods.HasKnownIncompatibleMods(_modHelper, out var incompatibleMods))
            {
                DisconnectPermanently();
                return new IncompatibleModsConnectionResult(incompatibleMods);
            }

            return baseResult;
        }

        protected override void InitializeAfterConnection()
        {
            base.InitializeAfterConnection();
            _bigIntegerDataStorage = new BigIntegerDataStorageWrapper(Logger, GetSession());
            GiftingService = new GiftingService(GetSession());
        }

        protected override void InitializeDeathLink()
        {
            _deathManager = new DeathManager(Logger, _modHelper, _harmony, this);
            _deathManager.HookIntoDeathlinkEvents();
            base.InitializeDeathLink();
        }

        protected override void OnMessageReceived(LogMessage message)
        {
            var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
            var stardewFullMessage = fullMessage.TurnHeartsIntoStardewHearts();

            if (_messagesToIgnore.Any(x => x.Contains(fullMessage) || fullMessage.Contains(x)) || _messagesToIgnore.Any(x => x.Contains(stardewFullMessage) || fullMessage.Contains(x)))
            {
                Logger.LogDebug($"Ignoring Chat Message: {fullMessage}");
                return;
            }

            Logger.LogInfo(fullMessage);
            stardewFullMessage = stardewFullMessage.AnonymizePlayerNames(GetSession().Players);

            switch (message)
            {
                case ChatLogMessage chatMessage:
                {
                    if (!ModEntry.Instance.Config.EnableChatMessages)
                    {
                        return;
                    }

                    var color = chatMessage.Player.Name.GetAsBrightColor();
                    Game1.chatBox?.addMessage(stardewFullMessage, color);
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

                    Game1.chatBox?.addMessage(stardewFullMessage, color);
                    return;
                }
                case GoalLogMessage:
                {
                    var color = Color.Green;
                    Game1.chatBox?.addMessage(stardewFullMessage, color);
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
                    Game1.chatBox?.addMessage(stardewFullMessage, color);
                    return;
                }
                case CommandResultLogMessage:
                case not null:
                {
                    var color = Color.Gray;
                    Game1.chatBox?.addMessage(stardewFullMessage, color);
                    return;
                }
            }
        }

        protected override void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            switch (packet)
            {
                case BouncePacket bouncePacket:
                    MovementInjections.HandleBouncePacket(bouncePacket);
                    break;
                default:
                    return;
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
            _deathManager = null;
        }

        protected override void OnError(string message, Exception e)
        {
            base.OnError(message, e);
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

        private bool ShouldHint(bool createAsHint)
        {
            var session = GetSession();
            if (!MakeSureConnected() || session == null)
            {
                return false;
            }

            var playerCount = session.Players.AllPlayers.Count(x => !x.Game.Equals("Archipelago")) > 1;
            var shouldHint = createAsHint && (playerCount || ModEntry.Instance.Config.ScoutHintInSoloMultiworld);
            return shouldHint;
        }

        public ScoutedLocation ScoutStardewLocation(string locationName, bool createAsHint = false)
        {
            var scoutedLocation = ScoutSingleLocation(locationName, ShouldHint(createAsHint));
            if (scoutedLocation != null && ModEntry.Instance.Config.AnonymizeNamesInChat)
            {
                scoutedLocation = new ScoutedLocation(scoutedLocation.LocationName, scoutedLocation.ItemName, scoutedLocation.PlayerName.AnonymizePlayerNames(GetSession().Players), scoutedLocation.GameName, scoutedLocation.LocationId,
                    scoutedLocation.ItemId, scoutedLocation.PlayerId, scoutedLocation.ClassificationFlags);
            }
            return scoutedLocation;
        }

        public Dictionary<string, ScoutedLocation> ScoutStardewLocations(IEnumerable<string> locationNames, bool createAsHint = false)
        {
            var scoutedLocations = ScoutManyLocations(locationNames, ShouldHint(createAsHint));
            foreach (var scoutedLocationKey in scoutedLocations.Keys)
            {
                var scoutedLocation = scoutedLocations[scoutedLocationKey];
                scoutedLocations[scoutedLocationKey] = new ScoutedLocation(scoutedLocation.LocationName, scoutedLocation.ItemName, scoutedLocation.PlayerName.AnonymizePlayerNames(GetSession().Players), scoutedLocation.GameName, scoutedLocation.LocationId,
                        scoutedLocation.ItemId, scoutedLocation.PlayerId, scoutedLocation.ClassificationFlags);
            }

            return scoutedLocations;
        }

        public string SendFakeItemMessage(string itemName, string locationName)
        {
            var message = $"Trap Bundle sent {itemName} to {GetPlayerName()} ({locationName})";
            _messagesToIgnore.Add(message);

            if (!this.MakeSureConnected())
            {
                return message;
            }

            var session = GetSession();
            var packet = new SayPacket()
            {
                Text = message,
            };

            session.Socket.SendPacket(packet);
            return message;
        }

        public Hint[] GetActiveDesiredHintsForMe()
        {
            var desiredHintStatus = new[] { HintStatus.Priority };
            return this.GetActiveHintsForMeMatchingStatus(desiredHintStatus);
        }

        public Hint[] GetActiveAvoidedHintsForMe()
        {
            var avoidedHintStatus = new[] { HintStatus.Priority };
            return this.GetActiveHintsForMeMatchingStatus(avoidedHintStatus);
        }

        public Hint[] GetActiveHintsForMeMatchingStatus(HintStatus[] statusToMatch)
        {
            if (!this.MakeSureConnected())
            {
                return Array.Empty<Hint>();
            }

            var hints = GetHints();
            var hintsMatchingStatus = hints.Where(x => !x.Found && this.GetPlayerName(x.ReceivingPlayer) == this._slotData.SlotName && statusToMatch.Contains(x.Status));
            return hintsMatchingStatus.ToArray();
        }

        public string GetHintString(Hint relatedHint)
        {
            if (!MakeSureConnected() || relatedHint == null)
            {
                return ScoutedLocation.GenericItemName();
            }

            var playerName = GetPlayerName(relatedHint.ReceivingPlayer);
            var playerGame = GetPlayerGame(relatedHint.ReceivingPlayer);
            var itemName = GetSession().Items.GetItemName(relatedHint.ItemId, playerGame);

            return playerName + "'s " + itemName;
        }

        public StardewDatapackageCache DataPackageCache => (StardewDatapackageCache)LocalDataPackage;
    }
}
