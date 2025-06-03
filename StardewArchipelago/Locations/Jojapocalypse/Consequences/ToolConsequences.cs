using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.Festival;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class ToolConsequences
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

        // public virtual void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        public static void DoFunction_ConsumeExtraEnergy_Postfix(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            try
            {
                var numberPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.TOOL_UPGRADE);
                if (numberPurchased <= 0)
                {
                    return;
                }

                who.Stamina -= numberPurchased * power;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoFunction_ConsumeExtraEnergy_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
