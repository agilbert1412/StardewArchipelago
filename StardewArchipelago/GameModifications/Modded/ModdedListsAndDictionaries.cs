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

        public readonly Dictionary<string, string> ArchaeologyAPNametoActualName = new(){
            { "Ancient Battery Production Station", "moonslime.excavation.ancient_battery" },
            { "Glass Bazier", "moonslime.excavation.glass_bazier" },
            { "Grinder", "moonslime.excavation.grinder" },
            { "Preservation Chamber", "moonslime.excavation.preservation_chamber" },
            { "Hardwood Preservation Chamber", "moonslime.excavation.h_preservation_chamber" },
            { "Glass Fence", "moonslime.excavation.glass_fence" },
            { "Bone Path", "moonslime.excavation.dummy_path_bone" },
            { "Glass Path", "moonslime.excavation.dummy_path_glass" },
            { "Water Shifter", "moonslime.excavation.dummy_water_strainer" },
            { "Hardwood Display", "moonslime.excavation.h_display" },
            { "Wooden Display", "moonslime.excavation.w_display" },
            { "Dwarf Gadget: Infinite Volcano Simulation", "moonslime.excavation.totem_volcano_warp" }
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