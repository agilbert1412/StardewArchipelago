using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations
{
    public class ShopReplacer
    {
        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public ShopReplacer(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public void ReplaceShopItem(Dictionary<ISalable, int[]> itemPriceAndStock, ISalable itemOnSale, string apLocation, Func<Object, bool> conditionToMeet, int price)
        {
            if (itemOnSale is not Object salableObject || !conditionToMeet(salableObject))
            {
                return;
            }

            itemPriceAndStock.Remove(itemOnSale);
            if (_locationChecker.IsLocationChecked(apLocation))
            {
                return;
            }

            var purchaseableLocation =
                new PurchaseableArchipelagoLocation(salableObject.Name, apLocation, _locationChecker,
                    _archipelago);
            itemPriceAndStock.Add(purchaseableLocation, new[] { price, 1 });
        }
    }
}
