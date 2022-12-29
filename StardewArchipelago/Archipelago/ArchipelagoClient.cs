using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class ArchipelagoClient
    {
        private const string GAME_NAME = "Stardew Valley";
        private IMonitor _console;
        private ArchipelagoSession _session;

        private Action _itemReceivedFunction;

        public bool IsConnected { get; private set; }
        public SlotData SlotData { get; private set; }

        public ArchipelagoClient(IMonitor console, Action itemReceivedFunction)
        {
            _console = console;
            _itemReceivedFunction = itemReceivedFunction;
            IsConnected = false;
        }

        public void Connect(ArchipelagoConnectionInfo archipelagoConnectionInfo)
        {
            LoginResult result;
            try
            {
                InitSession(archipelagoConnectionInfo);
                var itemsHandling = ItemsHandlingFlags.IncludeOwnItems;
                var minimumVersion = new Version(0, 3, 7);
                var tags = archipelagoConnectionInfo.DeathLink ? new[] { "AP" } : new[] { "AP", "DeathLink" };
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
            _session.Items.ItemReceived += OnItemReceived;
            _itemReceivedFunction();
            InitializeSlotData(loginSuccess.SlotData);
            IsConnected = true;
        }

        private void InitializeSlotData(Dictionary<string, object> slotDataFields)
        {
            SlotData = new SlotData(slotDataFields);
        }

        private void InitSession(ArchipelagoConnectionInfo archipelagoConnectionInfo)
        {
            _session = ArchipelagoSessionFactory.CreateSession(archipelagoConnectionInfo.HostUrl,
                archipelagoConnectionInfo.Port);
            _session.MessageLog.OnMessageReceived += OnMessageReceived;
            _session.Socket.ErrorReceived += SessionErrorReceived;
            _session.Socket.SocketClosed += SessionSocketClosed;
        }

        private void OnMessageReceived(LogMessage message)
        {
            var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
            _console.Log(fullMessage, LogLevel.Info);
            Game1.chatBox?.addInfoMessage(fullMessage);
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

        /*private string ScoutLocation(long locationId)
        {
            var a = _session.Locations.ScoutLocationsAsync(true, locationId);
        }*/

        /*private void PrintMessages()
        {
            dequeueTimeout -= 1; // this function gets called once every second
            if (!(dequeueTimeout <= 0)) return;
            var toProcess = new List<ChatMessage>();
            while (toProcess.Count < DequeueCount && ChatMessages.Count > 0)
            {
                toProcess.Add(ChatMessages[0]);
                ChatMessages.RemoveAt(0);
            }

            foreach (var message in toProcess)
                Game1.chatBox.addMessage(message.Message, message.Color);
            dequeueTimeout = 3;
        }*/
    }
}
