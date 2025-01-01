using System;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class EntranceInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static EntranceManager _entranceManager;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, EntranceManager entranceManager)
        {
            _logger = logger;
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
    }
}
