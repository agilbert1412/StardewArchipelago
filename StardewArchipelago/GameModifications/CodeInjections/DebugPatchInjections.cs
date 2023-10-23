using StardewArchipelago.Archipelago;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class DebugPatchInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }
    }
}
