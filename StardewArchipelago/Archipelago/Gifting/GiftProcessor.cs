using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using StardewArchipelago.Extensions;
using StardewArchipelago.Stardew;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Items.Traps;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftProcessor
    {
        private ILogger _logger;
        private ArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly ICloseTraitParser<string> _closeTraitParser;
        private readonly GiftTrapManager _giftTrapManager;
        private Dictionary<string, Func<int, ItemAmount>> _specialItems;
        private Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>> _recognizedTraits;

        public GiftProcessor(ILogger logger, ArchipelagoClient archipelago, StardewItemManager itemManager, ICloseTraitParser<string> closeTraitParser, GiftTrapManager giftTrapManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _closeTraitParser = closeTraitParser;
            _giftTrapManager = giftTrapManager;
            InitializeSpecialItems();
            InitializeRecognizedTraits();
        }

        public bool ProcessGiftTrap(Gift gift)
        {
            if (gift.Traits.All(x => x.Trait != GiftFlag.Trap))
            {
                return false;
            }

            var senderName = _archipelago.GetPlayerName(gift.SenderSlot);

            foreach (var giftTrait in gift.Traits)
            {
                _giftTrapManager.TriggerTrapForTrait(giftTrait, gift.Amount, senderName.ToAnonymousName());
            }

            return true;

        }

        public bool TryMakeStardewItem(Gift gift, out string itemName, out int amount)
        {
            if (_itemManager.ObjectExists(gift.ItemName))
            {
                itemName = gift.ItemName;
                amount = gift.Amount;
                return true;
            }

            var capitalizedName = gift.ItemName.ToCapitalized();
            if (_specialItems.ContainsKey(capitalizedName))
            {
                var specialItem = _specialItems[capitalizedName](gift.Amount);
                itemName = specialItem.ItemName;
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
                    var itemAmount = itemFunction(gift.Amount, traitsByName);
                    itemName = itemAmount.ItemName;
                    amount = itemAmount.Amount;
                    return true;
                }
            }

            var closestGifts = _closeTraitParser.FindClosestAvailableGift(gift.Traits);
            if (closestGifts.Any())
            {
                var random = new Random(gift.ItemName.GetHashCode());
                var chosenIndex = random.Next(0, closestGifts.Count);
                var chosenGift = closestGifts[chosenIndex];
                itemName = chosenGift;
                amount = gift.Amount;
                return true;
            }

            foreach (var trait in gift.Traits)
            {
                if (_itemManager.ObjectExists(trait.Trait))
                {
                    itemName = trait.Trait;
                    amount = (int)Math.Round(trait.Quality * gift.Amount);
                    return true;
                }
            }

            itemName = null;
            amount = 0;
            return false;
        }

        private void InitializeSpecialItems()
        {
            _specialItems = new Dictionary<string, Func<int, ItemAmount>>();
            _specialItems.Add("Tree", (amount) => ("Wood", amount * 15));
            _specialItems.Add("Lumber", (amount) => ("Hardwood", amount * 15));
            _specialItems.Add("Boulder", (amount) => ("Stone", amount * 15));
            _specialItems.Add("Rock", (amount) => ("Stone", amount * 2));
            _specialItems.Add("Vine", (amount) => ("Fiber", amount * 2));
        }

        private void InitializeRecognizedTraits()
        {
            _recognizedTraits = new Dictionary<int, Dictionary<string[], Func<int, Dictionary<string, GiftTrait>, ItemAmount>>>();
            InitializeSingleRecognizedTraits();
            InitializeDualRecognizedTraits();
            InitializeTrioRecognizedTraits();
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

            singleRecognizedTraits.Add(new[] { GiftFlag.Speed }, MakeCoffee);
            singleRecognizedTraits.Add(new[] { "Fan" }, (amount, _) => ("Ornamental Fan", amount));

            _recognizedTraits.Add(1, singleRecognizedTraits);
        }

        private ItemAmount MakeCoffee(int amount, Dictionary<string, GiftTrait> traits)
        {
            var speedTrait = traits[GiftFlag.Speed];
            var totalSpeed = speedTrait.Duration + speedTrait.Quality - 1;
            return totalSpeed >= 3 ? ("Triple Shot Espresso", (int)Math.Round(amount * (totalSpeed / 3))) : ("Coffee", (int)Math.Round(amount * totalSpeed));
        }
    }
}
