using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;

public class ModEntranceManager
{

    public static List<string> GrandpaShedEdgeCase = new(){
        "Custom_GrandpasShed to Custom_GrandpasShedGreenhouse", "Custom_GrandpasShedGreenhouse to Custom_GrandpasShed"
    };

    private static readonly Dictionary<string, string> _entranceSVE = new()
    {
    };

    private static readonly Dictionary<string, string> _locationSVE = new()
    {
            { "Grandpa's Shed Interior", "Custom_GrandpasShedRuins"},
            { "Grandpa's Shed Upstairs", "Custom_GrandpasShedGreenhouse"},
            { "Grandpa's Shed", "Custom_GrandpasShedOutside"},
            { "Marnie's Shed", "Custom_MarnieShed"},
            { "Fairhaven Farm Cellar", "Custom_AndyCellar"},
            { "Fairhaven Farm", "Custom_AndyHouse"},
            { "Blue Moon Vineyard", "Custom_BlueMoonVineyard"},
            { "Sophia's House", "Custom_SophiaHouse"},
            { "Sophia's Cellar", "Custom_SophiaCellar"},
            { "Jenkins' Residence", "Custom_JenkinsHouse"},
            { "Jenkins' Cellar", "Custom_OliviaCellar"},
            { "Unclaimed Plot", "Custom_TownEast"},
            { "Shearwater Bridge", "Custom_ShearwaterBridge"},
            { "Fable Reef", "Custom_FableReef"},
            { "First Slash Guild", "Custom_FirstSlashGuild"},
            { "Highlands", "Custom_Highlands"},
            { "Lance's House Ladder", "Custom_HighlandsOutpost|12|5"},
            { "Lance's House Main", "Custom_HighlandsOutpost|7|9"},
            { "Aurora Vineyard Basement", "Custom_ApplesRoom"},
            { "Aurora Vineyard", "Custom_AuroraVineyard"},
            { "Sprite Spring Cave", "Custom_SpriteSpringCave"},
            { "Sprite Spring", "Custom_SpriteSpring2"},
            { "Lost Woods", "Custom_JunimoWoods|37|2"},
            { "Junimo Woods", "Custom_JunimoWoods"},
            { "Badlands Entrance", "Custom_DesertRailway"},
            { "Enchanted Grove", "Custom_EnchantedGrove"},
            { "Grove Aurora Vineyard Warp", "Custom_EnchantedGrove|20|41"},
            { "Grove Junimo Woods Warp", "Custom_EnchantedGrove|40|41"},
            { "Grove Outpost Warp", "Custom_EnchantedGrove|40|10"},
            { "Grove Farm Warp", "Custom_EnchantedGrove|30|14"},
            { "Grove Wizard Warp", "Custom_EnchantedGrove|17|25"},
            { "Grove Adventurer's Guild Warp", "Custom_EnchantedGrove|43|25"},
            { "Grove Sprite Spring Warp", "Custom_EnchantedGrove|20|10"},
            { "Galmoran Outpost", "Custom_CastleVillageOutpost"},
            { "Crimson Badlands", "Custom_CrimsonBadlands"},
            { "Badlands Cave", "Custom_TreasureCave"},
            { "Susan's House", "Custom_SusanHouse"},
            { "Adventure's Guild Summit", "Custom_AdventurerSummit"},
            { "Marlon's Boat", "Custom_AdventurersSummit"},
            { "Forest West", "Custom_ForestWest"},
            { "First Slash Hallway", "Custom_FirstSlashHallway"},
            { "First Slash Spare Room", "Custom_FirstSlashGuestRoom"},
            { "Grampleton Suburbs", "Custom_GrampletonSuburbs"},
            { "Scarlett's House", "Custom_ScarlettHouse"}
    };
    private static readonly Dictionary<string, string> _locationEugene = new()
    {
        { "Eugene's Garden", "Custom_EugeneNPC_EugeneHouse" },
        { "Eugene's Bedroom", "Custom_EugeneNPC_EugeneRoom" },
    };

    private static readonly Dictionary<string, string> _locationDeepWoods = new()
    {
        { "Deep Woods House", "DeepWoodsMaxHouse" },
    };

    private static readonly Dictionary<string, string> _locationAlec = new()
    {
        { "Alec's Pet Shop", "Custom_AlecsPetShop" },
        { "Alec's Bedroom", "Custom_AlecsRoom" },
    };

    private static readonly Dictionary<string, string> _locationJuna = new()
    {
        { "Juna's Cave", "Custom_JunaNPC_JunaCave" },
    };

    private static readonly Dictionary<string, string> _locationAyeisha = new()
    {
        { "Ayeisha's Mail Van", "Custom_AyeishaVanRoad" },
    };

    private static readonly Dictionary<string, string> _locationJasper = new()
    {
        { "Jasper's Bedroom", "Custom_LK_Museum2" },
    };
    
    private static readonly Dictionary<string, string> _locationYoba = new()
    {
        { "Yoba's Clearing", "Custom_Woods3" },
    };                

    private static readonly Dictionary<string, Dictionary<string, string>> _locationMods = new()
    {
        { ModNames.ALEC, _locationAlec },
        { ModNames.AYEISHA, _locationAyeisha },
        { ModNames.DEEP_WOODS, _locationDeepWoods },
        { ModNames.EUGENE, _locationEugene },
        { ModNames.JASPER, _locationJasper },
        { ModNames.JUNA, _locationJuna },
        { ModNames.SVE, _locationSVE },
        { ModNames.YOBA, _locationYoba },
    };

    private static readonly Dictionary<string, Dictionary<string, string>> _entranceMods = new()
    {
        {ModNames.SVE, _entranceSVE}
    };

    public static void IncludeModLocationAlias(Dictionary<string, string> aliases, SlotData slotData)
    {
        foreach (var mod in _locationMods)
        {
            if (slotData.Mods.HasMod(mod.Key))
            {
                foreach (var kvp in mod.Value)
                {
                    aliases.Add(kvp.Key, kvp.Value);
                }
                if (mod.Key == ModNames.SVE)
                {
                    aliases["Wizard Basement"] = "Custom_WizardBasement";
                }
            }
        }
    }

    public static void IncludeModEntranceAlias(Dictionary<string, string> aliases, SlotData slotData)
    {
        foreach (var mod in _entranceMods)
        {
            if (slotData.Mods.HasMod(mod.Key))
            {
                foreach (var kvp in mod.Value)
                {
                    aliases.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }

    public static bool CheckForGrandpasShedGreenhouseEdgeCase(string currentLocationName, string locationRequestName)
    {
        var case1 = currentLocationName == "Custom_GrandpasShed" & locationRequestName == "Custom_GrandpasShedGreenhouse";
        var case2 = locationRequestName == "Custom_GrandpasShed" & currentLocationName == "Custom_GrandpasShedGreenhouse";
        if (case1 | case2)
        {
            return true;
        }
        return false;

    }

            
}