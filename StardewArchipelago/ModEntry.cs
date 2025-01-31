using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.GameModifications.MultiSleep;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.GameModifications.Testing;
using StardewArchipelago.Goals;
using StardewArchipelago.Integrations.GenericModConfigMenu;
using StardewArchipelago.Items;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Json;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Locations.Patcher;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.TerrainFeatures;
using StardewValley.Triggers;

namespace StardewArchipelago
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        public ModConfig Config;

        private const string CONNECT_SYNTAX = "Syntax: connect_override ip:port slot password";
        private const string AP_DATA_KEY = "ArchipelagoData";
        private const string AP_EXPERIENCE_KEY = "ArchipelagoSkillsExperience";
        private const string AP_FRIENDSHIP_KEY = "ArchipelagoFriendshipPoints";

        private LogHandler _logger;
        private IModHelper _helper;
        private Harmony _harmony;
        private TesterFeatures _testerFeatures;
        private StardewArchipelagoClient _archipelago;
        private AdvancedOptionsManager _advancedOptionsManager;
        private Mailman _mail;
        private ChatForwarder _chatForwarder;
        private IGiftHandler _giftHandler;
        private APItemManager _itemManager;
        private WeaponsManager _weaponsManager;
        private RandomizedLogicPatcher _logicPatcher;
        private MailPatcher _mailPatcher;
        private BundlesManager _bundlesManager;
        private StardewLocationChecker _locationChecker;
        private LocationPatcher _locationsPatcher;
        private ItemPatcher _itemPatcher;
        private GoalManager _goalManager;
        private StardewItemManager _stardewItemManager;
        private MultiSleepManager _multiSleepManager;
        private SeasonsRandomizer _seasonsRandomizer;
        private AppearanceRandomizer _appearanceRandomizer;
        private QuestCleaner _questCleaner;
        private EntranceManager _entranceManager;
        private NightShippingBehaviors _shippingBehaviors;
        private TileSanityManager _tileSanityManager;
        private HintHelper _hintHelper;

        private ModRandomizedLogicPatcher _modLogicPatcher;
        private InitialModGameStateInitializer _modStateInitializer;
        private ModifiedVillagerEventChecker _villagerEvents;

        private ArchipelagoConnectionInfo _apConnectionOverride;

        public ArchipelagoStateDto State { get; set; }
        public ILogger Logger => _logger;

        public ModEntry() : base()
        {
            Instance = this;
        }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            _apConnectionOverride = null;

            _logger = new LogHandler(Monitor);
            _helper = helper;
            _harmony = new Harmony(ModManifest.UniqueID);
            _testerFeatures = new TesterFeatures(_logger, _helper);

            _archipelago = new StardewArchipelagoClient(_logger, _helper, ModManifest, _harmony, OnItemReceived, new SmapiJsonLoader(_helper), _testerFeatures);

            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            _helper.Events.GameLoop.SaveCreating += OnSaveCreating;
            _helper.Events.GameLoop.SaveCreated += OnSaveCreated;
            _helper.Events.GameLoop.Saving += OnSaving;
            _helper.Events.GameLoop.Saved += OnSaved;
            _helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            _helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            _helper.Events.GameLoop.DayStarted += OnDayStarted;
            _helper.Events.GameLoop.DayEnding += OnDayEnding;
            _helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;


            _helper.ConsoleCommands.Add("connect_override", $"Overrides your next connection to Archipelago. {CONNECT_SYNTAX}", OnCommandConnectToArchipelago);
            _helper.ConsoleCommands.Add("export_all_gifts", "Export all currently loaded giftable items and their traits", ExportGifts);
            _helper.ConsoleCommands.Add("deathlink", "Override the deathlink setting", OverrideDeathlink);
            _helper.ConsoleCommands.Add("trap_difficulty", "Override the trap difficulty setting", OverrideTrapDifficulty);

