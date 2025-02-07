using System;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class EntranceInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static EntranceManager _entranceManager;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, EntranceManager entranceManager)
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
                if (Game1.currentLocation.Name.ToLower() == locationRequest.Name.ToLower() || Game1.player.passedOut || Game1.player.FarmerSprite.isPassingOut() || Game1.player.isInBed.Value)
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

    }
}
