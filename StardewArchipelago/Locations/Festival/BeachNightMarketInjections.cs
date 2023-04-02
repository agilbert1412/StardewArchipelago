using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Locations.Events;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Festival
{
    public class BeachNightMarketInjections
    {
        private const int CATEGORY_SEEDS = -74;

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _shopReplacer = shopReplacer;
        }

        // public Dictionary<ISalable, int[]> geMagicShopStock()
        public static void GetMagicShopStock_UniqueItemsAndSeeds_Postfix(ShopMenu __instance, ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                var seedShuffle = _archipelago.SlotData.SeedShuffle == SeedShuffle.Shuffled;
                foreach (var salableItem in __result.Keys.ToArray())
                {
                    if (salableItem is not Object salableObject)
                    {
                        continue;
                    }

                    if (seedShuffle && salableObject.Category == CATEGORY_SEEDS && !_archipelago.HasReceivedItem(salableObject.Name, out var sendingPlayerName))
                    {
                        __result.Remove(salableItem);
                        continue;
                    }

                    if (_archipelago.SlotData.FestivalObjectives == FestivalObjectives.Vanilla)
                    {
                        continue;
                    }

                    _shopReplacer.ReplaceShopItem(__result, salableItem, FestivalLocationNames.RARECROW_7, item => item.IsScarecrow() && item.Name != "Rarecrow #7");
                    _shopReplacer.ReplaceShopItem(__result, salableItem, FestivalLocationNames.RARECROW_8, item => item.IsScarecrow() && item.Name != "Rarecrow #8");

                    if (_archipelago.SlotData.FestivalObjectives != FestivalObjectives.Difficult)
                    {
                        continue;
                    }

                    _shopReplacer.ReplaceShopItem(__result, salableItem, FestivalLocationNames.CONE_HAT,
                        item => item.which.Value == 39);
                    _shopReplacer.ReplaceShopItem(__result, salableItem, FestivalLocationNames.IRIDIUM_FIREPLACE,
                        (Furniture item) => item.ParentSheetIndex == 1796);
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetMagicShopStock_UniqueItemsAndSeeds_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
