using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

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

                var toolUsuallyCostsEnergy = __instance is Axe or Pickaxe or Hoe or WateringCan;

                who.Stamina -= numberPurchased * power * 0.1f * (toolUsuallyCostsEnergy ? 1f : 0.05f);
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
