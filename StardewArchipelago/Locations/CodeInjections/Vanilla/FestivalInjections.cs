using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Linq;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.CodeInjections;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FestivalInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public static bool AreStoresClosedForFestival()
        public static bool AreStoresClosedForFestival_LeaveStoresOpenWhenER_Prefix(ref bool __result)
        {
            try
            {
                if (_archipelago.SlotData.EntranceRandomization >= EntranceRandomization.Overworld)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AreStoresClosedForFestival_LeaveStoresOpenWhenER_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
