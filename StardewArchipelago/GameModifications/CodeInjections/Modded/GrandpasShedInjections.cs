using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Network;
using xTile.Tiles;

namespace StardewArchipelago.GameModifications.CodeInjections.Modded
{
    public class GrandpasShedInjections
    {
        private static Point ChestTileLeft = new(17, 9);
        private static readonly string GRANDPA_SHED_RUINS = "Custom_GrandpaShedRuins";
        private static readonly string GRANDPA_SHED = "Custom_GrandpaShed";
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public virtual void MakeMapModifications(bool force = false)
        public static bool MakeMapModifications_PlaceRobinChest_Prefix(GameLocation __instance, bool force)
        {
            try
            {
                if (__instance.Name != GRANDPA_SHED_RUINS && __instance.Name != GRANDPA_SHED)
                {
                    return true; // run original logic
                }

                var abandonedJojaMart = Game1.getLocationFromName(GRANDPA_SHED_RUINS);
                var chestTileLeft = ChestTileLeft;

                if (Game1.MasterPlayer.hasOrWillReceiveMail("apShedRepaired"))
                {
                    __instance.removeTile(chestTileLeft.X, chestTileLeft.Y, "Buildings");
                    __instance.removeTile(chestTileLeft.X+1, chestTileLeft.Y, "Buildings");
                    return false; // don't run original logic
                }

                if (__instance.map.TileSheets.Count < 3)
                {
                    var abandonedJojaIndoorTileSheet = abandonedJojaMart.map.GetTileSheet("indoor");

                    // aaa is to make it get sorted to index 0, because the dumbass CC assumes the first tilesheet is the correct one
                    var indoorTileSheet = new TileSheet("aaa" + abandonedJojaIndoorTileSheet.Id, __instance.map, abandonedJojaIndoorTileSheet.ImageSource, abandonedJojaIndoorTileSheet.SheetSize, abandonedJojaIndoorTileSheet.TileSize);
                    __instance.map.AddTileSheet(indoorTileSheet);
                }

                var junimoNoteTileFrames = CommunityCenter.getJunimoNoteTileFrames(0, __instance.map);
                var layerId = "Buildings";
                __instance.map.GetLayer(layerId).Tiles[chestTileLeft.X, chestTileLeft.Y] = new AnimatedTile(__instance.map.GetLayer(layerId), junimoNoteTileFrames, 70L);

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(MakeMapModifications_PlaceRobinChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public bool checkTileIndexAction(int tileIndex)
        public static bool CheckTileIndexAction_InteractWithMissingBundleNote_Prefix(GameLocation __instance, int tileIndex, ref bool __result)
        {
            try
            {
                if (__instance.Name != GRANDPA_SHED_RUINS && __instance.Name != GRANDPA_SHED)
                {
                    return true; // run original logic
                }

                switch (tileIndex)
                {
                    // I think these are... bundle animation sprites... yeah wtf
                    case 1799:
                    case 1824:
                    case 1825:
                    case 1826:
                    case 1827:
                    case 1828:
                    case 1829:
                    case 1830:
                    case 1831:
                    case 1832:
                    case 1833:
                        // Game1.activeClickableMenu = (IClickableMenu) new JunimoNoteMenu(6, (Game1.getLocationFromName("CommunityCenter") as CommunityCenter).bundlesDict())
                    
                        var oldShed = Game1.getLocationFromName("Custom_GrandpaShedRuins");
                        var newShed = Game1.getLocationFromName("Custom_GrandpaShed");
                        var thePlayer = Game1.getAllFarmers().FirstOrDefault(x => x.currentLocation == oldShed || x.currentLocation == newShed);
                        // oldShed.checkAction(ChestTileLeft, ChestTileLeft.Y, thePlayer);
                        __result = true;
                        return false; // don't run original logic
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckTileIndexAction_InteractWithMissingBundleNote_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }


    }
}