using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
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
    public const string TILESANITY_PREFIX = "Tilesanity: ";

    private readonly Harmony _harmony;
    private readonly StardewArchipelagoClient _archipelago;
    private readonly StardewLocationChecker _locationChecker;
    private readonly IMonitor _monitor;

    private static int _tilesanitySize;
    private static string _currentLocation;
    private static string _currentMapName;

    public TileSanityManager(Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, IMonitor monitor)
    {
        _harmony = harmony;
        _archipelago = archipelago;
        _locationChecker = locationChecker;
        _monitor = monitor;

#if TILESANITY
        _tilesanitySize = _archipelago.SlotData.TilesanitySize;
#endif
    }

    public static string GetMapName(Farmer farmer)
    {
        return GetMapName(farmer.currentLocation);
    }
    
    public static string GetMapName(GameLocation location)
    {
        var internalMapName = location.Name;
        if (internalMapName != _currentLocation)
        {
            string mapName;
            var map = location.DisplayName;
            if (map == $"{Game1.player.farmName} Farm")
            {
                if (internalMapName == "Farm")
                {
                    mapName = Game1.GetFarmTypeKey() switch
                    {
                        "FourCorners" => "Four Corners Farm",
                        "MeadowlandsFarm" => "Meadowlands Farm",
                        var farmName => $"{farmName} Farm",
                    };
                }
                else
                {
                    mapName = internalMapName;
                }
            }
            else
            {
                mapName = map switch
                {
                    "Club" => "Casino",
                    "IslandWestCave1" => "Colored Crystals Cave",
                    "IslandNorthCave1" => "Island Mushroom Cave",
                    "Wizard's Tower" when internalMapName == "WizardHouseBasement" => "WizardBasement",
                    "Spa" => internalMapName switch
                    {
                        "BathHouse_Entry" => "Bathhouse Entrance",
                        "BathHouse_MensLocker" => "Men's Locker Room",
                        "BathHouse_WomensLocker" => "Women's Locker Room",
                        _ => "Public Bath",
                    },
                    "QiNutRoom" => "Qi's Walnut Room",
                    "CaptainRoom" => "Shipwreck",
                    _ => map,
                };
            }
            _currentMapName = mapName;
            _currentLocation = internalMapName;
        }
        return _currentMapName;
    }

    public static string GetTileName(int x, int y, string map)
    {
#if !NOWALK
        x /= _tilesanitySize;
        y /= _tilesanitySize;
#endif
        return $"{TILESANITY_PREFIX}{map} ({x}-{y})";
    }

    public static string GetTileName(int x, int y, Farmer farmer)
    {
        var map = GetMapName(farmer);
        return GetTileName(x, y, map);
    }

    public IEnumerable<(string map, int x, int y)> GetTilesFromName(string name)
    {
        var pattern = $@"{Regex.Escape(TILESANITY_PREFIX)}([ \w'&,]+) +\((\d+)\-(\d+)\)";

        var match = Regex.Match(name, pattern);

        var map = match.Groups[1].Value;
        var x = int.Parse(match.Groups[2].Value) * _tilesanitySize;
        var y = int.Parse(match.Groups[3].Value) * _tilesanitySize;

        if (map == $"{Game1.GetFarmTypeKey()} Farm")
        {
            map = "Farm";
        }

        for (var i = 0; i < _tilesanitySize; i++)
        {
            for (var j = 0; j < _tilesanitySize; j++)
            {
                yield return (map, x + i, y + j);
            }
        }
    }

    public bool HasLocationLeft(string tileName)
    {
        Debug.Assert(_archipelago.LocationExists(tileName));
        if (_locationChecker.IsLocationChecked(tileName))
        {
            var luckyName = tileName + " (lucky)";
            if (!_archipelago.LocationExists(luckyName) || _locationChecker.IsLocationChecked(luckyName))
                return false;
        }
        return true;
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

    public void UnpatchWalk(IModHelper modHelper)
    {
#if !TILESANITY
        return;
#endif

        modHelper.Events.Display.RenderedWorld -= TileUI.RenderTiles;
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
        if (message.StartsWith($"{ChatForwarder.COMMAND_PREFIX}walk"))
        {
            var split = message.Split(" ");
            if (split.Length == 1)
            {
                HandleWalkCommand(Game1.currentCursorTile);
            }
            else
            {
                var x1 = int.Parse(split[1]);
                var y1 = int.Parse(split[2]);
                var x2 = int.Parse(split[3]);
                var y2 = int.Parse(split[4]);
                for (int i = x1; i <= x2; i++)
                {
                    for (int j = y1; j <= y2; j++)
                    {
                        HandleWalkCommand(new Vector2(i, j));
                    }
                }
            }
            return true;
        }
        switch (message)
        {
            case $"{ChatForwarder.COMMAND_PREFIX}walk":
            {
                HandleWalkCommand(Game1.currentCursorTile);
                return true;
            }
            case $"{ChatForwarder.COMMAND_PREFIX}unwalk":
            {
                HandleUnwalkCommand();
                return true;
            }
            case $"{ChatForwarder.COMMAND_PREFIX}tile":
            {
                HandleRegisterTileCommand();
                return true;
            }
        }
        return false;
    }

    private static void HandleWalkCommand(Vector2 tile)
    {
        const string tileFile = "tiles.json";
        var dictionary = JsonConvert.DeserializeObject<SortedDictionary<string, List<Vector2>>>(File.ReadAllText(tileFile));
        if (dictionary[GetMapName(Game1.player)].Contains(tile))
        {
            TileUI.SwitchToDebug(dictionary[GetMapName(Game1.player)]);
            return;
        }
        dictionary[GetMapName(Game1.player)].Add(tile);
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

    private class ImportantTile
    {
        [JsonPropertyName("position")] public Vector2 Position;
        [JsonPropertyName("required_items")] public List<string> RequiredItems = new List<string>();
        [JsonPropertyName("entrances")] public List<string> Entrances;

        public ImportantTile(Vector2 position)
        {
            Position = position;
            Entrances = new List<string> { "New" };
        }
    }

    private static void HandleRegisterTileCommand()
    {
        const string tileFile = "important_tiles.json";
        var dictionary = JsonConvert.DeserializeObject<SortedDictionary<string, List<ImportantTile>>>(File.ReadAllText(tileFile));
        if (!dictionary.ContainsKey(GetMapName(Game1.player)))
            dictionary.Add(GetMapName(Game1.player), new List<ImportantTile>());

        dictionary[GetMapName(Game1.player)].Add(new ImportantTile(Game1.currentCursorTile));
        dictionary[GetMapName(Game1.player)].Sort(((tile1, tile2) =>
            (tile1.Position.X.CompareTo(tile2.Position.X) * 2 + tile1.Position.Y.CompareTo(tile2.Position.Y))));
        File.WriteAllText(tileFile, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
    }
}
