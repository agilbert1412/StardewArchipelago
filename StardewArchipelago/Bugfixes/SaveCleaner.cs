using StardewValley.Monsters;
using StardewValley;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.InGameLocations;
using StardewModdingAPI.Events;
using StardewValley.Internal;

namespace StardewArchipelago.Bugfixes
{
    public class SaveCleaner
    {
        private ILogger _logger;
        private ILocationChecker _locationChecker;

        public SaveCleaner(ILogger logger, ILocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }

        public void OnSaving(object sender, SavingEventArgs e)
        {
            CleanBeforeSaveGame();
        }

        private void CleanBeforeSaveGame()
        {
            RemoveUnserializableMonsters();
            RemoveUnserializableItems();
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

        private void RemoveUnserializableItems()
        {
            Utility.ForEachItemContext(RemoveUnserializable);
        }

        private bool RemoveUnserializable(in ForEachItemContext context)
        {
            if (context.Item is ObtainableArchipelagoLocation obtainableArchipelagoLocation)
            {
                var path = context.GetDisplayPath();
                var pathString = string.Join(" -> ", path);
                _logger.LogWarning($"The game attempted to save with an AP check for location '{obtainableArchipelagoLocation.LocationName}' at [{pathString}]. Archipelago will remove the item and send the location.");
                _locationChecker.AddCheckedLocation(obtainableArchipelagoLocation.LocationName);
                context.RemoveItem();
            }

            return true;
        }
    }
}
