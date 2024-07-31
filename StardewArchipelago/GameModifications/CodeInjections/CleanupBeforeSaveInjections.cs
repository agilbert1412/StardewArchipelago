using System;
using StardewValley;
using StardewValley.Monsters;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class CleanupBeforeSaveInjections
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public virtual void cleanupBeforeSave()
        public static void CleanupBeforeSave_RemoveIllegalMonsters_Postfix(GameLocation __instance)
        {
            try
            {
                for (var i = __instance.characters.Count - 1; i >= 0; --i)
                {
                    if (__instance.characters[i] is Bat || __instance.characters[i] is Serpent || __instance.characters[i] is ShadowBrute || __instance.characters[i] is RockGolem)
                    {
                        __instance.characters.RemoveAt(i);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CleanupBeforeSave_RemoveIllegalMonsters_Postfix)} ({__instance?.GetType()} version):\n{ex}");
                return;
            }
        }
    }
}
