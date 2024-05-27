using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections.Tilesanity;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications;

public class TileSanityManager
{
    private readonly Harmony _harmony;
    private readonly ArchipelagoClient _archipelago;
    private readonly LocationChecker _locationChecker;
    private readonly IMonitor _monitor;
    public const string TILESANITY_PREFIX = "Tilesanity: ";

    public static string GetMapName(Farmer farmer)
    {

        var map = farmer.currentLocation.DisplayName;
        if (map == $"{farmer.farmName} Farm")
        {
            map = farmer.currentLocation.Name;
            if (map == "Farm")
                return $"{Game1.GetFarmTypeKey()} Farm";
        }
        return map;
    }

    public string GetTileName(int x, int y, Farmer farmer)
    {
        var map = GetMapName(farmer);
        x /= _archipelago.SlotData.TilesanitySize;
        y /= _archipelago.SlotData.TilesanitySize;

        return $"{TILESANITY_PREFIX}{map} ({x}-{y})";
    }

    public IEnumerable<(string map, int x, int y)> GetTilesFromName(string name)
    {
        var pattern = $@"{Regex.Escape(TILESANITY_PREFIX)}([ \w]+) +\((\d+)\-(\d+)\)";

        var match = Regex.Match(name, pattern);

        var map = match.Groups[1].Value;
        var x = int.Parse(match.Groups[2].Value) * _archipelago.SlotData.TilesanitySize;
        var y = int.Parse(match.Groups[3].Value) * _archipelago.SlotData.TilesanitySize;

        if (map == $"{Game1.GetFarmTypeKey()} Farm")
        {
            map = "Farm";
        }

        for (var i = 0; i < _archipelago.SlotData.TilesanitySize; i++)
        {
            for (var j = 0; j < _archipelago.SlotData.TilesanitySize; j++)
            {
                yield return (map, x + i, y + j);
            }
        }
    }

    public TileSanityManager(Harmony harmony, ArchipelagoClient archipelago, LocationChecker locationChecker, IMonitor monitor)
    {
        _harmony = harmony;
        _archipelago = archipelago;
        _locationChecker = locationChecker;
        _monitor = monitor;
    }

    public void PatchWalk(IModHelper modHelper)
    {
        WalkSanityInjections.Initialize(_monitor, _archipelago, _locationChecker, this);
        TileUI.Initialize(_locationChecker, this);
        
        _harmony.Patch(
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.Update)),
            prefix: new HarmonyMethod(typeof(WalkSanityInjections),
                nameof(WalkSanityInjections.MovePosition_Update_Prefix)),
            postfix: new HarmonyMethod(typeof(WalkSanityInjections),
                nameof(WalkSanityInjections.MovePosition_Update_Postfix))
        );

        _harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition),
                new[]
                {
                    typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool),
                    typeof(Character),
                }),
            prefix: new HarmonyMethod(typeof(WalkSanityInjections),
                nameof(WalkSanityInjections.isCollidingPosition_ForbidMove_Prefix))
        );

        modHelper.Events.Display.RenderedWorld += TileUI.RenderTiles;
    }
}
