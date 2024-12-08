using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla;

namespace StardewArchipelago.Jojapocalypse
{
    public class JojaLocationsInitializer
    {
        private ArchipelagoClient _archipelago;
        private List<JojaLocationGroup> _locationGroups;

        public JojaLocationsInitializer(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
            _locationGroups = new List<JojaLocationGroup>();
            var locationNames = archipelago.GetAllLocationNames();

            AddLevelLocations(locationNames);
            AddToolLocations(locationNames);

            AddRemainingLocations(locationNames);
        }

        private void AddLevelLocations(List<string> locationNames)
        {
            foreach (var skill in Enum.GetValues<Skill>())
            {
                AddLevelLocations(locationNames, skill);
            }
        }

        private void AddLevelLocations(List<string> locationNames, Skill skill)
        {
            var levelPattern = @$"^Level (\d+) {skill}$";
            var levelsGroup = CreateAndAddToGroup($"{skill} Levels", JojaShop.Skills, locationNames,
                x => Regex.IsMatch(x, levelPattern),
                x => ReceivedPreviousLevels(x, levelPattern, skill));
            // levelsGroup.AddPatch(JojaPatch.LevelPatches[skill]);
        }

        private static Dictionary<string, int> ReceivedPreviousLevels(string locationWithNumber, string pattern, Skill skill)
        {
            var match = Regex.Match(locationWithNumber, pattern);
            var level = int.Parse(match.Captures[0].Value);
            if (level <= 1)
            {
                return new Dictionary<string, int>();
            }

            return new Dictionary<string, int>() { { $"{skill} Level", level - 1 } };
        }

        private void AddToolLocations(List<string> locationNames)
        {
            foreach (var tool in Tools.ToolNames)
            {
                AddToolLocations(locationNames, tool);
            }
        }

        private void AddToolLocations(List<string> locationNames, string tool)
        {
            var materialPattern = string.Join("|", Materials.MaterialNames);
            var toolPattern = @$"^({materialPattern}) {tool} Upgrade$";
            var toolsGroup = CreateAndAddToGroup($"{tool} Upgrades", JojaShop.Tools, locationNames,
                x => Regex.IsMatch(x, toolPattern),
                x => ReceivedPreviousTools(x, toolPattern, tool));
            // levelsGroup.AddPatch(JojaPatch.ToolPatches[tool]);
        }

        private Dictionary<string, int> ReceivedPreviousTools(string locationWithNumber, string pattern, string tool)
        {
            var match = Regex.Match(locationWithNumber, pattern);
            var material = match.Captures[0].Value;
            var materialIndex = Array.IndexOf(Materials.MaterialNames, material);
            if (materialIndex <= 1)
            {
                return new Dictionary<string, int>();
            }

            return new Dictionary<string, int>() { { $"Progressive {tool}", materialIndex - 1 } };
        }

        private void AddRemainingLocations(List<string> locationNames)
        {
            var group = CreateAndAddToGroup("Ungrouped", JojaShop.DefaultLocationsShop, locationNames, _ => true, _ => new Dictionary<string, int>());
        }

        private JojaLocationGroup CreateAndAddToGroup(string groupName, JojaShop shop, List<string> locationNames, Func<string, bool> filter, Func<string, Dictionary<string, int>> preRequisites)
        {
            var locationsForThisGroup = locationNames.Where(filter);
            var group = CreateAndAddToGroup(groupName, shop, locationsForThisGroup, preRequisites);
            locationNames = locationNames.Where(x => !filter(x)).ToList();
            return group;
        }

        private JojaLocationGroup CreateAndAddToGroup(string groupName, JojaShop shop, IEnumerable<string> locationsForThisGroup, Func<string, Dictionary<string, int>> preRequisites)
        {
            var locations = locationsForThisGroup.Select(x => new JojaLocation(x, preRequisites(x)));
            return CreateAndAddToGroup(groupName, shop, locations);
        }

        private JojaLocationGroup CreateAndAddToGroup(string groupName, JojaShop shop, IEnumerable<JojaLocation> locations)
        {
            var group = new JojaLocationGroup(groupName, shop, locations);
            _locationGroups.Add(group);
            return group;
        }
    }
}
