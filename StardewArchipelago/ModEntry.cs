using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewArchipelago
{
    public class ModEntry : Mod
    {
        private const string CONNECT_SYNTAX = "Syntax: connect ip:port slot password";

        private IModHelper _helper;
        private Harmony _harmony;
        private BundleReader _bundleReader;
        private ArchipelagoClient _archipelago;
        private StardewJsonSerializer _serializer;
        private ItemManager _itemManager;
        private LocationManager _locationsManager;
        private StardewItemManager _stardewItemManager;
        private UnlockManager _unlockManager;

        private ArchipelagoStateDto _state;

        public ModEntry() : base()
        {
            _state = new ArchipelagoStateDto();
            _unlockManager = new UnlockManager();
        }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            _harmony = new Harmony(this.ModManifest.UniqueID);

            _archipelago = new ArchipelagoClient(Monitor, OnItemReceived);
            _helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            _helper.Events.GameLoop.SaveCreated += this.OnSaveCreated;
            _helper.Events.GameLoop.Saved += this.OnSaved;
            _helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            _helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
            _helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            _helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            _helper.Events.GameLoop.DayEnding += this.OnDayEnding;

            _helper.ConsoleCommands.Add("connect", $"Connect to Archipelago. {CONNECT_SYNTAX}", this.OnConnectToArchipelago);
            _helper.ConsoleCommands.Add("debugMethod", "Runs whatever is currently in the debug method", this.DebugMethod);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {

        }

        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            _state.ItemsReceived = new Dictionary<long, int>();
            _helper.Data.WriteJsonFile(GetApDataJsonPath(), _state);
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
            _state.ItemsReceived = _itemManager.GetAllItemsAlreadyProcessed();
            _helper.Data.WriteJsonFile(GetApDataJsonPath(), _state);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var state = _helper.Data.ReadJsonFile<ArchipelagoStateDto>(GetApDataJsonPath());
            if (state != null)
            {
                _state = state;
            }

            _stardewItemManager = new StardewItemManager();
            _bundleReader = new BundleReader();
            _itemManager = new ItemManager(_archipelago, _stardewItemManager, _unlockManager, _state.ItemsReceived);
            _locationsManager = new LocationManager(Monitor, _archipelago, _bundleReader, _helper, _harmony);
            _locationsManager.RemoveDefaultRewardsOnAllLocations();

            if (_state.APConnectionInfo != null)
            {
                _archipelago.Connect(_state.APConnectionInfo);
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            _locationsManager.CheckAllLocations(true);
            _itemManager.ReceiveAllNewItems();
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            _locationsManager.CheckAllLocations();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            _archipelago.APUpdate(_state.APConnectionInfo);
        }

        private void OnConnectToArchipelago(string arg1, string[] arg2)
        {
            if (arg2.Length < 2)
            {
                Monitor.Log($"You must provide an IP with a port, and a slot name, to connect to archipelago. {CONNECT_SYNTAX}");
                return;
            }

            var ipAndPort = arg2[0].Split(":");
            if (ipAndPort.Length < 2)
            {
                Monitor.Log($"You must provide an IP with a port, and a slot name, to connect to archipelago. {CONNECT_SYNTAX}");
                return;
            }

            var ip = ipAndPort[0];
            var port = int.Parse(ipAndPort[1]);
            var slot = arg2[1];
            var password = arg2.Length >= 3 ? arg2[2] : "";
            var apConnection = new ArchipelagoConnectionInfo(ip, port, slot, false, password);
            _archipelago.Connect(apConnection);
            if (!_archipelago.IsConnected)
            {
                return;
            }

            _state.APConnectionInfo = apConnection;
        }

        private void OnItemReceived(long itemId)
        {
            if (_itemManager == null)
            {
                return;
            }

            _itemManager.ReceiveAllNewItems();
        }

        private void DebugMethod(string arg1, string[] arg2)
        {
            _bundleReader.ReadCurrentBundleStates();
        }

        private string GetApDataJsonPath()
        {
            var path = $"APData - {Game1.getFarm().Name} - {Game1.player.Name}.json";
            return path;
        }
    }
}
