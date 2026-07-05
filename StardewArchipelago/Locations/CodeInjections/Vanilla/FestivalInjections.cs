using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using System;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

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

        // public static bool tryToLoadFestival(string festival, out Event ev)
        //public static bool TryToLoadFestival_LeaveStoresOpenWhenER_Prefix(ref bool __result)
        //{
        //    try
        //    {
        //        if (_archipelago.SlotData.EntranceRandomization >= EntranceRandomization.Overworld)
        //        {
        //            __result = false;
        //            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
        //        }

        //        return MethodPrefix.RUN_ORIGINAL_METHOD;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Failed in {nameof(AreStoresClosedForFestival_LeaveStoresOpenWhenER_Prefix)}:\n{ex}");
        //        return MethodPrefix.RUN_ORIGINAL_METHOD;
        //    }
        //}
    }
}
