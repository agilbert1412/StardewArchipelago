using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewArchipelago.GameModifications.Buildings;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CarpenterMenuInjections
    {
        private static readonly List<string> ExcludedBuildings = new List<string>{
            "Stone Cabin", "Plank Cabin", "Log Cabin", "Greenhouse", "Mine Elevator",
        };

        private const string STABLE_NAME = "Stable";
        private const string TRACTOR_GARAGE_NAME = "Tractor Garage";
        private const int TRACTOR_MAX_OCCUPANT_VALUE = -794739;

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public CarpenterMenu(string builder, GameLocation targetLocation = null)
        public static void Constructor_SetupArchipelagoBlueprints_Postfix(CarpenterMenu __instance, string builder, GameLocation targetLocation = null)
        {
            try
            {
                var blueprints = GetAvailableBlueprints(builder);

                if (blueprints == null || !blueprints.Any())
                {
                    return;
                }

                // public readonly List<CarpenterMenu.BlueprintEntry> Blueprints = new List<CarpenterMenu.BlueprintEntry>();
                var blueprintsField = _modHelper.Reflection.GetField<List<CarpenterMenu.BlueprintEntry>>(__instance, "Blueprints");
                blueprintsField.SetValue(blueprints);

                __instance.SetNewActiveBlueprint(0);
                if (!Game1.options.SnappyMenus)
                    return;
                __instance.populateClickableComponentList();
                __instance.snapToDefaultClickableComponent();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Constructor_SetupArchipelagoBlueprints_Postfix)}:\n{ex}", LogLevel.Error);
                return; // run original logic
            }
        }

        public static List<CarpenterMenu.BlueprintEntry> GetAvailableBlueprints(string builder)
        {
            var blueprints = new List<CarpenterMenu.BlueprintEntry>();
            foreach (var (buildingId, buildingData) in RelevantBuildingData(builder))
            {
                // This breakpoint is to check if the tractor garage shows up properly
                var blueprintUpgrade = buildingData.BuildingToUpgrade;

                var onlyOne = buildingData.Name == "Stable";
                AddBuildingBlueprintIfReceived(blueprints, buildingId, buildingData, onlyOne, requiredBuilding: blueprintUpgrade);
            }

            //if (_archipelago.SlotData.Mods.HasMod(ModNames.TRACTOR))
            //{
            //    AddBuildingBlueprintIfReceived(blueprints, CarpenterInjections.TRACTOR_GARAGE_ID, CarpenterInjections.TRACTOR_GARAGE_NAME, true, null);
            //}

            return blueprints;
        }

        private static void AddBuildingBlueprintIfReceived(List<CarpenterMenu.BlueprintEntry> blueprints, string buildingID, BuildingData building, bool onlyOne = false, string requiredBuilding = null)
        {
            var hasReceivedBuilding = CarpenterInjections.HasReceivedBuilding(building.Name, out var sendingPlayer);
            if (!hasReceivedBuilding)
            {
                return;
            }

            AddBuildingBlueprint(blueprints, buildingID, building, sendingPlayer, onlyOne, requiredBuilding);
        }

        private static IDictionary<string, BuildingData> RelevantBuildingData(string builder)
        {
            var relevantBuildingData = new Dictionary<string, BuildingData>();
            foreach (var (buildingId, buildingData) in Game1.buildingData)
            {
                if (ExcludedBuildings.Contains(buildingId))
                {
                    continue;
                }

                if (builder != null && buildingData.Builder != builder)
                {
                    continue;
                }

                relevantBuildingData.Add(buildingId, buildingData);
            }

            return relevantBuildingData;
        }

        private static void AddBuildingBlueprint(List<CarpenterMenu.BlueprintEntry> blueprints, string buildingID, BuildingData building, string sendingPlayer, bool onlyOne = false, string requiredBuilding = null)
        {
            var farm = Game1.getFarm();
            var isConstructedAlready = IsBuildingConstructed(farm, building.Name);
            if (onlyOne && isConstructedAlready)
            {
                return;
            }

            if (requiredBuilding != null)
            {
                var requiredBuildingExists = farm.isBuildingConstructed(requiredBuilding);
                if (!requiredBuildingExists)
                {
                    return;
                }
            }

            var shouldBePaid = isConstructedAlready;
            if (!shouldBePaid && (building.Name.EndsWith("Coop") || building.Name.EndsWith("Barn") || building.Name.EndsWith("Shed")))
            {
                if (building.Name.StartsWith("Big"))
                {
                    shouldBePaid |= farm.isBuildingConstructed(building.Name.Replace("Big", "Deluxe"));
                }
                else
                {
                    shouldBePaid |= farm.isBuildingConstructed($"Big {building.Name}");
                    shouldBePaid |= farm.isBuildingConstructed($"Deluxe {building.Name}");
                }
            }
            if (shouldBePaid)
            {
                blueprints.Add(new CarpenterMenu.BlueprintEntry(blueprints.Count, buildingID, building, null));
            }
            else
            {
                blueprints.Add(new FreeBlueprint(blueprints.Count, buildingID, building, null, sendingPlayer));
            }
        }

        private static bool IsBuildingConstructed(GameLocation location, string name)
        {
            if (!location.IsBuildableLocation())
            {
                return false;
            }
            foreach (var building in location.buildings)
            {
                if (IsCorrectType(building, name) && building.daysOfConstructionLeft.Value <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsCorrectType(Building building, string name)
        {
            if (name == STABLE_NAME || name == TRACTOR_GARAGE_NAME)
            {
                return building.buildingType.Value.Equals(STABLE_NAME, StringComparison.OrdinalIgnoreCase) && IsCorrectTypeOfStable(building, name);
            }

            return building.buildingType.Value.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCorrectTypeOfStable(Building building, string name)
        {
            if (name.Equals(STABLE_NAME, StringComparison.OrdinalIgnoreCase) && building.maxOccupants.Value != TRACTOR_MAX_OCCUPANT_VALUE)
            {
                return true;
            }

            if (name.Equals(TRACTOR_GARAGE_NAME, StringComparison.OrdinalIgnoreCase) && building.maxOccupants.Value == TRACTOR_MAX_OCCUPANT_VALUE)
            {
                return true;
            }

            return false;
        }

        // I suspect this isn't necessary in 1.6 anymore. Worth testing!

        //// This override exists pretty much only because the Tractor Garage tries to sneakily add the real garage and stable for no reason, AFTER initializing this menu
        //public override void update(GameTime time)
        //{
        //    base.update(time);

        //    CleanExtraStables();
        //}

        //private void CleanExtraStables()
        //{
        //    if (_hasCleanedGarage)
        //    {
        //        return;
        //    }

        //    var blueprints = _blueprintsField.GetValue();
        //    for (var i = blueprints.Count - 1; i >= 0; i--)
        //    {
        //        if (blueprints[i].Data.Name != _stableName && blueprints[i].Data.Name != _tractorGarageName)
        //        {
        //            continue;
        //        }

        //        if (!blueprints[i].DisplayName.Contains("Free", StringComparison.OrdinalIgnoreCase))
        //        {
        //            blueprints.RemoveAt(i);
        //        }
        //    }

        //    _hasCleanedGarage = true;
        //}
    }
}
