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

    private static readonly Dictionary<string, Dictionary<string, string>> _locationMods = new()
    {
    };

    public static void IncludeModLocationAlias(Dictionary<string, string> aliases, SlotData slotData)
    {
        CompleteModLocations();
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

    private static void CompleteModLocations()
    {
        _locationMods[ModNames.ALEC] = _locationAlec;
        _locationMods[ModNames.AYEISHA] = _locationAyeisha;
        _locationMods[ModNames.DEEP_WOODS] =_locationDeepWoods;
        _locationMods[ModNames.EUGENE] = _locationEugene;
        _locationMods[ModNames.JASPER] = _locationJasper;
        _locationMods[ModNames.JUNA] = _locationJuna;
        _locationMods[ModNames.SVE] = _locationSVE;
        _locationMods[ModNames.YOBA] = _locationYoba;
    }


    private static readonly Dictionary<string, string> _locationSVE = new()
    {
            { "Grandpa's Shed Interior", "Custom_GrandpasShed"},
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
            { "Highlands Cavern", "Custom_HighlandsCavern"},
            { "Highlands", "Custom_Highlands"},
            { "Drawf Prison", "Custom_HighlandsCavernPrison"},
            { "Lance's House Ladder", "Custom_Highlands|30|30"},
            { "Lance's House Main", "Custom_HighlandsOutpost"},
            { "Aurora Vineyard Basement", "Custom_ApplesRoom"},
            { "Aurora Vineyard", "Custom_AuroraVineyard"},
            { "Sprite Spring", "Custom_SpriteSpring2"},
            { "Junimo Woods", "Custom_JunimoWoods"},
            { "Badlands Entrance", "Custom_DesertRailway"},
            { "Enchanted Grove", "Custom_EnchantedGrove"},
            { "Galmoran Outpost Roof", "Custom_CastleVillageOutpost|31|27"},
            { "Galmoran Outpost", "Custom_CastleVillageOutpost"},
            { "Crimson Badlands", "Custom_CrimsonBadlands"},
            { "Badlands Cave", "Custom_TreasureCave"},
            { "Susan's House", "Custom_SusanHouse"},
            { "Adventure's Guild Summit", "Custom_AdventurerSummit"},
            { "Marlon's Boat", "Custom_AdventurersSummit"},
            { "Forest West", "Custom_ForestWest"},
            { "First Slash Hallway", "Custom_FirstSlashHallway"},
            { "First Slash Spare Bedroom", "Custom_FirstSlashGuestRoom"}
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
            
}