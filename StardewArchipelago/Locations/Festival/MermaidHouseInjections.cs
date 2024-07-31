using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.Locations.Festival
{
    internal class MermaidHouseInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(PlayClamTone_SongFinished_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
