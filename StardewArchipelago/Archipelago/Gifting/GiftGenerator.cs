using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Versioning.Gifts;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.GameData.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftGenerator
    {
        private const int DEFAULT_BUFF_DURATION = 120;

        private readonly ILogger _logger;
        private readonly StardewItemManager _itemManager;

        public GiftGenerator(ILogger logger, StardewItemManager itemManager)
        {
            _logger = logger;
            _itemManager = itemManager;
        }

        public bool TryCreateGiftItem(Object giftObject, bool isTrap, out GiftItem giftItem, out GiftTrait[] traits, out string failureMessage)
        {
            giftItem = null;
            traits = null;
            var giftOrTrap = isTrap ? "trap" : "gift";
            if (giftObject == null)
            {
                failureMessage = $"You must hold an item in your hand to {giftOrTrap} it";
                return false;
            }

            var name = _itemManager.NormalizeName(giftObject.ItemId, giftObject.Name);

            if (!_itemManager.ObjectExists(name) || giftObject.questItem.Value)
            {
                failureMessage = $"{name} cannot be sent to other players";
                return false;
            }


            giftItem = new GiftItem(name, giftObject.Stack, giftObject.salePrice() * BankHandler.EXCHANGE_RATE);
            traits = GenerateGiftTraits(giftObject, isTrap);
            failureMessage = $"";
            return true;
        }

        private GiftTrait[] GenerateGiftTraits(Item giftItem, bool isTrap)
        {
            var traits = new List<GiftTrait>();

            if (isTrap)
            {
                traits.Add(new GiftTrait(GiftFlag.Trap, 1, 1));
            }

            traits.AddRange(GetNameTraits(giftItem));
            traits.AddRange(GetCategoryTraits(giftItem));

            if (giftItem is Object giftObject && Game1.objectData.ContainsKey(giftItem.ItemId))
            {
                traits.AddRange(GetObjectTraits(giftObject));
            }

            traits.AddRange(GetContextTagsTraits(giftItem));

            return SimplifyDuplicates(traits).ToArray();
        }

        private IEnumerable<GiftTrait> GetNameTraits(Item giftItem)
        {
            var nameTraits = new List<GiftTrait>();
            nameTraits.AddRange(GetNameTraits(giftItem.Name));
            nameTraits.AddRange(GetNameTraits(giftItem.DisplayName));
            return nameTraits;
        }

        private IEnumerable<GiftTrait> GetNameTraits(string itemName)
        {
            var nameFlag = GetFromAllFlags(itemName);
            if (!string.IsNullOrWhiteSpace(nameFlag))
            {
                yield return CreateTrait(nameFlag);
            }

            if (ReplaceFlags.ContainsKey(itemName))
            {
                var replacedNameFlag = GetFromAllFlags(ReplaceFlags[itemName]);
                if (!string.IsNullOrWhiteSpace(replacedNameFlag))
                {
                    yield return CreateTrait(replacedNameFlag);
                }
            }

            foreach (var word in itemName.Split(' '))
            {
                var wordFlag = GetFromAllFlags(word);
                if (!string.IsNullOrWhiteSpace(wordFlag))
                {
                    yield return CreateTrait(wordFlag, 0.5D);
                }

                if (CustomWordFlags.Contains(word))
                {
                    yield return CreateTrait(word);
                }

                if (!ReplaceFlags.ContainsKey(word))
                {
                    continue;
                }

                var replacedWordFlag = GetFromAllFlags(ReplaceFlags[word]);
                if (!string.IsNullOrWhiteSpace(replacedWordFlag))
                {
                    yield return CreateTrait(replacedWordFlag, 0.5D);
                }
            }
        }

        private static string GetFromAllFlags(string itemName)
        {
            return GiftFlag.AllFlags.FirstOrDefault(x => x.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));
        }

        private IEnumerable<GiftTrait> GetObjectTraits(Object giftObject)
        {
            var objectInfo = Game1.objectData[giftObject.ItemId];

            var edibility = objectInfo.Edibility;
            if (Convert.ToInt32(edibility) > 0)
            {
                foreach (var consumableTrait in GetConsumableTraits(objectInfo))
                {
                    yield return consumableTrait;
                }
            }

            var type = objectInfo.Type;
            if (type == null)
            {
                yield break;
            }

            if (ReplaceFlags.ContainsKey(type))
            {
                type = ReplaceFlags[type];
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                yield return CreateTrait(type);
            }
        }

        private IEnumerable<GiftTrait> GetConsumableTraits(ObjectData objectInfo)
        {
            yield return CreateTrait(GiftFlag.Consumable);

            if (objectInfo.IsDrink)
            {
                yield return CreateTrait(GiftFlag.Drink);
            }
            else
            {
                yield return CreateTrait(GiftFlag.Food);
            }

            var buffsData = objectInfo.Buffs;
            if (buffsData is not null)
            {
                foreach (var buffData in buffsData)
                {
                    var buffDuration = buffData.Duration / DEFAULT_BUFF_DURATION;
                    foreach (var buffTrait in GetBuffTraits(buffData, buffDuration))
                    {
                        yield return buffTrait;
                    }
                }
            }
        }

        private IEnumerable<GiftTrait> GetBuffTraits(ObjectBuffData buffData, double buffDuration)
        {
            if (buffDuration <= 0)
            {
                yield break;
            }

            // buffData.BuffId should be used for non-standard, not yet compatible buffs such as Oil of Garlic

            if (buffData.CustomAttributes is null)
            {
                yield break;
            }

            var effects = buffData.CustomAttributes;

            if (effects.FarmingLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Tool, effects.FarmingLevel, buffDuration);
            }

            if (effects.FishingLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Fish, effects.FishingLevel, buffDuration);
            }

            if (effects.MiningLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Tool, effects.MiningLevel, buffDuration);
            }

            if (effects.ForagingLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Tool, effects.ForagingLevel, buffDuration);
            }

            if (effects.LuckLevel != 0)
            {
                yield return CreateTrait("Luck", effects.LuckLevel, buffDuration);
            }

            if (effects.MagneticRadius != 0)
            {
                yield return CreateTrait("Magnetism", effects.MagneticRadius, buffDuration);
            }

            if (effects.MaxStamina != 0)
            {
                yield return CreateTrait(GiftFlag.Mana, effects.MaxStamina, buffDuration);
            }

            if (effects.Speed != 0)
            {
                yield return CreateTrait(GiftFlag.Speed, effects.Speed, buffDuration);
            }

            if (effects.Defense != 0)
            {
                yield return CreateTrait(GiftFlag.Armor, effects.Defense, buffDuration);
            }

            if (effects.Attack != 0)
            {
                yield return CreateTrait(GiftFlag.Weapon, effects.Attack, buffDuration);
            }
        }

        private IEnumerable<GiftTrait> GetCategoryTraits(Item giftItem)
        {
            var category = giftItem.Category;

            if (_categoryFlags.ContainsKey(category))
            {
                foreach (var categoryName in _categoryFlags[category])
                {
                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        yield return CreateTrait(categoryName);
                    }
                }
            }
        }

        private IEnumerable<GiftTrait> GetContextTagsTraits(Item giftItem)
        {
            const string colorPrefix = "color_";
            const string foodPrefix = "color_";
            const string itemSuffix = "_item";
            const string largePrefix = "large_";
            var contextTags = giftItem.GetContextTags();

            foreach (var contextTag in contextTags)
            {
                if (_contextTags.ContainsKey(contextTag))
                {
                    yield return CreateTrait(_contextTags[contextTag]);
                    continue;
                }

                if (contextTag == "large_egg_item")
                {
                    yield return CreateTrait(GiftFlag.Egg, 2D);
                    continue;
                }

                if (contextTag == "large_milk_item")
                {
                    yield return CreateTrait("Milk", 2D);
                    continue;
                }

                if (contextTag.StartsWith(colorPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait(ToPascalCase(contextTag[colorPrefix.Length..]));
                }

                if (contextTag.StartsWith(foodPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait(ToPascalCase(contextTag[foodPrefix.Length..]));
                }

                if (contextTag.StartsWith("dye_", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait("Dye", contextTag.EndsWith("strong", StringComparison.InvariantCultureIgnoreCase) ? 2 : 1);
                }

                if (contextTag.EndsWith(itemSuffix, StringComparison.InvariantCultureIgnoreCase))
                {
                    var item = contextTag[..^itemSuffix.Length];
                    var quality = 1D;
                    if (item.StartsWith(largePrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        item = item[largePrefix.Length..];
                        quality *= 2;
                    }
                    item = ToPascalCase(item);
                    if (item.Equals(GiftFlag.Bomb, StringComparison.InvariantCultureIgnoreCase))
                    {
                        quality = GetBombQuality(giftItem);
                    }
                    yield return CreateTrait(item, quality);
                }
            }
        }

        private double GetBombQuality(Item bombItem)
        {
            switch (bombItem.ItemId)
            {
                case "286":
                    return 0.5;
                case "287":
                    return 1.0;
                case "288":
                    return 2.0;
            }

            _logger.LogError($"Could not parse bomb item {bombItem.Name} with ID {bombItem.ItemId}");
            return 1.0;
        }

        private string ToPascalCase(string text)
        {
            var lowerText = text.ToLower();
            var spacedText = lowerText.Replace("_", " ");
            var titleText = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(spacedText);
            var pascalText = titleText.Replace(" ", "");
            return pascalText;
        }

        private IEnumerable<GiftTrait> SimplifyDuplicates(IEnumerable<GiftTrait> traits)
        {
            var bestTraits = new Dictionary<string, GiftTrait>();
            foreach (var trait in traits)
            {
                if (!bestTraits.ContainsKey(trait.Trait))
                {
                    bestTraits.Add(trait.Trait, trait);
                    continue;
                }

                bestTraits[trait.Trait] = BestOf(trait, bestTraits[trait.Trait]);
            }

            return bestTraits.Values;
        }

        private GiftTrait CreateTrait(string trait, double quality = 1.0, double duration = 1.0)
        {
            return new GiftTrait(trait, quality, duration);
        }

        private static GiftTrait BestOf(GiftTrait trait, GiftTrait previousTrait)
        {
            return new GiftTrait(trait.Trait, BestOf(trait.Quality, previousTrait.Quality), BestOf(trait.Duration, previousTrait.Duration));
        }

        private static double BestOf(double trait1, double trait2)
        {
            const double epsilon = 0.0001;
            if (Math.Abs(trait1 - 1.0) < epsilon)
            {
                return trait2;
            }
            if (Math.Abs(trait2 - 1.0) < epsilon)
            {
                return trait1;
            }
            return Math.Max(trait1, trait2);
        }

        private static readonly Dictionary<int, string[]> _categoryFlags = new()
        {
            { Category.GEM, new[] { "Gem" } },
            { Category.FISH, new[] { GiftFlag.Fish } },
            { Category.EGG, new[] { GiftFlag.Egg } },
            { Category.MILK, new[] { GiftFlag.Animal, "Milk", "AnimalProduct" } },
            { Category.COOKING, new[] { "Cooking" } },
            //{ Category.CRAFTING, new[] {GiftFlag.Crafting}},
            //{ Category.BIG_CRAFTABLE, new[] {GiftFlag.BigCraftable}},
            { Category.MINERAL, new[] { "Mineral" } },
            { Category.MEAT, new[] { GiftFlag.Meat } },
            { Category.METAL, new[] { GiftFlag.Metal } },
            { Category.BUILDING, new[] { GiftFlag.Material } },
            //{ Category.SELL_AT_PIERRE, new[] {GiftFlag.SellAtPierre}},
            //{ Category.SELL_AT_PIERRE_AND_MARNIE, new[] {GiftFlag.SellAtPierreAndMarnie}},
            { Category.FERTILIZER, new[] { "Fertilizer" } },
            { Category.TRASH, new[] { "Trash" } },
            { Category.BAIT, new[] { "Bait" } },
            { Category.TACKLE, new[] { "Tackle" } },
            //{ Category.SELL_AT_FISH_SHOP, new[] {GiftFlag.SellAtFishShop}},
            { Category.FURNITURE, new[] { "Furniture" } },
            { Category.INGREDIENT, new[] { "Ingredient" } },
            { Category.ARTISAN_GOOD, new[] { "ArtisanGood" } },
            { Category.SYRUP, new[] { "Syrup", "ArtisanGood" } },
            { Category.MONSTER_LOOT, new[] { GiftFlag.Monster } },
            { Category.EQUIPMENT, new[] { GiftFlag.Armor } },
            { Category.SEEDS, new[] { GiftFlag.Seed } },
            { Category.VEGETABLE, new[] { GiftFlag.Vegetable } },
            { Category.FRUIT, new[] { GiftFlag.Fruit } },
            { Category.FLOWER, new[] { "Flower" } },
            { Category.FORAGE, new[] { "Forage" } },
            { Category.HAT, new[] { "Cosmetic", "Hat" } },
            { Category.RING, new[] { GiftFlag.Armor } },
            { Category.WEAPON, new[] { GiftFlag.Weapon } },
            { Category.TOOL, new[] { GiftFlag.Tool } },
        };

        private static readonly Dictionary<string, string> ReplaceFlags = new()
        {
            { "Arch", "Artifact" },
            { "Basic", "" },
            { "asdf", "" },
            { "interactive", "" },
            { "QualityFertilizer", "" },
            { "Noble", "Fancy" },
            { "Minerals", "Mineral" },
            { "Radioactive", "Radioactive" },
            { "Seeds", GiftFlag.Seed },
            { "Frozen", GiftFlag.Ice },
            { "Winter", GiftFlag.Ice },
            { "Magma", GiftFlag.Fire },
            { "Bulb", GiftFlag.Seed },
            { "Starter", GiftFlag.Seed },
            { "Medicine", GiftFlag.Cure },
            { "Warp", "Teleport" },
        };

        private static readonly List<string> CustomWordFlags = new()
        {
            "Radioactive",
        };

        private static readonly Dictionary<string, string> _contextTags = new()
        {
            { "light_source", GiftFlag.Light },
            { "season_spring", "Spring" },
            { "season_summer", "Summer" },
            { "season_fall", "Fall" },
            { "season_winter", "Winter" },
            { "fish_ocean", "Ocean" },
            { "fish_river", "River" },
            { "fish_lake", "Lake" },
            { "fish_swamp", "Swamp" },
            { "fish_pond", "Pond" },
            { "fish_legendary", "Legendary" },
            { "fish_legendary_family", "Legendary" },
            { "fish_desert", "Desert" },
            { "fish_crab_pot", "CrabPot" },
            { "fish_carnivorous", "Carnivorous" },
            { "fish_freshwater", "Freshwater" },
            { "edible_mushroom", "Mushroom" },
            { "geode", "Geode" },
            { "book_xp_farming", "Book" },
            { "book_xp_foraging", "Book" },
            { "book_xp_fishing", "Book" },
            { "book_xp_mining", "Book" },
            { "book_xp_combat", "Book" },
            { "cow_milk_item", "Cow" },
            { "goat_milk_item", "Goat" },
            { "slime_egg_item", GiftFlag.Egg },
        };
    }
}
