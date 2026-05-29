using Archipelago.MultiClient.Net.Enums;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.MultiSleep;
using StardewArchipelago.Items.Traps.Shuffle;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Network.ChestHit;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Object = StardewValley.Object;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace StardewArchipelago.Items.Traps
{
    public class TrapExecutor
    {
        private static StardewArchipelagoClient _archipelago;
        private static ILogger _logger;
        private readonly IModHelper _helper;
        private static TrapsStateDto _permanentState;
        public static TrapDifficultyBalancer _difficultyBalancer;

        public readonly BombSpawner BombSpawner;
        public readonly TileChooser TileChooser;
        public readonly MonsterSpawner MonsterSpawner;
        public readonly BabyBirther BabyBirther;
        public readonly DebrisSpawner DebrisSpawner;
        public readonly InventoryShuffler InventoryShuffler;
        public readonly BuffApplier DebuffApplier;
        private readonly string[] _availableSoundCues;

        public TrapExecutor(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, IGiftHandler giftHandler, ArchipelagoStateDto state)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _permanentState = state.TrapsState;
            _difficultyBalancer = new TrapDifficultyBalancer();
            BombSpawner = new BombSpawner(_helper);
            TileChooser = new TileChooser();
            MonsterSpawner = new MonsterSpawner(TileChooser);
            BabyBirther = new BabyBirther();
            DebrisSpawner = new DebrisSpawner(_logger);
            InventoryShuffler = new InventoryShuffler(_logger, giftHandler);
            DebuffApplier = new BuffApplier(_permanentState);

            // private SoundBank soundBank;
            var soundBankField = _helper.Reflection.GetField<SoundBank>(Game1.soundBank, "soundBank");
            var soundBank = soundBankField.GetValue();

            // private readonly Dictionary<string, CueDefinition> _cues = new Dictionary<string, CueDefinition>();
            var cuesField = _helper.Reflection.GetField<Dictionary<string, CueDefinition>>(soundBank, "_cues");
            _availableSoundCues = cuesField.GetValue().Keys.ToArray();
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
            TeleportRandomly(destination, difficulty == TrapItemsDifficulty.Eldritch ? 20 : 1);
        }

        public void TeleportRandomly(TeleportDestination destination, int numberOfTeleports)
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

            if (numberOfTeleports == 1)
            {
                TeleportRandomly(validMaps, destination);
                return;
            }

            TeleportRandomlyAsync(validMaps, destination, numberOfTeleports).FireAndForget();
        }

        private async Task TeleportRandomlyAsync(List<GameLocation> validMaps, TeleportDestination destination, int numberOfTeleports)
        {
            for (var i = 0; i < numberOfTeleports; i++)
            {
                await Task.Run(() => Thread.Sleep(4000));
                TeleportRandomly(validMaps, destination, numberOfTeleports);
            }
        }

        private void TeleportRandomly(List<GameLocation> validMaps, TeleportDestination destination)
        {

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
                InventoryShuffler.InitiateExtraSwaps(extraSwaps);
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
                _permanentState.PariahShunning.TryAdd(name, 0);
                _permanentState.PariahShunning[name] += shunningDays;
            }
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

            if (droughtTargets != DroughtTarget.CropsAndWateringCan)
            {
                return;
            }

            foreach (var wateringCan in GetAllWateringCans())
            {
                wateringCan.WaterLeft = 0;
            }

            if (droughtTargets != DroughtTarget.All)
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
            var (minPitch, maxPitch) = GetPitchBounds();
            PlaySoundsAsync(numberOfMeows, "cat", minPitch, maxPitch).FireAndForget();
        }

        public void PlayBarks()
        {
            var numberOfMeows = _difficultyBalancer.MeowBarkNumber[_archipelago.SlotData.TrapItemsDifficulty];
            var (minPitch, maxPitch) = GetPitchBounds();
            PlaySoundsAsync(numberOfMeows, "dog_bark", minPitch, maxPitch).FireAndForget();
        }

        public void PlayNoises()
        {
            var numberOfMeows = _difficultyBalancer.NoiseNumber[_archipelago.SlotData.TrapItemsDifficulty];
            var (minPitch, maxPitch) = GetPitchBounds();
            PlaySoundsAsync(numberOfMeows, "random", minPitch, maxPitch).FireAndForget();
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
                return (100, 2300);
            }

            return (null, null);
        }

        private async Task PlaySoundsAsync(int numberOfSounds, string sound, int? minPitch, int? maxPitch)
        {
            for (var i = 0; i < numberOfSounds; i++)
            {
                await Task.Run(() => Thread.Sleep(2000));
                var soundToPlay = sound == "random" ? GetRandomSoundCue() : sound;
                int ? pitch = minPitch == null || maxPitch == null ? null : Game1.random.Next(minPitch.Value, maxPitch.Value + 1);
                Game1.playSound(soundToPlay, pitch);
            }
        }

        public void ForceNextMultisleep()
        {
            var daysToSkip = _difficultyBalancer.DepressionTrapDays[_archipelago.SlotData.TrapItemsDifficulty];
            MultiSleepManager.SetDaysToSkip(daysToSkip);
        }

        public void UngrowCrops()
        {
            var ungrowthDays = _difficultyBalancer.UngrowthDays[_archipelago.SlotData.TrapItemsDifficulty];
            var hoeDirts = GetAllHoeDirt(DroughtTarget.CropsIncludingInside);
            foreach (var hoeDirt in hoeDirts)
            {
                UngrowCrop(hoeDirt.crop, ungrowthDays);
            }

            var treeUngrowthDays = _difficultyBalancer.TreeUngrowthDays[_archipelago.SlotData.TrapItemsDifficulty];
            var fruitTrees = GetAllFruitTrees();
            foreach (var fruitTree in fruitTrees)
            {
                UngrowFruitTree(fruitTree, treeUngrowthDays);
            }
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
            var receivedCount = _archipelago.GetReceivedItemCount("Inflation Trap");
            var inflationMultiplier = GetInflationMultiplier(inflationRate, receivedCount);
            return (int)(price * inflationMultiplier);
        }

        public static double GetInflationMultiplier(double inflationRate, int trapCount)
        {
            var totalInflation = Math.Pow(inflationRate, trapCount);
            if (double.IsInfinity(totalInflation))
            {
                totalInflation = SoftCapBigInteger(inflationRate, trapCount);
            }
            else
            {
                var softCapTier = 2;
                var baseSoftCap = Math.Pow(10, softCapTier - 1);
                while (totalInflation > baseSoftCap)
                {
                    totalInflation = baseSoftCap + Math.Pow(totalInflation - baseSoftCap, 1.0 / softCapTier);
                    baseSoftCap = Math.Pow(10, softCapTier);
                    softCapTier++;
                }
            }

            return totalInflation;
        }

        private static double SoftCapBigInteger(double inflationRate, int trapCount)
        {
            var totalInflation = BigInteger.Pow((BigInteger)inflationRate, trapCount);
            var softCapTier = 2;
            var baseSoftCap = BigInteger.Pow(10, softCapTier - 1);
            while (totalInflation > baseSoftCap)
            {
                totalInflation = baseSoftCap + BigIntegerRoot(totalInflation - baseSoftCap, softCapTier);
                baseSoftCap = BigInteger.Pow(10, softCapTier);
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
            var allLocations = Game1.locations.ToList();
            allLocations.AddRange(Game1.getFarm().buildings.Where(building => building?.indoors.Value != null).Select(building => building.indoors.Value));

            NudgeObjectsEverywhere(allLocations, baseNudgeChance);
            NudgeBuildings(baseNudgeChance);
        }

        private void NudgeObjectsEverywhere(List<GameLocation> allLocations, double baseNudgeChance)
        {
            foreach (var gameLocation in allLocations)
            {
                NudgeObjectsAtLocation(gameLocation, baseNudgeChance);
            }
        }

        private void NudgeObjectsAtLocation(GameLocation gameLocation, double baseNudgeChance)
        {
            foreach (var gameObject in gameLocation.Objects.Values.ToArray())
            {
                if (gameObject is not Chest chest)
                {
                    continue;
                }

                NudgeChest(chest, baseNudgeChance);
            }
        }

        private void NudgeChest(Chest chest, double baseNudgeChance)
        {
            var seed = (int)Game1.stats.DaysPlayed + (int)(chest.TileLocation.X * 77) + (int)(chest.TileLocation.Y * 1933);
            var random = new Random(seed);
            var chestNudgeChance = baseNudgeChance;
            while (random.NextDouble() < chestNudgeChance)
            {
                chestNudgeChance /= 2;
                var mutex = chest.GetMutex();

                mutex.RequestLock(() =>
                {
                    chest.clearNulls();
                    var chestTileBefore = chest.TileLocation;
                    chest.TryMoveToSafePosition(random.Next(0, 4));

                    // internal readonly ChestHitSynchronizer chestHit;
                    var chestHitField = _helper.Reflection.GetField<ChestHitSynchronizer>(Game1.player.team, "chestHit");
                    var chestHit = chestHitField.GetValue();

                    chestHit.SignalMove(chest.Location, (int)chestTileBefore.X, (int)chestTileBefore.Y, (int)chest.TileLocation.X, (int)chest.TileLocation.Y);
                    mutex.ReleaseLock();
                });
            }
        }

        private void NudgeBuildings(double baseNudgeChance)
        {
            if (_archipelago.SlotData.TrapItemsDifficulty < TrapItemsDifficulty.Hell)
            {
                return;
            }

            var buildingsNudgeChance = baseNudgeChance / 8;
            foreach (var building in Game1.getFarm().buildings)
            {
                // TODO: Nudge buildings because I'm evil
            }
        }
    }
}
