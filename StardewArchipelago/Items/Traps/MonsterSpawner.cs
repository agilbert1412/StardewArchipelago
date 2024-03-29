﻿using System;
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

        private readonly string[] _monsterTypes =
        {
            "Bat", "Frost Bat", "Lava Bat", "Iridium Bat", "Serpent", "Shadow Brute", "Rock Golem", "Green Slime",
            "Frost Jelly", "Sludge", "Purple Slime",
        };

        public void SpawnOneMonster(GameLocation map)
        {
            var monster = ChooseRandomMonster(map);
            monster.focusedOnFarmers = true;
            monster.wildernessFarmMonster = true;
            map.characters.Add(monster);
        }

        private Monster ChooseRandomMonster(GameLocation map)
        {
            var spawnPosition = _tileChooser.GetRandomTileInboundsOffScreen(map);

            var chosenMonsterType = _monsterTypes[Game1.random.Next(0, _monsterTypes.Length)];
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
                default:
                    throw new Exception("Could not choose a monster");
            }
        }
    }
}
