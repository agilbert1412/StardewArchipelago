using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class CropConsequences
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
        public static bool NewDay_ChanceOfDying_Prefix(Crop __instance, int state)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.CROPSANITY);
                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (JojapocalypseConsequencesPatcher.RollConsequenceChance(0.005, numberPurchased, nameof(NewDay_ChanceOfDying_Prefix).GetHash(), __instance.currentLocation.Name.GetHash(), (__instance.tilePosition.X*877) + __instance.tilePosition.Y))
                {
                    __instance.Kill();
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(NewDay_ChanceOfDying_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
