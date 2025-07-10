﻿using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ConnectionResults;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.GameModifications.Testing;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago
{

    public class ModsManager
    {
        private readonly ILogger _logger;
        private readonly TesterFeatures _testerFeatures;
        private readonly List<string> _activeMods;
        private readonly VersionValidator _versionValidator;

        public ModsManager(ILogger logger, TesterFeatures testerFeatures, List<string> activeMods)
        {
            _logger = logger;
            _testerFeatures = testerFeatures;
            _activeMods = activeMods;
            _versionValidator = new VersionValidator();
        }

        public bool IsModded => _activeMods.Any();

        public bool HasMod(string modName)
        {
            return _activeMods.Contains(modName);
        }

        public string GetRequiredVersion(string modName)
        {
            return ModVersions.Versions[modName];
        }

        public bool HasModdedSkill()
        {
            return HasMod(ModNames.LUCK) || HasMod(ModNames.BINNING) || HasMod(ModNames.COOKING) ||
                   HasMod(ModNames.MAGIC) || HasMod(ModNames.SOCIALIZING) || HasMod(ModNames.ARCHAEOLOGY);
        }

        public bool ModIsInstalledAndLoaded(IModHelper modHelper, string desiredModName)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            var normalizedDesiredModName = GetNormalizedModName(desiredModName);
            foreach (var modInfo in loadedModData)
            {
                var modName = GetNormalizedModName(modInfo.Manifest.Name);
                if (!modName.Equals(normalizedDesiredModName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return true;
            }
            return false;
        }

        public bool IsModStateCorrect(IModHelper modHelper, out List<MissingMod> missingMods)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            missingMods = new List<MissingMod>();
            foreach (var modName in _activeMods)
            {
                if (!ModVersions.Versions.ContainsKey(modName))
                {
                    _logger.LogWarning($"Unrecognized mod requested by the server's slot data: {modName}");
                    continue;
                }

                var desiredVersion = ModVersions.Versions[modName];
                if (IsModActiveAndCorrectVersion(loadedModData, modName, desiredVersion, out var existingVersion))
                {
                    continue;
                }

                missingMods.Add(new MissingMod(modName, desiredVersion, existingVersion));
            }

            return !missingMods.Any();
        }

        public bool IsExtraRequirementsStateCorrect(IModHelper modHelper, out List<MissingMod> problemMods)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            problemMods = new List<MissingMod>();
            foreach (var modName in _activeMods)
            {
                if (!ModVersions.ExtraRequirementsVersions.ContainsKey(modName))
                {
                    continue;
                }
                var requirements = ModVersions.ExtraRequirementsVersions[modName];

                foreach (var requirement in requirements)
                {
                    if (IsModActiveAndCorrectVersion(loadedModData, requirement.ContentPatcherMod, requirement.ContentPatcherVersion, out var existingVersion))
                    {
                        continue;
                    }

                    problemMods.Add(new MissingMod(requirement.ContentPatcherMod, requirement.ContentPatcherVersion, existingVersion));
                }
            }

            return !problemMods.Any();
        }

        private bool IsModActiveAndCorrectVersion(List<IModInfo> loadedModData, string desiredModName, string desiredVersion, out string existingVersion)
        {
            var normalizedDesiredModName = GetNormalizedModName(desiredModName);
            var foundIncorrectVersion = false;
            existingVersion = "[NOT FOUND]";
            foreach (var modInfo in loadedModData)
            {
                var modName = GetNormalizedModName(modInfo.Manifest.Name);
                if (!modName.Equals(normalizedDesiredModName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                existingVersion = modInfo.Manifest.Version.ToString();
                if (_testerFeatures.UnstableMods.Value != VerifyMods.MODS_AND_VERSIONS)
                {
                    return true;
                }
                var isCorrectVersion = _versionValidator.IsVersionCorrect(existingVersion, desiredVersion);
                if (isCorrectVersion)
                {
                    return true;
                }
                else
                {
                    foundIncorrectVersion = true;
                }

            }

            if (foundIncorrectVersion)
            {
                return false;
            }

            if (_testerFeatures.UnstableMods.Value == VerifyMods.NOTHING)
            {
                return true;
            }
            return false;
        }

        public bool HasKnownIncompatibleMods(IModHelper modHelper, out List<IncompatibleMod> incompatibleMods)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            incompatibleMods = new List<IncompatibleMod>();
            foreach (var modInfo in loadedModData)
            {
                var modName = GetNormalizedModName(modInfo.Manifest.Name);
                if (!ModVersions.IncompatibleMods.ContainsKey(modName))
                {
                    continue;
                }

                if (_activeMods.Any(x => GetNormalizedModName(x).Equals(modName)))
                {
                    continue;
                }

                var justification = ModVersions.IncompatibleMods[modName];
                incompatibleMods.Add(new IncompatibleMod(modName, justification));
            }

            return !incompatibleMods.Any();
        }

        private static string GetNormalizedModName(string modName)
        {
            var aliasedName = modName;
            if (ModInternalNames.InternalNames.ContainsKey(modName))
            {
                aliasedName = ModInternalNames.InternalNames[modName];
            }
            var cleanName = aliasedName
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace("'", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "");
            return cleanName;
        }
    }
}
