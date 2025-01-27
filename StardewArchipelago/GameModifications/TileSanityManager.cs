using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections.Tilesanity;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications;

public class TileSanityManager
{
    private readonly Harmony _harmony;
    private readonly StardewArchipelagoClient _archipelago;
    private readonly StardewLocationChecker _locationChecker;
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

    public TileSanityManager(Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, IMonitor monitor)
    {
        _harmony = harmony;
        _archipelago = archipelago;
        _locationChecker = locationChecker;
        _monitor = monitor;
    }

    public void PatchWalk(IModHelper modHelper)
    {
#if !TILESANITY
        return;
#endif

        WalkSanityInjections.Initialize(_monitor, _archipelago, _locationChecker, this);
        TileUI.Initialize(_locationChecker, this);

#if !NOWALK
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
#endif

        modHelper.Events.Display.RenderedWorld += TileUI.RenderTiles;
    }

    public bool HandleTilesanityCommands(string message)
    {
#if !TILESANITY
        return false;
#endif
        if (TileUI.ProcessCommand(message))
        {
            return true;
        }

        if (HandleWhereCommand(message))
        {
            return true;
        }
        if (HandleDebugCommands(message))
        {
            return true;
        }
        return false;
    }

    private bool HandleWhereCommand(string message)
    {
        if (message != $"{ChatForwarder.COMMAND_PREFIX}where")
        {
            return false;
        }

        var (x, y) = Game1.player.TilePoint;
        Game1.chatBox?.addMessage($"You are currently at {GetTileName(x, y, Game1.player)}",
            Color.Gold);
        var (f1, f2) = Game1.currentCursorTile;
        x = (int)f1;
        y = (int)f2;
        var apLocation = GetTileName(x, y, Game1.player);
        var walkable = _archipelago.GetLocationId(apLocation) > -1 ? "walkable" : "not walkable";
        Game1.chatBox?.addMessage(
            $"You are currently pointing at {GetTileName(x, y, Game1.player)} ({walkable})",
            Color.Gold);
        return true;
    }

    private static bool HandleDebugCommands(string message)
    {
#if !DEBUG
        return false;
#endif
        switch (message)
        {
            case $"{ChatForwarder.COMMAND_PREFIX}walk":
            {
                HandleWalkCommand();
                return true;
            }
            case $"{ChatForwarder.COMMAND_PREFIX}unwalk":
            {
                HandleUnwalkCommand();
                return true;
            }
        }
        return false;
    }

    private static void HandleWalkCommand()
    {
        const string tileFile = "tiles.json";
        var dictionary = JsonConvert.DeserializeObject<SortedDictionary<string, List<Vector2>>>(File.ReadAllText(tileFile));
        dictionary[GetMapName(Game1.player)].Add(Game1.currentCursorTile);
        dictionary[GetMapName(Game1.player)].Sort(((vector2, vector3) =>
            vector2.X.CompareTo(vector3.X) * 2 + vector2.Y.CompareTo(vector3.Y)));
        File.WriteAllText(tileFile, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
        TileUI.SwitchToDebug(dictionary[GetMapName(Game1.player)]);
    }

    private static void HandleUnwalkCommand()
    {
        const string tileFile = "tiles.json";
        var dictionary = JsonConvert.DeserializeObject<SortedDictionary<string, List<Vector2>>>(File.ReadAllText(tileFile));
        dictionary[GetMapName(Game1.player)].Remove(Game1.currentCursorTile);
        dictionary[GetMapName(Game1.player)].Sort(((vector2, vector3) =>
            vector2.X.CompareTo(vector3.X) * 2 + vector2.Y.CompareTo(vector3.Y)));
        File.WriteAllText(tileFile, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
        TileUI.SwitchToDebug(dictionary[GetMapName(Game1.player)]);
    }
}
