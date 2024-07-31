using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class BinningNameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> BinningToEnglishNamesMap = new()
        {
            { "Trash Can", "Trash Bin" },
        };

        private static readonly Dictionary<string, string> BinningCraftIDsToEnglishNamesMap = new()
        {
            { "drbirbdev.BinningSkill_TrashCan", "Trash Bin" },
        };

        private static readonly Dictionary<string, string> EnglishToBinningNamesMap = BinningToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public BinningNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return BinningToEnglishNamesMap.ContainsKey(internalName) ? BinningToEnglishNamesMap[internalName] : internalName;
        }

        public string GetInternalName(string englishName)
        {
            return EnglishToBinningNamesMap.ContainsKey(englishName) ? EnglishToBinningNamesMap[englishName] : englishName;
        }

        public string GetItemName(string recipeName)
        {
            var transformedIDName = BinningCraftIDsToEnglishNamesMap.ContainsKey(recipeName) ? BinningCraftIDsToEnglishNamesMap[recipeName] : recipeName;
            return GetEnglishName(transformedIDName);
        }

        public string GetRecipeName(string itemName)
        {
            return GetInternalName(itemName);
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return EnglishToBinningNamesMap.ContainsKey(itemName);
        }
    }
}
