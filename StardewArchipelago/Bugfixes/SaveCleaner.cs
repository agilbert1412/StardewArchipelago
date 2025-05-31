using StardewValley.Monsters;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewModdingAPI.Events;

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
