using System;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Tiles;
using StardewValley.Extensions;

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
                if (__instance.Name != ABANDONED_JOJA_MART && __instance.Name != MOVIE_THEATER)
                {
                    return true; // run original logic
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
                    return false; // don't run original logic
                }

                if (__instance.map.TileSheets.Count < 3)
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

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_PlaceMissingBundleNote_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        // public bool checkTileIndexAction(int tileIndex)
        public static bool CheckTileIndexAction_InteractWithMissingBundleNote_Prefix(GameLocation __instance, int tileIndex, ref bool __result)
        {
            try
            {
                if (__instance.Name != ABANDONED_JOJA_MART && __instance.Name != MOVIE_THEATER)
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
                        ((AbandonedJojaMart)(Game1.getLocationFromName("AbandonedJojaMart"))).checkBundle();
                        __result = true;
                        return false; // don't run original logic
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckTileIndexAction_InteractWithMissingBundleNote_Prefix)}:\n{ex}");
                return true; // run original logic
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
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoRestoreAreaCutscene_InteractWithMissingBundleNote_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        private static bool hasSeenCCCeremonyCutscene;
        private static bool hasPamHouseUpgrade;
        private static bool hasShortcuts;

        // public override void MakeMapModifications(bool force = false)
        public static bool MakeMapModifications_JojamartAndTheater_Prefix(Town __instance, bool force)
        {
            try
            {
                if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 2)
                {
                    var rectangle = new Rectangle(84, 41, 27, 15);
                    __instance.ApplyMapOverride("Town-Theater", rectangle, rectangle);
                    return true; // run original logic
                }

                if (_archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 1)
                {
                    // private void showDestroyedJoja()
                    var showDestroyedJojaMethod = _modHelper.Reflection.GetMethod(__instance, "showDestroyedJoja");
                    showDestroyedJojaMethod.Invoke();
                    if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible"))
                    {
                        __instance.crackOpenAbandonedJojaMartDoor();
                    }
                    if (!Game1.player.mailReceived.Contains(string.Join("ap", ABANDONED_JOJA_MART)))
                    {
                        Game1.player.mailReceived.Add("apAbandonedJojaMart");
                    }
                }
                else
                {
                    hasSeenCCCeremonyCutscene = Utility.HasAnyPlayerSeenEvent(EventIds.COMMUNITY_CENTER_COMPLETE);
                    if (hasSeenCCCeremonyCutscene)
                    {
                        Game1.player.eventsSeen.Remove(EventIds.COMMUNITY_CENTER_COMPLETE);
                    }
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_JojamartAndTheater_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        // public override void MakeMapModifications(bool force = false)
        public static void MakeMapModifications_JojamartAndTheater_Postfix(Town __instance, bool force)
        {
            try
            {
                if (hasSeenCCCeremonyCutscene && !Game1.player­.eventsSeen.Contains(EventIds.COMMUNITY_CENTER_COMPLETE))
                {
                    Game1.player.eventsSeen.Add(EventIds.COMMUNITY_CENTER_COMPLETE);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_JojamartAndTheater_Postfix)}:\n{ex}");
                return;
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
                    return true; // run original logic
                }

                if (locationName != "JojaMart" || !_archipelago.HasReceivedItem(APItem.MOVIE_THEATER))
                {
                    // no fallback
                    __result = false;
                    return false; // don't run original logic
                }

                if (!__instance.hasMasterScheduleEntry(locationName + "_Replacement"))
                {
                    // Fallback on the default schedule
                    __result = true;
                    return false; // don't run original logic
                }

                var strArray = __instance.getMasterScheduleEntry(locationName + "_Replacement").Split(' ');
                locationName = strArray[0];
                tileX = Convert.ToInt32(strArray[1]);
                tileY = Convert.ToInt32(strArray[2]);
                facingDirection = Convert.ToInt32(strArray[3]);

                // no fallback
                __result = false;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChangeScheduleForLocationAccessibility_JojamartAndTheater_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
