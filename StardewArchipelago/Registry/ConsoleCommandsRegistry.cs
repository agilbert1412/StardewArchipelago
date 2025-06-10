using System;
using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using StardewValley;
using static StardewArchipelago.ModEntry;
using StardewArchipelago.GameModifications;
using StardewArchipelago.Stardew;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Serialization;
using StardewArchipelago.Locations;

namespace StardewArchipelago.Registry
{
    public class ConsoleCommandsRegistry : IRegistry
    {
        private const string CONNECT_SYNTAX = "Syntax: connect_override ip:port slot password";

        private LogHandler _logger;
        private IModHelper _modHelper;
        private ModEntry _mod;

        private StardewArchipelagoClient _archipelago;
        private StardewItemManager _stardewItemManager;
        private LocationChecker _locationChecker;
        private IGiftHandler _giftHandler;
        private WeaponsManager _weaponsManager;
        private ArchipelagoStateDto _state;

        public ConsoleCommandsRegistry(LogHandler logger, IModHelper modHelper, ModEntry mod)
        {
            _logger = logger;
            _modHelper = modHelper;
            _mod = mod;
        }

        public void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, StardewLocationChecker locationChecker, IGiftHandler giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state)
        {
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _locationChecker = locationChecker;
            _giftHandler = giftHandler;
            _weaponsManager = weaponsManager;
            _state = state;
        }

