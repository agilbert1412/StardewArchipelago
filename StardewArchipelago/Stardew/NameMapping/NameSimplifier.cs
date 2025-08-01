﻿using System.Collections.Generic;
using StardewArchipelago.Constants.Vanilla;
using StardewValley;
using static StardewValley.Object;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class NameSimplifier
    {
        public NameSimplifier()
        {
        }

        public string GetSimplifiedName(Item item)
        {
            return GetSimplifiedName(item.Name, item.QualifiedItemId, item.ItemId, item is Object itemObject ? itemObject.preserve.Value : null);
        }

        public string GetSimplifiedName(string itemName, string itemQualifiedId, string itemId, PreserveType? preserveType = null)
        {
            var name = itemName;
            if (_renamedObjects.ContainsKey(itemQualifiedId))
            {
                name = _renamedObjects[itemQualifiedId];
            }

            if (PowerBooks.BookIdsToNames.ContainsKey(itemId))
            {
                name = PowerBooks.BookIdsToNames[itemId];
            }

            foreach (var (oldChar, newChar) in _simplifiedChars)
            {
                name = name.Replace(oldChar, newChar);
            }

            foreach (var simplifiedName in _simplifiedNames)
            {
                if (name.Contains(simplifiedName))
                {
                    return simplifiedName;
                }
            }

            if (preserveType.HasValue)
            {
                switch (preserveType.GetValueOrDefault())
                {
                    case Object.PreserveType.Wine:
                        return "Wine";
                    case Object.PreserveType.Jelly:
                        return "Jelly";
                    case Object.PreserveType.Pickle:
                        return "Pickles";
                    case Object.PreserveType.Juice:
                        return "Juice";
                    case Object.PreserveType.Roe:
                        return "Roe";
                    case Object.PreserveType.AgedRoe:
                        return "Aged Roe";
                    case Object.PreserveType.DriedFruit:
                        return "Dried Fruit";
                    case Object.PreserveType.DriedMushroom:
                        return "Dried Mushrooms";
                    case Object.PreserveType.SmokedFish:
                        return "Smoked Fish";
                    case Object.PreserveType.Bait:
                        return "Targeted Bait";
                }
            }

            return name;
        }

        private static readonly Dictionary<string, string> _renamedObjects = new()
        {
            { QualifiedItemIds.STRANGE_DOLL_GREEN, "Strange Doll (Green)" },
            { QualifiedItemIds.BROWN_EGG, "Egg (Brown)" },
            { QualifiedItemIds.LARGE_BROWN_EGG, "Large Egg (Brown)" },
            { QualifiedItemIds.LARGE_GOAT_MILK, "Large Goat Milk" },
            { QualifiedItemIds.COOKIES, "Cookies" },
            { QualifiedItemIds.DRIED_FRUIT, "Dried Fruit" },
            { QualifiedItemIds.DRIED_MUSHROOMS, "Dried Mushrooms" },
            { QualifiedItemIds.SMOKED_FISH, "Smoked Fish" },
            { QualifiedItemIds.IRIDIUM_SNAKE_MILK, "Iridium Snake Milk" },
        };

        private static readonly List<string> _simplifiedNames = new()
        {
            "Honey",
            "Secret Note",
            "Journal Scrap",
        };

        private static readonly Dictionary<string, string> _simplifiedChars = new()
        {
            { "ñ", "n" },
            { "Ñ", "N" },
        };
    }
}
