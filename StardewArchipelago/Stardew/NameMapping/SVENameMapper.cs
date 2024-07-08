using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class SVENameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> SVEToEnglishNamesMap = new()
        {
            {"Bearberrys", "Bearberry"},
            {"Big Conch", "Conch"},
            {"Dried Sand Dollar", "Sand Dollar"},
            {"Lucky Four Leaf Clover", "Four Leaf Clover"},
            {"Smelly Rafflesia", "Rafflesia"},
            {"Ancient Ferns Seed", "Ancient Fern Seed"},
        };

        private static readonly Dictionary<string, string> SVECraftIDsToEnglishNamesMap = new()
        {

        };

        private static readonly Dictionary<string, string> EnglishToSVENamesMap = SVEToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public SVENameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return SVEToEnglishNamesMap.ContainsKey(internalName) ? SVEToEnglishNamesMap[internalName] : internalName;
        }

        public string GetInternalName(string englishName)
        {
            return EnglishToSVENamesMap.ContainsKey(englishName) ? EnglishToSVENamesMap[englishName] : englishName;
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
            return EnglishToSVENamesMap.ContainsKey(itemName);
        }
    }
}