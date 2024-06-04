using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class ArchaeologyNameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> ArchaeologyToEnglishNamesMap = new()
        {
            { "moonslime.archaeology.ancient_battery", "Ancient Battery Production Station" },
            { "moonslime.archaeology.glass_bazier", "Glass Bazier" },
            { "moonslime.archaeology.grinder", "Grinder" },
            { "moonslime.archaeology.preservation_chamber", "Preservation Chamber" },
            { "moonslime.archaeology.h_preservation_chamber", "Hardwood Preservation Chamber" },
            { "moonslime.archaeology.glass_fence", "Glass Fence" },
            { "moonslime.archaeology.dummy_path_bone", "Bone Path" },
            { "moonslime.archaeology.dummy_path_glass", "Glass Path" },
            { "moonslime.archaeology.dummy_water_strainer", "Water Shifter" },
            { "moonslime.archaeology.h_display", "Hardwood Display" },
            { "moonslime.archaeology.w_display", "Wooden Display" },
            { "moonslime.archaeology.totem_volcano_warp", "Dwarf Gadget: Infinite Volcano Simulation" },
        };

        private static readonly Dictionary<string, string> EnglishToArchaeologyNamesMap = ArchaeologyToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public ArchaeologyNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return ArchaeologyToEnglishNamesMap.ContainsKey(internalName) ? ArchaeologyToEnglishNamesMap[internalName] : internalName;
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
