using StardewValley.Monsters;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewModdingAPI.Events;
using StardewArchipelago.Constants.Vanilla;
using System.Threading;
using xTile;

namespace StardewArchipelago.Bugfixes
{
    public class SaveCleaner
    {
        private ILogger _logger;

        public SaveCleaner(ILogger logger)
        {
            _logger = logger;
        }

        public void OnSaving(object sender, SavingEventArgs e)
        {
            CleanBeforeSaveGame();
        }

        private void CleanBeforeSaveGame()
        {
            RemoveUnserializableMonsters();
            RemoveCorruptedMonsters();
        }

        private void RemoveUnserializableMonsters()
        {
            foreach (var gameLocation in Game1.locations)
            {
                foreach (var character in gameLocation.characters.ToArray())
                {
                    RemoveUnserializableMonster(character, gameLocation);
                }
            }
        }

        private void RemoveCorruptedMonsters()
        {
            const int corruptionThreshold = 200;

            var counter = 0;
            foreach (var gameLocation in Game1.locations)
            {
                var monstersOnMap = gameLocation.characters.OfType<Monster>().ToArray();
                if (monstersOnMap.Length <= corruptionThreshold)
                {
                    continue;
                }
                foreach (var monster in monstersOnMap)
                {
                    gameLocation.characters.Remove(monster);
                    counter++;
                }
            }

            if (counter > 0)
            {
                _logger.LogWarning($"Suspiciously high number of monsters detected attempting to save. Culled {counter} monsters to avoid lag growth");
            }
        }

        private void LogCurrentMonsters()
        {
            var allMonsters = new List<Monster>();
            var monstersByMap = new Dictionary<string, List<Monster>>();
            var monstersByName = new Dictionary<string, List<Monster>>();
            var monstersByMapByName = new Dictionary<string, Dictionary<string, List<Monster>>>();
            foreach (var gameLocation in Game1.locations)
            {
                var mapName = gameLocation.Name;

                monstersByMap.Add(mapName, new List<Monster>());
                monstersByMapByName.Add(mapName, new Dictionary<string, List<Monster>>());

                foreach (var monster in gameLocation.characters.OfType<Monster>().ToArray())
                {
                    allMonsters.Add(monster);
                    var monsterName = monster.Name;
                    monstersByMap[mapName].Add(monster);
                    if (!monstersByName.ContainsKey(monsterName))
                    {
                        monstersByName.Add(monsterName, new List<Monster>());
                    }
                    monstersByName[monsterName].Add(monster);
                    if (!monstersByMapByName[mapName].ContainsKey(monsterName))
                    {
                        monstersByMapByName[mapName].Add(monsterName, new List<Monster>());
                    }
                    monstersByMapByName[mapName][monsterName].Add(monster);
                }
            }

            var totalMonsters = monstersByMap.Sum(x => x.Value.Count);
            var top5Maps = monstersByMap.OrderByDescending(x => x.Value.Count).Take(5).Select(x => x.Key).ToArray();
            var top5Names = monstersByName.OrderByDescending(x => x.Value.Count).Take(8).Select(x => x.Key).ToArray();
            _logger.LogWarning($"Total Number of Monsters: {totalMonsters} across {monstersByMap.Count(x => x.Value.Any())} maps");

            _logger.LogWarning($"Top 5 Maps:");
            foreach (var mapName in top5Maps)
            {
                _logger.LogWarning($"{mapName}: {monstersByMap[mapName].Count} Monsters");
            }
            _logger.LogWarning($"Top 5 Names:");
            foreach (var monsterName in top5Names)
            {
                _logger.LogWarning($"{monsterName}: {monstersByName[monsterName].Count} Monsters");
            }
        }

        private void RemoveUnserializableMonster(NPC character, GameLocation gameLocation)
        {
            if (character is not Monster monster)
            {
                return;
            }
            var typeName = monster.GetType().FullName;
            if (typeName == null || !typeName.EndsWith("FTM"))
            {
                return;
            }

            _logger.LogInfo($"Removing a monster of type '{typeName}' in {gameLocation.Name} to avoid save game troubles");
            gameLocation.characters.Remove(monster);
        }
    }
}
