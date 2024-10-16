using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class CompoundNameMapper : INameMapper, IRecipeNameMapper
    {
        private readonly List<INameMapper> _mappers;
        private readonly List<IRecipeNameMapper> _recipeMappers;

        public CompoundNameMapper(SlotData slotData)
        {
            _mappers = new List<INameMapper>();
            _recipeMappers = new List<IRecipeNameMapper>();

            // This one is not the same type of mapping
            var craftingRecipeMapper = new CraftingRecipeNameMapper();
            _recipeMappers.Add(craftingRecipeMapper);

            if (slotData.Mods.HasMod(ModNames.ARCHAEOLOGY))
            {
                var archaeologyMapper = new ArchaeologyNameMapper();
                _mappers.Add(archaeologyMapper);
                _recipeMappers.Add(archaeologyMapper);
            }
            if (slotData.Mods.HasMod(ModNames.SVE))
            {
                var sveMapper = new SVENameMapper();
                _mappers.Add(sveMapper);
                _recipeMappers.Add(sveMapper);
            }
            if (slotData.Mods.HasMod(ModNames.BINNING))
            {
                var binningMapper = new BinningNameMapper();
                _mappers.Add(binningMapper);
                _recipeMappers.Add(binningMapper);
            }
        }

        public string GetEnglishName(string internalName)
        {
            return _mappers.Aggregate(internalName, (current, nameMapper) => nameMapper.GetEnglishName(current));
        }

        public string GetInternalName(string englishName)
        {
            return _mappers.Aggregate(englishName, (current, nameMapper) => nameMapper.GetInternalName(current));
        }

        public string GetItemName(string recipeName)
        {
            return _recipeMappers.Aggregate(recipeName, (current, nameMapper) => nameMapper.GetItemName(current));
        }

        public string GetRecipeName(string itemName)
        {
            return _recipeMappers.Aggregate(itemName, (current, nameMapper) => nameMapper.GetRecipeName(current));
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return _recipeMappers.Any(x => x.RecipeNeedsMapping(itemName));
        }
    }
}
