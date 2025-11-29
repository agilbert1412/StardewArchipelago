using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using Object = StardewValley.Object;

namespace StardewArchipelago.Items.Traps
{
    public class TrapManager
    {
        private const string BURNT = "Burnt";
        private const string DARKNESS = "Darkness";
        private const string FROZEN = "Frozen";
        private const string JINXED = "Jinxed";
        private const string NAUSEATED = "Nauseated";
        private const string SLIMED = "Slimed";
        private const string WEAKNESS = "Weakness";
        private const string TAXES = "Taxes";
        private const string RANDOM_TELEPORT = "Random Teleport";
        private const string CROWS = "The Crows";
        private const string MONSTERS = "Monsters";
        private const string ENTRANCE_RESHUFFLE = "Entrance Reshuffle";
        private const string DEBRIS = "Debris";
        private const string SHUFFLE = "Shuffle";
        private const string WINTER = "Temporary Winter";
        private const string PARIAH = "Pariah";
        private const string DROUGHT = "Drought";
        private const string TIME_FLIES = "Time Flies";
        private const string BABIES = "Babies";
        private const string MEOW = "Meow";
        private const string BARK = "Bark";
        private const string DEPRESSION = "Depression";
        private const string UNGROWTH = "Benjamin Budton";
        private const string INFLATION = "Inflation";
        private const string BOMB = "Bomb";
        private const string NUDGE = "Nudge";

        private static ILogger _logger;
        private readonly IModHelper _helper;
        private readonly Harmony _harmony;
        private static StardewArchipelagoClient _archipelago;
        public readonly TrapExecutor TrapExecutor;
        private readonly GiftTrapManager _giftTrapManager;

        private readonly Dictionary<string, Action> _traps;

        private ConcurrentQueue<QueuedTrap> _queuedTraps;
        private object _trapLock = new();

