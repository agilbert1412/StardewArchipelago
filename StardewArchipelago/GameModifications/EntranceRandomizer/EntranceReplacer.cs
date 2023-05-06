using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.EntranceRandomizer
{
    internal class EntranceReplacer
    {
        private IMonitor _console;
        private SlotData _slotData;
        private EntranceManager _entranceManager;

        public EntranceReplacer(IMonitor console, SlotData slotData, EntranceManager entranceManager)
        {
            _console = console;
            _slotData = slotData;
            _entranceManager = entranceManager;
        }

        public void ReplaceEntrances()
        {
            if (_slotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            foreach (var (original, replacement) in _slotData.ModifiedEntrances)
            {
                var originalExists = _entranceManager.TryGetEntrance(original, out var originalEntrance);
                var replacementExists = _entranceManager.TryGetEntrance(replacement, out var replacementEntrance);
                if (!originalExists || !replacementExists)
                {
                    if (!originalExists)
                    {
                        _console.Log($"Entrance \"{original}\" not found. Could not apply randomization provided by the AP server", LogLevel.Warn);
                    }
                    if (!replacementExists)
                    {
                        _console.Log($"Entrance \"{replacement}\" not found. Could not apply randomization provided by the AP server", LogLevel.Warn);
                    }
                    continue;
                }

                originalEntrance.ReplaceWith(replacementEntrance);
                DoReplacementOnEquivalentAreasAsWell(originalEntrance, original, replacementEntrance);
            }
        }

        private void DoReplacementOnEquivalentAreasAsWell(OneWayEntrance originalEntrance, string original,
            OneWayEntrance replacementEntrance)
        {
            foreach (var equivalentGroup in _entranceManager.Equivalencies.EquivalentAreas)
            {
                ReplaceEquivalentEntrances(originalEntrance.OriginName, original, replacementEntrance, equivalentGroup);
                ReplaceEquivalentEntrances(originalEntrance.DestinationName, original, replacementEntrance, equivalentGroup);
            }
        }

        private void ReplaceEquivalentEntrances(string locationName, string originalLocationName, OneWayEntrance replacementEntrance,
            string[] equivalentAreasGroup)
        {
            if (!equivalentAreasGroup.Contains(locationName))
            {
                return;
            }

            foreach (var equivalentArea in equivalentAreasGroup)
            {
                if (locationName == equivalentArea)
                {
                    continue;
                }

                var newWarpName = originalLocationName.Replace(locationName, equivalentArea);
                var newEntranceExists = _entranceManager.TryGetEntrance(newWarpName, out var newEntrance);
                if (newEntranceExists)
                {
                    newEntrance.ReplaceWith(replacementEntrance);
                }
            }
        }
    }
}