#if DEBUG
            _helper.ConsoleCommands.Add("connect", $"Connect to Archipelago. {CONNECT_SYNTAX}", OnCommandConnectToArchipelago);
            _helper.ConsoleCommands.Add("disconnect", $"Disconnects from Archipelago. {CONNECT_SYNTAX}", OnCommandDisconnectFromArchipelago);
            _helper.ConsoleCommands.Add("set_next_season", "Sets the next season to a chosen value", SetNextSeason);
            //_helper.ConsoleCommands.Add("test_sendalllocations", "Tests if every AP item in the stardew_valley_location_table json file are supported by the mod", _tester.TestSendAllLocations);
            // _helper.ConsoleCommands.Add("load_entrances", "Loads the entrances file", (_, _) => _entranceRandomizer.LoadTransports());
            // _helper.ConsoleCommands.Add("save_entrances", "Saves the entrances file", (_, _) => EntranceInjections.SaveNewEntrancesToFile());
            _helper.ConsoleCommands.Add("export_shippables", "Export all currently loaded shippable items", ExportShippables);
            _helper.ConsoleCommands.Add("export_mismatches", "Export all items where Name and DisplayName mismatch which can be shipped", ExportMismatchedItems);
            _helper.ConsoleCommands.Add("release_slot", "Release the current slot completely", ReleaseSlot);
            _helper.ConsoleCommands.Add("debug_method", "Runs whatever is currently in the debug method", DebugMethod);
#endif
#if TILESANITY
            _helper.ConsoleCommands.Add("walkable_tiles", "Gets the list of every walkable tile",
                this.ListWalkableTiles);
            _helper.ConsoleCommands.Add("walkable_csv", "Gets the csv of every walkable tile",
                this.ConvertWalkablesToCSV);