        public TrapManager(ILogger logger, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago, TrapExecutor trapExecutor, GiftTrapManager giftTrapManager)
        {
            _logger = logger;
            _helper = helper;
            _harmony = harmony;
            _archipelago = archipelago;
            _giftTrapManager = giftTrapManager;
            TrapExecutor = trapExecutor;
            _traps = new Dictionary<string, Action>();
            RegisterTraps();
            _queuedTraps = new ConcurrentQueue<QueuedTrap>();
            _giftTrapManager.AssignTrapQueue(_queuedTraps);

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.salePrice)),
                prefix: new HarmonyMethod(typeof(TrapExecutor), nameof(TrapExecutor.SalePrice_GetCorrectInflation_Prefix))
            );
            InitializeTemporaryBaby(logger, helper, harmony);
        }

        private void InitializeTemporaryBaby(ILogger logger, IModHelper helper, Harmony harmony)
        {
            TemporaryBaby.Initialize(logger, helper);
            harmony.Patch(
                original: AccessTools.Method(typeof(Child), nameof(Child.tenMinuteUpdate)),
                prefix: new HarmonyMethod(typeof(TemporaryBaby), nameof(TemporaryBaby.ChildTenMinuteUpdate_MoveBabiesAnywhere_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate)),
                prefix: new HarmonyMethod(typeof(TemporaryBaby), nameof(TemporaryBaby.GameLocationPerformTenMinuteUpdate_MoveBabiesAnywhere_Prefix))
            );
        }

        public bool IsTrap(string unlockName)
        {
            return _traps.ContainsKey(unlockName);
        }

        public LetterAttachment GenerateTrapLetter(ReceivedItem unlock)
        {
            return new LetterTrapAttachment(unlock, unlock.ItemName);
        }

        public bool CanGetTrappedRightNow()
        {
            var isSafeLocation = Game1.player.currentLocation is (FarmHouse or IslandFarmHouse);
            var isSleepTime = Game1.player.isInBed.Value || Game1.player.FarmerSprite.isPassingOut() || Game1.player.passedOut;
            // || Game1.eventUp || Game1.fadeToBlack || Game1.currentMinigame != null || Game1.isWarping || Game1.killScreen;

            return !isSafeLocation && !isSleepTime;
        }

        public string ExecuteRandomTrapImmediately(int seed)
        {
            var random = Utility.CreateRandom(Game1.uniqueIDForThisGame, seed);
            var trapNames = _traps.Keys.Where(x => x.EndsWith(" Trap") && !x.Contains("_")).Distinct().OrderBy(x => x).ToArray();
            var randomIndex = random.Next(0, trapNames.Length);
            var randomTrap = trapNames[randomIndex];
            ExecuteTrapImmediately(randomTrap);
            return randomTrap;
        }

        public bool TryExecuteTrapImmediately(string trapName)
        {
            if (!CanGetTrappedRightNow())
            {
                return false;
            }

            ExecuteTrapImmediately(trapName);
            return true;
        }

        public void ExecuteTrapImmediately(string trapName)
        {
            _queuedTraps.Enqueue(new QueuedItemTrap(trapName, _traps[trapName]));
        }

        public void DequeueTrap()
        {
            if (!CanGetTrappedRightNow())
            {
                return;
            }

            if (_queuedTraps.IsEmpty)
            {
                return;
            }

            if (!_queuedTraps.TryDequeue(out var trap))
            {
                return;
            }
            
            lock (_trapLock)
            {
                _logger.LogDebug($"Executing Trap {trap.Name}");
                trap.ExecuteNow();
            }
        }

        private void RegisterTraps()
        {
            _traps.Add(BURNT, TrapExecutor.AddBurntDebuff);
            _traps.Add(DARKNESS, TrapExecutor.AddDarknessDebuff);
            _traps.Add(FROZEN, TrapExecutor.AddFrozenDebuff);
            _traps.Add(JINXED, TrapExecutor.AddJinxedDebuff);
            _traps.Add(NAUSEATED, TrapExecutor.AddNauseatedDebuff);
            _traps.Add(SLIMED, TrapExecutor.AddSlimedDebuff);
            _traps.Add(WEAKNESS, TrapExecutor.AddWeaknessDebuff);
            _traps.Add(TAXES, TrapExecutor.ChargeTaxes);
            _traps.Add(RANDOM_TELEPORT, TrapExecutor.TeleportRandomly);
            _traps.Add(CROWS, TrapExecutor.SendCrows);
            _traps.Add(MONSTERS, TrapExecutor.SpawnMonsters);
            // _traps.Add(ENTRANCE_RESHUFFLE, );
            _traps.Add(DEBRIS, TrapExecutor.CreateDebris);
            _traps.Add(SHUFFLE, TrapExecutor.ShuffleInventory);
            // _traps.Add(WINTER, );
            _traps.Add(PARIAH, TrapExecutor.SendDislikedGiftToEveryone);
            _traps.Add(DROUGHT, TrapExecutor.PerformDroughtTrap);
            _traps.Add(TIME_FLIES, TrapExecutor.SkipTimeForward);
            _traps.Add(BABIES, TrapExecutor.SpawnTemporaryBabies);
            _traps.Add(MEOW, TrapExecutor.PlayMeows);
            _traps.Add(BARK, TrapExecutor.PlayBarks);
            _traps.Add(DEPRESSION, TrapExecutor.ForceNextMultisleep);
            _traps.Add(UNGROWTH, TrapExecutor.UngrowCrops);
            _traps.Add(INFLATION, TrapExecutor.ActivateInflation);
            _traps.Add(BOMB, TrapExecutor.Explode);
            _traps.Add(NUDGE, TrapExecutor.NudgePlayerItems);

            RegisterTrapsWithTrapSuffix();
            RegisterTrapsWithDifferentSpace();
        }

        private void RegisterTrapsWithDifferentSpace()
        {
            foreach (var trapName in _traps.Keys.ToArray())
            {
                var differentSpacedTrapName = trapName.Replace(" ", "_");
                if (differentSpacedTrapName != trapName)
                {
                    _traps.Add(differentSpacedTrapName, _traps[trapName]);
                }
            }
        }

        private void RegisterTrapsWithTrapSuffix()
        {
            foreach (var trapName in _traps.Keys.ToArray())
            {
                var trapWithSuffix = $"{trapName} Trap";
                _traps.Add(trapWithSuffix, _traps[trapName]);
            }
        }
    }
}
