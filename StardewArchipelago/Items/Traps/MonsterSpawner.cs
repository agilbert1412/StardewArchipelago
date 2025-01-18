using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Monsters;

namespace StardewArchipelago.Items.Traps
{
    public class MonsterSpawner
    {
        private readonly TileChooser _tileChooser;

        public MonsterSpawner(TileChooser tileChooser)
        {
            _tileChooser = tileChooser;
        }

        private readonly string[] _easyMonsterTypes =
        {
            "Bat", "Frost Bat", "Rock Golem", "Green Slime", "Frost Jelly",
        };

        private readonly string[] _mediumMonsterTypes =
        {
            "Lava Bat", "Shadow Brute", "Sludge", "Purple Slime",
        };

        private readonly string[] _hardMonsterTypes =
        {
            "Iridium Bat", "Serpent", "Tiger Slime",
        };

        public void SpawnOneMonster(GameLocation map, TrapItemsDifficulty trapDifficulty)
        {
            var monster = ChooseRandomMonster(map, trapDifficulty);
            monster.focusedOnFarmers = true;
            monster.wildernessFarmMonster = true;
            map.characters.Add(monster);
        }

        private Monster ChooseRandomMonster(GameLocation map, TrapItemsDifficulty trapDifficulty)
        {
            var spawnPosition = _tileChooser.GetRandomTileInboundsOffScreen(map);

            var monsters = new List<string>(_easyMonsterTypes);
            if (trapDifficulty >= TrapItemsDifficulty.Hard)
            {
                monsters.AddRange(_mediumMonsterTypes);
            }
            if (trapDifficulty >= TrapItemsDifficulty.Hell)
            {
                monsters.AddRange(_hardMonsterTypes);
            }

            var chosenMonsterType = monsters[Game1.random.Next(0, monsters.Count)];
            switch (chosenMonsterType)
            {
                case "Bat":
                    return new Bat(spawnPosition * 64f, 1);
                case "Frost Bat":
                    return new Bat(spawnPosition * 64f, 41);
                case "Lava Bat":
                    return new Bat(spawnPosition * 64f, 81);
                case "Iridium Bat":
                    return new Bat(spawnPosition * 64f, 172);
                case "Serpent":
                    return new Serpent(spawnPosition * 64f);
                case "Shadow Brute":
                    return new ShadowBrute(spawnPosition * 64f);
                case "Rock Golem":
                    return new RockGolem(spawnPosition * 64f, Game1.player.CombatLevel);
                case "Green Slime":
                    return new GreenSlime(spawnPosition * 64f, 1);
                case "Frost Jelly":
                    return new GreenSlime(spawnPosition * 64f, 41);
                case "Purple Slime":
                    return new GreenSlime(spawnPosition * 64f, 121);
                case "Sludge":
                    return new GreenSlime(spawnPosition * 64f, 77377);
                case "Tiger Slime":
                    var slime = new GreenSlime(spawnPosition * 64f, 0);
                    slime.makeTigerSlime();
                    return slime;
                default:
                    throw new Exception($"Failed at spawning a monster of type {chosenMonsterType}");
            }
        }
    }
}
