using System;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Locations.Festival
{
    internal class MermaidHouseInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void playClamTone(int which, Farmer who)
        public static void PlayClamTone_SongFinished_Postfix(MermaidHouse __instance, int which, Farmer who)
        {
            try
            {
                var pearlRecipientField = _modHelper.Reflection.GetField<Farmer>(__instance, "pearlRecipient");
                var pearlRecipient = pearlRecipientField.GetValue();
                if (pearlRecipient == null || pearlRecipient != Game1.player)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.MERMAID_PEARL);
                pearlRecipientField.SetValue(null);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PlayClamTone_SongFinished_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
