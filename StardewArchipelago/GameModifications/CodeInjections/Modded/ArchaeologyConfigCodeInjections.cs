using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.CodeInjections.Modded
{
    internal class ArchaeologyConfigCodeInjections
    {
        private const double ARTIFACT_SPOT_MULTIPLIER = 1.6; // Default Value: 10
        private const double PANNING_MULTIPLIER = 1.5; // Default Value: 20
        private const double DIGGING_MULTIPLIER = 1.6; // Default Value: 5
        private const double WATER_SIFTER_MULTIPLIER = 1.8; // Default Value: 2

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public int ExperienceFromArtifactSpots { get; set; } = 10;
        public static void ExperienceFromArtifactSpots_APMultiplier_Postfix(object __instance, ref int __result)
        {
            try
            {
                __result = (int)(__result * ARTIFACT_SPOT_MULTIPLIER);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExperienceFromArtifactSpots_APMultiplier_Postfix)}:\n{ex}");
                return;
            }
        }

        // public int ExperienceFromPanSpots { get; set; } = 20;
        public static void ExperienceFromPanSpots_APMultiplier_Postfix(object __instance, ref int __result)
        {
            try
            {
                __result = (int)(__result * PANNING_MULTIPLIER);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExperienceFromPanSpots_APMultiplier_Postfix)}:\n{ex}");
                return;
            }
        }

        // public int ExperienceFromMinesDigging { get; set; } = 5;
        public static void ExperienceFromMinesDigging_APMultiplier_Postfix(object __instance, ref int __result)
        {
            try
            {
                __result = (int)(__result * DIGGING_MULTIPLIER);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExperienceFromMinesDigging_APMultiplier_Postfix)}:\n{ex}");
                return;
            }
        }

        // public int ExperienceFromWaterShifter { get; set; } = 2;
        public static void ExperienceFromWaterSifter_APMultiplier_Postfix(object __instance, ref int __result)
        {
            try
            {
                __result = (int)(__result * WATER_SIFTER_MULTIPLIER);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExperienceFromWaterSifter_APMultiplier_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
