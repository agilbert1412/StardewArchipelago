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
        public const string SKULL_KEY = "Skull Key";

        public const int ELEVATOR_STEP = 25;
        private const int DIFFICULTY = 1;


        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static int _realDeepestMineLevel = -1;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public MyElevatorMenu(int elevatorStep, double difficulty, int elevatorCostPerStep)
        public static bool MyElevatorMenuConstructor_SkullCavernElevator_Prefix(MineElevatorMenu __instance, ref int elevatorStep, ref double difficulty, ref int elevatorCostPerStep)
        {
            try
            {
                if (!_archipelago.HasReceivedItem(SKULL_KEY))
                {
                    return true; // Don't bother updating anything until then.
                }
                var receivedElevators = _archipelago.GetReceivedItemCount(SKULL_CAVERN_ELEVATOR_ITEM);
                elevatorStep = ELEVATOR_STEP;
                difficulty = DIFFICULTY;

                if (_realDeepestMineLevel == -1)
                {
                    _realDeepestMineLevel = Game1.player.deepestMineLevel;
                }

                if (receivedElevators >= 8 && Game1.player.deepestMineLevel >= 320)
                {
                    return true; //let the player gain these floors on their own since they've "collected" the floors already
                }

                var elevatorMaxLevel = (receivedElevators * ELEVATOR_STEP) + 120;
                Game1.player.deepestMineLevel = elevatorMaxLevel;

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MyElevatorMenuConstructor_SkullCavernElevator_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public MyElevatorMenu(int elevatorStep, double difficulty, int elevatorCostPerStep)
        public static void MyElevatorMenuConstructor_SkullCavernElevator_Postfix(MineElevatorMenu __instance, int elevatorStep, double difficulty, int elevatorCostPerStep)
        {
            try
            {
                if (_realDeepestMineLevel > -1)
                {
                    Game1.player.deepestMineLevel = _realDeepestMineLevel;
                    _realDeepestMineLevel = -1;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MyElevatorMenuConstructor_SkullCavernElevator_Postfix)}:\n{ex}");
                return;
            }
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
