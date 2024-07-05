using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Stardew.NameMapping
{
    public class ArchaeologyNameMapper : INameMapper, IRecipeNameMapper
    {
        private static readonly Dictionary<string, string> ArchaeologyToEnglishNamesMap = new()
        {
            {"Water Shifter", "Water Sifter"},
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
