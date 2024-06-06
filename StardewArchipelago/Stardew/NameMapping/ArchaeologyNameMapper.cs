using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class ArchaeologyNameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> ArchaeologyToEnglishNamesMap = new()
        {
            { "moonslime.Archaeology.ancient_battery", "Ancient Battery Production Station" },
            { "moonslime.Archaeology.glass_brazier", "Glass Brazier" },
            { "moonslime.Archaeology.grinder", "Grinder" },
            { "moonslime.Archaeology.preservation_chamber", "Preservation Chamber" },
            { "moonslime.Archaeology.h_preservation_chamber", "Hardwood Preservation Chamber" },
            { "moonslime.Archaeology.glass_fence", "Glass Fence" },
            { "moonslime.Archaeology.dummy_path_bone", "Bone Path" },
            { "moonslime.Archaeology.dummy_path_glass", "Glass Path" },
            { "moonslime.Archaeology.dummy_water_strainer", "Water Shifter" },
            { "moonslime.Archaeology.h_display", "Hardwood Display" },
            { "moonslime.Archaeology.w_display", "Wooden Display" },
            { "moonslime.Archaeology.totem_volcano_warp", "Dwarf Gadget: Infinite Volcano Simulation" },
            { "moonslime.Archaeology.restoration_table", "Restoration Table" },
            { "moonslime.Archaeology.rust_path", "Rusty Path" },
            { "moonslime.Archaeology.rusty_scrap", "Scrap Rust" },
            { "moonslime.Archaeology.skill_book", "Digging Like Worms" },
            { "moonslime.Archaeology.diggers_delight", "Digger's Delight" },
            { "moonslime.Archaeology.rocky_root", "Rocky Root Coffee" },
            { "moonslime.Archaeology.ancient_jello", "Ancient Jello" },
            { "moonslime.Archaeology.rusty_brazier", "Rusty Brazier" },
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
