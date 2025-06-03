using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    public class SkillsConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public virtual void newDay(int state)
        public static bool NewDay_ChanceOfNotGrowing_Prefix(Crop __instance, int state)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.FARMING_LEVEL);

                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.1, numberPurchased, __instance.currentLocation.Name.GetHash(), __instance.tilePosition.X, __instance.tilePosition.Y))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(NewDay_ChanceOfNotGrowing_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void dayUpdate()
        public static bool DayUpdate_ChanceToNotGrowTree_Prefix(Tree __instance)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.FORAGING_LEVEL);

                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.15, numberPurchased, __instance.Location.Name.GetHash(), __instance.Tile.X, __instance.Tile.Y))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_ChanceToNotGrowTree_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // private float calculateTimeUntilFishingBite(Vector2 bobberTile, bool isFirstCast, Farmer who)
        public static void CalculateTimeUntilFishingBite_AddTimePerFish_Postfix(FishingRod __instance, Vector2 bobberTile, bool isFirstCast, Farmer who, ref float __result)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.FISHING_LEVEL);

                if (numberPurchased <= 0)
                {
                    return;
                }

                var biteTimeIncrease = (3000 * numberPurchased);
                __result = __result + biteTimeIncrease;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CalculateTimeUntilFishingBite_AddTimePerFish_Postfix)}:\n{ex}");
                return;
            }
        }

        // private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
        public static void AdjustLevelChances_FewerThingsInMine_Postfix(MineShaft __instance, ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
        {
            try
            {
                AdjustMiningChances(ref stoneChance, ref gemStoneChance);
                AdjustCombatChances(ref monsterChance, ref itemChance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AdjustLevelChances_FewerThingsInMine_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void AdjustMiningChances(ref double stoneChance, ref double gemStoneChance)
        {
            var numberMiningPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.MINING_LEVEL);
            if (numberMiningPurchased <= 0)
            {
                return;
            }

            var multiplierStone = JojapocalypseConsequencesPatcher.GetEventChance(0.2, numberMiningPurchased);
            var multiplierGem = JojapocalypseConsequencesPatcher.GetEventChance(0.25, numberMiningPurchased);
            stoneChance *= multiplierStone;
            gemStoneChance *= multiplierGem;
        }

        private static void AdjustCombatChances(ref double monsterChance, ref double itemChance)
        {
            var numberCombatPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.COMBAT_LEVEL);
            if (numberCombatPurchased <= 0)
            {
                return;
            }

            var multiplierMonster = JojapocalypseConsequencesPatcher.GetEventChance(0.15, numberCombatPurchased);
            var multiplierItem = JojapocalypseConsequencesPatcher.GetEventChance(0.25, numberCombatPurchased);
            monsterChance *= multiplierMonster;
            itemChance *= multiplierItem;
        }
    }
}
