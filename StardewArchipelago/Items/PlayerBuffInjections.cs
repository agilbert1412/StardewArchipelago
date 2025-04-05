using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewArchipelago.Items
{
    public class PlayerBuffInjections
    {
        private const int STAMINA_AMOUNT = 12;

        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;

        private static int _numberOfSpeedBonuses = 0;
        private static int _numberOfLuckBonuses = 0;
        private static int _numberOfDamageBonuses = 0;
        private static int _numberOfDefenseBonuses = 0;
        private static int _numberOfImmunityBonuses = 0;
        // private static int _numberOfHealthBonuses = 0; // Handled as an unlock
        private static int _numberOfStaminaBonuses = 0;
        private static int _numberOfBiteRateBonuses = 0;
        private static int _numberOfFishTrapBonuses = 0;
        private static int _numberOfFishingBarBonuses = 0;
        // private static int _numberOfQualityBonuses = 0; // I might implement this someday
        // private static int _numberOfGlowBonuses = 0; // I might implement this someday

        public static int CurrentStaminaBonus => STAMINA_AMOUNT * _numberOfStaminaBonuses;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
        }

        public static void CheckForApBuffs()
        {
            _numberOfSpeedBonuses = GetNumberOfBuff(APItem.MOVEMENT_SPEED_BONUS);
            _numberOfLuckBonuses = GetNumberOfBuff(APItem.LUCK_BONUS, 24); // If the daily luck reaches 0.75, weird shit starts to happen. 24 buffs is 0.6, plus the max daily of 0.1 and the trinket of 0.025 makes 0.725
            _numberOfDamageBonuses = GetNumberOfBuff(APItem.DAMAGE_BONUS);
            _numberOfDefenseBonuses = GetNumberOfBuff(APItem.DEFENSE_BONUS);
            _numberOfImmunityBonuses = GetNumberOfBuff(APItem.IMMUNITY_BONUS);
            _numberOfStaminaBonuses = GetNumberOfBuff(APItem.ENERGY_BONUS);
            _numberOfBiteRateBonuses = GetNumberOfBuff(APItem.BITE_RATE_BONUS);
            _numberOfFishTrapBonuses = GetNumberOfBuff(APItem.FISH_TRAP_BONUS);
            _numberOfFishingBarBonuses = GetNumberOfBuff(APItem.FISHING_BAR_SIZE_BONUS, 65); // If you max out every possible fishing buff from every source, and add 65 of these, it makes the bar take exactly the whole height of the minigame
            // _numberOfQualityBonuses = GetNumberOfBuff(APItem.QUALITY_BONUS);
            // _numberOfGlowBonuses = GetNumberOfBuff(APItem.GLOW_BONUS);
        }

        private static int GetNumberOfBuff(string buffName, int maximum = int.MaxValue)
        {
            var numberReceived = _archipelago.GetReceivedItemCount(buffName);
            return Math.Min(numberReceived, maximum);
        }

        public static void GetMovementSpeed_AddApBuffs_Postfix(Farmer __instance, ref float __result)
        {
            try
            {
                if (Game1.eventUp && Game1.CurrentEvent is { isFestival: false })
                {
                    return;
                }

                if (Game1.eventUp && Game1.CurrentEvent != null && Game1.currentLocation is DesertFestival desertFestival)
                {
                    var makeoverEvent = desertFestival.GetMakeoverEvent();
                    var makeoverEventCommands = Event.ParseCommands(makeoverEvent, null);
                    if (makeoverEventCommands.SequenceEqual(Game1.CurrentEvent.eventCommands))
                    {
                        return;
                    }
                }

                var baseCoefficient = 1.0f;
                var configValue = ModEntry.Instance.Config.BonusPerMovementSpeed;
                var valuePerMovementSpeed = 0.05f * configValue;
                var totalCoefficient = baseCoefficient + (valuePerMovementSpeed * _numberOfSpeedBonuses);

                __result *= totalCoefficient;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetMovementSpeed_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void DailyLuck_AddApBuffs_Postfix(Farmer __instance, ref double __result)
        {
            try
            {
                var totalBonus = 0.025f * _numberOfLuckBonuses;

                __result += totalBonus;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DailyLuck_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        // public float AttackMultiplier => this.GetValues().AttackMultiplier.Value;
        public static void GetAttackMultiplier_AddApBuffs_Postfix(BuffManager __instance, ref float __result)
        {
            try
            {
                var baseCoefficient = 1.0f;
                var totalCoefficient = baseCoefficient + (0.1f * _numberOfDamageBonuses);

                __result *= totalCoefficient;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetAttackMultiplier_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        // public int Defense => (int) this.GetValues().Defense.Value;
        public static void GetDefense_AddApBuffs_Postfix(BuffManager __instance, ref int __result)
        {
            try
            {
                var bonusDefense = (1 * _numberOfDefenseBonuses);
                __result += bonusDefense;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetDefense_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        // public int Immunity => (int) this.GetValues().Immunity.Value;
        public static void GetImmunity_AddApBuffs_Postfix(BuffManager __instance, ref int __result)
        {
            try
            {
                var bonusImmunity = (1 * _numberOfImmunityBonuses);
                __result += bonusImmunity;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetImmunity_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        // public int MaxStamina => (int)this.GetValues().MaxStamina.Value;
        public static void GetMaxStamina_AddApBuffs_Postfix(BuffManager __instance, ref int __result)
        {
            try
            {
                var bonusStamina = CurrentStaminaBonus;
                __result += bonusStamina;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetMaxStamina_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        // private float calculateTimeUntilFishingBite(Vector2 bobberTile, bool isFirstCast, Farmer who)
        public static void CalculateTimeUntilFishingBite_AddApBuffs_Postfix(FishingRod __instance, Vector2 bobberTile, bool isFirstCast, Farmer who, ref float __result)
        {
            try
            {
                var biteTimeReduction = (1000 * _numberOfBiteRateBonuses);
                __result = Math.Max(300f, __result - biteTimeReduction);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CalculateTimeUntilFishingBite_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        // public BobberBar(string whichFish, float fishSize, bool treasure, List<string> bobbers, string setFlagOnCatch, bool isBossFish, string baitID = "", bool goldenTreasure = false)
        public static void BobberBarConstructor_AddApBuffs_Postfix(BobberBar __instance, string whichFish, float fishSize, bool treasure,
            List<string> bobbers, string setFlagOnCatch, bool isBossFish, string baitID, bool goldenTreasure)
        {
            try
            {
                AddTrapBuff(__instance);
                AddFishingBarBuff(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BobberBarConstructor_AddApBuffs_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void AddTrapBuff(BobberBar bar)
        {
            var trapMultiplier = (float)Math.Pow(0.8f, _numberOfFishTrapBonuses);
            bar.distanceFromCatchPenaltyModifier *= trapMultiplier;
        }

        private static void AddFishingBarBuff(BobberBar bar)
        {
            var fishingBarSizeIncrease = (4 * _numberOfFishingBarBonuses);
            bar.bobberBarHeight += fishingBarSizeIncrease;
            bar.bobberBarPos = (568 - bar.bobberBarHeight);
        }
    }
}
