using System;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class PlayerBuffInjections
    {
        private const string MOVEMENT_SPEED_AP_LOCATION = "Progressive Movement Speed Bonus";
        private const string LUCK_AP_LOCATION = "Progressive Luck Bonus";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
        }

        public static void GetMovementSpeed_AddApBuffs_Postfix(Farmer __instance, ref float __result)
        {
            try
            {
                var baseCoefficient = 1.0f;
                var numberOfSpeedBonus = _archipelago.GetReceivedItemCount(MOVEMENT_SPEED_AP_LOCATION);
                var totalCoefficient = baseCoefficient + (0.25f * numberOfSpeedBonus);

                __result *= totalCoefficient;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetMovementSpeed_AddApBuffs_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void DailyLuck_AddApBuffs_Postfix(Farmer __instance, ref double __result)
        {
            try
            {
                var numberOfLuckBonus = _archipelago.GetReceivedItemCount(LUCK_AP_LOCATION);
                var totalBonus = 0.025f * numberOfLuckBonus;

                __result += totalBonus;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetMovementSpeed_AddApBuffs_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
