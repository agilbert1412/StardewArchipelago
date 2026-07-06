using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Serialization;
using StardewValley;
using StardewValley.Locations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class NoHouseInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, ArchipelagoStateDto state)
        {
            _logger = logger;
            _archipelago = archipelago;
            _state = state;
        }

        public static void BeHomelessIfNeeded()
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.House))
            {
                _state.HasFoundFarmhouseBed = true;
            }

            if (_state.HasFoundFarmhouseBed)
            {
                return;
            }

            if (Game1.player.currentLocation is FarmHouse)
            {
                var farm = Game1.RequireLocation("Farm") as Farm;
                var farmhouseEntry = farm.GetMainFarmHouseEntry();
                var farmhouseEntryPosition = Utility.PointToVector2(farmhouseEntry) * 64f;
                if (Game1.eventUp && Game1.CurrentEvent != null)
                {
                    Game1.player.locationBeforeForcedEvent.Set("");
                    Game1.CurrentEvent.setExitLocation("Farm", farmhouseEntry.X, farmhouseEntry.Y);
                    EntranceInjections.SkipNextER();
                    return;
                }

                Game1.player.currentLocation = farm;
                Game1.player.Position = farmhouseEntryPosition;
                Game1.currentLocation = Game1.player.currentLocation;
            }
        }

        // public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
        //public static bool AddCharacterIfNecessary_ConsiderArrivals_Prefix(string characterId, ref bool bypassConditions)
        //{
        //    try
        //    {
        //        var allowed = AllowedToExist(characterId);

        //        if (!allowed)
        //        {
        //            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
        //        }

        //        bypassConditions = true;
        //        return MethodPrefix.RUN_ORIGINAL_METHOD;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Failed in {nameof(AddCharacterIfNecessary_ConsiderArrivals_Prefix)}:\n{ex}");
        //        return MethodPrefix.RUN_ORIGINAL_METHOD;
        //    }
        //}
    }
}