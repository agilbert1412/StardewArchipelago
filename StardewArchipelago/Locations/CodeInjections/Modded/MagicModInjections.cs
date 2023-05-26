    using System;
using System.Reflection;
using System.Collections.Generic;
using Netcode;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Unlocks;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public static class MagicModInjections
    {
        private const string LEARN_BLINK_AP_LOCATION = "Analyze All Toil School Locations";
        private const string LEARN_SPIRIT_AP_LOCATION = "Analyze All Eldritch School Locations";
        private const string LEARN_CLEARDEBRIS_AP_LOCATION = "Analyze: Clear Debris";
        private const string LEARN_TILL_AP_LOCATION = "Analyze: Till";
        private const string LEARN_WATER_AP_LOCATION = "Analyze: Water";
        private const string LEARN_EVAC_AP_LOCATION = "Analyze: Evac";
        private const string LEARN_HASTE_AP_LOCATION = "Analyze: Haste";
        private const string LEARN_HEAL_AP_LOCATION = "Analyze: Heal";
        private const string LEARN_BUFF_AP_LOCATION = "Analyze All Life School Locations";
        private const string LEARN_SHOCKWAVE_AP_LOCATION = "Analyze: Shockwave";
        private const string LEARN_FIREBALL_AP_LOCATION = "Analyze: Fireball";
        private const string LEARN_FROSTBITE_AP_LOCATION = "Analyze: Frostbite";
        private const string LEARN_TELEPORT_AP_LOCATION = "Analyze All Elemental School Locations";
        private const string LEARN_LANTERN_AP_LOCATION = "Analyze: Lantern";
        private const string LEARN_TENDRILS_AP_LOCATION = "Analyze: Tendrils";
        private const string LEARN_PHOTOSYNTHESIS_AP_LOCATION = "Analyze All Nature School Locations";
        private const string LEARN_DESCEND_AP_LOCATION = "Analyze: Descend";
        private const string LEARN_METEOR_AP_LOCATION = "Analyze: Meteor";
        private const string LEARN_LUCKSTEAL_AP_LOCATION = "Analyze: Lucksteal";
        private const string LEARN_BLOODMANA_AP_LOCATION = "Analyze: Bloodmana";
        private const string LEARN_REWIND_AP_LOCATION = "Analyze Every Magic School Location";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;


        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool OnCast_AnalyzeGivesLocations_Prefix(Farmer player, int level, int targetX, int targetY)
        {
            try
            {
                if (player != Game1.player)
                    return false;
                List<string> spellsLearnedList = new List<string>();
                if (player.CurrentItem != null)
                {
                    if (player.CurrentTool != null)
                    {
                        if (player.CurrentTool is StardewValley.Tools.Axe || player.CurrentTool is StardewValley.Tools.Pickaxe)
                            spellsLearnedList.Add(LEARN_CLEARDEBRIS_AP_LOCATION);
                        else if (player.CurrentTool is StardewValley.Tools.Hoe)
                            spellsLearnedList.Add(LEARN_TILL_AP_LOCATION);
                        else if (player.CurrentTool is StardewValley.Tools.WateringCan)
                            spellsLearnedList.Add(LEARN_WATER_AP_LOCATION);
                    }
                    else if (player.CurrentItem is StardewValley.Objects.Boots)
                    {
                        spellsLearnedList.Add(LEARN_EVAC_AP_LOCATION);
                    }
                    else if (player.ActiveObject != null)
                    {
                        if (!player.ActiveObject.bigCraftable.Value)
                        {
                            int index = player.ActiveObject.ParentSheetIndex;
                            if (index == 395) // Coffee
                                spellsLearnedList.Add(LEARN_HASTE_AP_LOCATION);
                            else if (index == 773) // Life elixir
                                spellsLearnedList.Add(LEARN_HEAL_AP_LOCATION);
                            else if (index == 86) // Earth crystal
                                spellsLearnedList.Add(LEARN_SHOCKWAVE_AP_LOCATION);
                            else if (index == 82) // Fire quartz
                                spellsLearnedList.Add(LEARN_FIREBALL_AP_LOCATION);
                            else if (index == 161) // Ice Pip
                                spellsLearnedList.Add(LEARN_FROSTBITE_AP_LOCATION);
                        }
                    }
                }
                foreach (var lightSource in player.currentLocation.sharedLights.Values)
                {
                    if (Utility.distance(targetX, lightSource.position.X, targetY, lightSource.position.Y) < lightSource.radius.Value * Game1.tileSize)
                    {
                        spellsLearnedList.Add(LEARN_LANTERN_AP_LOCATION);
                        break;
                    }
                }
                var tilePos = new Vector2(targetX / Game1.tileSize, targetY / Game1.tileSize);
                if (player.currentLocation.terrainFeatures.ContainsKey(tilePos) && player.currentLocation.terrainFeatures[tilePos] is StardewValley.TerrainFeatures.HoeDirt hd)
                {
                    if (hd.crop != null)
                        spellsLearnedList.Add(LEARN_TENDRILS_AP_LOCATION);
                }
                var tile = player.currentLocation.map.GetLayer("Buildings").Tiles[(int)tilePos.X, (int)tilePos.Y];
                if (tile != null && tile.TileIndex == 173)
                    spellsLearnedList.Add(LEARN_DESCEND_AP_LOCATION);
                if (player.currentLocation is Farm farm)
                {
                    foreach (var clump in farm.resourceClumps)
                    {
                        if (clump.parentSheetIndex.Value == 622 && new Rectangle((int)clump.tile.Value.X, (int)clump.tile.Value.Y, clump.width.Value, clump.height.Value).Contains((int)tilePos.X, (int)tilePos.Y))
                            spellsLearnedList.Add(LEARN_METEOR_AP_LOCATION);
                    }
                }
                if (player.currentLocation.doesTileHaveProperty((int)tilePos.X, (int)tilePos.Y, "Action", "Buildings") == "EvilShrineLeft")
                    spellsLearnedList.Add(LEARN_LUCKSTEAL_AP_LOCATION);
                if (player.currentLocation is StardewValley.Locations.MineShaft ms && ms.mineLevel == 100 && ms.waterTiles[(int)tilePos.X, (int)tilePos.Y])
                    spellsLearnedList.Add(LEARN_BLOODMANA_AP_LOCATION);

                for (int i = spellsLearnedList.Count - 1; i >= 0; --i)
                    if (_locationChecker.IsLocationChecked(spellsLearnedList[i]))
                        spellsLearnedList.RemoveAt(i);
                if (spellsLearnedList.Count > 0)
                {
                    Game1.playSound("secret1");
                    foreach (var spell in spellsLearnedList)
                    {
                        _locationChecker.AddCheckedLocation(spell);
                    }
                }
                if (KnowsAllToilSpells() & _locationChecker.IsLocationNotChecked(LEARN_BLINK_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(LEARN_BLINK_AP_LOCATION);
                }
                if (KnowsAllEldrichSpells() & _locationChecker.IsLocationNotChecked(LEARN_SPIRIT_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(LEARN_SPIRIT_AP_LOCATION);
                }
                if (KnowsAllElementalSpells() & _locationChecker.IsLocationNotChecked(LEARN_TELEPORT_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(LEARN_TELEPORT_AP_LOCATION);
                }
                if (KnowsAllLifeSpells() & _locationChecker.IsLocationNotChecked(LEARN_BUFF_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(LEARN_BUFF_AP_LOCATION);
                }
                if (KnowsAllNatureSpells() & _locationChecker.IsLocationNotChecked(LEARN_PHOTOSYNTHESIS_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(LEARN_PHOTOSYNTHESIS_AP_LOCATION);
                }
                if (KnowsAllSpellsButRewind() & _locationChecker.IsLocationNotChecked(LEARN_REWIND_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(LEARN_REWIND_AP_LOCATION);
                }
                return false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(OnCast_AnalyzeGivesLocations_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
            
        }

        private static bool KnowsAllToilSpells()
        {
            if (_locationChecker.IsLocationChecked(LEARN_TILL_AP_LOCATION) 
            & _locationChecker.IsLocationChecked(LEARN_CLEARDEBRIS_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_WATER_AP_LOCATION))
            {
                return true;
            }
            return false;
        }
        private static bool KnowsAllEldrichSpells()
        {
            if (_locationChecker.IsLocationChecked(LEARN_METEOR_AP_LOCATION) 
            & _locationChecker.IsLocationChecked(LEARN_BLOODMANA_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_LUCKSTEAL_AP_LOCATION))
            {
                return true;
            }
            return false;
        }
        private static bool KnowsAllElementalSpells()
        {
            if (_locationChecker.IsLocationChecked(LEARN_FIREBALL_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_FROSTBITE_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_DESCEND_AP_LOCATION))
            {
                return true;
            }
            return false;
        }
        private static bool KnowsAllLifeSpells()
        {
            if (_locationChecker.IsLocationChecked(LEARN_EVAC_AP_LOCATION) 
            & _locationChecker.IsLocationChecked(LEARN_HEAL_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_HASTE_AP_LOCATION))
            {
                return true;
            }
            return false;
        }
        private static bool KnowsAllNatureSpells()
        {
            if (_locationChecker.IsLocationChecked(LEARN_TENDRILS_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_SHOCKWAVE_AP_LOCATION)
            & _locationChecker.IsLocationChecked(LEARN_LANTERN_AP_LOCATION))
            {
                return true;
            }
            return false;
        }
        private static bool KnowsAllSpellsButRewind()
        {
            if (KnowsAllEldrichSpells() & KnowsAllElementalSpells() & KnowsAllLifeSpells() & KnowsAllNatureSpells()
            & KnowsAllToilSpells())
            {
                return true;
            }
            return false;
        }
    }
}