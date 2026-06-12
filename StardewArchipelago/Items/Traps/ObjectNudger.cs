using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using StardewValley.Network.ChestHit;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace StardewArchipelago.Items.Traps
{
    public class ObjectNudger
    {
        private static ILogger _logger;
        private readonly IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private Dictionary<Object, NetMutex> _objectMutexes;
        private Dictionary<Building, NetMutex> _buildingMutexes;
        //private Dictionary<Object, NetVector2> _objectKickStartTiles;
        //private Dictionary<Object, Vector2?> _objectLocalKickStartTiles;

        public ObjectNudger(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _objectMutexes = new Dictionary<Object, NetMutex>();
            _buildingMutexes = new Dictionary<Building, NetMutex>();
        }


        public void NudgeObjectsEverywhere(double baseNudgeChance)
        {
            var allLocations = Game1.locations.ToList();
            allLocations.AddRange(Game1.getFarm().buildings.Where(building => building?.indoors.Value != null).Select(building => building.indoors.Value));

            foreach (var gameLocation in allLocations)
            {
                NudgeObjectsAtLocation(gameLocation, baseNudgeChance);
            }
        }

        private void NudgeObjectsAtLocation(GameLocation gameLocation, double baseNudgeChance)
        {
            foreach (var gameObject in gameLocation.Objects.Values.ToArray())
            {
                var seed = (int)Game1.stats.DaysPlayed + (int)(gameObject.TileLocation.X * 77) + (int)(gameObject.TileLocation.Y * 1933);
                var random = new Random(seed);
                TryNudgeChest(gameObject, random, baseNudgeChance);
                TryOtherObject(gameObject, random, baseNudgeChance);
            }
        }

        private void TryNudgeChest(Object gameObject, Random random, double baseNudgeChance)
        {
            if (gameObject is not Chest chest)
            {
                return;
            }

            NudgeChest(chest, random, baseNudgeChance);
        }

        private void NudgeChest(Chest chest, Random random, double baseNudgeChance)
        {
            var chestNudgeChance = baseNudgeChance;
            while (random.NextDouble() < chestNudgeChance)
            {
                chestNudgeChance /= 2;
                var mutex = chest.GetMutex();

                mutex.RequestLock(() =>
                {
                    chest.clearNulls();
                    var chestTileBefore = chest.TileLocation;
                    chest.TryMoveToSafePosition(random.Next(0, 4));

                    // internal readonly ChestHitSynchronizer chestHit;
                    var chestHitField = _helper.Reflection.GetField<ChestHitSynchronizer>(Game1.player.team, "chestHit");
                    var chestHit = chestHitField.GetValue();

                    chestHit.SignalMove(chest.Location, (int)chestTileBefore.X, (int)chestTileBefore.Y, (int)chest.TileLocation.X, (int)chest.TileLocation.Y);
                    mutex.ReleaseLock();
                });
            }
        }

        private void TryOtherObject(Object gameObject, Random random, double baseNudgeChance)
        {
            if (gameObject is Chest || _archipelago.SlotData.TrapItemsDifficulty < TrapItemsDifficulty.Nightmare)
            {
                return;
            }

            NudgeOtherObject(gameObject, random, baseNudgeChance / 2);
        }

        private void NudgeOtherObject(Object gameObject, Random random, double baseNudgeChance)
        {
            var chestNudgeChance = baseNudgeChance;
            while (random.NextDouble() < chestNudgeChance)
            {
                chestNudgeChance /= 2;
                var mutex = GetMutex(gameObject);

                mutex.RequestLock(() =>
                {
                    var chestTileBefore = gameObject.TileLocation;
                    TryMoveToSafePosition(gameObject, random.Next(0, 4));

                    // internal readonly ChestHitSynchronizer chestHit;
                    var chestHitField = _helper.Reflection.GetField<ChestHitSynchronizer>(Game1.player.team, "chestHit");
                    var chestHit = chestHitField.GetValue();

                    chestHit.SignalMove(gameObject.Location, (int)chestTileBefore.X, (int)chestTileBefore.Y, (int)gameObject.TileLocation.X, (int)gameObject.TileLocation.Y);
                    mutex.ReleaseLock();
                });
            }
        }

        public NetMutex GetMutex(Object gameObject)
        {
            if (!_objectMutexes.ContainsKey(gameObject))
            {
                _objectMutexes.Add(gameObject, new NetMutex());
            }

            return _objectMutexes[gameObject];
        }

        public NetMutex GetMutex(Building building)
        {
            if (!_buildingMutexes.ContainsKey(building))
            {
                _buildingMutexes.Add(building, new NetMutex());
            }

            return _buildingMutexes[building];
        }

        public bool TryMoveToSafePosition(Object gameObject, int preferDirection)
        {
            var location = gameObject.Location;
            Vector2? prioritizeDirection;
            switch (preferDirection)
            {
                case 0:
                    prioritizeDirection = new Vector2(0.0f, -1f);
                    break;
                case 1:
                    prioritizeDirection = new Vector2(1f, 0.0f);
                    break;
                case 3:
                    prioritizeDirection = new Vector2(-1f, 0.0f);
                    break;
                default:
                    prioritizeDirection = new Vector2(0.0f, 1f);
                    break;
            }

            return TryMoveRecursively(gameObject, location, gameObject.tileLocation.Value, 0, prioritizeDirection);
        }

        private bool TryMoveRecursively(Object gameObject, GameLocation location, Vector2 objectTilePosition, int depth, Vector2? prioritizedDirection)
        {
            var validMovements = new List<Vector2>();
            validMovements.AddRange(new Vector2[]
            {
                new(1f, 0.0f),
                new(-1f, 0.0f),
                new(0.0f, -1f),
                new(0.0f, 1f)
            });
            Utility.Shuffle(Game1.random, validMovements);
            if (prioritizedDirection.HasValue)
            {
                validMovements.Remove(-prioritizedDirection.Value);
                validMovements.Insert(0, -prioritizedDirection.Value);
                validMovements.Remove(prioritizedDirection.Value);
                validMovements.Insert(0, prioritizedDirection.Value);
            }
            foreach (var movement in validMovements)
            {
                var newPosition = objectTilePosition + movement;
                if (!gameObject.canBePlacedHere(location, newPosition) || !location.CanItemBePlacedHere(newPosition))
                {
                    continue;
                }
                if (location.objects.ContainsKey(newPosition) || !location.objects.Remove(gameObject.TileLocation))
                {
                    return true;
                }
                // gameObject.kickStartTile.Value = gameObject.TileLocation;
                gameObject.TileLocation = newPosition;
                location.objects[newPosition] = gameObject;
                return true;
            }
            Utility.Shuffle(Game1.random, validMovements);
            if (prioritizedDirection.HasValue)
            {
                validMovements.Remove(-prioritizedDirection.Value);
                validMovements.Insert(0, -prioritizedDirection.Value);
                validMovements.Remove(prioritizedDirection.Value);
                validMovements.Insert(0, prioritizedDirection.Value);
            }
            if (depth >= 3)
            {
                return false;
            }
            foreach (var movement in validMovements)
            {
                var newTilePosition = objectTilePosition + movement;
                if (location.isPointPassable(new xTile.Dimensions.Location((int)(newTilePosition.X + 0.5) * 64, (int)(newTilePosition.Y + 0.5) * 64), Game1.viewport) &&
                    TryMoveRecursively(gameObject, location, newTilePosition, depth + 1, prioritizedDirection))
                {
                    return true;
                }
            }
            return false;
        }

        public void NudgeBuildings(double baseNudgeChance)
        {
            if (_archipelago.SlotData.TrapItemsDifficulty < TrapItemsDifficulty.Nightmare)
            {
                return;
            }

            var buildingsNudgeChance = baseNudgeChance / 8;
            foreach (var building in Game1.getFarm().buildings)
            {
                var seed = (int)Game1.stats.DaysPlayed + building.tileX.Value * 77 + building.tileY.Value * 1933;
                var random = new Random(seed);
                NudgeBuilding(building, random, buildingsNudgeChance);
            }
        }

        private void NudgeBuilding(Building building, Random random, double baseNudgeChance)
        {
            var chestNudgeChance = baseNudgeChance;
            while (random.NextDouble() < chestNudgeChance)
            {
                chestNudgeChance /= 2;
                var mutex = GetMutex(building);

                mutex.RequestLock(() =>
                {
                    Game1.playSound("axchop");
                    TryMoveToSafePosition(building, random.Next(0, 4));
                    mutex.ReleaseLock();
                });
            }
        }

        public bool TryMoveToSafePosition(Building building, int preferDirection)
        {
            var location = building.GetParentLocation();
            Vector2? prioritizeDirection;
            switch (preferDirection)
            {
                case 0:
                    prioritizeDirection = new Vector2(0.0f, -1f);
                    break;
                case 1:
                    prioritizeDirection = new Vector2(1f, 0.0f);
                    break;
                case 3:
                    prioritizeDirection = new Vector2(-1f, 0.0f);
                    break;
                default:
                    prioritizeDirection = new Vector2(0.0f, 1f);
                    break;
            }

            return TryMoveRecursively(building, location, building.tileX.Value, building.tileY.Value, 0, prioritizeDirection);
        }

        private bool TryMoveRecursively(Building building, GameLocation location, int tileX, int tileY, int depth, Vector2? prioritizedDirection)
        {
            if (!HasPermissionsToMove(building))
            {
                return false;
            }

            var validMovements = new List<Vector2>();
            validMovements.AddRange(new Vector2[]
            {
                new(1f, 0.0f),
                new(-1f, 0.0f),
                new(0.0f, -1f),
                new(0.0f, 1f)
            });
            Utility.Shuffle(Game1.random, validMovements);
            if (prioritizedDirection.HasValue)
            {
                validMovements.Remove(-prioritizedDirection.Value);
                validMovements.Insert(0, -prioritizedDirection.Value);
                validMovements.Remove(prioritizedDirection.Value);
                validMovements.Insert(0, prioritizedDirection.Value);
            }
            foreach (var movement in validMovements)
            {
                building.isMoving = true;
                var newPosition = new Vector2(tileX, tileY) + movement;

                if (ConfirmBuildingAccessibility(building, location, newPosition) && location.buildStructure(building, newPosition, Game1.player))
                {
                    building.isMoving = false;
                    Game1.playSound("axchop");
                    DelayedAction.playSoundAfterDelay("dirtyHit", 50);
                    DelayedAction.playSoundAfterDelay("dirtyHit", 150);
                    return true;
                }
            }

            building.isMoving = false;
            return false;
        }

        public bool HasPermissionsToMove(Building building)
        {
            if (!Game1.getFarm().greenhouseUnlocked.Value && building is GreenhouseBuilding)
            {
                return false;
            }
            if (Game1.IsMasterGame)
            {
                return true;
            }
            switch (Game1.player.team.farmhandsCanMoveBuildings.Value)
            {
                case FarmerTeam.RemoteBuildingPermissions.OwnedBuildings:
                    if (building.hasCarpenterPermissions())
                    {
                        return true;
                    }
                    break;
                case FarmerTeam.RemoteBuildingPermissions.On:
                    return true;
            }
            return false;
        }

        public virtual bool ConfirmBuildingAccessibility(Building building, GameLocation location, Vector2 buildingPosition)
        {
            if (building == null)
            {
                return false;
            }
            if (building.buildingType.Value != "Farmhouse")
            {
                return true;
            }

            var point1 = building.humanDoor.Value;
            point1.X += (int)buildingPosition.X;
            point1.Y += (int)buildingPosition.Y;
            ++point1.Y;
            var pointSet1 = new HashSet<Point>();
            var pointStack = new Stack<Point>();
            pointStack.Push(point1);
            pointSet1.Add(point1);
            var pointSet2 = new HashSet<Point>();
            foreach (var warp in location.warps)
            {
                if (!(warp.TargetName == "FarmCave"))
                {
                    pointSet2.Add(new Point(warp.X, warp.Y));
                }
            }
            var flag = false;
            while (pointStack.Count > 0)
            {
                var point2 = pointStack.Pop();
                if (pointSet2.Contains(point2))
                {
                    flag = true;
                    break;
                }
                if (location.isTileOnMap(point2.X, point2.Y) && VerifyTileAccessibility(building, location, point2.X, point2.Y, buildingPosition))
                {
                    var point3 = point2;
                    ++point3.X;
                    if (pointSet1.Add(point3))
                    {
                        pointStack.Push(point3);
                    }
                    point3 = point2;
                    --point3.X;
                    if (pointSet1.Add(point3))
                    {
                        pointStack.Push(point3);
                    }
                    point3 = point2;
                    --point3.Y;
                    if (pointSet1.Add(point3))
                    {
                        pointStack.Push(point3);
                    }
                    point3 = point2;
                    ++point3.Y;
                    if (pointSet1.Add(point3))
                    {
                        pointStack.Push(point3);
                    }
                }
            }
            return flag;
        }

        protected bool VerifyTileAccessibility(Building building, GameLocation location, int tileX, int tileY, Vector2 buildingPosition)
        {
            if (!location.isTilePassable(new Location(tileX, tileY), Game1.viewport) ||
                !building.isTilePassable(new Vector2(building.tileX.Value + (tileX - (int)buildingPosition.X), building.tileY.Value + (tileY - (int)buildingPosition.Y))))
            {
                return false;
            }
            var buildingAt = location.getBuildingAt(new Vector2(tileX, tileY));
            if (buildingAt != null && !buildingAt.isMoving && !buildingAt.isTilePassable(new Vector2(tileX, tileY)))
            {
                return false;
            }
            var rectangle = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
            rectangle.Inflate(-1, -1);
            foreach (var resourceClump in location.resourceClumps)
            {
                if (resourceClump.getBoundingBox().Intersects(rectangle))
                {
                    return false;
                }
            }
            foreach (TerrainFeature largeTerrainFeature in location.largeTerrainFeatures)
            {
                if (largeTerrainFeature.getBoundingBox().Intersects(rectangle))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
