using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class SocializingNameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> SocializingToEnglishNamesMap = new()
        {
            { "Bouquet", "Bouquet" },
        };

        private static readonly Dictionary<string, string> SocializingCraftIDsToEnglishNamesMap = new()
        {
            {"drbirbdev.SocializingSkill_Bouquet", "Bouquet"},
        };

        private static readonly Dictionary<string, string> EnglishToSocializingNamesMap = SocializingToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public SocializingNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return SocializingToEnglishNamesMap.ContainsKey(internalName) ? SocializingToEnglishNamesMap[internalName] : internalName;
        }

        public string GetInternalName(string englishName)
        {
            return EnglishToSocializingNamesMap.ContainsKey(englishName) ? EnglishToSocializingNamesMap[englishName] : englishName;
        }

        public string GetItemName(string recipeName)
        {
            var transformedIDName = SocializingCraftIDsToEnglishNamesMap.ContainsKey(recipeName) ? SocializingCraftIDsToEnglishNamesMap[recipeName] : recipeName;
            return GetEnglishName(transformedIDName);
        }

        public string GetRecipeName(string itemName)
        {
            return GetInternalName(itemName);
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return EnglishToSocializingNamesMap.ContainsKey(itemName);
        }
    }
}