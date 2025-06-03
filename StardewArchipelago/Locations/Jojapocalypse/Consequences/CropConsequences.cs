using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewArchipelago.Locations.Festival;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

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
        public static bool NewDay_ChanceOfNotGrowing_Prefix(Crop __instance, int state)
        {
            try
            {

                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.TOOL_UPGRADE);
                if (numberPurchased <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var chanceToSkipDayPerPurchase = 0.01;
                var chanceToGrowPerPurchase = 1 - chanceToSkipDayPerPurchase;
                var chanceToGrow = Math.Pow(chanceToGrowPerPurchase, numberPurchased);

                var random = Utility.CreateDaySaveRandom(__instance.currentLocation.Name.GetHash(), __instance.tilePosition.X, __instance.tilePosition.Y);
                if (random.NextDouble() <= chanceToGrow)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(NewDay_ChanceOfNotGrowing_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
