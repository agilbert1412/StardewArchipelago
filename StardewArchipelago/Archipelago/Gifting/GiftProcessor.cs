using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using StardewArchipelago.Extensions;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftProcessor
    {
        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private StardewItemManager _itemManager;
        private Dictionary<string, Func<int, ItemAmount>> _specialItems;
        private Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>> _recognizedTraits;

        public GiftProcessor(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _itemManager = itemManager;
            InitializeSpecialItems();
        }

        public bool TryMakeStardewItem(Gift gift, out StardewItem item, out int amount)
        {
            if (_itemManager.ObjectExists(gift.Item.Name))
            {
                item = _itemManager.GetObjectByName(gift.Item.Name);
                amount = gift.Item.Amount;
                return true;
            }

            var capitalizedName = gift.Item.Name.ToCapitalized();
            if (_specialItems.ContainsKey(capitalizedName))
            {
                var specialItem = _specialItems[capitalizedName](gift.Item.Amount);
                item = specialItem.Item;
                amount = specialItem.Amount;
                return true;
            }

            foreach (var traitNumber in _recognizedTraits.Keys.OrderByDescending(x => x))
            {
                foreach (var (traits, itemFunction) in _recognizedTraits[traitNumber])
                {
                    if (traits.Any(x => !gift.Traits.Select(t => t.Trait).Contains(x)))
                    {
                        continue;
                    }

                    var traitsByName = gift.Traits.ToDictionary(t => t.Trait, t => t);
                    var itemAmount = itemFunction(gift.Item.Amount, traitsByName);
                    item = itemAmount.Item;
                    amount = itemAmount.Amount;
                    return true;
                }
            }

            foreach (var trait in gift.Traits)
            {
                if (_itemManager.ObjectExists(trait.Trait))
                {
                    item = _itemManager.GetObjectByName(trait.Trait);
                    amount = (int)Math.Round(trait.Quality * gift.Item.Amount);
                    return true;
                }
            }

            item = null;
            amount = 0;
            return false;
        }

        private void InitializeSpecialItems()
        {
            _specialItems = new Dictionary<string, Func<int, ItemAmount>>();
            _specialItems.Add("Tree", (amount) => (_itemManager.GetItemByName("Wood"), amount * 15));
            _specialItems.Add("Boulder", (amount) => (_itemManager.GetItemByName("Stone"), amount * 15));
            _specialItems.Add("Rock", (amount) => (_itemManager.GetItemByName("Stone"), amount * 2));
            _specialItems.Add("Vine", (amount) => (_itemManager.GetItemByName("Fiber"), amount * 2));
        }

        private void InitializeRecognizedTraits()
        {
            _recognizedTraits = new Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>>();
            InitializeSingleRecognizedTraits();
        }

        private void InitializeTrioRecognizedTraits()
        {
            var trioRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();
            _recognizedTraits.Add(3, trioRecognizedTraits);
        }

        private void InitializeDualRecognizedTraits()
        {
            var dualRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();
            // Speed + Animal = Horse
            _recognizedTraits.Add(2, dualRecognizedTraits);
        }

        private void InitializeSingleRecognizedTraits()
        {
            var singleRecognizedTraits = new Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>();

            singleRecognizedTraits.Add(new[] {GiftFlag.Speed}, MakeCoffee);
            singleRecognizedTraits.Add(new[] {"Fan"}, (amount, _) => (_itemManager.GetItemByName("Ornamental Fan"), amount));

            _recognizedTraits.Add(1, singleRecognizedTraits);
        }

        private ItemAmount MakeCoffee(int amount, Dictionary<string, GiftTrait> traits)
        {
            var speedTrait = traits[GiftFlag.Speed];
            var totalSpeed = speedTrait.Duration + speedTrait.Quality - 1;
            return totalSpeed >= 3 ? (_itemManager.GetItemByName("Triple Shot Espresso"), (int)Math.Round(amount * (totalSpeed / 3))) : (_itemManager.GetItemByName("Coffee"), (int)Math.Round(amount * totalSpeed));
        }
    }
}
