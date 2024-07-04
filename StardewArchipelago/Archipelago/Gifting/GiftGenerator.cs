using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Traits;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.GameData.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftGenerator
    {
        private const int DEFAULT_BUFF_DURATION = 120;

        private StardewItemManager _itemManager;

        public GiftGenerator(StardewItemManager itemManager)
        {
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
                yield return CreateTrait(nameFlag, 1D, 3D);
            }

            if (ReplaceFlags.ContainsKey(itemName))
            {
                var replacedNameFlag = GetFromAllFlags(ReplaceFlags[itemName]);
                if (!string.IsNullOrWhiteSpace(replacedNameFlag))
                {
                    yield return CreateTrait(replacedNameFlag, 1D, 2D);
                }
            }

            foreach (var word in itemName.Split(' '))
            {
                var wordFlag = GiftFlag.AllFlags.FirstOrDefault(x => x.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrWhiteSpace(wordFlag))
                {
                    continue;
                }

                yield return CreateTrait(wordFlag, 1D, 1D);

                if (!ReplaceFlags.ContainsKey(wordFlag))
                {
                    continue;
                }

                var replacedWordFlag = GetFromAllFlags(ReplaceFlags[wordFlag]);
                if (!string.IsNullOrWhiteSpace(replacedWordFlag))
                {
                    yield return CreateTrait(replacedWordFlag, 1D, 0.5D);
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
                yield return CreateTrait(GiftFlag.Tool, buffDuration, effects.FarmingLevel);
            }

            if (effects.FishingLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Fish, buffDuration, effects.FishingLevel);
            }

            if (effects.MiningLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Tool, buffDuration, effects.MiningLevel);
            }

            if (effects.ForagingLevel != 0)
            {
                yield return CreateTrait(GiftFlag.Tool, buffDuration, effects.ForagingLevel);
            }
            
            if (effects.LuckLevel != 0)
            {
                yield return CreateTrait("Luck", buffDuration, effects.LuckLevel);
            }

            if (effects.MagneticRadius != 0)
            {
                yield return CreateTrait("Magnetism", buffDuration, effects.MagneticRadius);
            }

            if (effects.MaxStamina != 0)
            {
                yield return CreateTrait(GiftFlag.Mana, buffDuration, effects.MaxStamina);
            }

            if (effects.Speed != 0)
            {
                yield return CreateTrait(GiftFlag.Speed, buffDuration, effects.Speed);
            }

            if (effects.Defense != 0)
            {
                yield return CreateTrait(GiftFlag.Armor, buffDuration, effects.Defense);
            }

            if (effects.Attack != 0)
            {
                yield return CreateTrait(GiftFlag.Weapon, buffDuration, effects.Attack);
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
            var contextTags = giftItem.GetContextTags();
            
            foreach (var contextTag in contextTags)
            {
                if (_contextTags.ContainsKey(contextTag))
                {
                    yield return CreateTrait(_contextTags[contextTag]);
                }

                if (contextTag.StartsWith(colorPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(contextTag[colorPrefix.Length..].ToLower()));
                }

                if (contextTag.StartsWith(foodPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(contextTag[foodPrefix.Length..].ToLower()));
                }

                if (contextTag.StartsWith("dye_", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait("Dye", contextTag.EndsWith("strong", StringComparison.InvariantCultureIgnoreCase) ? 2 : 1);
                }

                if (contextTag.EndsWith(itemSuffix, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return CreateTrait(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(contextTag[..^itemSuffix.Length].ToLower()));
                }
            }
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

                bestTraits[trait.Trait] = MaxOf(trait, bestTraits[trait.Trait]);
            }

            return bestTraits.Values;
        }

        private GiftTrait CreateTrait(string trait, double duration = 1.0, double quality = 1.0)
        {
            return new GiftTrait(trait, duration, quality);
        }

        private static GiftTrait MaxOf(GiftTrait trait, GiftTrait previousTrait)
        {
            return new GiftTrait(trait.Trait, Math.Max(trait.Duration, previousTrait.Duration), Math.Max(trait.Quality, previousTrait.Quality));
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
            { Category.SEED, new[] { GiftFlag.Seed } },
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
            { "Minerals", "Mineral" },
            { "Seeds", GiftFlag.Seed },
            { "Frozen", "Ice" },
            { "Winter", "Ice" },
            { "Magma", "Fire" },
            { "Bulb", GiftFlag.Seed },
            { "Starter", GiftFlag.Seed },
        };

        private static readonly Dictionary<string, string> _contextTags = new()
        {
            { "light_source", "Light" },
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
        };
    }
}
