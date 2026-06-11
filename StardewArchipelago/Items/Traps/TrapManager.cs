using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private const string BUTTERFINGERS = "Butterfingers";
        private const string SALE = "Sale";
        private const string EXOTIC = "Exotic";
        private const string ENCUMBERED = "Encumbered";
        private const string SPOILED = "Spoiled";
        private const string SUPER_MONSTER = "Super Monster";
        private const string WE_MOO_UNSEEN = "We Moo Unseen";
        private const string CONSTRUCTION = "Construction";
        private const string ERROR = "Error";
        private const string CUTSCENE = "Cutscene";
        private const string LORAX = "Lorax";
        private const string CLIMATE_CHANGE = "Climate Change";
        private const string NOISE = "Noise";
        private const string FISHING = "Fishing";
        private const string MENU = "Menu";
        private const string EMOTE = "Emote";
        private const string MAKEOVER = "Makeover";
        private const string BACK_TO_SCHOOL = "Back To School";
        private const string TIRED = "Tired";
        private const string INJURY = "Injury";

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
                original: AccessTools.Method(typeof(NPC), nameof(NPC.update), new[] { typeof(GameTime), typeof(GameLocation) }),
                postfix: new HarmonyMethod(typeof(TrapExecutor), nameof(TrapExecutor.Update_ShunPlayer_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.MovePosition)),
                prefix: new HarmonyMethod(typeof(TrapExecutor), nameof(TrapExecutor.MovePosition_SkipIfShunningPlayer_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.salePrice)),
                prefix: new HarmonyMethod(typeof(TrapExecutor), nameof(TrapExecutor.SalePrice_GetCorrectInflation_Prefix))
            );

            InitializeTemporaryBaby(logger, helper, harmony, archipelago);
            InitializeInvisibleCows(logger, helper, harmony, archipelago);
        }

        private void InitializeTemporaryBaby(ILogger logger, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago)
        {
            TemporaryBabyInjections.Initialize(logger, helper, archipelago);
            harmony.Patch(
                original: AccessTools.Method(typeof(Child), nameof(Child.dayUpdate)),
                prefix: new HarmonyMethod(typeof(TemporaryBabyInjections), nameof(TemporaryBabyInjections.DayUpdate_TemporaryBaby_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Child), nameof(Child.tenMinuteUpdate)),
                prefix: new HarmonyMethod(typeof(TemporaryBabyInjections), nameof(TemporaryBabyInjections.TenMinuteUpdate_TemporaryBaby_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate)),
                prefix: new HarmonyMethod(typeof(TemporaryBabyInjections), nameof(TemporaryBabyInjections.GameLocationPerformTenMinuteUpdate_MoveBabiesAnywhere_Prefix))
            );
        }

        private void InitializeInvisibleCows(ILogger logger, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago)
        {
            CowInjections.Initialize(logger, helper, archipelago);

            harmony.Patch(
                original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.draw), new[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(CowInjections), nameof(CowInjections.Draw_InvisibleCow_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.dayUpdate)),
                prefix: new HarmonyMethod(typeof(CowInjections), nameof(CowInjections.DayUpdate_InvisibleCow_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(FarmAnimal), nameof(FarmAnimal.farmerPushing)),
                prefix: new HarmonyMethod(typeof(CowInjections), nameof(CowInjections.FarmerPushing_TakeLongerToReact_Prefix))
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
            ExecuteTrapImmediately(trapName);
            return true;
        }

        public void ExecuteTrapImmediately(string trapName)
        {
            _queuedTraps.Enqueue(new QueuedItemTrap(trapName, _traps[trapName]));
        }

        public void DequeueTrap()
        {
            if (!TrapExecutor.CanGetTrappedRightNow())
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
            _traps.Add(PARIAH, TrapExecutor.BecomePariah);
            _traps.Add(DROUGHT, TrapExecutor.PerformDroughtTrap);
            _traps.Add(TIME_FLIES, TrapExecutor.SkipTimeForward);
            _traps.Add(BABIES, TrapExecutor.SpawnTemporaryBabies);
            _traps.Add(MEOW, TrapExecutor.PlayMeows);
            _traps.Add(BARK, TrapExecutor.PlayBarks);
            // _traps.Add(DEPRESSION, TrapExecutor.ForceNextMultisleep);
            _traps.Add(UNGROWTH, TrapExecutor.UngrowThings);
            _traps.Add(INFLATION, TrapExecutor.ActivateInflation);
            _traps.Add(BOMB, TrapExecutor.Explode);
            _traps.Add(NUDGE, TrapExecutor.NudgePlayerItems);

            _traps.Add(BUTTERFINGERS, TrapExecutor.Butterfingers);
            _traps.Add(SALE, TrapExecutor.SellItems);
            // _traps.Add(EXOTIC, TrapExecutor.Exotic);
            _traps.Add(ENCUMBERED, TrapExecutor.EncumberPlayer);
            _traps.Add(SPOILED, TrapExecutor.SpoilItems);
            _traps.Add(SUPER_MONSTER, TrapExecutor.SpawnSuperMonsters);
            _traps.Add(WE_MOO_UNSEEN, TrapExecutor.SpawnInvisibleCows);
            //_traps.Add(CONSTRUCTION, TrapExecutor.Construction);
            //_traps.Add(ERROR, TrapExecutor.Error);
            //_traps.Add(CUTSCENE, TrapExecutor.PlayCutscene);
            _traps.Add(LORAX, TrapExecutor.GrowTrees);
            _traps.Add(CLIMATE_CHANGE, TrapExecutor.ChangeWeather);
            _traps.Add(NOISE, TrapExecutor.PlayNoises);
            _traps.Add(FISHING, TrapExecutor.CatchFish);
            _traps.Add(MENU, TrapExecutor.OpenMenus);
            _traps.Add(EMOTE, TrapExecutor.PerformEmotes);
            _traps.Add(MAKEOVER, TrapExecutor.PerformMakeover);
            _traps.Add(BACK_TO_SCHOOL, TrapExecutor.RandomizeProfessions);
            _traps.Add(TIRED, TrapExecutor.Tired);
            _traps.Add(INJURY, TrapExecutor.Injury);

            RegisterTrapsWithTrapSuffix();
            RegisterTrapsWithDifferentSpace();
            RegisterTrapsWithDifferentCasing();
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
                differentSpacedTrapName = trapName.Replace(" ", "");
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

        private void RegisterTrapsWithDifferentCasing()
        {
            foreach (var trapName in _traps.Keys.ToArray())
            {
                var trapLower = trapName.ToLower();
                var trapUpper = trapName.ToUpper();
                if (!_traps.ContainsKey(trapLower))
                {
                    _traps.Add(trapLower, _traps[trapName]);
                }
                if (!_traps.ContainsKey(trapUpper))
                {
                    _traps.Add(trapUpper, _traps[trapName]);
                }
            }
        }
    }
}
