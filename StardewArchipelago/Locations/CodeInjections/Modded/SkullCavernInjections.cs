using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public static class SkullCavernInjections
    {
        public const string SKULL_CAVERN_ELEVATOR_ITEM = "Progressive Skull Cavern Elevator";
        public const string SKULL_CAVERN_FLOOR_LOCATION = "Skull Cavern: Floor {0}";

        public const int ELEVATOR_STEP = 25;


        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public static void enterMine(int whatLevel)
        public static void EnterMine_SendSkullCavernElevatorCheck_PostFix(int whatLevel)
        {
            try
            {
                var realSkullCavernLevel = whatLevel - 120;
                if (whatLevel < 121 || realSkullCavernLevel % ELEVATOR_STEP != 0)
                {
                    return;
                }

                var progression = _archipelago.SlotData.ElevatorProgression;
                var currentMineshaft = Game1.player.currentLocation as MineShaft;
                var currentMineLevel = currentMineshaft?.mineLevel ?? 0;
                if (progression == ElevatorProgression.ProgressiveFromPreviousFloor && currentMineLevel != whatLevel - 1)
                {
                    return;
                }

                var locationToCheck = string.Format(SKULL_CAVERN_FLOOR_LOCATION, realSkullCavernLevel);
                _locationChecker.AddCheckedLocation(locationToCheck);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(EnterMine_SendSkullCavernElevatorCheck_PostFix)}:\n{ex}");
                return;
            }
        }
    }
}
