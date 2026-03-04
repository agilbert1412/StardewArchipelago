using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using HarmonyLib;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class EntranceInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static EntranceManager _entranceManager;

        private static bool _skipER = false;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, EntranceManager entranceManager)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _entranceManager = entranceManager;
        }

        public static bool PerformWarpFarmer_EntranceRandomization_Prefix(ref LocationRequest locationRequest, ref int tileX,
            ref int tileY, ref int facingDirectionAfterWarp)
        {
            try
            {
                if (_skipER || Game1.player.passedOut || Game1.player.FarmerSprite.isPassingOut() || Game1.player.isInBed.Value)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var targetPosition = new Point(tileX, tileY);
                var entranceIsReplaced = _entranceManager.TryGetEntranceReplacement(Game1.currentLocation.Name, locationRequest.Name, targetPosition, out var replacedWarp);
                if (!entranceIsReplaced)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                locationRequest.Name = replacedWarp.LocationRequest.Name;


                foreach (var activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
                {
                    if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) &&
                        Game1.dayOfMonth >= data.StartDay && Game1.dayOfMonth <= data.EndDay && data.Season == Game1.season &&
                        data.MapReplacements != null && data.MapReplacements.TryGetValue(locationRequest.Name, out var name))
                    {
                        locationRequest.Name = name;
                    }
                }

                locationRequest.Location = replacedWarp.LocationRequest.Location;
                locationRequest.IsStructure = replacedWarp.LocationRequest.IsStructure;
                tileX = replacedWarp.TileX;
                tileY = replacedWarp.TileY;
                facingDirectionAfterWarp = (int)replacedWarp.FacingDirectionAfterWarp;
                Game1.player.forceCanMove();

                SetCorrectSwimsuitState(locationRequest, tileX, tileY);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformWarpFarmer_EntranceRandomization_Prefix)} going from {Game1.currentLocation.Name} to {locationRequest.Name}:{Environment.NewLine}\t{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        public static void PlacementAction_DontGlowShortsMaze_Postfix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who)
        {
            try
            {
                if (__instance.QualifiedItemId != "(BC)71")
                {
                    return;
                }

                Game1.screenGlowHold = false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlacementAction_DontGlowShortsMaze_Postfix)}\t{ex}");
                return;
            }
        }

        // private void wandWarpForReal()
        public static bool WandWarpForReal_ReturnScepterBehaviorChanges_Prefix(Wand __instance)
        {
            try
            {
                const string returnScepterEntranceName = "UseReturnScepter";
                var entranceIsReplaced = _entranceManager.TryGetEntranceReplacement(returnScepterEntranceName, out var replacedWarp);
                if (!entranceIsReplaced)
                {
                    ReturnScepterQoLWarp(__instance);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
                if (homeOfFarmer == null)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var locationRequest = replacedWarp.LocationRequest;
                locationRequest.Name = replacedWarp.LocationRequest.Name;

                foreach (var activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
                {
                    if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) &&
                        Game1.dayOfMonth >= data.StartDay && Game1.dayOfMonth <= data.EndDay && data.Season == Game1.season &&
                        data.MapReplacements != null && data.MapReplacements.TryGetValue(locationRequest.Name, out var name))
                    {
                        locationRequest.Name = name;
                    }
                }

                locationRequest.Location = replacedWarp.LocationRequest.Location;
                locationRequest.IsStructure = replacedWarp.LocationRequest.IsStructure;
                var tileX = replacedWarp.TileX;
                var tileY = replacedWarp.TileY;
                var facingDirectionAfterWarp = (int)replacedWarp.FacingDirectionAfterWarp;
                Game1.player.forceCanMove();

                SetCorrectSwimsuitState(locationRequest, tileX, tileY);

                _skipER = true;
                Game1.warpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
                Game1.fadeToBlackAlpha = 0.99f;
                Game1.screenGlow = false;
                __instance.lastUser.temporarilyInvincible = false;
                __instance.lastUser.temporaryInvincibilityTimer = 0;
                Game1.displayFarmer = true;
                __instance.lastUser.CanMove = true;
                _skipER = false;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(WandWarpForReal_ReturnScepterBehaviorChanges_Prefix)}:{Environment.NewLine}\t{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ReturnScepterQoLWarp(Wand returnScepter)
        {
            var homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
            if (homeOfFarmer == null)
            {
                return;
            }

            _skipER = true;
            if (Game1.player.currentLocation is Farm farm)
            {
                var insideHouseSpot = homeOfFarmer.GetPlayerBedSpot();
                Game1.warpFarmer(homeOfFarmer.Name, insideHouseSpot.X, insideHouseSpot.Y, false);
            }
            else
            {
                var frontDoorSpot = homeOfFarmer.getFrontDoorSpot();
                Game1.warpFarmer("Farm", frontDoorSpot.X, frontDoorSpot.Y, false);
            }
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            returnScepter.lastUser.temporarilyInvincible = false;
            returnScepter.lastUser.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
            returnScepter.lastUser.CanMove = true;
            _skipER = false;
        }

        // private void totemWarpForReal()
        public static bool TotemWarpForReal_WarpTotemRandomizer_Prefix(Object __instance)
        {
            try
            {
                var warpName = "";
                switch (__instance.QualifiedItemId)
                {
                    case "(O)688":
                        const string farmTotemEntranceName = "UseFarmTotem";
                        warpName = farmTotemEntranceName;
                        break;
                    case "(O)689":
                        const string mountainTotemEntranceName = "UseMountainTotem";
                        warpName = mountainTotemEntranceName;
                        break;
                    case "(O)690":
                        const string beachTotemEntranceName = "UseBeachTotem";
                        warpName = beachTotemEntranceName;
                        break;
                    case "(O)261":
                        const string desertTotemEntranceName = "UseDesertTotem";
                        warpName = desertTotemEntranceName;
                        break;
                    case "(O)886":
                        const string islandTotemEntranceName = "UseIslandTotem";
                        warpName = islandTotemEntranceName;
                        break;
                }

                var entranceIsReplaced = _entranceManager.TryGetEntranceReplacement(warpName, out var replacedWarp);
                if (!entranceIsReplaced)
                {
                    TotemWarpForReal(__instance);
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var locationRequest = replacedWarp.LocationRequest;
                locationRequest.Name = replacedWarp.LocationRequest.Name;

                foreach (var activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
                {
                    if (Utility.TryGetPassiveFestivalData(activePassiveFestival, out var data) &&
                        Game1.dayOfMonth >= data.StartDay && Game1.dayOfMonth <= data.EndDay && data.Season == Game1.season &&
                        data.MapReplacements != null && data.MapReplacements.TryGetValue(locationRequest.Name, out var name))
                    {
                        locationRequest.Name = name;
                    }
                }

                locationRequest.Location = replacedWarp.LocationRequest.Location;
                locationRequest.IsStructure = replacedWarp.LocationRequest.IsStructure;
                var tileX = replacedWarp.TileX;
                var tileY = replacedWarp.TileY;
                var facingDirectionAfterWarp = (int)replacedWarp.FacingDirectionAfterWarp;
                Game1.player.forceCanMove();

                SetCorrectSwimsuitState(locationRequest, tileX, tileY);

                _skipER = true;
                Game1.warpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
                Game1.fadeToBlackAlpha = 0.99f;
                Game1.screenGlow = false;
                Game1.player.temporarilyInvincible = false;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.displayFarmer = true;
                _skipER = false;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TotemWarpForReal_WarpTotemRandomizer_Prefix)}:{Environment.NewLine}\t{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void TotemWarpForReal(Object totem)
        {
            _skipER = true;
            switch (totem.QualifiedItemId)
            {
                case "(O)688":
                    Point parsed;
                    if (!Game1.getFarm().TryGetMapPropertyAs("WarpTotemEntry", out parsed))
                    {
                        switch (Game1.whichFarm)
                        {
                            case 5:
                                parsed = new Point(48, 39);
                                break;
                            case 6:
                                parsed = new Point(82, 29);
                                break;
                            default:
                                parsed = new Point(48, 7);
                                break;
                        }
                    }
                    Game1.warpFarmer("Farm", parsed.X, parsed.Y, false);
                    break;
                case "(O)689":
                    Game1.warpFarmer("Mountain", 31, 20, false);
                    break;
                case "(O)690":
                    Game1.warpFarmer("Beach", 20, 4, false);
                    break;
                case "(O)261":
                    Game1.warpFarmer("Desert", 35, 43, false);
                    break;
                case "(O)886":
                    Game1.warpFarmer("IslandSouth", 11, 11, false);
                    break;
            }
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
            _skipER = false;
        }

        private static void SetCorrectSwimsuitState(LocationRequest locationRequest, int tileX, int tileY)
        {
            var shouldBeInSwimsuit = GetCorrectSwimsuitState(locationRequest, tileX, tileY);
            if (shouldBeInSwimsuit)
            {
                Game1.player.changeIntoSwimsuit();
            }
            else
            {
                Game1.player.changeOutOfSwimSuit();
            }
        }

        private static bool GetCorrectSwimsuitState(LocationRequest locationRequest, int tileX, int tileY)
        {
            if (locationRequest.Location.Name.Equals("BathHouse_Pool"))
            {
                return true;
            }

            if (!locationRequest.Location.Name.StartsWith("BathHouse_", StringComparison.OrdinalIgnoreCase) ||
                !locationRequest.Location.Name.EndsWith("Locker", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (locationRequest.Name.Contains("Women", StringComparison.OrdinalIgnoreCase))
            {
                return tileX < 5;
            }
            else
            {
                return tileX > 12;
            }
        }


        // public static void warpFarmer(
        // string locationName,
        // int tileX,
        // int tileY,
        // int facingDirectionAfterWarp)
        public static bool WarpFarmer_InterceptProblemEntrances_Prefix(string locationName, int tileX, int tileY, int facingDirectionAfterWarp)
        {
            try
            {
                if (Game1.currentLocation is not IslandSouthEastCave cave ||
                    locationName != "IslandSouthEast" || tileX != 29 || tileY != 19 || facingDirectionAfterWarp != 1 ||
                    !IslandSouthEastCave.isPirateNight() || cave.wasPirateCaveOnLoad)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // protected override void resetLocalState()
                var resetLocalStateMethod = _helper.Reflection.GetMethod(cave, "resetLocalState");
                resetLocalStateMethod.Invoke();
                cave.resetForPlayerEntry();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(WarpFarmer_InterceptProblemEntrances_Prefix)}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }


        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_LockerRoomKeys_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (__instance.ShouldIgnoreAction(action, who, tileLocation))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGet(action, 0, out var key1, out var error, name: "string actionType"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!who.IsLocalPlayer || key1 == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (key1 == "WarpMensLocker")
                {
                    if (!ArgUtility.TryGetPoint(action, 1, out var point, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName, out error, name: "string locationName"))
                    {
                        __instance.LogTileActionError(action, tileLocation.X, tileLocation.Y, error);
                        __result = false;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    var flag = action.Length < 5;
                    if (!_archipelago.HasReceivedItem(APItem.MENS_LOCKER_KEY))
                    {
                        if (who.IsLocalPlayer)
                        {
                            Game1.drawObjectDialogue("The Men's locker room is locked... Maybe you'll find a key for it eventually?");
                        }
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    who.faceGeneralDirection(new Vector2((float)tileLocation.X, (float)tileLocation.Y) * 64f);
                    if (flag)
                    {
                        __instance.playSound("doorClose", new Vector2((float)tileLocation.X, (float)tileLocation.Y));
                    }
                    Game1.warpFarmer(locationName, point.X, point.Y, false);

                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (key1 == "WarpWomensLocker")
                {
                    if (!ArgUtility.TryGetPoint(action, 1, out var point, out error, "Point tile") || !ArgUtility.TryGet(action, 3, out var locationName, out error, name: "string locationName"))
                    {
                        __instance.LogTileActionError(action, tileLocation.X, tileLocation.Y, error);
                        __result = false;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    var flag = action.Length < 5;
                    if (!_archipelago.HasReceivedItem(APItem.WOMENS_LOCKER_KEY))
                    {
                        if (who.IsLocalPlayer)
                        {
                            Game1.drawObjectDialogue("The Women's locker room is locked... Maybe you'll find a key for it eventually?");
                        }
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    who.faceGeneralDirection(new Vector2((float)tileLocation.X, (float)tileLocation.Y) * 64f);
                    if (flag)
                    {
                        __instance.playSound("doorClose", new Vector2((float)tileLocation.X, (float)tileLocation.Y));
                    }
                    Game1.warpFarmer(locationName, point.X, point.Y, false);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_LockerRoomKeys_Prefix)}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

    }
}
