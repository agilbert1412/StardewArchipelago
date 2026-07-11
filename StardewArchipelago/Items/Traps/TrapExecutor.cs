using Archipelago.MultiClient.Net.Enums;
using Force.DeepCloner;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.MultiSleep;
using StardewArchipelago.Items.Traps.Shuffle;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using StardewValley.Network;
using Object = StardewValley.Object;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace StardewArchipelago.Items.Traps
{
    public class TrapExecutor
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static TrapsStateDto _permanentState;
        public static TrapDifficultyBalancer _difficultyBalancer;

        private readonly DebtManager _debtManager;
        public readonly BombSpawner BombSpawner;
        public readonly TileChooser TileChooser;
        public readonly MonsterSpawner MonsterSpawner;
        public readonly BabyBirther BabyBirther;
        public readonly CowSpawner CowSpawner;
        public readonly DebrisSpawner DebrisSpawner;
        public readonly InventoryShuffler InventoryShuffler;
        public readonly BuffApplier DebuffApplier;
        public readonly ObjectNudger Nudger;
        public readonly OutfitChanger _outfitChanger;
        private readonly string[] _availableSoundCues;
        private List<ICue> _activeSounds;

        public TrapExecutor(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, IGiftHandler giftHandler, ArchipelagoStateDto state)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _permanentState = state.TrapsState;
            _difficultyBalancer = new TrapDifficultyBalancer();
            _debtManager = new DebtManager(_permanentState);
            BombSpawner = new BombSpawner(_helper);
            TileChooser = new TileChooser();
            MonsterSpawner = new MonsterSpawner(TileChooser);
            BabyBirther = new BabyBirther();
            CowSpawner = new CowSpawner();
            DebrisSpawner = new DebrisSpawner(_logger);
            InventoryShuffler = new InventoryShuffler(_logger, giftHandler);
            DebuffApplier = new BuffApplier(_permanentState);
            Nudger = new ObjectNudger(_logger, _helper, _archipelago);
            _outfitChanger = new OutfitChanger(_logger, _helper);
            _activeSounds = new List<ICue>();

            // private SoundBank soundBank;
            var soundBankField = _helper.Reflection.GetField<SoundBank>(Game1.soundBank, "soundBank");
            var soundBank = soundBankField.GetValue();

            // private readonly Dictionary<string, CueDefinition> _cues = new Dictionary<string, CueDefinition>();
            var cuesField = _helper.Reflection.GetField<Dictionary<string, CueDefinition>>(soundBank, "_cues");
            _availableSoundCues = cuesField.GetValue().Keys.ToArray();
        }

        public bool CanGetTrappedRightNow()
        {
            var isSafeLocation = Game1.player.currentLocation is (FarmHouse or IslandFarmHouse);
            var isSleepTime = Game1.player.isInBed.Value || Game1.player.FarmerSprite.isPassingOut() || Game1.player.passedOut;
            var isFestival = Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival;
            var isInFade = Game1.fadeIn || Game1.fadeToBlack || Game1.globalFade || Game1.nonWarpFade;
            var isInMenu = Game1.activeClickableMenu != null || Game1.nextClickableMenu.Any();

            return !isSafeLocation && !isSleepTime && !isFestival && !isInFade && !isInMenu;
        }

        public void PerformTrapManyTimes(int iterations, int delayInSeconds, Func<bool> trapMethod, int delayVariance, Action endAction = null)
        {
            PerformTrapManyTimesAsync(iterations, delayInSeconds, trapMethod, delayVariance, endAction).FireAndForget();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iterations">Number of times to run the trap code</param>
        /// <param name="delayInSeconds">Delay between iterations</param>
        /// <param name="trapMethod">Method to try to run the trap code. If it returns true, then it has run successfully and counts as an iteration. If it returns false, then it has been skipped and should not count.</param>
        /// <returns></returns>
        private async Task PerformTrapManyTimesAsync(int iterations, int delayInSeconds, Func<bool> trapMethod, int delayVariance, Action endAction = null)
        {
            var minDelay = Math.Max(1, delayInSeconds - delayVariance);
            var maxDelay = Math.Max(1, delayInSeconds + delayVariance);
            var delayRange = maxDelay - minDelay;
            while (iterations > 0)
            {
                if (trapMethod())
                {
                    iterations--;
                }

                var delay = Game1.random.NextDouble() * delayRange + minDelay;
                await Task.Run(() => Thread.Sleep((int)(delay * 1000)));
            }

            var endDelay = (Game1.random.NextDouble() * delayRange + minDelay) * 2;
            await Task.Run(() => Thread.Sleep((int)(endDelay * 1000)));
            endAction?.Invoke();
        }

        public void AddBurntDebuff()
        {
            AddDebuff(Buffs.GoblinsCurse);
        }

        public void AddDarknessDebuff()
        {
            AddDebuff(Buffs.Darkness);
        }

        public void AddFrozenDebuff()
        {
            var duration = _difficultyBalancer.FrozenDebuffDurations[_archipelago.SlotData.TrapItemsDifficulty];
            DebuffApplier.AddBuff(Buffs.Frozen, duration);
        }

        public void AddJinxedDebuff()
        {
            AddDebuff(Buffs.EvilEye);
        }

        public void AddNauseatedDebuff()
        {
            AddDebuff(Buffs.Nauseous);
        }

        public void AddSlimedDebuff()
        {
            AddDebuff(Buffs.Slimed);
        }

        public void AddWeaknessDebuff()
        {
            AddDebuff(Buffs.Weakness);
        }

        private void AddDebuff(Buffs whichBuff)
        {
            var duration = _difficultyBalancer.DefaultDebuffDurations[_archipelago.SlotData.TrapItemsDifficulty];
            DebuffApplier.AddBuff(whichBuff, duration);
        }

        public void CreateDebris()
        {
            var amountOfDebris = _difficultyBalancer.AmountOfDebris[_archipelago.SlotData.TrapItemsDifficulty];
            DebrisSpawner.CreateDebris(amountOfDebris);
        }

        public void GrowTrees()
        {
            var amountOfTrees = _difficultyBalancer.AmountOfTrees[_archipelago.SlotData.TrapItemsDifficulty];
            DebrisSpawner.CreateTrees(amountOfTrees);
        }

        public void TeleportRandomly()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var destination = _difficultyBalancer.TeleportDestinations[difficulty];
            var numberOfTeleports = _difficultyBalancer.NumberOfTeleports[difficulty];
            TeleportRandomly(destination, numberOfTeleports);
        }

        public void TeleportRandomly(TeleportDestination destination, int numberOfTeleports = 1)
        {
            var validMaps = new List<GameLocation>();
            switch (destination)
            {
                case TeleportDestination.None:
                    return;
                case TeleportDestination.Nearby:
                case TeleportDestination.SameMap:
                    validMaps.Add(Game1.player.currentLocation);
                    break;
                case TeleportDestination.SameMapOrHome:
                    validMaps.Add(Game1.getFarm());
                    validMaps.Add(Game1.getLocationFromName("FarmHouse"));
                    if (!Game1.player.currentLocation.Name.Contains("Farm"))
                    {
                        validMaps.Add(Game1.player.currentLocation);
                    }

                    break;
                case TeleportDestination.PelicanTown:
                    validMaps.AddRange(Game1.locations.Where(x => x is not IslandLocation));
                    break;
                case TeleportDestination.Anywhere:
                    validMaps.AddRange(Game1.locations);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TeleportRandomly(validMaps, destination, numberOfTeleports);
        }

        private void TeleportRandomly(List<GameLocation> validMaps, TeleportDestination destination, int numberOfTeleports)
        {
            if (numberOfTeleports <= 0)
            {
                return;
            }

            PerformTrapManyTimes(numberOfTeleports, 4, () => TeleportRandomly(validMaps, destination), 1);
        }

        private bool TeleportRandomly(List<GameLocation> validMaps, TeleportDestination destination)
        {
            if (!CanGetTrappedRightNow())
            {
                return false;
            }

            GameLocation chosenLocation = null;
            Vector2? chosenTile = null;
            while (chosenLocation == null || chosenTile == null)
            {
                chosenLocation = validMaps[Game1.random.Next(validMaps.Count)];
                if (destination == TeleportDestination.Nearby)
                {
                    chosenTile = TileChooser.GetRandomTileInbounds(chosenLocation, Game1.player.TilePoint, 20);
                }
                else
                {
                    chosenTile = TileChooser.GetRandomTileInbounds(chosenLocation);
                }
            }

            TeleportFarmerTo(chosenLocation.Name, chosenTile.Value);
            return true;
        }

        private void TeleportFarmerTo(string locationName, Vector2 tile)
        {
            _logger.LogInfo($"Teleporting the player to {locationName} [{tile}]");
            var farmer = Game1.player;
            var multiplayerField = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
            var multiplayer = multiplayerField.GetValue();
            for (var index = 0; index < 12; ++index)
            {
                multiplayer.broadcastSprites(farmer.currentLocation,
                    new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1,
                        new Vector2(Game1.random.Next((int)farmer.position.X - 256, (int)farmer.position.X + 192),
                            Game1.random.Next((int)farmer.position.Y - 256, (int)farmer.position.Y + 192)), false, Game1.random.NextDouble() < 0.5));
            }

            Game1.currentLocation.playSound("wand");
            Game1.displayFarmer = false;
            farmer.temporarilyInvincible = true;
            farmer.temporaryInvincibilityTimer = -2000;
            farmer.Halt();
            farmer.faceDirection(2);
            farmer.CanMove = false;
            farmer.freezePause = 2000;
            Game1.flashAlpha = 1f;
            DelayedAction.fadeAfterDelay(() => AfterTeleport(farmer, locationName, tile), 1000);
            new Rectangle(farmer.GetBoundingBox().X, farmer.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
            var num = 0;
            for (var x1 = farmer.Tile.X + 8; x1 >= farmer.Tile.X - 8; --x1)
            {
                multiplayer.broadcastSprites(farmer.currentLocation,
                    new TemporaryAnimatedSprite(6, new Vector2(x1, farmer.Tile.Y) * 64f, Color.White, animationInterval: 50f)
                    {
                        layerDepth = 1f,
                        delayBeforeAnimationStart = num * 25,
                        motion = new Vector2(-0.25f, 0.0f),
                    });
                ++num;
            }
        }

        private void AfterTeleport(Farmer farmer, string locationName, Vector2 tile)
        {
            var destination = Utility.Vector2ToPoint(tile);
            Game1.warpFarmer(locationName, destination.X, destination.Y, false);
            Game1.changeMusicTrack("none");
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            farmer.temporarilyInvincible = false;
            farmer.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
            farmer.CanMove = true;
        }

        public void ChargeTaxes()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var taxRate = _difficultyBalancer.TaxRates[difficulty];
            var player = Game1.player;
            var currentMoney = player.Money;
            var tax = (int)(currentMoney * taxRate);
            var maxSpend = (int)Math.Round(currentMoney * 0.9);
            if (tax > maxSpend)
            {
                var debt = tax - maxSpend;
                tax = maxSpend;
                _permanentState.CurrentDebt += debt;
            }
            Game1.player.addUnearnedMoney(tax * -1);
            if (difficulty >= TrapItemsDifficulty.Nightmare)
            {
                RemoveTaxTrapFromBankAsync(difficulty).FireAndForget();
            }
        }

        public async Task RemoveTaxTrapFromBankAsync(TrapItemsDifficulty difficulty)
        {
            var bankingKey = string.Format(BankHandler.BANKING_TEAM_KEY, _archipelago.GetTeam());
            var currentAmountJoules = await _archipelago.ReadBigIntegerFromDataStorageAsync(Scope.Global, bankingKey);
            if (currentAmountJoules == null || currentAmountJoules <= 0)
            {
                return;
            }

            var divisor = difficulty == TrapItemsDifficulty.Eldritch ? 10 : 2;
            _archipelago.DivideBigIntegerDataStorage(Scope.Global, bankingKey, divisor);
        }

        public void DayUpdateDebt()
        {
            _debtManager.DayUpdateDebt().FireAndForget();
        }

        public void SendCrows()
        {
            var crowRate = _difficultyBalancer.CrowAttackRate[_archipelago.SlotData.TrapItemsDifficulty];
            var crowTargets = _difficultyBalancer.CrowValidTargets[_archipelago.SlotData.TrapItemsDifficulty];
            if (crowTargets == CrowTargets.None)
            {
                return;
            }

            var scarecrowEfficiency = _difficultyBalancer.ScarecrowEfficiency[_archipelago.SlotData.TrapItemsDifficulty];

            if (crowTargets == CrowTargets.Farm)
            {
                var farm = Game1.getFarm();
                SendCrowsForLocation(farm, crowRate, scarecrowEfficiency);
                return;
            }

            if (crowTargets == CrowTargets.Outside)
            {
                foreach (var gameLocation in Game1.locations)
                {
                    if (!gameLocation.IsOutdoors)
                    {
                        continue;
                    }

                    SendCrowsForLocation(gameLocation, crowRate, scarecrowEfficiency);
                }
                return;
            }

            foreach (var gameLocation in Game1.locations)
            {
                SendCrowsForLocation(gameLocation, crowRate, scarecrowEfficiency);
            }
        }

        private static void SendCrowsForLocation(GameLocation map, double crowRate, double scarecrowEfficiency)
        {
            var scarecrowPositions = GetScarecrowPositions(map);
            var crops = GetAllCrops(map);
            map.critters ??= new List<Critter>();
            foreach (var (cropPosition, crop) in crops)
            {
                var roll = Game1.random.NextDouble();
                if (roll > crowRate)
                {
                    continue;
                }

                if (IsCropDefended(map, scarecrowPositions, cropPosition, scarecrowEfficiency))
                {
                    continue;
                }

                crop.destroyCrop(true);
                map.critters.Add(new Crow((int)crop.Tile.X, (int)crop.Tile.Y));
            }
        }

        private static bool IsCropDefended(GameLocation map, List<Vector2> scarecrowPositions, Vector2 cropPosition, double scarecrowEfficiency)
        {
            var defendingScarecrows = GetCropDefense(map, scarecrowPositions, cropPosition);
            for (var i = 0; i < defendingScarecrows; i++)
            {
                if (Game1.random.NextDouble() < scarecrowEfficiency)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<Vector2> GetScarecrowPositions(GameLocation farm)
        {
            var scarecrowPositions = new List<Vector2>();
            foreach (var (position, placedObject) in farm.objects.Pairs)
            {
                if (placedObject.bigCraftable.Value && placedObject.IsScarecrow())
                {
                    scarecrowPositions.Add(position);
                }
            }

            return scarecrowPositions;
        }

        private static IEnumerable<KeyValuePair<Vector2, HoeDirt>> GetAllCrops(GameLocation location)
        {
            foreach (var (cropPosition, cropTile) in location.terrainFeatures.Pairs)
            {
                if (cropTile is not HoeDirt dirt || dirt.crop == null || dirt.crop.currentPhase.Value <= 1)
                {
                    continue;
                }

                yield return new KeyValuePair<Vector2, HoeDirt>(cropPosition, dirt);
            }

            foreach (var (cropPosition, gameObject) in location.Objects.Pairs)
            {
                if (gameObject is not IndoorPot gardenPot || gardenPot.hoeDirt.Value.crop == null || gardenPot.hoeDirt.Value.crop.currentPhase.Value <= 1)
                {
                    continue;
                }

                yield return new KeyValuePair<Vector2, HoeDirt>(cropPosition, gardenPot.hoeDirt.Value);
            }
        }

        private static int GetCropDefense(GameLocation farm, List<Vector2> scarecrowPositions, Vector2 cropPosition)
        {
            var numberOfDefendingScarecrows = 0;
            foreach (var scarecrowPosition in scarecrowPositions)
            {
                var radiusForScarecrow = farm.objects[scarecrowPosition].GetRadiusForScarecrow();
                if (Vector2.Distance(scarecrowPosition, cropPosition) < radiusForScarecrow)
                {
                    numberOfDefendingScarecrows++;
                }
            }

            return numberOfDefendingScarecrows;
        }

        public void SpawnMonsters()
        {
            var numberMonsters = _difficultyBalancer.NumberOfMonsters[_archipelago.SlotData.TrapItemsDifficulty];
            for (var i = 0; i < numberMonsters; i++)
            {
                MonsterSpawner.SpawnOneMonster(Game1.player.currentLocation, _archipelago.SlotData.TrapItemsDifficulty);
            }
        }

        public void SpawnSuperMonsters()
        {
            var numberMonsters = _difficultyBalancer.NumberOfSuperMonsters[_archipelago.SlotData.TrapItemsDifficulty];
            var monsterPower = _difficultyBalancer.SuperMonsterStrength[_archipelago.SlotData.TrapItemsDifficulty];
            for (var i = 0; i < numberMonsters; i++)
            {
                MonsterSpawner.SpawnOneBoostedMonster(Game1.player.currentLocation, _archipelago.SlotData.TrapItemsDifficulty, monsterPower);
            }
        }

        public void ShuffleInventory()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var targets = _difficultyBalancer.ShuffleTargets[difficulty];
            var rate = _difficultyBalancer.ShuffleRate[difficulty];
            var rateToFriends = _difficultyBalancer.ShuffleRateToFriends[difficulty];
            InventoryShuffler.ShuffleInventories(targets, rate, rateToFriends);
            var extraSwaps = _difficultyBalancer.ExtraSwapsAfterShuffle[difficulty];
            if (extraSwaps > 0)
            {
                InventoryShuffler.InitiateExtraSwaps(this, extraSwaps);
            }
        }

        public void BecomePariah()
        {
            var player = Game1.player;
            var friendshipLoss = _difficultyBalancer.PariahFriendshipLoss[_archipelago.SlotData.TrapItemsDifficulty];
            var shunningDays = _difficultyBalancer.PariahShunningDays[_archipelago.SlotData.TrapItemsDifficulty];
            foreach (var name in player.friendshipData.Keys)
            {
                var npc = Game1.getCharacterFromName(name) ?? Game1.getCharacterFromName<Child>(name, false);
                if (npc == null)
                {
                    continue;
                }

                ++Game1.stats.GiftsGiven;
                player.currentLocation.localSound("give_gift");
                ++player.friendshipData[name].GiftsToday;
                ++player.friendshipData[name].GiftsThisWeek;
                player.friendshipData[name].LastGiftDate = new WorldDate(Game1.Date);
                Game1.player.changeFriendship(friendshipLoss, npc);
            }
            _permanentState.DaysShunRemaining += shunningDays;
        }

        private static bool _isShunMovement = false;

        // public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        public static bool MovePosition_SkipIfShunningPlayer_Prefix(NPC __instance, GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
        {
            try
            {
                if (ShouldShun(__instance) && !_isShunMovement)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MovePosition_SkipIfShunningPlayer_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void update(GameTime time, GameLocation location)
        public static void Update_ShunPlayer_Postfix(NPC __instance, GameTime time, GameLocation location)
        {
            try
            {
                if (ShouldShun(__instance))
                {
                    ShunPlayer(__instance, time, location);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Update_ShunPlayer_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool ShouldShun(NPC npc)
        {
            if (_permanentState.DaysShunRemaining <= 0)
            {
                return false;
            }

            if (!npc.IsVillager || !Game1.player.friendshipData.ContainsKey(npc.Name))
            {
                return false;
            }

            if (IsInShunDistance(npc))
            {
                return true;
            }

            return false;
        }

        private static bool IsInShunDistance(NPC npc)
        {
            return npc.withinPlayerThreshold(_difficultyBalancer.PariahShunningDistance[_archipelago.SlotData.TrapItemsDifficulty]);
        }

        private static void ShunPlayer(NPC npc, GameTime time, GameLocation location)
        {
            npc.faceDirection(Game1.player.FacingDirection);

            var playerPosition = Game1.player.Position;
            var npcPosition = npc.Position;

            var deltaX = npcPosition.X - playerPosition.X;
            var deltaY = npcPosition.Y - playerPosition.Y;
            var totalDelta = Math.Abs(deltaX) + Math.Abs(deltaY);

            //protected bool moveUp;
            //protected bool moveRight;
            //protected bool moveDown;
            //protected bool moveLeft;

            var moveUpField = _helper.Reflection.GetField<bool>(npc, "moveUp");
            var moveRightField = _helper.Reflection.GetField<bool>(npc, "moveRight");
            var moveDownField = _helper.Reflection.GetField<bool>(npc, "moveDown");
            var moveLeftField = _helper.Reflection.GetField<bool>(npc, "moveLeft");

            moveUpField.SetValue(deltaY < 0 && deltaY < (Math.Abs(deltaX) * -1));
            moveRightField.SetValue(deltaX > 0 && deltaX > (Math.Abs(deltaY)));
            moveDownField.SetValue(deltaY > 0 && deltaY > (Math.Abs(deltaX)));
            moveLeftField.SetValue(deltaX < 0 && deltaX < (Math.Abs(deltaY) * -1));

            var usualSpeed = npc.Speed;

            if (totalDelta < 1.5 * 64)
            {
                npc.Speed = Math.Max((int)Math.Round(usualSpeed * 6.0), (int)Math.Round(Game1.player.Speed * 1.2));
            }
            else if (totalDelta < 2.5 * 64)
            {
                npc.Speed = Math.Max((int)Math.Round(usualSpeed * 4.0), (int)Math.Round(Game1.player.Speed * 1.1));
            }
            else if (totalDelta < 5 * 64)
            {
                npc.Speed = Math.Max((int)Math.Round(usualSpeed * 2.0), (int)Math.Round(Game1.player.Speed * 0.8));
            }
            else if (totalDelta < 12 * 64)
            {
                npc.Speed = Math.Max((int)Math.Round(usualSpeed * 1.5), (int)Math.Round(Game1.player.Speed * 0.6));
            }

            _isShunMovement = true;
            npc.MovePosition(time, Game1.viewport, location);
            _isShunMovement = false;

            npc.Speed = usualSpeed;
        }

        public void PerformDroughtTrap()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var droughtTargets = _difficultyBalancer.DroughtTargets[difficulty];
            var hoeDirts = GetAllHoeDirt(droughtTargets);
            foreach (var hoeDirt in hoeDirts)
            {
                if (hoeDirt.state.Value == 1)
                {
                    hoeDirt.state.Value = 0;
                }
            }

            if (droughtTargets < DroughtTarget.CropsAndWateringCan)
            {
                return;
            }

            foreach (var wateringCan in GetAllWateringCans())
            {
                wateringCan.WaterLeft = 0;
            }

            if (droughtTargets == DroughtTarget.All)
            {
                DryFishPonds(difficulty);
            }
        }

        private static void DryFishPonds(TrapItemsDifficulty difficulty)
        {
            Utility.ForEachBuilding(building =>
            {
                if (building is FishPond fishPond && fishPond.FishCount > 1)
                {
                    if (difficulty >= TrapItemsDifficulty.Eldritch)
                    {
                        fishPond.currentOccupants.Set(1);
                    }
                    else if (difficulty >= TrapItemsDifficulty.Nightmare)
                    {
                        fishPond.currentOccupants.Set(fishPond.FishCount - 1);
                    }

                    fishPond.ClearPond();
                }
                return true;
            });
        }

        private IEnumerable<HoeDirt> GetAllHoeDirt(DroughtTarget validTargets)
        {
            if (validTargets == DroughtTarget.None)
            {
                yield break;
            }

            foreach (var gameLocation in Game1.locations)
            {
                if (!gameLocation.IsOutdoors && validTargets < DroughtTarget.CropsIncludingInside)
                {
                    continue;
                }

                foreach (var terrainFeature in gameLocation.terrainFeatures.Values)
                {
                    if (terrainFeature is not HoeDirt groundDirt)
                    {
                        continue;
                    }

                    if (validTargets == DroughtTarget.Soil && groundDirt.crop != null)
                    {
                        continue;
                    }

                    yield return groundDirt;
                }

                foreach (var (tile, gameObject) in gameLocation.Objects.Pairs)
                {
                    if (gameObject is not IndoorPot gardenPot)
                    {
                        continue;
                    }

                    yield return gardenPot.hoeDirt.Value;
                }
            }
        }

        private IEnumerable<WateringCan> GetAllWateringCans()
        {
            foreach (var item in Game1.player.Items)
            {
                if (item is not WateringCan wateringCan)
                {
                    continue;
                }

                yield return wateringCan;
            }


            foreach (var gameLocation in Game1.locations)
            {
                foreach (var (tile, gameObject) in gameLocation.Objects.Pairs)
                {
                    if (gameObject is not Chest chest)
                    {
                        continue;
                    }

                    foreach (var chestItem in chest.Items)
                    {
                        if (chestItem is not WateringCan wateringCan)
                        {
                            continue;
                        }

                        yield return wateringCan;
                    }
                }
            }
        }

        public void SkipTimeForward()
        {
            var timeToSkip = (int)_difficultyBalancer.TimeFliesDurations[_archipelago.SlotData.TrapItemsDifficulty];
            if (timeToSkip > 120)
            {
                MultiSleepManager.SetDaysToSkip((timeToSkip / 120) - 1);
                Game1.timeOfDay = 2800;
                Game1.player.startToPassOut();
                return;
            }

            for (var i = 0; i < timeToSkip; i++)
            {
                Game1.performTenMinuteClockUpdate();
            }
        }

        public void SpawnTemporaryBabies()
        {
            var numberBabies = _difficultyBalancer.NumberOfBabies[_archipelago.SlotData.TrapItemsDifficulty];
            for (var i = 0; i < numberBabies; i++)
            {
                BabyBirther.SpawnTemporaryBaby(i);
            }
        }

        public void PlayMeows()
        {
            var numberOfMeows = _difficultyBalancer.MeowBarkNumber[_archipelago.SlotData.TrapItemsDifficulty];
            PerformTrapManyTimes(numberOfMeows, 2, () => PlaySound("cat"), 1, StopAllSounds);
        }

        public void PlayBarks()
        {
            var numberOfBarks = _difficultyBalancer.MeowBarkNumber[_archipelago.SlotData.TrapItemsDifficulty];
            PerformTrapManyTimes(numberOfBarks, 2, () => PlaySound("dog_bark"), 1, StopAllSounds);
        }

        public void PlayNoises()
        {
            var numberOfNoises = _difficultyBalancer.NoiseNumber[_archipelago.SlotData.TrapItemsDifficulty];
            PerformTrapManyTimes(numberOfNoises, 2, () => PlaySound("random"), 1, StopAllSounds);
        }

        private string GetRandomSoundCue()
        {
            var soundCue = _availableSoundCues[Game1.random.Next(_availableSoundCues.Length)];
            return soundCue;
        }

        private (int?, int?) GetPitchBounds()
        {
            if (_archipelago.SlotData.TrapItemsDifficulty == TrapItemsDifficulty.Eldritch)
            {
                return (20, 3500);
            }

            return (null, null);
        }

        private bool PlaySound(string sound)
        {
            var (minPitch, maxPitch) = GetPitchBounds();
            return PlaySound(sound, minPitch, maxPitch);
        }

        private bool PlaySound(string sound, int? minPitch, int? maxPitch)
        {
            var soundToPlay = sound == "random" ? GetRandomSoundCue() : sound;
            int? pitch = minPitch == null || maxPitch == null ? null : Game1.random.Next(minPitch.Value, maxPitch.Value + 1);
            if (pitch.HasValue)
            {
                var success = Game1.playSound(soundToPlay, pitch.Value, out var cue);
                _activeSounds.Add(cue);
                return success;
            }
            else
            {
                var success = Game1.playSound(soundToPlay, out var cue);
                _activeSounds.Add(cue);
                return success;
            }
        }

        private void StopAllSounds()
        {
            if (_activeSounds == null)
            {
                return;
            }

            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            if (difficulty < TrapItemsDifficulty.Nightmare)
            {
                var stopOptions = difficulty <= TrapItemsDifficulty.Hard ? AudioStopOptions.Immediate : AudioStopOptions.AsAuthored;
                foreach (var activeSound in _activeSounds)
                {
                    activeSound?.Stop(stopOptions);
                }
            }

            _activeSounds.Clear();
        }

        public void ForceNextMultisleep()
        {
            var daysToSkip = _difficultyBalancer.DepressionTrapDays[_archipelago.SlotData.TrapItemsDifficulty];
            MultiSleepManager.SetDaysToSkip(daysToSkip);
        }

        public void UngrowThings()
        {
            UngrowCrops();
            UngrowFruitTrees();
            UngrowItems();
        }

        private void UngrowCrops()
        {
            var ungrowthDays = _difficultyBalancer.UngrowthDays[_archipelago.SlotData.TrapItemsDifficulty];
            var hoeDirts = GetAllHoeDirt(DroughtTarget.CropsIncludingInside);
            foreach (var hoeDirt in hoeDirts)
            {
                UngrowCrop(hoeDirt.crop, ungrowthDays);
            }
        }

        private void UngrowFruitTrees()
        {
            var treeUngrowthDays = _difficultyBalancer.TreeUngrowthDays[_archipelago.SlotData.TrapItemsDifficulty];
            var fruitTrees = GetAllFruitTrees();
            foreach (var fruitTree in fruitTrees)
            {
                UngrowFruitTree(fruitTree, treeUngrowthDays);
            }
        }

        private void UngrowItems()
        {
            if (_archipelago.SlotData.TrapItemsDifficulty >= TrapItemsDifficulty.Eldritch)
            {
                var cropsData = DataLoader.Crops(Game1.content);
                var cropsToSeedMap = cropsData.ToDictionary(x => QualifiedItemIds.QualifiedObjectId(x.Value.HarvestItemId), x => QualifiedItemIds.QualifiedObjectId(x.Key));
                Utility.ForEachItemContext((in ForEachItemContext x) => UngrowInventoryItem(cropsToSeedMap, in x));
            }
        }

        private bool UngrowInventoryItem(Dictionary<string, string> cropsToSeedMap, in ForEachItemContext context)
        {
            if (context.Item is not Object contextObject)
            {
                return true;
            }

            if (!cropsToSeedMap.ContainsKey(contextObject.QualifiedItemId))
            {
                return true;
            }

            var path = context.GetPath();
            var container = path.Last();

            if (container is OverlaidDictionary)
            {
                return true;
            }

            var qualifiedReplacementId = cropsToSeedMap[contextObject.QualifiedItemId];

            var replacementItem = ItemRegistry.Create<Object>(qualifiedReplacementId);
            if (replacementItem != null)
            {
                replacementItem.Stack = contextObject.Stack;
                replacementItem.Quality = contextObject.Quality;
                replacementItem.HasBeenInInventory = contextObject.HasBeenInInventory;
                foreach (var pair in contextObject.modData.Pairs)
                {
                    replacementItem.modData[pair.Key] = pair.Value;
                }
                context.ReplaceItemWith(replacementItem);
            }
            return true;
        }

        private void UngrowCrop(Crop crop, int days)
        {
            if (crop == null)
            {
                return;
            }

            if (crop.fullyGrown.Value)
            {
                crop.fullyGrown.Set(false);
            }

            var dayOfCurrentPhase = crop.dayOfCurrentPhase.Value;
            var currentPhase = crop.currentPhase.Value;
            var daysPerPhase = crop.phaseDays.ToList();

            if (crop.RegrowsAfterHarvest() && currentPhase >= daysPerPhase.Count - 1)
            {
                var daysSinceLastReady = Math.Max(0, crop.GetData().RegrowDays - dayOfCurrentPhase);
                days = Math.Max(1, days - daysSinceLastReady);
                dayOfCurrentPhase = 0;
            }

            dayOfCurrentPhase -= days;

            while (dayOfCurrentPhase < 0)
            {
                if (currentPhase <= 0 || !daysPerPhase.Any())
                {
                    break;
                }

                if (currentPhase > daysPerPhase.Count)
                {
                    currentPhase = daysPerPhase.Count;
                }

                currentPhase -= 1;
                var daysInCurrentPhase = daysPerPhase[currentPhase];
                dayOfCurrentPhase += daysInCurrentPhase;
            }

            if (dayOfCurrentPhase < 0)
            {
                dayOfCurrentPhase = 0;
            }

            crop.currentPhase.Set(currentPhase);
            crop.dayOfCurrentPhase.Set(dayOfCurrentPhase);
            // private Vector2 tilePosition;
            var tilePositionField = _helper.Reflection.GetField<Vector2>(crop, "tilePosition");
            crop.updateDrawMath(tilePositionField.GetValue());
        }

        private IEnumerable<FruitTree> GetAllFruitTrees()
        {
            foreach (var gameLocation in Game1.locations)
            {
                foreach (var terrainFeature in gameLocation.terrainFeatures.Values)
                {
                    if (terrainFeature is not FruitTree fruitTree)
                    {
                        continue;
                    }

                    yield return fruitTree;
                }
            }
        }

        private void UngrowFruitTree(FruitTree fruitTree, int days)
        {
            fruitTree.daysUntilMature.Value += days;
        }

        public void ActivateInflation()
        {
            Game1.player.RemoveMail("spring_1_2");
            Game1.player.mailForTomorrow.Add("spring_1_2");
        }

        // public override int salePrice()
        public static bool SalePrice_GetCorrectInflation_Prefix(Object __instance, ref int __result)
        {
            try
            {
                if (GetInflatableSalePrice(__instance, out var salePrice))
                {
                    __result = GetInflatedPrice(salePrice);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SalePrice_GetCorrectInflation_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool GetInflatableSalePrice(Object objectToPrice, out int salePrice)
        {

            switch (objectToPrice.ParentSheetIndex)
            {
                case 378:
                    salePrice = 80;
                    return true;
                case 380:
                    salePrice = 150;
                    return true;
                case 382:
                    salePrice = 120;
                    return true;
                case 384:
                    salePrice = 350;
                    return true;
                case 388:
                    salePrice = 10;
                    return true;
                case 390:
                    salePrice = 20;
                    return true;
                default:
                    salePrice = 0;
                    return false;
            }
        }

        private static int GetInflatedPrice(int price)
        {
            var inflationRate = _difficultyBalancer.InflationAmount[_archipelago.SlotData.TrapItemsDifficulty];
            var softcapMultiplier = _difficultyBalancer.InflationSoftcapThreshold[_archipelago.SlotData.TrapItemsDifficulty];
            var receivedCount = _archipelago.GetReceivedItemCount("Inflation Trap");
            var inflationMultiplier = GetInflationMultiplier(inflationRate, softcapMultiplier, receivedCount);
            return (int)(price * inflationMultiplier);
        }

        public static double GetInflationMultiplier(double inflationRate, int softcapThreshold, int trapCount)
        {
            var totalInflation = Math.Pow(inflationRate, trapCount);
            if (double.IsInfinity(totalInflation))
            {
                totalInflation = SoftCapBigInteger(inflationRate, trapCount, softcapThreshold);
            }
            else
            {
                var softCapTier = 2;
                var baseSoftCap = Math.Pow(softcapThreshold, softCapTier - 1);
                while (totalInflation > baseSoftCap)
                {
                    totalInflation = baseSoftCap + Math.Pow(totalInflation - baseSoftCap, 1.0 / softCapTier);
                    baseSoftCap = Math.Pow(softcapThreshold, softCapTier);
                    softCapTier++;
                }
            }

            return totalInflation;
        }

        private static double SoftCapBigInteger(double inflationRate, int trapCount, int softcapMultiplier)
        {
            var totalInflation = BigInteger.Pow((BigInteger)inflationRate, trapCount);
            var softCapTier = 2;
            var baseSoftCap = BigInteger.Pow(softcapMultiplier, softCapTier - 1);
            while (totalInflation > baseSoftCap)
            {
                totalInflation = baseSoftCap + BigIntegerRoot(totalInflation - baseSoftCap, softCapTier);
                baseSoftCap = BigInteger.Pow(softcapMultiplier, softCapTier);
                softCapTier++;
            }

            return (double)totalInflation;
        }

        private static BigInteger BigIntegerSqrt(BigInteger value)
        {
            return BigIntegerRoot(value, 2);
        }

        private static BigInteger BigIntegerCbrt(BigInteger value)
        {
            return BigIntegerRoot(value, 3);
        }

        private static BigInteger BigIntegerRoot(BigInteger value, int rootDegree)
        {
            return new BigInteger(Math.Exp(BigInteger.Log(value) / rootDegree));
        }

        public void Explode()
        {
            var explosionRadius = _difficultyBalancer.ExplosionSize[_archipelago.SlotData.TrapItemsDifficulty];
            BombSpawner.SpawnBomb(explosionRadius);
        }

        public void NudgePlayerItems()
        {
            var baseNudgeChance = _difficultyBalancer.NudgeChance[_archipelago.SlotData.TrapItemsDifficulty];

            Nudger.NudgeObjectsEverywhere(baseNudgeChance);
            Nudger.NudgeBuildings(baseNudgeChance);
        }

        public void Butterfingers()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var targets = _difficultyBalancer.ButterfingersTargets[difficulty];
            var rate = _difficultyBalancer.ButterfingersRate[difficulty];
            var extraDrops = _difficultyBalancer.ButterfingersExtraDrops[difficulty];

            Butterfingers(targets, rate);
            if (extraDrops > 0)
            {
                PerformTrapManyTimes(extraDrops, 10, () => ButterfingersOneRandomItem(), 4);
            }
        }

        private void Butterfingers(ButterfingersTarget targets, double rate)
        {
            switch (targets)
            {
                case ButterfingersTarget.None:
                    return;
                case ButterfingersTarget.ActiveItem:
                    Butterfingers(Game1.player, Game1.player.CurrentToolIndex, rate);
                    return;
                case ButterfingersTarget.Hotbar:
                    for (var i = 0; i < Math.Min(12, Game1.player.MaxItems); i++)
                    {
                        Butterfingers(Game1.player, i, rate);
                    }
                    return;
                case ButterfingersTarget.Inventory:
                    ButterfingersWholeInventory(rate);
                    return;
                case ButterfingersTarget.InventoryAndChestsOnSameMap:
                    ButterfingersWholeInventory(rate);
                    ButterfingerChests(InventoryShuffler.FindAllChests().Values.Where(x => x.Location == Game1.currentLocation), rate);
                    return;
                case ButterfingersTarget.InventoryAndAllChests:
                    ButterfingersWholeInventory(rate);
                    ButterfingerChests(InventoryShuffler.FindAllChests().Values, rate);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targets), targets, null);
            }
        }

        private void ButterfingersWholeInventory(double rate)
        {
            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                Butterfingers(Game1.player, i, rate);
            }
        }

        protected void Butterfingers(Farmer player, int slotToModify, double chance)
        {
            var roll = Game1.random.NextDouble();
            if (roll > chance)
            {
                return;
            }

            var item = player.Items[slotToModify];
            if (item == null || item.Stack <= 0 || !item.canBeDropped())
            {
                return;
            }

            Game1.createItemDebris(item, player.getStandingPosition(), player.FacingDirection, player.currentLocation, flopFish:true);
            player.removeItemFromInventory(item);
        }

        private void ButterfingerChests(IEnumerable<Chest> chests, double rate)
        {
            foreach (var chest in chests)
            {
                ButterfingerChest(chest, rate);
            }
        }

        private void ButterfingerChest(Chest chest, double rate)
        {
            foreach (var item in chest.Items)
            {
                ButterfingerChestItem(chest, item, rate);
            }
        }

        protected void ButterfingerChestItem(Chest chest, Item item, double chance)
        {
            var roll = Game1.random.NextDouble();
            if (roll > chance)
            {
                return;
            }

            if (item == null || item.Stack <= 0)
            {
                return;
            }

            DropChestItem(chest, item);
        }

        public void DropChestItem(Chest chest, Item item)
        {
            if (item == null || !item.canBeDropped())
            {
                return;
            }
            Game1.createItemDebris(item, chest.TileLocation * 64, Game1.random.Next(0, 4), chest.Location, flopFish:true);
            chest.Items.Remove(item);
        }

        private bool ButterfingersOneRandomItem()
        {
            var validSlots = new List<int>();
            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                var item = Game1.player.Items[i];
                if (item != null && item.Stack >= 1 && item.canBeDropped())
                {
                    validSlots.Add(i);
                }
            }

            if (!validSlots.Any())
            {
                return false;
            }

            var chosenSlot = validSlots[Game1.random.Next(validSlots.Count)];
            Butterfingers(Game1.player, chosenSlot, 1.0);
            return true;
        }

        public void SellItems()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var sellCap = _difficultyBalancer.SellNumberCap[difficulty];
            var rate = _difficultyBalancer.SellRate[difficulty];
            var sellPriceMultiplier = _difficultyBalancer.SellMultiplier[difficulty];

            SellItems(rate, sellCap, sellPriceMultiplier);
        }

        private void SellItems(double rate, int sellCap, double sellPriceMultiplier)
        {
            var chests = InventoryShuffler.FindAllChests().Values.OrderBy(x => Game1.random.NextDouble()).ToArray();
            var salesSoFar = 0;

            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                if (TrySellPlayerItem(rate, sellPriceMultiplier, i))
                {
                    salesSoFar++;
                    if (salesSoFar >= sellCap)
                    {
                        return;
                    }
                }
            }

            foreach (var chest in chests)
            {
                foreach (var chestItem in chest.Items)
                {
                    if (TrySellChestItem(rate, sellPriceMultiplier, chest, chestItem))
                    {
                        salesSoFar++;
                        if (salesSoFar >= sellCap)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private static bool TrySellPlayerItem(double rate, double sellPriceMultiplier, int itemIndex)
        {
            var roll = Game1.random.NextDouble();
            if (roll > rate)
            {
                return false;
            }

            var item = Game1.player.Items[itemIndex];
            if (item == null || item.Stack <= 0 || !item.canBeTrashed() || !item.canBeDropped())
            {
                return false;
            }

            SellItem(item, sellPriceMultiplier);
            Game1.player.removeItemFromInventory(item);
            return true;
        }

        private static bool TrySellChestItem(double rate, double sellPriceMultiplier, Chest chest, Item chestItem)
        {
            var roll = Game1.random.NextDouble();
            if (roll > rate)
            {
                return false;
            }

            if (chestItem == null || chestItem.Stack <= 0)
            {
                return false;
            }

            SellItem(chestItem, sellPriceMultiplier);
            chest.Items.Remove(chestItem);
            return true;
        }

        private static void SellItem(Item item, double sellPriceMultiplier)
        {
            var price = (int)Math.Round(item.sellToStorePrice() * item.Stack * sellPriceMultiplier);
            Game1.player.Money += price;
            Game1.playSound("sell");
        }

        public void EncumberPlayer()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var amountOfTrash = _difficultyBalancer.EncumberAmount[difficulty];
            var trashRemaining = amountOfTrash;
            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                if (Game1.player.Items[i] != null)
                {
                    continue;
                }

                GivePlayerOneTrash(i);
                trashRemaining--;
            }

            if (trashRemaining > 0)
            {
                PerformTrapManyTimes(trashRemaining, 10, TryEncumberOneItem, 4);
            }
        }

        private void GivePlayerOneTrash(int inventoryIndex)
        {
            var itemId = GetRandomTrashId();
            var item = ItemRegistry.Create(itemId);
            Game1.player.Items[inventoryIndex] = item;
        }

        private string GetRandomTrashId()
        {
            var trashItems = new[] { "92", "388", "390", "167", "168", "169", "170", "171", "172", "747" };
            var chosenItemId = trashItems[Game1.random.Next(trashItems.Length)];
            return $"(O){chosenItemId}";
        }

        private bool TryEncumberOneItem()
        {
            var validSlots = new List<int>();
            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                if (Game1.player.Items[i] == null)
                {
                    validSlots.Add(i);
                }
            }

            if (validSlots.Any())
            {
                var chosenSlot = validSlots[Game1.random.Next(validSlots.Count)];
                GivePlayerOneTrash(chosenSlot);
                return true;
            }

            return false;
        }

        public void SpoilItems()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var spoilsRemaining = _difficultyBalancer.SpoilsNumber[difficulty];

            var allItemsWithQuality = new List<Item>();
            Utility.ForEachItem(item =>
            {
                if (item.Quality > 0)
                {
                    allItemsWithQuality.Add(item);
                }
                return true;
            });

            allItemsWithQuality = FilterItemsThatCanSpoil(allItemsWithQuality, spoilsRemaining);

            while (spoilsRemaining >= 0 && allItemsWithQuality.Any())
            {
                var itemToSpoil = allItemsWithQuality[Game1.random.Next(allItemsWithQuality.Count)];
                itemToSpoil.Quality -= 1;
                spoilsRemaining -= itemToSpoil.Stack;
                allItemsWithQuality = FilterItemsThatCanSpoil(allItemsWithQuality, spoilsRemaining);
            }
        }

        private List<Item> FilterItemsThatCanSpoil(List<Item> items, int spoilsRemaining)
        {
            return items.Where(x => x.Quality > 0 && x.Stack <= spoilsRemaining).ToList();
        }

        public void SpawnInvisibleCows()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var amount = _difficultyBalancer.NumberOfCows[difficulty];
            CowSpawner.SpawnManyInvisibleCows(amount);
        }

        public void ChangeWeather()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var validWeathers = new List<string> { "Sun", "Rain", "Wind", "Snow" };
            if (difficulty >= TrapItemsDifficulty.Medium)
            {
                validWeathers.Add("Storm");
            }
            if (difficulty >= TrapItemsDifficulty.Hard)
            {
                validWeathers.Add("GreenRain");
            }
            if (difficulty >= TrapItemsDifficulty.Nightmare)
            {
                validWeathers.Add("Festival");
                validWeathers.Add("Wedding");
            }
            var chosenWeather = validWeathers[Game1.random.Next(validWeathers.Count)];

            SetWeather(chosenWeather);
        }

        private void SetWeather(string chosenWeather)
        {
            LightningStrikeOnce();
            Game1.weatherForTomorrow = chosenWeather;
            switch (chosenWeather)
            {
                case "Rain":
                    Game1.isRaining = true;
                    break;
                case "GreenRain":
                    Game1.isGreenRain = true;
                    break;
                case "Storm":
                    Game1.isRaining = true;
                    Game1.isLightning = true;
                    break;
                case "Wind":
                    Game1.isDebrisWeather = true;
                    break;
                case "Snow":
                    Game1.isSnowing = true;
                    break;
            }

            Game1.updateWeather(Game1.currentGameTime);
        }

        public static void LightningStrikeOnce()
        {
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
            Game1.playSound("thunder");
        }

        public void CatchFish()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var numberFish = _difficultyBalancer.NumberOfFish[difficulty];
            PerformTrapManyTimes(numberFish, 10, () =>
            {
                if (CanFishRightNow())
                {
                    StartFishing();
                    return true;
                }

                return false;
            }, 4);
        }

        private bool CanFishRightNow()
        {
            if (!CanGetTrappedRightNow())
            {
                return false;
            }

            if (!Game1.currentLocation.canFishHere() || !AnyWaterOnMap())
            {
                return false;
            }

            var currentFish = Game1.currentLocation.getFish(1, "", 1, Game1.player, 0, Vector2.Zero);
            if (currentFish == null || currentFish.Category != Category.FISH || !DataLoader.Fish(Game1.content).ContainsKey(currentFish.ItemId))
            {
                return false;
            }

            if (!TryFindFishingRod(out _))
            {
                return false;
            }

            return true;
        }

        private void StartFishing()
        {
            if (!TryFindFishingRod(out var rod) || !TryFindNearestWater(out var waterTile))
            {
                return;
            }

            // rod.lastUser = Game1.player;
            // Game1.chatBox.addMessage($"Fishy event trying to catch a {_currentFish.Name}", Color.Yellow);
            // rod.startMinigameEndFunction(_currentFish);
            var waterPixel = new Vector2((int)waterTile.X * 64 + 32, (int)waterTile.Y * 64 + 32);
            rod.bobber.Set(waterPixel);
            rod.DoFunction(Game1.currentLocation, (int)waterPixel.X, (int)waterPixel.Y, 0, Game1.player);
            rod.timeUntilFishingBite = 0;
            rod.isNibbling = true;
            rod.DoFunction(Game1.currentLocation, (int)waterPixel.X, (int)waterPixel.Y, 0, Game1.player);
        }

        private bool AnyWaterOnMap()
        {
            var tiles = Game1.currentLocation.map.Layers[0].Tiles.Array;
            for (var x = 0; x < tiles.GetLength(0); x += 1)
            {
                for (var y = 0; y < tiles.GetLength(1); y += 1)
                {
                    if (Game1.currentLocation.isTileFishable(x, y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryFindNearestWater(out Vector2 waterTile)
        {
            var tiles = Game1.currentLocation.map.Layers[0].Tiles.Array;
            var fishableTiles = new List<Vector2>();
            for (var x = 0; x < tiles.GetLength(0); x += 1)
            {
                for (var y = 0; y < tiles.GetLength(1); y += 1)
                {
                    if (Game1.currentLocation.isTileFishable(x, y))
                    {
                        fishableTiles.Add(new Vector2(x, y));
                    }
                }
            }

            if (!fishableTiles.Any())
            {
                waterTile = Game1.player.Tile;
                return false;
            }

            var sortedTiles = fishableTiles.OrderBy(x => Vector2.Distance(x, Game1.player.Tile)).ToArray();
            waterTile = sortedTiles.First();
            return true;
        }

        private bool PlayerHasFishingRod()
        {
            var rodAnywhere = false;
            Utility.ForEachItem(x =>
            {
                if (x is FishingRod rod)
                {
                    rodAnywhere = true;
                    return false;
                }

                return true;
            });

            return rodAnywhere;
        }

        private bool TryFindFishingRod(out FishingRod rod)
        {
            rod = null;
            for (var i = 0; i < Math.Min(Game1.player.MaxItems, 12); i++)
            {
                var item = Game1.player.Items[i];
                if (item is FishingRod playerRod)
                {
                    rod = playerRod;
                    Game1.player.CurrentToolIndex = i;
                    return true;
                }
            }

            if (rod == null)
            {
                Utility.ForEachItemContext(GiveFishingRodToPlayer);
                if (Game1.player.CurrentTool is FishingRod playerRod)
                {
                    rod = playerRod;
                    return true;
                }
            }

            return false;
        }

        private bool GiveFishingRodToPlayer(in ForEachItemContext context)
        {
            if (context.Item is not FishingRod fishingRod)
            {
                return true;
            }

            var slotToFill = 0;
            for (var i = 0; i < Game1.player.MaxItems; i++)
            {
                var item = Game1.player.Items[i];
                if (item is null)
                {
                    slotToFill = i;
                    break;
                }
            }

            context.ReplaceItemWith(Game1.player.Items[slotToFill]);
            Game1.player.Items[slotToFill] = fishingRod;
            Game1.player.CurrentToolIndex = slotToFill;
            return false;
        }

        public void OpenMenus()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var numberOfMenus = _difficultyBalancer.NumberOfMenus[difficulty];
            PerformTrapManyTimes(numberOfMenus, 3, OpenMenuOnce, 2);
        }

        public bool OpenMenuOnce()
        {
            if (Game1.activeClickableMenu != null)
            {
                return false;
            }

            var chosenMenuType = Game1.random.Next(11);

            Game1.PushUIMode();
            switch (chosenMenuType)
            {
                case 0:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.inventoryTab);
                    break;
                case 1:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.skillsTab);
                    break;
                case 2:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.socialTab);
                    break;
                case 3:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab);
                    break;
                case 4:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.craftingTab);
                    break;
                case 5:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.collectionsTab);
                    break;
                case 6:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.optionsTab);
                    break;
                case 7:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.animalsTab);
                    break;
                case 8:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.powersTab);
                    break;
                case 9:
                    Game1.activeClickableMenu = new GameMenu(GameMenu.exitTab);
                    break;
                case 10:
                    Game1.activeClickableMenu = new QuestLog();
                    break;
            }
            Game1.PopUIMode();
            return true;
        }

        public void PerformEmotes()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var numberOfEmotes = _difficultyBalancer.NumberOfEmotes[difficulty];
            PerformTrapManyTimes(numberOfEmotes, 3, PerformOneEmote, 1);
        }

        public bool PerformOneEmote()
        {
            if (Game1.eventUp || Game1.player.isEmoting || !Game1.player.CanEmote() || Game1.player.isEmoteAnimating)
            {
                return false;
            }

            var emotes = Farmer.EMOTES;
            var emote = emotes[Game1.random.Next(emotes.Length)];
            var emoteName = emote.emoteString;
            Game1.player.performPlayerEmote(emoteName);

            return true;
        }

        public void PerformMakeover()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var makeoverTargets = _difficultyBalancer.MakeoverTargets[difficulty];

            MakeoverOutfit makeoverOutfit = null;
            if (makeoverTargets.HasFlag(MakeoverTargets.FollowTheme))
            {
                _outfitChanger.TryGetMakeoverOutfit(out makeoverOutfit);
            }

            var includeHat = makeoverTargets.HasFlag(MakeoverTargets.Hat);
            var includeShirt = makeoverTargets.HasFlag(MakeoverTargets.Shirt);
            var includePants = makeoverTargets.HasFlag(MakeoverTargets.Pants);

            if (makeoverOutfit != null)
            {
                _outfitChanger.EquipMakeoverOutfit(makeoverOutfit, includeHat, includeShirt, includePants);
            }
            else
            {
                if (includeHat)
                {
                    _outfitChanger.RandomizeHat();
                }
                if (includeShirt)
                {
                    _outfitChanger.RandomizeShirt();
                }
                if (includePants)
                {
                    _outfitChanger.RandomizePants();
                }
            }

            if (makeoverTargets.HasFlag(MakeoverTargets.Hair))
            {
                _outfitChanger.RandomizeHair();
            }

            if (makeoverTargets.HasFlag(MakeoverTargets.Eyes))
            {
                _outfitChanger.RandomizeEyes();
            }

            if (makeoverTargets.HasFlag(MakeoverTargets.Gender))
            {
                _outfitChanger.RandomizeGender();
            }
        }

        public void RandomizeProfessions()
        {
            var hasProfessions = Game1.player.professions.Any() &&
                                 (Game1.player.FarmingLevel >= 5 ||
                                  Game1.player.FishingLevel >= 5 ||
                                  Game1.player.ForagingLevel >= 5 ||
                                  Game1.player.MiningLevel >= 5 ||
                                  Game1.player.CombatLevel >= 5);
            if (!hasProfessions)
            {
                return;
            }

            RandomizeFarmingProfessions();
            RandomizeForagingProfessions();
            RandomizeFishingProfessions();
            RandomizeMiningProfessions();
            RandomizeCombatProfessions();
        }

        private void RandomizeFarmingProfessions()
        {
            //SkillType.Farming = 0;
            var level = Game1.player.FarmingLevel;
            RandomizeSkillProfessions(level, new[] { 0, 1 }, new[] { 2, 3 }, new[] { 4, 5 });
        }

        private void RandomizeForagingProfessions()
        {
            //SkillType.Foraging = 2;
            var level = Game1.player.ForagingLevel;
            RandomizeSkillProfessions(level, new[] { 12, 13 }, new[] { 14, 15 }, new[] { 16, 17 });
        }

        private void RandomizeFishingProfessions()
        {
            //SkillType.Fishing = 1;
            var level = Game1.player.FishingLevel;
            RandomizeSkillProfessions(level, new[] { 6, 7 }, new[] { 8, 9 }, new[] { 10, 11 });
        }

        private void RandomizeMiningProfessions()
        {
            //SkillType.Mining = 3;
            var level = Game1.player.MiningLevel;
            RandomizeSkillProfessions(level, new[] { 18, 19 }, new[] { 20, 21 }, new[] { 22, 23 });
        }

        private void RandomizeCombatProfessions()
        {
            //SkillType.Combat = 4;
            var level = Game1.player.CombatLevel;
            RandomizeSkillProfessions(level, new[] { 24, 25 }, new[] { 26, 27 }, new[] { 28, 29 });
        }

        private static void RandomizeSkillProfessions(int level, int[] level5Professions, int[] level10Professions1, int[] level10Professions2)
        {
            if (level < 5)
            {
                return;
            }

            foreach (var profession in level5Professions)
            {
                Game1.player.professions.Remove(profession);
            }

            var level5Profession = level5Professions[Game1.random.Next(level5Professions.Length)];
            Game1.player.professions.Add(level5Profession);

            if (level < 10)
            {
                return;
            }

            var level10Professions = level5Profession == level5Professions.First() ? level10Professions1 : level10Professions2;

            foreach (var profession in level10Professions)
            {
                Game1.player.professions.Remove(profession);
            }

            var level10Profession = level10Professions[Game1.random.Next(level10Professions.Length)];
            Game1.player.professions.Add(level10Profession);
        }

        public void Tired()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var energyToRemove = _difficultyBalancer.EnergyToRemove[difficulty];
            var energyToRemoveOverTime = 0f;
            if (energyToRemove > Game1.player.Stamina)
            {
                energyToRemoveOverTime = energyToRemove - Game1.player.Stamina;
                energyToRemove = Game1.player.Stamina;
            }
            Game1.player.Stamina -= energyToRemove;
            PerformTrapManyTimes((int)Math.Ceiling(energyToRemoveOverTime), 1, LittleBitTired, 0);
        }

        private bool LittleBitTired()
        {
            if (Game1.player.Stamina > 0)
            {
                Game1.player.Stamina -= 1;
                return true;
            }

            return false;
        }

        public void Injury()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var healthToRemove = _difficultyBalancer.HealthToRemove[difficulty];
            var healthToRemoveOverTime = 0f;
            if (healthToRemove >= Game1.player.health)
            {
                healthToRemoveOverTime = healthToRemove - Game1.player.health - 1;
                healthToRemove = Game1.player.health - 1;
            }
            Game1.player.health -= healthToRemove;
            PerformTrapManyTimes((int)Math.Ceiling(healthToRemoveOverTime), 1, LittleBitInjured, 0);
        }

        private bool LittleBitInjured()
        {
            if (Game1.player.health > 1)
            {
                Game1.player.health -= 1;
                return true;
            }

            return false;
        }

        public void ReverseControls()
        {
            var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
            var millisecondsDuration = _difficultyBalancer.ReversedControlsDuration[difficulty];
            var originalBindings = Game1.options.DeepClone();
            var newBindings = InvertBindings(Game1.options);
            ChangeBindingsForAWhile(originalBindings, newBindings, millisecondsDuration);
        }

        private Options InvertBindings(Options options)
        {
            var invertedOptions = options.DeepClone();

            invertedOptions.actionButton = options.cancelButton;
            invertedOptions.cancelButton = options.actionButton;

            invertedOptions.chatButton = options.emoteButton;
            invertedOptions.emoteButton = options.chatButton;

            invertedOptions.journalButton = options.mapButton;
            invertedOptions.mapButton = options.journalButton;

            invertedOptions.moveDownButton = options.moveUpButton;
            invertedOptions.moveUpButton = options.moveDownButton;

            invertedOptions.moveLeftButton = options.moveRightButton;
            invertedOptions.moveRightButton = options.moveLeftButton;

            invertedOptions.runButton = options.useToolButton;
            invertedOptions.useToolButton = options.runButton;

            invertedOptions.zoomButtons = !options.zoomButtons;
            invertedOptions.alwaysShowToolHitLocation = !options.alwaysShowToolHitLocation;
            invertedOptions.autoRun = !options.autoRun;
            invertedOptions.hideToolHitLocationWhenInMotion = !options.hideToolHitLocationWhenInMotion;

            invertedOptions.inventorySlot1 = options.inventorySlot12;
            invertedOptions.inventorySlot2 = options.inventorySlot11;
            invertedOptions.inventorySlot3 = options.inventorySlot10;
            invertedOptions.inventorySlot4 = options.inventorySlot9;
            invertedOptions.inventorySlot5 = options.inventorySlot8;
            invertedOptions.inventorySlot6 = options.inventorySlot7;
            invertedOptions.inventorySlot7 = options.inventorySlot6;
            invertedOptions.inventorySlot8 = options.inventorySlot5;
            invertedOptions.inventorySlot9 = options.inventorySlot4;
            invertedOptions.inventorySlot10 = options.inventorySlot3;
            invertedOptions.inventorySlot11 = options.inventorySlot2;
            invertedOptions.inventorySlot12 = options.inventorySlot1;

            return invertedOptions;
        }

        public void ChangeBindingsForAWhile(Options originalBindings, Options newBindings, int millisecondsDuration)
        {
            ChangeBindingsForAWhileAsync(originalBindings, newBindings, millisecondsDuration).FireAndForget();
        }

        private async Task ChangeBindingsForAWhileAsync(Options originalBindings, Options newBindings, int millisecondsDuration)
        {
            Game1.options = newBindings;
            await Task.Run(() => Thread.Sleep(millisecondsDuration));
            Game1.options = originalBindings.DeepClone();
        }
    }
}
