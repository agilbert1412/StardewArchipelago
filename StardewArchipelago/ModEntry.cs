using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ConnectionResults;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Bugfixes;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.GameModifications.MoveLink;
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
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewArchipelago.Locations.Patcher;
using StardewArchipelago.Logging;
using StardewArchipelago.Registry;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewArchipelago
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        public ModConfig Config;

        private const string AP_DATA_KEY = "ArchipelagoData";
        private const string AP_EXPERIENCE_KEY = "ArchipelagoSkillsExperience";
        private const string AP_FRIENDSHIP_KEY = "ArchipelagoFriendshipPoints";

        public readonly string UniqueIdentifier = Guid.NewGuid().ToString();

        private LogHandler _logger;
        private IModHelper _helper;
        private Harmony _harmony;
        private IRegistry _registry;
        private TesterFeatures _testerFeatures;
        private StardewArchipelagoClient _archipelago;
        private AdvancedOptionsManager _advancedOptionsManager;
        private Mailman _mail;
        private ChatForwarder _chatForwarder;
        private IGiftHandler _giftHandler;
        private APItemManager _itemManager;
        private WeaponsManager _weaponsManager;
        private RandomizedLogicPatcher _logicPatcher;
        private JojapocalypseManager _jojapocalypseManager;
        private MailPatcher _mailPatcher;
        private BundlesManager _bundlesManager;
        private StardewLocationChecker _locationChecker;
        private JojaLocationChecker _jojaLocationChecker;
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
        private SaveCleaner _saveCleaner;

        private ModRandomizedLogicPatcher _modLogicPatcher;
        private InitialModGameStateInitializer _modStateInitializer;
        private ModifiedVillagerEventChecker _villagerEvents;

        public ArchipelagoConnectionInfo ArchipelagoConnectionOverride { get; set; }

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

            ArchipelagoConnectionOverride = null;

            _logger = new LogHandler(Monitor);
            _helper = helper;
            _harmony = new Harmony(ModManifest.UniqueID);
            _registry = new RegistryManager(_logger, _helper, this);
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

            _registry.RegisterOnModEntry();

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
            _multiSleepManager = new MultiSleepManager(_logger, _helper, _archipelago, _harmony);
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
            ArchipelagoTextures.Initialize(_logger, Helper);
        }

        private void OnSaveCreating(object sender, SaveCreatingEventArgs e)
        {
            Game1.UseLegacyRandom = Config.UseLegacyRandomization;
            State.ItemsReceived = new List<ReceivedItem>();
            State.LocationsChecked = new List<string>();
            State.JojaLocationsChecked = new List<string>();
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
            _saveCleaner.OnSaving(sender, e);

            SeasonsRandomizer.PrepareDateForSaveGame();
            State.ItemsReceived = _itemManager.GetAllItemsAlreadyProcessed();
            State.LocationsChecked = _locationChecker.GetAllLocationsAlreadyChecked();
            if (_locationChecker is DisabledLocationChecker disabledLocationChecker)
            {
                State.AttemptedLocationChecks = new List<string>(disabledLocationChecker.LocationsAlreadyAttemptedToCheck);
            }
            State.JojaLocationsChecked = _jojaLocationChecker.GetAllLocationsCheckedByJoja();
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

                if (!AttemptConnectionToArchipelago().Success)
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
            _mail = new Mailman(_logger, _archipelago, State);
            if (_archipelago.SlotData.Jojapocalypse.Jojapocalypse == JojapocalypseSetting.Forced)
            {
                _locationChecker = new DisabledLocationChecker(_logger, _archipelago, State.LocationsChecked, State.AttemptedLocationChecks);
            }
            else
            {
                _locationChecker = new StardewLocationChecker(_logger, _archipelago, State.LocationsChecked);
            }
            _saveCleaner = new SaveCleaner(_logger, _locationChecker);
            _jojaLocationChecker = new JojaLocationChecker(_archipelago, _locationChecker, State.JojaLocationsChecked);
            _itemPatcher = new ItemPatcher(_logger, _helper, _harmony, _archipelago);
            _goalManager = new GoalManager(_logger, _helper, _harmony, _archipelago, _locationChecker);
            _entranceManager = new EntranceManager(_logger, _archipelago, State);
            var seedShopStockModifier = new SeedShopStockModifier(_logger, _helper, _archipelago, _locationChecker, _stardewItemManager);
            var nameSimplifier = new NameSimplifier();
            var friends = new Friends();
            ArchipelagoLocationDataDefinition.Initialize(_logger, _helper, _archipelago, _locationChecker);
            _logicPatcher = new RandomizedLogicPatcher(_logger, _helper, Config, _harmony, _archipelago, _locationChecker, _stardewItemManager, _entranceManager, seedShopStockModifier, nameSimplifier, friends, State, new BundleReader());
            _jojapocalypseManager = new JojapocalypseManager(_logger, _helper, Config, _harmony, _archipelago, _locationChecker, _jojaLocationChecker);
            _seasonsRandomizer = new SeasonsRandomizer(_logger, _helper, _archipelago, State);
            _appearanceRandomizer = new AppearanceRandomizer(_logger, _helper, _archipelago, _harmony);

            var trapExecutor = new TrapExecutor(_logger, Helper, _archipelago, _giftHandler);
            var giftTrapManager = new GiftTrapManager(_logger, trapExecutor);
            _giftHandler.Initialize(_logger, _archipelago, _stardewItemManager, _mail, giftTrapManager);

            _tileSanityManager = new TileSanityManager(_harmony, _archipelago, _locationChecker, Monitor);
            _tileSanityManager.PatchWalk(this.Helper);
            var bank = new BankHandler(_archipelago);
            _chatForwarder = new ChatForwarder(_logger, Monitor, _helper, _harmony, _archipelago, _giftHandler, _goalManager, trapExecutor.TileChooser, _tileSanityManager, bank);
            _questCleaner = new QuestCleaner(_locationChecker);

            _itemManager = new APItemManager(_logger, _helper, _harmony, _archipelago, _locationChecker, _stardewItemManager, _mail, trapExecutor, giftTrapManager, State.ItemsReceived);
            _weaponsManager = new WeaponsManager(_archipelago, _stardewItemManager, _archipelago.SlotData.Mods);

            _registry.Initialize(_archipelago, _stardewItemManager, _locationChecker, _giftHandler, _weaponsManager, State);

            _mailPatcher = new MailPatcher(_logger, _harmony, _archipelago, _locationChecker, State, new LetterActions(_logger, _helper, _mail, _archipelago, _weaponsManager, _itemManager.TrapManager, trapExecutor.BabyBirther, _stardewItemManager));
            _bundlesManager = new BundlesManager(_logger, _helper, _archipelago, _stardewItemManager);
            _locationsPatcher = new LocationPatcher(_logger, _helper, Config, _harmony, _archipelago, State, _locationChecker, _jojaLocationChecker, _stardewItemManager, _weaponsManager, _bundlesManager, seedShopStockModifier, friends, _itemManager.TrapManager, bank, nameSimplifier, _giftHandler.Sender);
            _shippingBehaviors = new NightShippingBehaviors(_logger, _archipelago, _locationChecker, nameSimplifier);
            _chatForwarder.ListenToChatMessages();
            _logicPatcher.PatchAllGameLogic();
            _jojapocalypseManager.PatchAllJojaLogic();
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
            Game1.chatBox?.addMessage($"Connected to Archipelago as {_archipelago.SlotData.SlotName}. Type !!help for client commands", Color.Green);
            ArchipelagoJunimoNoteMenu.CompleteBundleIfExists(MemeBundleNames.CONNECTION);
        }

        private ConnectionResult AttemptConnectionToArchipelago()
        {
            if (_archipelago.IsConnected)
            {
                return new AlreadyConnectedResult();
            }

            if (ArchipelagoConnectionOverride != null)
            {
                State.APConnectionInfo = ArchipelagoConnectionOverride;
                ArchipelagoConnectionOverride = null;
            }

            if (State.APConnectionInfo == null)
            {
                var noConnectionResult = new NoConnectionInformationResult();
                ShowErrorAndExit(noConnectionResult.Message);
                return noConnectionResult;
            }

            var result = _archipelago.ConnectToMultiworld(State.APConnectionInfo);
            if (result.Success)
            {
                return result;
            }

            if (result is FailedConnectionResult failedResult)
            {
                if (failedResult.RetryPossible)
                {
                    OfferRetry(failedResult.Message);
                }
                else
                {
                    ShowErrorAndExit(failedResult.Message);
                }
            }

            return result;
        }

        private void ShowErrorAndExit(string errorMessage)
        {
            _logger.LogError($"Connection to Archipelago failed: {errorMessage}");
            Game1.activeClickableMenu = new InformationDialog(errorMessage, (_) => OnCloseBehavior());
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
            ArchipelagoConnectionOverride = new ArchipelagoConnectionInfo(ipAndPort[0], int.Parse(ipAndPort[1]), State.APConnectionInfo.SlotName, State.APConnectionInfo.DeathLink, State.APConnectionInfo.Password);
            if (AttemptConnectionToArchipelago().Success)
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
            ArchipelagoJunimoNoteMenu.OnDayStarted(_giftHandler.Receiver);

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
            FarmInjections.PlaceAutoBuildings();
            _mail.SendToday();
            FarmInjections.ForcePetIfNeeded(_mail);
            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _itemManager.ReceiveAllNewItems(false);
            _itemManager.MakeSureBackpacksAreFirst();
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
            ArchipelagoJunimoNoteMenu.OnDayEnded();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            //    _itemManager.ItemParser.TrapManager.DequeueTrap();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            _archipelago.APUpdate();
            ArchipelagoJunimoNoteMenu.OnUpdateTickedStatic(e);
            State?.Wallet?.CookieClicker?.DoFrame();
            if (!_archipelago.IsConnected || _itemManager == null)
            {
                return;
            }

            if (e.IsMultipleOf(60))
            {
                _itemManager.ItemParser.TrapManager.DequeueTrap();
            }

            MovementInjections.UpdateMove(e);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            ResetArchipelago();
        }

        private void OnItemReceived()
        {
            _itemManager?.ReceiveAllNewItems();
        }

        public ConnectionResult ArchipelagoConnect(string ip, int port, string slot, string password)
        {
            var apConnection = new ArchipelagoConnectionInfo(ip, port, slot, null, password);
            var result = _archipelago.ConnectToMultiworld(apConnection);
            if (result.Success)
            {
                State.APConnectionInfo = apConnection;
            }

            return result;
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
        private void ResetModIntegrations()
        {
            var GenericModConfigMenu = new GenericModConfig(this);
            GenericModConfigMenu.RegisterConfig();
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
