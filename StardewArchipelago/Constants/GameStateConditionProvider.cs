using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Constants.Locations;

namespace StardewArchipelago.Constants
{
    public static class GameStateConditionProvider
    {
        private static readonly string[] _progressiveBuildings = new[] { "Coop", "Barn", "Shed" };
        private static readonly string[] _progressiveBuildingPrefixes = new[] { string.Empty, Prefix.BUILDING_BIG, Prefix.BUILDING_DELUXE };

        public static string CreateHasReceivedItemCondition(string itemName, int amount = 1)
        {
            if (amount < 1)
            {
                return string.Empty;
            }

            var arguments = new[] { amount.ToString(), itemName };
            return CreateCondition(GameStateCondition.HAS_RECEIVED_ITEM, arguments);
        }

        public static string CreateHasBuildingAnywhereCondition(string buildingName, bool hasBuilding)
        {
            if (buildingName.Contains(" "))
            {
                buildingName = $"\"{buildingName}\"";
            }

            if (hasBuilding)
            {
                return $"BUILDINGS_CONSTRUCTED ALL {buildingName} 1";
            }
            return $"BUILDINGS_CONSTRUCTED ALL {buildingName} 0 0";
        }

        public static string CreateHasBuildingOrHigherCondition(string buildingName, bool hasBuilding)
        {
            var noBuildingConditions = new List<string>();
            noBuildingConditions.Add(CreateHasBuildingAnywhereCondition(buildingName, false));

            if (!_progressiveBuildings.Any(x => buildingName.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)))
            {
                return ConcatenateConditions(noBuildingConditions, hasBuilding);
            }

            if (buildingName.StartsWith(Prefix.BUILDING_DELUXE, StringComparison.InvariantCultureIgnoreCase))
            {
                return ConcatenateConditions(noBuildingConditions, hasBuilding);
            }
            if (buildingName.StartsWith(Prefix.BUILDING_BIG, StringComparison.InvariantCultureIgnoreCase))
            {
                var deluxeBuildingName = buildingName.Replace(Prefix.BUILDING_BIG, Prefix.BUILDING_DELUXE);
                noBuildingConditions.Add(CreateHasBuildingAnywhereCondition(deluxeBuildingName, false));
                return ConcatenateConditions(noBuildingConditions, hasBuilding);
            }
            if (_progressiveBuildings.Contains(buildingName))
            {
                var bigBuildingName = $"{Prefix.BUILDING_BIG}{buildingName}";
                var deluxeBuildingName = $"{Prefix.BUILDING_DELUXE}{buildingName}";
                noBuildingConditions.Add(CreateHasBuildingAnywhereCondition(bigBuildingName, false));
                noBuildingConditions.Add(CreateHasBuildingAnywhereCondition(deluxeBuildingName, false));
                return ConcatenateConditions(noBuildingConditions, hasBuilding);
            }

            return ConcatenateConditions(noBuildingConditions, hasBuilding);
        }


        public static string GetReceivedBuildingCondition(string buildingName)
        {
            var itemName = buildingName;
            var amount = 1;
            if (buildingName.StartsWith(Prefix.BUILDING_BIG, StringComparison.InvariantCultureIgnoreCase))
            {
                amount = 2;
                itemName = $"Progressive {itemName[Prefix.BUILDING_BIG.Length..]}";
            }
            else if (buildingName.StartsWith(Prefix.BUILDING_DELUXE, StringComparison.InvariantCultureIgnoreCase))
            {
                amount = 3;
                itemName = $"Progressive {itemName[Prefix.BUILDING_DELUXE.Length..]}";
            }
            else if (_progressiveBuildings.Contains(buildingName))
            {
                itemName = $"Progressive {itemName}";
            }
            else if (_buildingNameReplacements.ContainsKey(buildingName))
            {
                itemName = _buildingNameReplacements[buildingName];
            }
            return CreateHasReceivedItemCondition(itemName, amount);
        }

        private static Dictionary<string, string> _buildingNameReplacements = new()
        {
            { "Pathoschild.TractorMod_Stable", "Tractor Garage" },
        };

        public static string CreateHasStockSizeCondition(double minimumStock)
        {
            var arguments = new[] { minimumStock.ToString() };
            return CreateCondition(GameStateCondition.HAS_STOCK_SIZE, arguments);
        }

        public static string CreateSeasonsCondition(string[] seasons)
        {
            return CreateCondition("SEASON", seasons);
        }

        public static string CreateArtifactsCondition(string[] artifacts)
        {
            return CreateCondition(GameStateCondition.FOUND_ARTIFACT, artifacts);
        }

        public static string CreateMineralsCondition(string[] minerals)
        {
            return CreateCondition(GameStateCondition.FOUND_MINERAL, minerals);
        }

        public static string CreateCondition(string condition, string[] arguments)
        {
            return !arguments.Any() ? condition : $"{condition} {string.Join(' ', arguments)}";
        }

        public static string RemoveCondition(string condition, string conditionToRemove)
        {
            return FilterCondition(condition, x => !x.Contains(conditionToRemove));
        }

        public static string FilterCondition(string condition, Func<string, bool> filter)
        {
            return string.Join(',', condition.Split(',').Where(filter));
        }

        private static string ConcatenateConditions(IReadOnlyList<string> conditions, bool invert)
        {
            if (conditions == null || !conditions.Any())
            {
                return "";
            }

            if (invert)
            {
                if (conditions.Count == 1)
                {
                    return InvertCondition(conditions[0]);
                }

                return $"ANY {string.Join(' ', conditions.Select(x => SurroundWithQuotes(InvertCondition(x))))}";
            }
            else
            {
                return string.Join(", ", conditions);
            }
        }

        private static string SurroundWithQuotes(string condition)
        {
            return $"\"{condition.Replace("\"", "\\\"")}\"";
        }

        private static string InvertCondition(string condition)
        {
            return $"!{condition}";
        }
    }
}
