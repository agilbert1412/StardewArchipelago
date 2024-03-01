using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class GoldenEggInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }

        // public static Dictionary<ISalable, int[]> getAnimalShopStock()
        public static void GetAnimalShopStock_GoldenEggIfReceived_Postfix(ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
                {
                    return;
                }

                if (!_archipelago.HasReceivedItem("Golden Egg"))
                {
                    return;
                }

                __result.Add(new Object(928, 1), new int[]
                {
                    100000,
                    int.MaxValue,
                });

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetAnimalShopStock_GoldenEggIfReceived_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
