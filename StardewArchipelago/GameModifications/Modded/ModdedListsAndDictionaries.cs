using System.Collections.Generic;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class ModdedListsAndDictionaries
    {
        // Just a repository of lists and dictionaries in some hope to keep it out of vanilla code.
        public readonly List<string> FriendsNotMetImmediatelyButModded = new(){
            "Yoba", "Lance", "Apples", "Scarlett", "Morgan", "Zic", "Alecto", "Gunther",
            "Marlon", "Gregory"
        };

        public readonly Dictionary<string, string> ArchaeologyNameToDisplayName = new(){
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

        public readonly List<string> IgnoredModShipments = new(){
            "Galmoran Gem", "Ancient Hilt", "Ancient Blade", "Ancient Doll Legs", "Ancient Doll Body", "Prismatic Shard Piece 3", 
            "Mask Piece 1", "Mask Piece 2", "Mask Piece 3", "Prismatic Shard Piece 1", "Prismatic Shard Piece 2", "Prismatic Shard Piece 4", 
            "Chipped Amphora Piece 1", "Chipped Amphora Piece 2", 
        };

        public readonly List<string> IgnoredQuestsModded = new(){
            "Transgressions"
        };

        public readonly List<string> IgnoredSpecialOrdersModded = new(){
            "Aurora Vineyard", "Monster Crops"
        };

        public readonly List<string> IgnoredCraftablesModded = new(){
            "Restore Prismatic Shard", "Restore Golden Mask", "Restore Ancient Sword", "Restore Ancient Doll", "Restore Chipped Amphora"
        };

    }
}