#endif

            ItemRegistry.AddTypeDefinition(new ArchipelagoLocationDataDefinition());
            ItemQueryResolver.Register(IDProvider.AP_LOCATION, PurchasableAPLocationQueryDelegate);
            ItemQueryResolver.Register(IDProvider.METAL_DETECTOR_ITEMS, TravelingMerchantInjections.CreateMetalDetectorItems);
            ItemQueryResolver.Register(IDProvider.TRAVELING_CART_DAILY_CHECK, TravelingMerchantInjections.CreateDailyCheck);
            ItemQueryResolver.Register(IDProvider.ARCHIPELAGO_EQUIPMENTS, AdventureGuildEquipmentsQueryDelegate);
            ItemQueryResolver.Register(IDProvider.TOOL_UPGRADES_CHEAPER, ToolShopStockModifier.ToolUpgradesCheaperQuery);
            GameStateQuery.Register(GameStateCondition.HAS_RECEIVED_ITEM, HasReceivedItemQueryDelegate);
            GameStateQuery.Register(GameStateCondition.HAS_STOCK_SIZE, TravelingMerchantInjections.HasStockSizeQueryDelegate);
            GameStateQuery.Register(GameStateCondition.FOUND_ARTIFACT, ArtifactsFoundQueryDelegate);
            GameStateQuery.Register(GameStateCondition.FOUND_MINERAL, MineralsFoundQueryDelegate);
            TriggerActionManager.RegisterAction(TriggerActionProvider.TRAVELING_MERCHANT_PURCHASE, TravelingMerchantInjections.OnPurchasedRandomItem);
        }

        private IEnumerable<ItemQueryResult> PurchasableAPLocationQueryDelegate(string key, string arguments, ItemQueryContext context, bool avoidrepeat, HashSet<string> avoiditemids, Action<string, string> logerror)
        {
            return ObtainableArchipelagoLocation.Create(arguments, _logger, Helper, _locationChecker, _archipelago);
        }

        private IEnumerable<ItemQueryResult> AdventureGuildEquipmentsQueryDelegate(string key, string arguments, ItemQueryContext context, bool avoidrepeat, HashSet<string> avoiditemids, Action<string, string> logerror)
        {
            return _weaponsManager.GetEquipmentsForSale(arguments);
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

        private void ResetArchipelago()
        {
            _archipelago.DisconnectPermanently();
            if (State != null)
            {
                State.APConnectionInfo = null;
            }
            State = new ArchipelagoStateDto();

            _harmony.UnpatchAll(ModManifest.UniqueID);
            _locationsPatcher?.CleanEvents();
            _logicPatcher?.CleanEvents();
            _modLogicPatcher?.CleanEvents();
            _bundlesManager?.CleanEvents();
            _bundlesManager = null;
            SeasonsRandomizer.ResetMailKeys();
            _multiSleepManager = new MultiSleepManager(_logger, _helper, _harmony);
            _advancedOptionsManager = new AdvancedOptionsManager(this, _logger, _helper, _harmony, _archipelago, _testerFeatures);
            _advancedOptionsManager.InjectArchipelagoAdvancedOptions();
            _giftHandler?.Dispose();
            _giftHandler = new CrossGiftHandler();
            _villagerEvents = new ModifiedVillagerEventChecker();
            SkillInjections.ResetSkillExperience();
            FriendshipInjections.ResetArchipelagoFriendshipPoints();

            IslandWestMapInjections.PatchMapInjections(_logger, _helper, _harmony);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ResetArchipelago();
            ResetModIntegrations();
        }

        private void OnSaveCreating(object sender, SaveCreatingEventArgs e)
        {
            State.ItemsReceived = new List<ReceivedItem>();
            State.LocationsChecked = new List<string>();
            State.LocationsScouted = new Dictionary<string, ScoutedLocation>();
            State.LettersGenerated = new Dictionary<string, string>();
            SkillInjections.ResetSkillExperience();
            FriendshipInjections.ResetArchipelagoFriendshipPoints();

            if (!_archipelago.IsConnected)
            {
                _logger.LogError("You are not allowed to create a new game without connecting to Archipelago");
                Game1.ExitToTitle();
                return;
            }

            _seasonsRandomizer = new SeasonsRandomizer(_logger, _helper, _archipelago, State);
            State.TrapDifficultyOverride = null;
            State.SeasonsOrder = new List<string>();
            State.SeasonsOrder.Add(_seasonsRandomizer.GetFirstSeason());
            SeasonsRandomizer.SetSeason(State.SeasonsOrder.Last());

            DebugAssertStateValues(State);
            _helper.Data.WriteSaveData(AP_DATA_KEY, State);
            _helper.Data.WriteSaveData(AP_EXPERIENCE_KEY, SkillInjections.GetArchipelagoExperience());
            _helper.Data.WriteSaveData(AP_FRIENDSHIP_KEY, FriendshipInjections.GetArchipelagoFriendshipPoints());
            _helper.WriteConfig(Config);
        }

        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            SeasonsRandomizer.PrepareDateForSaveGame();
            State.ItemsReceived = _itemManager.GetAllItemsAlreadyProcessed();
            State.LocationsChecked = _locationChecker.GetAllLocationsAlreadyChecked();
            State.LocationsScouted = _archipelago.ScoutedLocations;
            // _state.SeasonOrder should be fine?

            DebugAssertStateValues(State);
            _helper.Data.WriteSaveData(AP_DATA_KEY, State);
            _helper.Data.WriteSaveData(AP_EXPERIENCE_KEY, SkillInjections.GetArchipelagoExperience());
            _helper.Data.WriteSaveData(AP_FRIENDSHIP_KEY, FriendshipInjections.GetArchipelagoFriendshipPoints());
            _helper.WriteConfig(Config);
        }

        private void DebugAssertStateValues(ArchipelagoStateDto state)
        {
            if (state.APConnectionInfo == null)
            {
                _logger.Log($"About to write Archipelago State data, but the connectionInfo is null! This should never happen. Please contact KaitoKid and describe what you did last so it can be investigated.", LogLevel.Error);
            }

            if (state.LettersGenerated == null)
            {
                _logger.Log($"About to write Archipelago State data, but the there are no custom letters! This should never happen. Please contact KaitoKid and describe what you did last so it can be investigated.", LogLevel.Error);
            }
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                ReadPersistentArchipelagoData();

                if (!AttemptConnectionToArchipelago())
                {
                    return;
                }

                InitializeAfterConnection();
            }
            catch (Exception)
            {
                Game1.chatBox?.addMessage($"A Fatal error has occurred while initializing Archipelago. Check SMAPI for details to report the problem", Color.Red);
                throw;
            }
        }

        private void InitializeAfterConnection()
        {
            _stardewItemManager = new StardewItemManager(_logger);
            _mail = new Mailman(_logger, State);
            _locationChecker = new StardewLocationChecker(_logger, _archipelago, State.LocationsChecked);
            _itemPatcher = new ItemPatcher(_logger, _helper, _harmony, _archipelago);
            _goalManager = new GoalManager(_logger, _helper, _harmony, _archipelago, _locationChecker);
            _entranceManager = new EntranceManager(_logger, _archipelago, State);
            var seedShopStockModifier = new SeedShopStockModifier(_logger, _helper, _archipelago, _locationChecker, _stardewItemManager);
            var nameSimplifier = new NameSimplifier();
            var friends = new Friends();
            ArchipelagoLocationDataDefinition.Initialize(_logger, _helper, _archipelago, _locationChecker);
            _logicPatcher = new RandomizedLogicPatcher(_logger, _helper, Config, _harmony, _archipelago, _locationChecker, _stardewItemManager, _entranceManager, seedShopStockModifier, nameSimplifier, friends, State);
            _seasonsRandomizer = new SeasonsRandomizer(_logger, _helper, _archipelago, State);
            _appearanceRandomizer = new AppearanceRandomizer(_logger, _helper, _archipelago, _harmony);

            var trapExecutor = new TrapExecutor(_logger, Helper, _archipelago, _giftHandler);
            var giftTrapManager = new GiftTrapManager(trapExecutor);
            _giftHandler.Initialize(_logger, _archipelago, _stardewItemManager, _mail, giftTrapManager);

            _tileSanityManager = new TileSanityManager(_harmony, _archipelago, _locationChecker, Monitor);
            _tileSanityManager.PatchWalk(this.Helper);
            _chatForwarder = new ChatForwarder(_logger, Monitor, _helper, _harmony, _archipelago, _giftHandler, _goalManager, trapExecutor.TileChooser, _tileSanityManager);
            _questCleaner = new QuestCleaner();

            _itemManager = new APItemManager(_logger, _helper, _harmony, _archipelago, _locationChecker, _stardewItemManager, _mail, trapExecutor, giftTrapManager, State.ItemsReceived);
            _weaponsManager = new WeaponsManager(_archipelago, _stardewItemManager, _archipelago.SlotData.Mods);
            _mailPatcher = new MailPatcher(_logger, _harmony, _archipelago, _locationChecker, State, new LetterActions(_helper, _mail, _archipelago, _weaponsManager, _itemManager.TrapManager, trapExecutor.BabyBirther, _stardewItemManager));
            _bundlesManager = new BundlesManager(_helper, _stardewItemManager, _archipelago.SlotData.BundlesData);
            _locationsPatcher = new LocationPatcher(_logger, _helper, Config, _harmony, _archipelago, State, _locationChecker, _stardewItemManager, _weaponsManager, _bundlesManager, seedShopStockModifier, friends);
            _shippingBehaviors = new NightShippingBehaviors(_logger, _archipelago, _locationChecker, nameSimplifier);
            _chatForwarder.ListenToChatMessages();
            _logicPatcher.PatchAllGameLogic();
            _modLogicPatcher = new ModRandomizedLogicPatcher(_logger, _helper, _harmony, _archipelago, seedShopStockModifier, _stardewItemManager);
            _modLogicPatcher.PatchAllGameLogic();
            _mailPatcher.PatchMailBoxForApItems();
            _entranceManager.SetEntranceRandomizerSettings(_archipelago.SlotData);
            _locationsPatcher.ReplaceAllLocationsRewardsWithChecks();
            _itemPatcher.PatchApItems();
            _goalManager.InjectGoalMethods();
            _multiSleepManager.InjectMultiSleepOption(_archipelago.SlotData);
            SeasonsRandomizer.ChangeMailKeysBasedOnSeasonsToDaysElapsed();
            _modStateInitializer = new InitialModGameStateInitializer(_logger, _archipelago);
            _hintHelper = new HintHelper();
            Game1.chatBox?.addMessage(
                $"Connected to Archipelago as {_archipelago.SlotData.SlotName}. Type !!help for client commands", Color.Green);
        }

        private bool AttemptConnectionToArchipelago()
        {
            if (_archipelago.IsConnected)
            {
                return true;
            }

            if (_apConnectionOverride != null)
            {
                State.APConnectionInfo = _apConnectionOverride;
                _apConnectionOverride = null;
            }

            var errorMessage = "";
            if (State.APConnectionInfo == null)
            {
                errorMessage =
                    $"The game being loaded has no connection information.{Environment.NewLine}Please use the connect_override command to input connection fields before loading it";
            }
            else if (_archipelago.ConnectToMultiworld(State.APConnectionInfo, out errorMessage))
            {
                return true;
            }

            OfferRetry(errorMessage);
            return false;
        }

        private void OfferRetry(string errorMessage)
        {
            _logger.LogError($"Connection to Archipelago failed: {errorMessage}");
            var reconnectDialog = new ReconnectDialog(errorMessage, State.APConnectionInfo,
                OnClickRetry, (_) => OnCloseBehavior());
            Game1.activeClickableMenu = reconnectDialog;
        }

        private void OnClickRetry(string serverAddress)
        {
            var ipAndPort = serverAddress.Split(":");
            _apConnectionOverride = new ArchipelagoConnectionInfo(ipAndPort[0], int.Parse(ipAndPort[1]), State.APConnectionInfo.SlotName, State.APConnectionInfo.DeathLink, State.APConnectionInfo.Password);
            if (AttemptConnectionToArchipelago())
            {
                InitializeAfterConnection();
                DoArchipelagoDayStartedProcesses();
            }
        }

        private void OnCloseBehavior()
        {
            _logger.LogError("You are not allowed to load a save without connecting to Archipelago");
            // TitleMenu.subMenu = previousMenu;
            Game1.ExitToTitle();
        }

        private void ReadPersistentArchipelagoData()
        {
            var state = _helper.Data.ReadSaveData<ArchipelagoStateDto>(AP_DATA_KEY);
            if (state != null)
            {
                State = state;
                _archipelago.ScoutedLocations = State.LocationsScouted;
            }

            var apExperience = _helper.Data.ReadSaveData<Dictionary<int, int>>(AP_EXPERIENCE_KEY);
            SkillInjections.SetArchipelagoExperience(apExperience);

            var apFriendship = _helper.Data.ReadSaveData<Dictionary<string, int>>(AP_FRIENDSHIP_KEY);
            FriendshipInjections.SetArchipelagoFriendshipPoints(apFriendship);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            SeasonsRandomizer.ChangeMailKeysBasedOnSeasonsToDaysElapsed();
            SeasonsRandomizer.SendMailHardcodedForToday();

            if (MultiSleepManager.TryDoMultiSleepOnDayStarted())
            {
                return;
            }

            DoArchipelagoDayStartedProcesses();
        }

        private void DoArchipelagoDayStartedProcesses()
        {
            if (!_archipelago.MakeSureConnected(5))
            {
                return;
            }

            _questCleaner.CleanQuests(Game1.player);
            FarmInjections.DeleteStartingDebris();
            FarmInjections.PlaceEarlyShippingBin();
            _mail.SendToday();
            FarmInjections.ForcePetIfNeeded(_mail);
            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _itemManager.ReceiveAllNewItems(false);
            _goalManager.CheckGoalCompletion();
            _mail.SendTomorrow();
            PlayerBuffInjections.CheckForApBuffs();
            if (State.TrapDifficultyOverride != null)
            {
                _archipelago.SlotData.TrapItemsDifficulty = State.TrapDifficultyOverride.Value;
            }
            AppearanceRandomizer.GenerateSeededShuffledAppearances();
            AppearanceRandomizer.RefreshAllNPCs();
            _entranceManager.ResetCheckedEntrancesToday(_archipelago.SlotData);
            TheaterInjections.UpdateScheduleForEveryone();
            Helper.GameContent.InvalidateCache("Data/Shops"); // This should be reworked someday

            var bugFixer = new BugFixer(_logger, _locationChecker);
            bugFixer.FixKnownBugs();

            _hintHelper.GiveHintTip(_archipelago.GetSession());
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            _giftHandler.ReceiveAllGiftsTomorrow();
            _villagerEvents.CheckJunaHearts(_archipelago);
            _shippingBehaviors?.CheckShipsanityLocationsBeforeSleep();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            //    _itemManager.ItemParser.TrapManager.DequeueTrap();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            _archipelago.APUpdate();
            if (!_archipelago.IsConnected || _itemManager == null)
            {
                return;
            }

            if (e.IsMultipleOf(60))
            {
                _itemManager.ItemParser.TrapManager.DequeueTrap();
            }
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            ResetArchipelago();
        }

        private void OnCommandConnectToArchipelago(string arg1, string[] arg2)
        {
            if (arg2.Length < 2)
            {
                _logger.Log($"You must provide an IP with a port, and a slot name, to connect to archipelago. {CONNECT_SYNTAX}", LogLevel.Info);
                return;
            }

            var ipAndPort = arg2[0].Split(":");
            if (ipAndPort.Length < 2)
            {
                _logger.Log($"You must provide an IP with a port, and a slot name, to connect to archipelago. {CONNECT_SYNTAX}", LogLevel.Info);
                return;
            }

            var ip = ipAndPort[0];
            var port = int.Parse(ipAndPort[1]);
            var slot = arg2[1];
            var password = arg2.Length >= 3 ? arg2[2] : "";
            _apConnectionOverride = new ArchipelagoConnectionInfo(ip, port, slot, null, password);
            _logger.Log($"Your next connection attempt will instead use {ip}:{port} on slot {slot}.", LogLevel.Info);
        }

        private void OnCommandDisconnectFromArchipelago(string arg1, string[] arg2)
        {
            ArchipelagoDisconnect();
        }

        private void OnItemReceived()
        {
            _itemManager?.ReceiveAllNewItems();
        }

        public bool ArchipelagoConnect(string ip, int port, string slot, string password, out string errorMessage)
        {
            var apConnection = new ArchipelagoConnectionInfo(ip, port, slot, null, password);
            if (!_archipelago.ConnectToMultiworld(apConnection, out errorMessage))
            {
                return false;
            }

            State.APConnectionInfo = apConnection;
            return true;
        }

        public void ArchipelagoDisconnect()
        {
            Game1.ExitToTitle();
            _archipelago.DisconnectPermanently();
            State.APConnectionInfo = null;
        }

        private void SetNextSeason(string arg1, string[] arg2)
        {
            if (arg2.Length < 1)
            {
                _logger.Log($"You must specify a season", LogLevel.Info);
                return;
            }

            var season = arg2[0];
            var currentSeasonNumber = (int)Game1.stats.DaysPlayed / 28;
            if (State.SeasonsOrder.Count <= currentSeasonNumber)
            {
                State.SeasonsOrder.Add(season);
            }
            else
            {
                State.SeasonsOrder[currentSeasonNumber] = season;
            }
        }

