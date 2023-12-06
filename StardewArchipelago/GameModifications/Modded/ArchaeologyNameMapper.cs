using System;
using System.Linq;
using System.Collections.Generic;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.GameModifications.Modded
{
    public class ArchaeologyNameMapper
    {
        private static Dictionary<string, string> InternalToEnglishNamesMap = new(){
            {"moonslime.excavation.ancient_battery", "Ancient Battery Production Station"},
            {"moonslime.excavation.glass_bazier", "Glass Bazier"},
            {"moonslime.excavation.grinder", "Grinder"},
            {"moonslime.excavation.preservation_chamber", "Preservation Chamber"},
            {"moonslime.excavation.h_preservation_chamber", "hardwood Preservation Chamber"},
            {"moonslime.excavation.glass_fence", "Glass Fence"},
            {"moonslime.excavation.dummy_path_bone", "Bone Path"},
            {"moonslime.excavation.dummy_path_glass", "Glass Path"},
            {"moonslime.excavation.dummy_water_strainer", "Water Shifter"},
            {"moonslime.excavation.h_display", "Hardwood Display"},
            {"moonslime.excavation.w_display", "Wooden Display"},
            {"moonslime.excavation.totem_volcano_warp", "Dwarf Gadget: Infinite Volcano Simulation"}
        };

        private static Dictionary<string, string> EnglishToInternalNamesMap = InternalToEnglishNamesMap.ToDictionary(x => x.Value, x => x.Key);

        public ArchaeologyNameMapper()
        {
        }

        public string GetEnglishName(string internalName)
        {
            return InternalToEnglishNamesMap.ContainsKey(internalName) ? InternalToEnglishNamesMap[internalName] : internalName;
        }

        public string GetInternalName(string englishName)
        {
            return EnglishToInternalNamesMap.ContainsKey(englishName) ? EnglishToInternalNamesMap[englishName] : englishName;
        }

        public bool RecipeNeedsMapping(string itemOfRecipe)
        {
            return EnglishToInternalNamesMap.ContainsKey(itemOfRecipe);
        }
    }
}