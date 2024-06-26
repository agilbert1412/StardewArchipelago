using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class CraftingRecipeNameMapper : IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> _recipeToItemNames = new()
        {
            { "Wild Seeds (Sp)", "Spring Seeds" },
            { "Wild Seeds (Su)", "Summer Seeds" },
            { "Wild Seeds (Fa)", "Fall Seeds" },
            { "Wild Seeds (Wi)", "Winter Seeds" },
        };

        private static readonly Dictionary<string, string> _itemToRecipeNames =
            _recipeToItemNames.ToDictionary(x => x.Value, x => x.Key);

        public CraftingRecipeNameMapper()
        {
        }

        public string GetItemName(string recipeName)
        {
            return _recipeToItemNames.ContainsKey(recipeName) ? _recipeToItemNames[recipeName] : recipeName;
        }

        public string GetRecipeName(string itemName)
        {
            return _itemToRecipeNames.ContainsKey(itemName) ? _itemToRecipeNames[itemName] : itemName;
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return _itemToRecipeNames.ContainsKey(itemName);
        }
    }
}
