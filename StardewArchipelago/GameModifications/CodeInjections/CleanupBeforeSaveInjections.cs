using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class CleanupBeforeSaveInjections
    {
        private static IMonitor _monitor;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
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
                _monitor.Log($"Failed in {nameof(CleanupBeforeSave_RemoveIllegalMonsters_Postfix)} ({__instance?.GetType()} version):\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