#if DEBUG

        private void ExportShippables(string arg1, string[] arg2)
        {
            _stardewItemManager.ExportAllItemsMatching(x => x.canBeShipped(), "shippables.json");
        }

        private void ExportMismatchedItems(string arg1, string[] arg2)
        {
            _stardewItemManager.ExportAllMismatchedItems(x => x.canBeShipped(), "mismatches.json");
        }

        private void ReleaseSlot(string arg1, string[] arg2)
        {
            if (!_archipelago.IsConnected || !Game1.hasLoadedGame || arg2.Length < 1)
            {
                return;
            }

            var slotName = arg2[0];

            if (slotName != _archipelago.GetPlayerName() || slotName != Game1.player.Name)
            {
                return;
            }

            foreach (var missingLocation in _locationChecker.GetAllMissingLocationNames())
            {
                _locationChecker.AddCheckedLocation(missingLocation);
            }
        }

        private void ListWalkableTiles(string arg1, string[] arg2)
        {
            Farmer farmer = Game1.player;
            GameLocation playerCurrentLocation = farmer.currentLocation;
            List<Vector2> walkables = new();
            int width = playerCurrentLocation.map.DisplayWidth / 64;
            int height = playerCurrentLocation.map.DisplayHeight / 64;
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    Vector2 position = new(x, y);
                    if (playerCurrentLocation.isTilePassable(position))
                    {
                        walkables.Add(position);
                    }
                }
            }

            List<Vector2> validatedWalkables = new();
            List<Vector2> toTest = new();
            Vector2 point = new(farmer.TilePoint.X, farmer.TilePoint.Y);
            if (walkables.Contains(point))
            {
                walkables.Remove(point);
                toTest.Add(point);
            }
            else
            {
                Console.Out.WriteLine("current tile is not walkable");
                return;
            }
            while (toTest.Count > 0)
            {
                point = toTest[0];
                validatedWalkables.Add(point);
                toTest.RemoveAt(0);
                point += new Vector2(1, 0);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
                point += new Vector2(-1, -1);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
                point += new Vector2(-1, 1);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
                point += new Vector2(1, 1);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
            }

            const string tileFile = "tiles.json";
            SortedDictionary<string, List<Vector2>> dictionary;
            if (File.Exists(tileFile))
            {
                dictionary =
                    JsonConvert.DeserializeObject<SortedDictionary<string, List<Vector2>>>(File.ReadAllText(tileFile));
            }
            else
            {
                dictionary = new SortedDictionary<string, List<Vector2>>();
            }

            validatedWalkables.Sort(((vector2, vector3) => vector2.X.CompareTo(vector3.X) * 2 + vector2.Y.CompareTo(vector3.Y)));
            string displayedName;
            switch (arg2.Length)
            {
                case 0:
                    displayedName = TileSanityManager.GetMapName(farmer);
                    break;
                default:
                    displayedName = arg2[0] switch
                    {
                        "0" => string.Join(' ', arg2.Skip(1)),
                        "1" => playerCurrentLocation.DisplayName,
                        "2" => playerCurrentLocation.Name,
                        _ => throw new ArgumentException()
                    };
                    break;
            }

            if (dictionary.ContainsKey(displayedName))
                dictionary[displayedName] = validatedWalkables;
            else
                dictionary.Add(displayedName, validatedWalkables);
            File.WriteAllText(tileFile, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
            Console.Out.WriteLine("Finished finding walkable tiles");
        }

        private void ConvertWalkablesToCSV(string arg1, string[] arg2)
        {
            const string tileFile = "tiles.json";
            var data_dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<Vector2>>>(File.ReadAllText(tileFile));

            var tiles_dictionary = new Dictionary<(string, int, int), bool>();
            foreach (var (map, tiles) in data_dictionary)
            {
                foreach (var (x, y) in tiles)
                {
                    tiles_dictionary.Add(((string, int, int))(map, x, y), true);
                }
            }
            foreach (var (map, tiles) in data_dictionary)
            {
                foreach (var (x, y) in tiles)
                {
                    for (var i = 2; i <= 10; i++)
                    {
                        tiles_dictionary.TryAdd(((string, int, int))(map, x / i, y / i), false);
                    }
                }
            }

            var sortedDict = from entry in tiles_dictionary
                orderby entry.Key.Item1, entry.Key.Item2, entry.Key.Item3
                select entry;

            const string locationFile = "tilesanity.csv";
            var id = 20_000;
            var locationText = "id,region,name,tags,mod_name\n" +
                               "20000,None,Progressive Tile,NOT_TILE,\n";
            var previous_map = "";
            foreach (var ((map, x, y), real) in sortedDict)
            {
                if (map != previous_map)
                {
                    id += 1_000 - id % 1_000;
                    previous_map = map;
                }
                var name = $"Tilesanity: {map} ({x}-{y})";
                if (real)
                    locationText += $"{id++},{map},{name},TILESANITY,\n";
                else
                    locationText += $"{id++},{map},{name},\"TILESANITY,NOT_TILE\",\n";
            }
            File.WriteAllText(locationFile, locationText);
            Console.Out.WriteLine("CSV updated");
        }

