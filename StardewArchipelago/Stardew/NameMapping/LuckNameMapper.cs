using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class LuckNameMapper : INameMapper, IRecipeNameMapper
    {

        private static readonly Dictionary<string, string> LuckCraftIDsToEnglishNamesMap = new()
        {
            { "moonslime.Luck.slots_copper", "Copper Slot Machine" },
            { "moonslime.Luck.slots_gold", "Gold Slot Machine" },
            { "moonslime.Luck.slots_iridium", "Iridium Slot Machine" },
            { "moonslime.Luck.slots_radioactive", "Radioactive Slot Machine"}
        };

        private static readonly Dictionary<string, string> LuckEnglishNamesToCraftIdsMap = LuckCraftIDsToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public LuckNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return internalName;
        }

        public string GetInternalName(string englishName)
        {
            return LuckEnglishNamesToCraftIdsMap.ContainsKey(englishName) ? LuckEnglishNamesToCraftIdsMap[englishName] : englishName;
        }

        public string GetItemName(string recipeName)
        {
            var transformedIDName = LuckCraftIDsToEnglishNamesMap.ContainsKey(recipeName) ? LuckCraftIDsToEnglishNamesMap[recipeName] : recipeName;
            return GetEnglishName(transformedIDName);
        }

        public string GetRecipeName(string itemName)
        {
            return GetInternalName(itemName);
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return LuckEnglishNamesToCraftIdsMap.ContainsKey(itemName);
        }
    }
}
