using System.Collections.Generic;

namespace StardewArchipelago.Constants
{
    public static class ArchaeologyCraftNames
    {
        public static List<string> ArchaeologyBigCraftables = new(){
            "moonslime.excavation.ancient_battery", "moonslime.excavation.glass_bazier", 
            "moonslime.excavation.grinder", "moonslime.excavation.preservation_chamber", 
            "moonslime.excavation.h_preservation_chamber"
        };

        public static Dictionary<string, string> CraftNames = new(){
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
    }
}