#endif
        private void ResetModIntegrations()
        {
            var GenericModConfigMenu = new GenericModConfig(this);
            GenericModConfigMenu.RegisterConfig();
        }

        private void ExportGifts(string arg1, string[] arg2)
        {
            const string giftsFile = "gifts.json";
            _giftHandler.ExportAllGifts(giftsFile);
            _logger.Log($"Gifts have been exported to {giftsFile}", LogLevel.Info);
        }

        private void OverrideDeathlink(string arg1, string[] arg2)
        {
            _archipelago?.ToggleDeathlink();
        }

        private void OverrideTrapDifficulty(string arg1, string[] arg2)
        {
            if (_archipelago == null || State == null || !_archipelago.MakeSureConnected(0))
            {
                _logger.Log($"This command can only be used from in-game, when connected to Archipelago", LogLevel.Info);
                return;
            }

            if (arg2.Length < 1)
            {
                _logger.Log($"Choose one of the following difficulties: [NoTraps, Easy, Medium, Hard, Hell, Nightmare].", LogLevel.Info);
                return;
            }

            var difficulty = arg2[0];
            if (!Enum.TryParse<TrapItemsDifficulty>(difficulty, true, out var difficultyOverride))
            {
                _logger.Log($"Choose one of the following difficulties: [NoTraps, Easy, Medium, Hard, Hell, Nightmare].", LogLevel.Info);
                return;
            }

            State.TrapDifficultyOverride = difficultyOverride;
            _logger.Log($"Trap Difficulty set to [{difficultyOverride}]. Change will be saved next time you sleep", LogLevel.Info);
        }

        private void DebugMethod(string arg1, string[] arg2)
        {
            ExportCropState("crops_before.json");
            _itemManager.TrapManager.TryExecuteTrapImmediately("Benjamin Budton");
            ExportCropState("crops_after.json");
        }

        private void ExportCropState(string cropsFile)
        {
            var cropsByLocation = new Dictionary<string, Dictionary<Vector2, CropInfo>>();
            foreach (var gameLocation in Game1.locations)
            {
                var cropsHere = new Dictionary<Vector2, CropInfo>();

                foreach (var terrainFeature in gameLocation.terrainFeatures.Values)
                {
                    if (terrainFeature is not HoeDirt groundDirt || groundDirt.crop == null)
                    {
                        continue;
                    }
                    var cropInfo = new CropInfo()
                    {
                        CurrentPhase = groundDirt.crop.currentPhase.Value,
                        DayOfCurrentPhase = groundDirt.crop.dayOfCurrentPhase.Value,
                        FullyGrown = groundDirt.crop.fullyGrown.Value,
                        PhaseDays = groundDirt.crop.phaseDays.ToArray(),
                    };
                    cropsHere.Add(groundDirt.Tile, cropInfo);
                }

                cropsByLocation.Add(gameLocation.Name, cropsHere);
            }
            var objectsAsJson = JsonConvert.SerializeObject(cropsByLocation);
            File.WriteAllText(cropsFile, objectsAsJson);
        }

        public struct CropInfo
        {
            public bool FullyGrown { get; set; }
            public int DayOfCurrentPhase { get; set; }
            public int CurrentPhase { get; set; }
            public int[] PhaseDays { get; set; }
        }
    }

}
