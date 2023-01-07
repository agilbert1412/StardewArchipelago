using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class ArchipelagoClient
    {
        private const string GAME_NAME = "Stardew Valley";
        private IMonitor _console;
        private IModHelper _modHelper;
        private ArchipelagoSession _session;
        private DeathLinkService _deathLinkService;
        private Harmony _harmony;
        private DeathManager _deathManager;
        private AdvancedOptionsManager _advancedOptionsManager;
        private ChatForwarder _chatForwarder;

        private Action _itemReceivedFunction;

        public bool IsConnected { get; private set; }
        public SlotData SlotData { get; private set; }
        public Dictionary<string, ScoutedLocation> ScoutedLocations { get; set; }

        public ArchipelagoClient(IMonitor console, IModHelper modHelper, Harmony harmony, Action itemReceivedFunction)
        {
            _console = console;
            _modHelper = modHelper;
            _harmony = harmony;
            _itemReceivedFunction = itemReceivedFunction;
            IsConnected = false;
            ScoutedLocations = new Dictionary<string, ScoutedLocation>();
        }

        public void Connect(ArchipelagoConnectionInfo archipelagoConnectionInfo)
        {
            Disconnect();
            LoginResult result;
            try
            {
                InitSession(archipelagoConnectionInfo);
                var itemsHandling = ItemsHandlingFlags.IncludeOwnItems;
                var minimumVersion = new Version(0, 3, 7);
                var tags = new[] { "AP", "DeathLink" };
                result = _session.TryConnectAndLogin(GAME_NAME, archipelagoConnectionInfo.SlotName, itemsHandling, minimumVersion, tags, null, archipelagoConnectionInfo.Password);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                var failure = (LoginFailure)result;
                var errorMessage = $"Failed to Connect to {archipelagoConnectionInfo.HostUrl}:{archipelagoConnectionInfo.Port} as {archipelagoConnectionInfo.SlotName}:";
                foreach (var error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (var error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                _console.Log(errorMessage, LogLevel.Error);
                IsConnected = false;
                return; // Did not connect, show the user the contents of `errorMessage`
            }

            // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
            var loginSuccess = (LoginSuccessful)result;
            var loginMessage = $"Connected to Archipelago server as {archipelagoConnectionInfo.SlotName} (Team {loginSuccess.Team}).";
            _console.Log(loginMessage, LogLevel.Info);

            // Must go AFTER a successful connection attempt
            InitializeAfterConnection(loginSuccess, archipelagoConnectionInfo.SlotName);
        }

        private void InitializeAfterConnection(LoginSuccessful loginSuccess, string slotName)
        {
            _session.Items.ItemReceived += OnItemReceived;
            _session.MessageLog.OnMessageReceived += OnMessageReceived;
            _session.Socket.ErrorReceived += SessionErrorReceived;
            _session.Socket.SocketClosed += SessionSocketClosed;

            if (_chatForwarder == null)
            {
                _chatForwarder = new ChatForwarder(_console, _harmony);
                _chatForwarder.ListenToChatMessages(this);
            }
            _itemReceivedFunction();
            InitializeSlotData(slotName, loginSuccess.SlotData);

            if (_advancedOptionsManager == null)
            {
                _advancedOptionsManager = new AdvancedOptionsManager(_console, _modHelper, _harmony, this);
                _advancedOptionsManager.InjectArchipelagoAdvancedOptions();
            }

            InitializeDeathLink();

            IsConnected = true;
        }

        private void InitializeDeathLink()
        {
            if (_deathManager == null)
            {
                _deathManager = new DeathManager(_console, _modHelper, _harmony, this);
                _deathManager.HookIntoDeathlinkEvents();
            }

            _deathLinkService = _session.CreateDeathLinkService();
            if (SlotData.DeathLink)
            {
                _deathLinkService.EnableDeathLink();
                _deathLinkService.OnDeathLinkReceived += ReceiveDeathLink;
            }
            else
            {
                _deathLinkService.DisableDeathLink();
            }
        }

        private void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            SlotData = new SlotData(slotName, slotDataFields);
        }

        private void InitSession(ArchipelagoConnectionInfo archipelagoConnectionInfo)
        {
            _session = ArchipelagoSessionFactory.CreateSession(archipelagoConnectionInfo.HostUrl,
                archipelagoConnectionInfo.Port);
        }

        private void OnMessageReceived(LogMessage message)
        {
            var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
            _console.Log(fullMessage, LogLevel.Info);

            fullMessage = fullMessage.Replace("<3", "<");

            if (fullMessage.StartsWith($"{SlotData.SlotName}: "))
            {
                return;
            }

            var messageParts = fullMessage.Split(':');
            var color = Color.Gray;
            if (message is ItemSendLogMessage itemSendLogMessage)
            {
                var receiver = itemSendLogMessage.ReceivingPlayerSlot;
                var sender = itemSendLogMessage.SendingPlayerSlot;

                if (_session.Players.GetPlayerName(receiver) != SlotData.SlotName &&
                    _session.Players.GetPlayerName(sender) != SlotData.SlotName)
                {
                    return;
                }
                color = Color.Gold;
            }
            else if (messageParts.Length >= 2)
            {
                var senderName = messageParts[0];
                color = senderName.GetAsBrightColor();
            }
            Game1.chatBox?.addMessage(fullMessage, color);
        }

        public void SendMessage(string text)
        {
            if (!IsConnected)
            {
                return;
            }

            var packet = new SayPacket()
            {
                Text = text
            };

            _session.Socket.SendPacket(packet);
        }

        private void OnItemReceived(ReceivedItemsHelper receivedItemsHelper)
        {
            if (!IsConnected)
            {
                return;
            }

            var receivedItem = receivedItemsHelper.DequeueItem();
            _itemReceivedFunction();
        }

        public void ReportCollectedLocations(long[] locationIds)
        {
            if (_session == null)
            {
                return;
            }

            _session.Locations.CompleteLocationChecks(locationIds);
        }

        public Dictionary<long, int> GetAllReceivedItems()
        {
            if (_session == null)
            {
                return new Dictionary<long, int>();
            }

            return _session.Items.AllItemsReceived.GroupBy(x => x.Item).ToDictionary(group => group.Key, group => group.Count());
        }

        public void ReportGoalCompletion()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            _session.Socket.SendPacket(statusUpdatePacket);
        }

        private string GetLocationName(long locationId)
        {
            return _session.Locations.GetLocationNameFromId(locationId);
        }

        public long GetLocationId(string locationName, string gameName = GAME_NAME)
        {
            return _session.Locations.GetLocationIdFromName(gameName, locationName);
        }

        public string GetItemName(long itemId)
        {
            return _session.Items.GetItemName(itemId);
        }

        public void SendDeathLink(string player, string reason = "Unknown cause")
        {
            _deathLinkService.SendDeathLink(new DeathLink(player, reason));
        }

        private void ReceiveDeathLink(DeathLink deathlink)
        {
            DeathManager.ReceiveDeathLink();

            var deathLinkMessage = $"You have been killed by {deathlink.Source} ({deathlink.Cause})";
            _console.Log(deathLinkMessage, LogLevel.Info);
            Game1.chatBox?.addInfoMessage(deathLinkMessage);
        }

        public ScoutedLocation ScoutSingleLocation(string locationName, bool createAsHint = false)
        {
            if (ScoutedLocations.ContainsKey(locationName))
            {
                return ScoutedLocations[locationName];
            }

            try
            {
                var locationId = GetLocationId(locationName);
                if (locationId == -1)
                {
                    _console.Log($"Could not find the id for location \"{locationName}\".");
                    return null;
                }

                var locationInfo = ScoutLocation(locationId, createAsHint);
                if (locationInfo.Locations.Length < 1)
                {
                    _console.Log($"Could not scout location \"{locationName}\".");
                    return null;
                }

                var firstLocationInfo = locationInfo.Locations[0];
                var itemName = GetItemName(firstLocationInfo.Item);
                var playerSlotName = _session.Players.GetPlayerName(firstLocationInfo.Player);

                var scoutedLocation = new ScoutedLocation(locationName, itemName, playerSlotName, locationId,
                    firstLocationInfo.Item, firstLocationInfo.Player);

                ScoutedLocations.Add(locationName, scoutedLocation);
                return scoutedLocation;
            }
            catch (Exception e)
            {
                _console.Log($"Could not scout location \"{locationName}\". Message: {e.Message}");
                return null;
            }
        }

        private LocationInfoPacket ScoutLocation(long locationId, bool createAsHint)
        {
            var scoutTask = _session.Locations.ScoutLocationsAsync(createAsHint, locationId);
            scoutTask.Wait();
            return scoutTask.Result;
        }

        private void SessionErrorReceived(Exception e, string message)
        {
            _console.Log(message, LogLevel.Error);
            Disconnect();
        }

        private void SessionSocketClosed(string reason)
        {
            _console.Log($"Connection to Archipelago lost: {reason}", LogLevel.Error);
            Disconnect();
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            _session.Items.ItemReceived -= OnItemReceived;
            _session.MessageLog.OnMessageReceived -= OnMessageReceived;
            _session.Socket.ErrorReceived -= SessionErrorReceived;
            _session.Socket.SocketClosed -= SessionSocketClosed;
            _session.Socket.DisconnectAsync();
            _session = null;
            IsConnected = false;
        }


        private int _lastTime;
        private float _disconnectTimeout = 5.0f;
        public void APUpdate(ArchipelagoConnectionInfo connectionInfo)
        {
            if (!IsConnected && connectionInfo != null)
            {
                Disconnect();
                var now = DateTime.Now.Second;
                var dT = now - _lastTime;
                _lastTime = now;
                _disconnectTimeout -= dT;
                if (!(_disconnectTimeout <= 0.0f)) return;

                Connect(connectionInfo);
                _disconnectTimeout = 5.0f;
                return;
            }

            /*Utils.PrintMessages();
            if (ServerData.Index >= Session.Items.AllItemsReceived.Count) return;
            var currentItemId = Session.Items.AllItemsReceived[Convert.ToInt32(ServerData.Index)].Item;
            ++ServerData.Index;
            ItemManager.Unlock(currentItemId);*/
        }
    }
}
