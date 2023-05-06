using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class EntranceInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private static EntranceManager _entranceManager;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago, EntranceManager entranceManager)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _entranceManager = entranceManager;
        }

        public static bool PerformWarpFarmer_EntranceRandomization_Prefix(ref LocationRequest locationRequest, ref int tileX,
            ref int tileY, ref int facingDirectionAfterWarp)
        {
            try
            {
                if (Game1.currentLocation.Name.ToLower() == locationRequest.Name.ToLower())
                {
                    return true; // run original logic
                }

                var entranceExists = _entranceManager.TryGetEntrance(Game1.currentLocation.Name, locationRequest.Name,
                    out var desiredEntrance);
                if (!entranceExists)
                {
                    return true; // run original logic
                }

                var warpRequest = new WarpRequest(locationRequest, tileX, tileY, (FacingDirection)facingDirectionAfterWarp);
                var warpIsModified = desiredEntrance.GetModifiedWarp(warpRequest, out var newWarp);
                if (!warpIsModified)
                {
                    return true; // run original logic
                }

                locationRequest.Name = newWarp.LocationRequest.Name;
                locationRequest.Location = newWarp.LocationRequest.Location;
                locationRequest.IsStructure = newWarp.LocationRequest.IsStructure;
                tileX = newWarp.TileX;
                tileY = newWarp.TileY;
                facingDirectionAfterWarp = (int)newWarp.FacingDirectionAfterWarp;
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformWarpFarmer_EntranceRandomization_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
