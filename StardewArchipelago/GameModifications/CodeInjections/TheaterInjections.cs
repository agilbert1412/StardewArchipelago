﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Tiles;
using StardewValley.Extensions;
using StardewArchipelago.Locations.Jojapocalypse;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class TheaterInjections
    {
        private const string MOVIE_THEATER_MAIL = "ccMovieTheater";
        private const string ABANDONED_JOJA_MART = "AbandonedJojaMart";
        private const string MOVIE_THEATER = "MovieTheater";
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        private static Point GetMissingBundleTile(GameLocation location) => location is MovieTheater ? new Point(17, 8) : new Point(8, 8);

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public virtual void MakeMapModifications(bool force = false)
        public static bool MakeMapModifications_PlaceMissingBundleNote_Prefix(GameLocation __instance, bool force)
        {
            try
            {
                if (__instance.Name != ABANDONED_JOJA_MART && __instance.Name != MOVIE_THEATER || Game1.player.hasOrWillReceiveMail(JojaConstants.MEMBERSHIP_MAIL))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (force)
                {
                    // __instance._appliedMapOverrides.Clear();
                }
                __instance.interiorDoors.MakeMapModifications();

                var abandonedJojaMart = Game1.getLocationFromName(ABANDONED_JOJA_MART);
                var junimoNotePoint = GetMissingBundleTile(__instance);

                if (Game1.MasterPlayer.hasOrWillReceiveMail("apccMovieTheater"))
                {
                    __instance.removeTile(junimoNotePoint.X, junimoNotePoint.Y, "Buildings");
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (__instance.map.TileSheets.All(x => x.Id != "indoor"))
                {
                    // The MovieTheater doesn't have an "indoor" layer, but it needs one to pull the tilesheet from in the CC method below. So we just duplicate the one from the abandoned joja mart.
                    var abandonedJojaIndoorTileSheet = abandonedJojaMart.map.GetTileSheet("indoor");
                    var indoorTileSheet = new TileSheet(abandonedJojaIndoorTileSheet.Id, __instance.map, abandonedJojaIndoorTileSheet.ImageSource, abandonedJojaIndoorTileSheet.SheetSize, abandonedJojaIndoorTileSheet.TileSize);
                    __instance.map.AddTileSheet(indoorTileSheet);
                }

                var junimoNoteTileFrames = CommunityCenter.getJunimoNoteTileFrames(0, __instance.map);
                var layerId = "Buildings";
                // __instance.map.GetLayer(layerId).Tiles[junimoNotePoint.X, junimoNotePoint.Y] = new AnimatedTile(__instance.map.GetLayer(layerId), junimoNoteTileFrames, 70L);
                var layer = __instance.map.RequireLayer(layerId);
                layer.Tiles[junimoNotePoint.X, junimoNotePoint.Y] = new AnimatedTile(layer, junimoNoteTileFrames, 70L);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_PlaceMissingBundleNote_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool checkTileIndexAction(int tileIndex)
        public static bool CheckTileIndexAction_InteractWithMissingBundleNote_Prefix(GameLocation __instance, int tileIndex, ref bool __result)
        {
            try
            {
                if (__instance.Name != ABANDONED_JOJA_MART && __instance.Name != MOVIE_THEATER || Game1.player.hasOrWillReceiveMail(JojaConstants.MEMBERSHIP_MAIL))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
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
                        ((AbandonedJojaMart)(Game1.getLocationFromName("AbandonedJojaMart"))).checkBundle();
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckTileIndexAction_InteractWithMissingBundleNote_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // private void doRestoreAreaCutscene()
        public static bool DoRestoreAreaCutscene_InteractWithMissingBundleNote_Prefix(AbandonedJojaMart __instance)
        {
            try
            {
                // Game1.player.freezePause = 1000;
                var junimoNotePoint = GetMissingBundleTile(__instance);
                DelayedAction.removeTileAfterDelay(junimoNotePoint.X, junimoNotePoint.Y, 100, Game1.currentLocation, "Buildings");

                Game1.addMailForTomorrow("apccMovieTheater", true, true);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoRestoreAreaCutscene_InteractWithMissingBundleNote_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static void UpdateScheduleForEveryone()
        {
            foreach (var character in Utility.getAllCharacters())
            {
                if (character.IsVillager)
                {
                    character.TryLoadSchedule();
                }
            }
        }

        // private bool changeScheduleForLocationAccessibility(ref string locationName, ref int tileX, ref int tileY, ref int facingDirection)
        public static bool ChangeScheduleForLocationAccessibility_JojamartAndTheater_Prefix(NPC __instance, ref string locationName, ref int tileX, ref int tileY, ref int facingDirection, ref bool __result)
        {
            try
            {
                if (locationName is "Railroad" or "CommunityCenter")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (locationName != "JojaMart" || !_archipelago.HasReceivedItem(APItem.MOVIE_THEATER))
                {
                    // no fallback
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (!__instance.hasMasterScheduleEntry(locationName + "_Replacement"))
                {
                    // Fallback on the default schedule
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var strArray = __instance.getMasterScheduleEntry(locationName + "_Replacement").Split(' ');
                locationName = strArray[0];
                tileX = Convert.ToInt32(strArray[1]);
                tileY = Convert.ToInt32(strArray[2]);
                facingDirection = Convert.ToInt32(strArray[3]);

                // no fallback
                __result = false;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChangeScheduleForLocationAccessibility_JojamartAndTheater_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
