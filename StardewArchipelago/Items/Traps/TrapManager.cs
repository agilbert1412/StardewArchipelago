using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

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

        private readonly IModHelper _helper;
        private readonly TileChooser _tileChooser;
        private readonly MonsterSpawner _monsterSpawner;
        private Dictionary<string, Action> _traps;

        public TrapManager(IModHelper helper)
        {
            _helper = helper;
            _tileChooser = new TileChooser();
            _monsterSpawner = new MonsterSpawner(_tileChooser);
            _traps = new Dictionary<string, Action>();
            RegisterTraps();
        }

        public bool IsTrap(string unlockName)
        {
            return _traps.ContainsKey(unlockName);
        }

        public LetterAttachment GenerateTrapLetter(ReceivedItem unlock)
        {
            return new LetterTrapAttachment(unlock, unlock.ItemName);
        }

        public bool TryExecuteTrapImmediately(string trapName)
        {
            if (Game1.player.currentLocation is FarmHouse or IslandFarmHouse)
            {
                return false;
            }

            _traps[trapName]();
            return true;
        }

        private void RegisterTraps()
        {
            _traps.Add(BURNT, AddBurntDebuff);
            _traps.Add(DARKNESS, AddDarknessDebuff);
            _traps.Add(FROZEN, AddFrozenDebuff);
            _traps.Add(JINXED, AddJinxedDebuff);
            _traps.Add(NAUSEATED, AddNauseatedDebuff);
            _traps.Add(SLIMED, AddSlimedDebuff);
            _traps.Add(WEAKNESS, AddWeaknessDebuff);
            _traps.Add(TAXES, ChargeTaxes);
            _traps.Add(RANDOM_TELEPORT, TeleportRandomly);
            _traps.Add(CROWS, SendCrows);
            _traps.Add(MONSTERS, SpawnMonsters);
            // _traps.Add(ENTRANCE_RESHUFFLE, );
            _traps.Add(DEBRIS, CreateDebris);
            _traps.Add(SHUFFLE, ShuffleInventory);
            // _traps.Add(WINTER, );
            _traps.Add(PARIAH, SendDislikedGiftToEveryone);
            _traps.Add(DROUGHT, UnwaterAllCrops);

            foreach (var trapName in _traps.Keys.ToArray())
            {
                var differentSpacedTrapName = trapName.Replace(" ", "_");
                if (differentSpacedTrapName != trapName)
                {
                    _traps.Add(differentSpacedTrapName, _traps[trapName]);
                }
            }
        }

        private void AddBurntDebuff()
        {
            AddDebuff(Buffs.GoblinsCurse);
        }

        private void AddDarknessDebuff()
        {
            AddDebuff(Buffs.Darkness);
        }

        private void AddFrozenDebuff()
        {
            AddDebuff(Buffs.Frozen, BuffDuration.OneHour);
        }

        private void AddJinxedDebuff()
        {
            AddDebuff(Buffs.EvilEye);
        }

        private void AddNauseatedDebuff()
        {
            AddDebuff(Buffs.Nauseous);
        }

        private void AddSlimedDebuff()
        {
            AddDebuff(Buffs.Slimed);
        }

        private void AddWeaknessDebuff()
        {
            AddDebuff(Buffs.Weakness);
        }

        private void AddDebuff(Buffs whichBuff, BuffDuration duration = BuffDuration.WholeDay)
        {
            var debuff = new Buff((int)whichBuff);
            debuff.millisecondsDuration = (int)duration;
            debuff.totalMillisecondsDuration = (int)duration;
            Game1.buffsDisplay.addOtherBuff(debuff);
        }

        private void ChargeTaxes()
        {
            const double taxRate = 0.4;
            var player = Game1.player;
            var currentMoney = player.Money;
            var tax = (int)(currentMoney * taxRate);
            Game1.player.addUnearnedMoney(tax * -1);
        }

        private void TeleportRandomly()
        {
            var area = Game1.locations[Game1.random.Next(Game1.locations.Count)];
            var tile = _tileChooser.GetRandomTileInbounds(area);
            TeleportFarmerTo(area.Name, tile);
        }

        private void TeleportFarmerTo(string locationName, Vector2 tile)
        {
            var farmer = Game1.player;
            var multiplayerField = _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
            var multiplayer = multiplayerField.GetValue();
            for (int index = 0; index < 12; ++index)
            {
                multiplayer.broadcastSprites(farmer.currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)farmer.position.X - 256, (int)farmer.position.X + 192), Game1.random.Next((int)farmer.position.Y - 256, (int)farmer.position.Y + 192)), false, Game1.random.NextDouble() < 0.5));
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
            for (var x1 = farmer.getTileX() + 8; x1 >= farmer.getTileX() - 8; --x1)
            {
                multiplayer.broadcastSprites(farmer.currentLocation, new TemporaryAnimatedSprite(6, new Vector2(x1, farmer.getTileY()) * 64f, Color.White, animationInterval: 50f)
                {
                    layerDepth = 1f,
                    delayBeforeAnimationStart = num * 25,
                    motion = new Vector2(-0.25f, 0.0f)
                });
                ++num;
            }
        }

        private void AfterTeleport(Farmer farmer, string locationName, Vector2 tile)
        {
            var destination = Utility.Vector2ToPoint(tile);
            Game1.warpFarmer(locationName, destination.X, destination.Y, false);
            if (!Game1.isStartingToGetDarkOut() && !Game1.isRaining)
                Game1.playMorningSong();
            else
                Game1.changeMusicTrack("none");
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            farmer.temporarilyInvincible = false;
            farmer.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
            farmer.CanMove = true;
        }

        private void SendCrows()
        {
            const double crowRateFarm = 0.5;
            const double crowRateRoof = 0.1;
            const double crowRateIsland = 0.3;
            var farm = Game1.getFarm();
            var greenHouse = Game1.getLocationFromName("Greenhouse");
            var islandSouth = Game1.getLocationFromName("IslandSouth");
            SendCrowsForLocation(farm, crowRateFarm);
            SendCrowsForLocation(greenHouse, crowRateRoof);
            SendCrowsForLocation(islandSouth, crowRateIsland);
        }

        private static void SendCrowsForLocation(GameLocation map, double crowRate)
        {
            var scarecrowPositions = GetScarecrowPositions(map);
            var vulnerableCrops = GetAllVulnerableCrops(map, scarecrowPositions);
            var numberCrowsToSend = vulnerableCrops.Count * crowRate;
            map.critters ??= new List<Critter>();
            for (var index1 = 0; index1 < numberCrowsToSend; ++index1)
            {
                var chosenIndex = Game1.random.Next(vulnerableCrops.Count);
                var cropToEat = vulnerableCrops[chosenIndex];
                vulnerableCrops.RemoveAt(chosenIndex);
                if (cropToEat.crop.currentPhase.Value <= 1)
                {
                    continue;
                }

                cropToEat.destroyCrop(cropToEat.currentTileLocation, true, map);
                map.critters.Add(new Crow((int)cropToEat.currentTileLocation.X, (int)cropToEat.currentTileLocation.Y));
            }
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

        private static List<HoeDirt> GetAllVulnerableCrops(GameLocation farm, List<Vector2> scarecrowPositions)
        {
            var vulnerableCrops = new List<HoeDirt>();
            foreach (var (cropPosition, cropTile) in farm.terrainFeatures.Pairs)
            {
                if (cropTile is not HoeDirt dirt || dirt.crop == null)
                {
                    continue;
                }

                var isVulnerable = IsCropVulnerable(farm, scarecrowPositions, cropPosition);
                if (isVulnerable)
                {
                    vulnerableCrops.Add(dirt);
                }
            }

            return vulnerableCrops;
        }

        private static bool IsCropVulnerable(GameLocation farm, List<Vector2> scarecrowPositions, Vector2 cropPosition)
        {
            foreach (var scarecrowPosition in scarecrowPositions)
            {
                var radiusForScarecrow = farm.objects[scarecrowPosition].GetRadiusForScarecrow();
                if (Vector2.Distance(scarecrowPosition, cropPosition) < radiusForScarecrow)
                {
                    return false;
                }
            }

            return true;
        }

        private void SpawnMonsters()
        {
            const int numberMonsters = 10;
            for (var i = 0; i < numberMonsters; i++)
            {
                _monsterSpawner.SpawnOneMonster(Game1.player.currentLocation);
            }
        }

        private void CreateDebris()
        {
            var farm = Game1.getFarm();
            var currentLocation = Game1.player.currentLocation;
            var locations = new List<GameLocation> { farm };
            if (currentLocation != farm)
            {
                locations.Add(currentLocation);
            }

            const int numberDebrisPerTrap = 100;
            foreach (var gameLocation in locations)
            {
                gameLocation.spawnWeedsAndStones(numberDebrisPerTrap);
            }
        }

        private void ShuffleInventory()
        {
            var inventory = Game1.player.Items.ToList();
            var random = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
            var shuffled = inventory.Shuffle(random);
            Game1.player.Items = shuffled;
        }

        private void SendDislikedGiftToEveryone()
        {
            var player = Game1.player;
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
                Game1.player.changeFriendship(-20, npc);
            }
        }

        private void UnwaterAllCrops()
        {
            var hoeDirts = GetAllHoeDirt();
            foreach (var hoeDirt in hoeDirts)
            {
                if (hoeDirt.state.Value == 1)
                {
                    hoeDirt.state.Value = 0;
                }
            }
        }

        private IEnumerable<HoeDirt> GetAllHoeDirt()
        {
            foreach (var gameLocation in Game1.locations)
            {
                foreach (var terrainFeature in gameLocation.terrainFeatures.Values)
                {
                    if (terrainFeature is not HoeDirt groundDirt)
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
    }
}