        public void RegisterOnModEntry()
        {
            try
            {
                RegisterPlayerCommands();
                RegisterDebugCommands();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ConsoleCommandsRegistry)} failed during {nameof(RegisterOnModEntry)}: {ex}");
            }
        }

        private void RegisterPlayerCommands()
        {
            _modHelper.ConsoleCommands.Add("connect_override", $"Overrides your next connection to Archipelago. {CONNECT_SYNTAX}", OnCommandConnectToArchipelago);
            _modHelper.ConsoleCommands.Add("export_all_gifts", "Export all currently loaded giftable items and their traits", ExportGifts);
            _modHelper.ConsoleCommands.Add("clear_scouts", "Clears your scouting cache to allow redownloading scouting data", ClearScouts);
            _modHelper.ConsoleCommands.Add("deathlink", "Override the deathlink setting", OverrideDeathlink);
            _modHelper.ConsoleCommands.Add("trap_difficulty", "Override the trap difficulty setting", OverrideTrapDifficulty);
        }

        private void RegisterDebugCommands()
        {
#if !DEBUG
            return;
#endif
            _modHelper.ConsoleCommands.Add("connect", $"Connect to Archipelago. {CONNECT_SYNTAX}", OnCommandConnectToArchipelago);
            _modHelper.ConsoleCommands.Add("disconnect", $"Disconnects from Archipelago. {CONNECT_SYNTAX}", OnCommandDisconnectFromArchipelago);
            _modHelper.ConsoleCommands.Add("export_shippables", "Export all currently loaded shippable items", ExportShippables);
            _modHelper.ConsoleCommands.Add("export_mismatches", "Export all items where Name and DisplayName mismatch which can be shipped", ExportMismatchedItems);
            _modHelper.ConsoleCommands.Add("export_weapons", "Export all weapons by category and tier", ExportWeapons);
            _modHelper.ConsoleCommands.Add("release_slot", "Release the current slot completely", ReleaseSlot);
            _modHelper.ConsoleCommands.Add("debug_method", "Runs whatever is currently in the debug method", DebugMethod);
            // _modHelper.ConsoleCommands.Add("set_next_season", "Sets the next season to a chosen value", SetNextSeason);
            // _modHelper.ConsoleCommands.Add("test_sendalllocations", "Tests if every AP item in the stardew_valley_location_table json file are supported by the mod", _tester.TestSendAllLocations);
            // _modHelper.ConsoleCommands.Add("load_entrances", "Loads the entrances file", (_, _) => _entranceRandomizer.LoadTransports());
            // _modHelper.ConsoleCommands.Add("save_entrances", "Saves the entrances file", (_, _) => EntranceInjections.SaveNewEntrancesToFile());

            RegisterTilesanityCommands();
        }

        private void RegisterTilesanityCommands()
        {
#if !TILESANITY
            return;
#endif
            _modHelper.ConsoleCommands.Add("walkable_tiles", "Gets the list of every walkable tile", this.ListWalkableTiles);
        }

        private void OnCommandConnectToArchipelago(string arg1, string[] arg2)
        {
            if (arg2.Length < 2)
            {
                _logger.Log($"You must provide an IP with a port, and a slot name, to connect to archipelago. {CONNECT_SYNTAX}", LogLevel.Info);
                return;
            }

            var ipAndPort = arg2[0].Split(":");
            if (ipAndPort.Length < 2)
            {
                _logger.Log($"You must provide an IP with a port, and a slot name, to connect to archipelago. {CONNECT_SYNTAX}", LogLevel.Info);
                return;
            }

            var ip = ipAndPort[0];
            var port = int.Parse(ipAndPort[1]);
            var slot = arg2[1];
            var password = arg2.Length >= 3 ? arg2[2] : "";
            _mod.ArchipelagoConnectionOverride = new ArchipelagoConnectionInfo(ip, port, slot, null, password);
            _logger.Log($"Your next connection attempt will instead use {ip}:{port} on slot {slot}.", LogLevel.Info);
        }

        private void ExportGifts(string arg1, string[] arg2)
        {
            const string giftsFile = "gifts.json";
            _giftHandler.ExportAllGifts(giftsFile);
            _logger.Log($"Gifts have been exported to {giftsFile}", LogLevel.Info);
        }

        private void ClearScouts(string arg1, string[] arg2)
        {
            _archipelago.ScoutHintedLocations.Clear();
            _archipelago.ScoutedLocations.Clear();
            _state.LocationsScouted.Clear();
        }

        private void OverrideDeathlink(string arg1, string[] arg2)
        {
            _archipelago?.ToggleDeathlink();
        }

        private void OverrideTrapDifficulty(string arg1, string[] arg2)
        {
            if (_archipelago == null || _state == null || !_archipelago.MakeSureConnected(0))
            {
                _logger.Log($"This command can only be used from in-game, when connected to Archipelago", LogLevel.Info);
                return;
            }

            if (arg2.Length < 1)
            {
                _logger.Log($"Choose one of the following difficulties: [NoTraps, Easy, Medium, Hard, Hell, Nightmare].", LogLevel.Info);
                return;
            }

            var difficulty = arg2[0];
            if (!Enum.TryParse<TrapItemsDifficulty>(difficulty, true, out var difficultyOverride))
            {
                _logger.Log($"Choose one of the following difficulties: [NoTraps, Easy, Medium, Hard, Hell, Nightmare].", LogLevel.Info);
                return;
            }

            _state.TrapDifficultyOverride = difficultyOverride;
            _logger.Log($"Trap Difficulty set to [{difficultyOverride}]. Change will be saved next time you sleep", LogLevel.Info);
        }

        private void DebugMethod(string arg1, string[] arg2)
        {
            var text = File.ReadAllText(@"D:\Jeux\Archipelago\Unpacked Stardew 1.6\Content\Data\Objects.json");
            var json = JsonConvert.DeserializeObject<JObject>(text);
            var constantsText = "";
            foreach (var (id, objectData) in json)
            {
                var name = objectData["Name"].ToString();
                var constantName = name.Replace(" ", "_").ToUpper();
                constantsText += $"        public const string {constantName} = \"{id}\";{Environment.NewLine}";
            }

            File.WriteAllText(@"D:\Jeux\Archipelago\Unpacked Stardew 1.6\Content\Data\Objects.cs", constantsText);
        }

        private void ExportCropState(string cropsFile)
        {
            var cropsByLocation = new Dictionary<string, Dictionary<Vector2, CropInfo>>();
            foreach (var gameLocation in Game1.locations)
            {
                var cropsHere = new Dictionary<Vector2, CropInfo>();

                foreach (var terrainFeature in gameLocation.terrainFeatures.Values)
                {
                    if (terrainFeature is not HoeDirt groundDirt || groundDirt.crop == null)
                    {
                        continue;
                    }
                    var cropInfo = new CropInfo()
                    {
                        CurrentPhase = groundDirt.crop.currentPhase.Value,
                        DayOfCurrentPhase = groundDirt.crop.dayOfCurrentPhase.Value,
                        FullyGrown = groundDirt.crop.fullyGrown.Value,
                        PhaseDays = groundDirt.crop.phaseDays.ToArray(),
                    };
                    cropsHere.Add(groundDirt.Tile, cropInfo);
                }

                cropsByLocation.Add(gameLocation.Name, cropsHere);
            }
            var objectsAsJson = JsonConvert.SerializeObject(cropsByLocation);
            File.WriteAllText(cropsFile, objectsAsJson);
        }

        private void ExportShippables(string arg1, string[] arg2)
        {
            _stardewItemManager.ExportAllItemsMatching(x => x.canBeShipped(), "shippables.json");
        }

        private void ExportMismatchedItems(string arg1, string[] arg2)
        {
            _stardewItemManager.ExportAllMismatchedItems(x => x.canBeShipped(), "mismatches.json");
        }

        private void ExportWeapons(string arg1, string[] arg2)
        {
            var weapons = new Dictionary<string, object>();
            weapons.Add("Weapons", _weaponsManager.WeaponsByCategoryByTier);
            weapons.Add("Boots", _weaponsManager.BootsByTier);
            weapons.Add("Rings", _weaponsManager.Rings);
            weapons.Add("Slingshots", _weaponsManager.SlingshotsByTier);
            var weaponsAsJson = JsonConvert.SerializeObject(weapons);
            File.WriteAllText("weapons.json", weaponsAsJson);
        }

        private void ReleaseSlot(string arg1, string[] arg2)
        {
            if (!_archipelago.IsConnected || !Game1.hasLoadedGame || arg2.Length < 1)
            {
                return;
            }

            var slotName = arg2[0];

            if (slotName != _archipelago.GetPlayerName() || slotName != Game1.player.Name)
            {
                return;
            }

            foreach (var missingLocation in _locationChecker.GetAllMissingLocationNames())
            {
                _locationChecker.AddCheckedLocation(missingLocation);
            }
        }

        private void ListWalkableTiles(string arg1, string[] arg2)
        {
            var farmer = Game1.player;
            var playerCurrentLocation = farmer.currentLocation;
            List<Vector2> walkables = new();
            var width = playerCurrentLocation.map.DisplayWidth / 64;
            var height = playerCurrentLocation.map.DisplayHeight / 64;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    Vector2 position = new(x, y);
                    if (playerCurrentLocation.isTilePassable(position))
                    {
                        walkables.Add(position);
                    }
                }
            }

            List<Vector2> validatedWalkables = new();
            List<Vector2> toTest = new();
            Vector2 point = new(farmer.TilePoint.X, farmer.TilePoint.Y);
            if (walkables.Contains(point))
            {
                walkables.Remove(point);
                toTest.Add(point);
            }
            else
            {
                Console.Out.WriteLine("current tile is not walkable");
                return;
            }
            while (toTest.Count > 0)
            {
                point = toTest[0];
                validatedWalkables.Add(point);
                toTest.RemoveAt(0);
                point += new Vector2(1, 0);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
                point += new Vector2(-1, -1);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
                point += new Vector2(-1, 1);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
                point += new Vector2(1, 1);
                if (walkables.Contains(point))
                {
                    walkables.Remove(point);
                    toTest.Add(point);
                }
            }

            const string tileFile = "tiles.json";
            SortedDictionary<string, List<Vector2>> dictionary;
            if (File.Exists(tileFile))
            {
                dictionary =
                    JsonConvert.DeserializeObject<SortedDictionary<string, List<Vector2>>>(File.ReadAllText(tileFile));
            }
            else
            {
                dictionary = new SortedDictionary<string, List<Vector2>>();
            }

            validatedWalkables.Sort(((vector2, vector3) => vector2.X.CompareTo(vector3.X) * 2 + vector2.Y.CompareTo(vector3.Y)));
            string displayedName;
            switch (arg2.Length)
            {
                case 0:
                    displayedName = TileSanityManager.GetMapName(farmer);
                    break;
                default:
                    displayedName = arg2[0] switch
                    {
                        "0" => string.Join(' ', arg2.Skip(1)),
                        "1" => playerCurrentLocation.DisplayName,
                        "2" => playerCurrentLocation.Name,
                        _ => throw new ArgumentException()
                    };
                    break;
            }

            dictionary[displayedName] = validatedWalkables;
            File.WriteAllText(tileFile, JsonConvert.SerializeObject(dictionary, Formatting.Indented));
            Console.Out.WriteLine("Finished finding walkable tiles");
        }

        private void OnCommandDisconnectFromArchipelago(string arg1, string[] arg2)
        {
            _mod.ArchipelagoDisconnect();
        }
    }
}
