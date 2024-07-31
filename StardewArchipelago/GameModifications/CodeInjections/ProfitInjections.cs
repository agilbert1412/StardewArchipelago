using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class ProfitInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public virtual int sellToStorePrice(long specificPlayerID = -1)
        public static void SellToStorePrice_ApplyProfitMargin_Postfix(Object __instance, long specificPlayerID, ref int __result)
        {
            try
            {
                __result = (int)Math.Round(__result * _archipelago.SlotData.ProfitMargin);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SellToStorePrice_ApplyProfitMargin_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
