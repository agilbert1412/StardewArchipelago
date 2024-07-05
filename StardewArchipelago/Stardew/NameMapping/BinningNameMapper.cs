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

        private static readonly Dictionary<string, string> EnglishToArchaeologyNamesMap = BinningToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public BinningNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return BinningToEnglishNamesMap.ContainsKey(internalName) ? BinningToEnglishNamesMap[internalName] : internalName;
        }

        public string GetInternalName(string englishName)
        {
            return EnglishToArchaeologyNamesMap.ContainsKey(englishName) ? EnglishToArchaeologyNamesMap[englishName] : englishName;
        }

        public string GetItemName(string recipeName)
        {
            return GetEnglishName(recipeName);
        }

        public string GetRecipeName(string itemName)
        {
            return GetInternalName(itemName);
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return EnglishToArchaeologyNamesMap.ContainsKey(itemName);
        }
    }
}