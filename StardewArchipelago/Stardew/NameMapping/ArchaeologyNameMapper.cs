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
            {"Hardwood Display: Strange Doll Green", "Hardwood Display: Strange Doll"},
            {"Hardwood Display: Strange Doll Yellow", "Hardwood Display: Strange Doll"},
            {"Hardwood Display: Trilobite", "Hardwood Display: Trilobite Fossil"},
            {"Wood Display: Amphibian Fossil", "Wooden Display: Amphibian Fossil"},
            {"Wood Display: Anchor", "Wooden Display: Anchor"},
            {"Wood Display: Ancient Doll", "Wooden Display: Ancient Doll"},
            {"Wood Display: Ancient Drum", "Wooden Display: Ancient Drum"},
            {"Wood Display: Ancient Seed", "Wooden Display: Ancient Seed"},
            {"Wood Display: Ancient Sword", "Wooden Display: Ancient Sword"},
            {"Wood Display: Arrowhead", "Wooden Display: Arrowhead"},
            {"Wood Display: Bone Flute", "Wooden Display: Bone Flute"},
            {"Wood Display: Chewing Stick", "Wooden Display: Chewing Stick"},
            {"Wood Display: Chicken Statue", "Wooden Display: Chicken Statue"},
            {"Wood Display: Chipped Amphora", "Wooden Display: Chipped Amphora"},
            {"Wood Display: Dinosaur Egg", "Wooden Display: Dinosaur Egg"},
            {"Wood Display", "Wooden Display"},
            {"Wood Display: Dried Starfish", "Wooden Display: Dried Starfish"},
            {"Wood Display: Dwarf Gadget", "Wooden Display: Dwarf Gadget"},
            {"Wood Display: Dwarf Scroll I", "Wooden Display: Dwarf Scroll I"},
            {"Wood Display: Dwarf Scroll II", "Wooden Display: Dwarf Scroll II"},
            {"Wood Display: Dwarf Scroll III", "Wooden Display: Dwarf Scroll III"},
            {"Wood Display: Dwarf Scroll IV", "Wooden Display: Dwarf Scroll IV"},
            {"Wood Display: Dwarvish Helm", "Wooden Display: Dwarvish Helm"},
            {"Wood Display: Elvish Jewelry", "Wooden Display: Elvish Jewelry"},
            {"Wood Display: Fossilized Leg", "Wooden Display: Fossilized Leg"},
            {"Wood Display: Fossilized Ribs", "Wooden Display: Fossilized Ribs"},
            {"Wood Display: Fossilized Skull", "Wooden Display: Fossilized Skull"},
            {"Wood Display: Fossilized Spine", "Wooden Display: Fossilized Spine"},
            {"Wood Display: Fossilized Tail", "Wooden Display: Fossilized Tail"},
            {"Wood Display: Glass Shards", "Wooden Display: Glass Shards"},
            {"Wood Display: Golden Mask", "Wooden Display: Golden Mask"},
            {"Wood Display: Golden Relic", "Wooden Display: Golden Relic"},
            {"Wood Display: Mummified Bat", "Wooden Display: Mummified Bat"},
            {"Wood Display: Mummified Frog", "Wooden Display: Mummified Frog"},
            {"Wood Display: nautilus Fossil", "Wooden Display: Nautilus Fossil"},
            {"Wood Display: Ornamental Fan", "Wooden Display: Ornamental Fan"},
            {"Wood Display: Palm Fossil", "Wooden Display: Palm Fossil"},
            {"Wood Display: prehistoric Handaxe", "Wooden Display: Prehistoric Handaxe"},
            {"Wood Display: Prehistoric Rib", "Wooden Display: Prehistoric Rib"},
            {"Wood Display: Prehistoric Scapula", "Wooden Display: Prehistoric Scapula"},
            {"Wood Display: Prehistoric Skull", "Wooden Display: Prehistoric Skull"},
            {"Wood Display: Prehistoric Tibia", "Wooden Display: Prehistoric Tibia"},
            {"Wood Display: Prehistoric Tool", "Wooden Display: Prehistoric Tool"},
            {"Wood Display: Prehistoric Vertabra", "Wooden Display: Prehistoric Vertebra"},
            {"Wood Display: Rare Disc", "Wooden Display: Rare Disc"},
            {"Wood Display: Rusty Cog", "Wooden Display: Rusty Cog"},
            {"Wood Display: Rusty Spoon", "Wooden Display: Rusty Spoon"},
            {"Wood Display: Rusty Spur", "Wooden Display: Rusty Spur"},
            {"Wood Display: Skeletal Hand", "Wooden Display: Skeletal Hand"},
            {"Wood Display: Skeletal Tail", "Wooden Display: Skeletal Tail"},
            {"Wood Display: Snake Skull", "Wooden Display: Snake Skull"},
            {"Wood Display: Snake Vertabra", "Wooden Display: Snake Vertebrae"},
            {"Wood Display: Strange Doll Green", "Wooden Display: Strange Doll"},
            {"Wood Display: Strange Doll Yellow", "Wooden Display: Strange Doll"},
            {"Wood Display: Trilobite", "Wooden Display: Trilobite Fossil"},
            /*{ "moonslime.Archaeology.ancient_battery", "Ancient Battery Production Station" },
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
            { "moonslime.Archaeology.rusty_brazier", "Rusty Brazier" },*/
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
            return itemName;
        }

        public bool RecipeNeedsMapping(string itemName)
        {
            return EnglishToArchaeologyNamesMap.ContainsKey(itemName);
        }
    }
}
