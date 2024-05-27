using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections.Tilesanity;

public static class WalkSanityInjections
{
    private static Point _farmerCurrentLocation;
    private static IMonitor _monitor;
    private static ArchipelagoClient _archipelago;
    private static LocationChecker _locationChecker;
    private static TileSanityManager _tileSanityManager;

    public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker, TileSanityManager tileSanityManager)
    {
        _monitor = monitor;
        _archipelago = archipelago;
        _locationChecker = locationChecker;
        _tileSanityManager = tileSanityManager;
    }

    public static bool MovePosition_Update_Prefix(Farmer __instance, GameTime time, GameLocation location)
    {
        _farmerCurrentLocation = __instance.TilePoint;
        return true;
    }

    public static void MovePosition_Update_Postfix(Farmer __instance, GameTime time, GameLocation location)
    {
        var tilePoint = __instance.TilePoint;
        if (!_farmerCurrentLocation.Equals(tilePoint))
        {
            var apLocation = _tileSanityManager.GetTileName(tilePoint.X, tilePoint.Y, __instance);
            if (_archipelago.LocationExists(apLocation))
            {
                if (!_locationChecker.IsLocationChecked(apLocation))
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                    TileUI.CheckLocation(apLocation);
                }
            }
            else
            {
                _monitor.Log($"Unrecognized Tilesanity Location: {apLocation}", LogLevel.Error);
            }
        }
    }

    public static bool isCollidingPosition_ForbidMove_Prefix(GameLocation __instance, Rectangle position,
        xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character,
        ref bool __result)
    {
        if (!isFarmer)
            return true;
        var (x, y) = position.Center;
        var tileName = _tileSanityManager.GetTileName(x / 64, y / 64, Game1.player);
        if (IsUnlocked(tileName))
            return true;
        __result = true;
        return false;
    }

    public static bool IsUnlocked(string name)
    {
        if (_archipelago.SlotData.Tilesanity < Archipelago.Tilesanity.Simplified)
            return true;
        if (name.Contains("FarmHouse"))
            return true;
        if (!_archipelago.LocationExists(name))
            return true;
        if (_archipelago.HasReceivedItem(name))
            return true;
        return false;
    }
}
