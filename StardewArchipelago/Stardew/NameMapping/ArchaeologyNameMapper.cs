using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class ArchaeologyNameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> ArchaeologyToEnglishNamesMap = new()
        {
            {"Rust Path", "Rusty Path"},
            {"Ancient Dwarven Volcano Simulator", "Dwarf Gadget: Infinite Volcano Simulation"},
            {"Rusted Scrap", "Scrap Rust"},
            {"moonslime.Archaeology.h_amphibian_fossil", "Hardwood Display: Amphibian Fossil"},
            {"Hardwood Display: Snake Vertabra", "Hardwood Display: Snake Vertebrae"},
            {"Hardwood Display: Strange Doll Green", "Hardwood Display: Strange Doll (Green)"},
            {"Hardwood Display: Strange Doll Yellow", "Hardwood Display: Strange Doll"},
            {"Hardwood Display: Trilobite", "Hardwood Display: Trilobite Fossil"},
            {"Wood Display", "Wooden Display"},
            {"Wooden Display: nautilus Fossil", "Wooden Display: Nautilus Fossil"},
            {"Wooden Display: prehistoric Handaxe", "Wooden Display: Prehistoric Handaxe"},
            {"Wooden Display: Prehistoric Vertabra", "Wooden Display: Prehistoric Vertebra"},
            {"Wooden Display: Snake Vertabra", "Wooden Display: Snake Vertebrae"},
            {"Wooden Display: Strange Doll Green", "Wooden Display: Strange Doll (Green)"},
            {"Wooden Display: Strange Doll Yellow", "Wooden Display: Strange Doll"},
            {"Wooden Display: Trilobite", "Wooden Display: Trilobite Fossil"},
        };

        private static readonly Dictionary<string, string> ArchaeologyCraftIDsToEnglishNamesMap = new()
        {
            {"moonslime.Archaeology.preservation_chamber", "Preservation Chamber"},
            {"moonslime.Archaeology.w_display", "Wooden Display"},
            {"moonslime.Archaeology.restoration_table", "Restoration Table"},
            {"moonslime.Archaeology.grinder", "Grinder"},
            {"moonslime.Archaeology.rust_path", "Rusty Path"},
            {"moonslime.Archaeology.glass_path", "Glass Path"},
            {"moonslime.Archaeology.rusty_brazier", "Rusty Brazier"},
            {"moonslime.Archaeology.bone_path", "Bone Path"},
            {"moonslime.Archaeology.glass_brazier", "Glass Brazier"},
            {"moonslime.Archaeology.h_preservation_chamber", "Hardwood Preservation Chamber"},
            {"moonslime.Archaeology.h_display", "Hardwood Display"},
            {"moonslime.Archaeology.ancient_battery", "Ancient Battery Production Station"},
            {"moonslime.Archaeology.glass_fence", "Glass Fence"},
            {"moonslime.Archaeology.lucky_ring", "Lucky Ring"},
            {"moonslime.Archaeology.water_shifter", "Water Shifter"},
            {"moonslime.Archaeology.bone_fence", "Bone Fence"},
            {"moonslime.Archaeology.totem_volcano_warp", "Dwarf Gadget: Infinite Volcano Simulation"},
        };

        private static readonly Dictionary<string, string> EnglishToArchaeologyNamesMap = ArchaeologyToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public ArchaeologyNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            var fixedWoodName = internalName.Replace("Wood Display: ", "Wooden Display: ");
            return   ArchaeologyToEnglishNamesMap.ContainsKey(fixedWoodName) ? ArchaeologyToEnglishNamesMap[fixedWoodName] : fixedWoodName;      
        }

        public string GetInternalName(string englishName)
        {
            var initialInternalName = EnglishToArchaeologyNamesMap.ContainsKey(englishName) ? EnglishToArchaeologyNamesMap[englishName] : englishName;
            return initialInternalName.Replace("Wooden Display: ", "Wood Display: ");
        }

        public string GetItemName(string recipeName)
        {
            var transformedIDName = ArchaeologyCraftIDsToEnglishNamesMap.ContainsKey(recipeName) ? ArchaeologyCraftIDsToEnglishNamesMap[recipeName] : recipeName;
            return GetEnglishName(transformedIDName);
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
