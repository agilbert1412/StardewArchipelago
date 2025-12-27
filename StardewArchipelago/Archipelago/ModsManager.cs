using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
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
        private static readonly VersionValidator _versionValidator = new VersionValidator();

        public ModsManager(ILogger logger, TesterFeatures testerFeatures, List<string> activeMods)
        {
            _logger = logger;
            _testerFeatures = testerFeatures;
            _activeMods = activeMods;
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

        public static void WarnForModVersions(ILogger logger, IModHelper modHelper)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            foreach (var loadedModInfo in loadedModData)
            {
                var modName = loadedModInfo.Manifest.Name;
                var normalizedName = GetNormalizedModName(modName);
                string cleanName = null;
                foreach (var nameMapping in ModInternalNames.InternalNames)
                {
                    if (GetNormalizedModName(nameMapping.Value).Equals(normalizedName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        cleanName = nameMapping.Key;
                        break;
                    }
                }

                if (cleanName == null)
                {
                    continue;
                }

                if (!ModVersions.Versions.ContainsKey(cleanName))
                {
                    continue;
                }

                var desiredVersion = ModVersions.Versions[cleanName];
                var existingVersion = loadedModInfo.Manifest.Version.ToString();
                if (_versionValidator.IsVersionCorrect(existingVersion, desiredVersion))
                {
                    logger.LogInfo($"Detected Mod `{cleanName}` [{existingVersion}]. This mod has Archipelago Integration available. Make sure you shuffle it from your generator options!");
                }
                else
                {
                    logger.LogWarning($"Detected Mod `{cleanName}` [{existingVersion}]. This mod has Archipelago Integration available, but this is not the supported version! The supported version is [{desiredVersion}].");
                }
            }
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
                var modName = modInfo.Manifest.Name;
                if (!ModVersions.IncompatibleMods.ContainsKey(modName))
                {
                    continue;
                }
;
                var justification = ModVersions.IncompatibleMods[modName];

                var simplifiedModName = GetNormalizedModName(modName);
                if (_activeMods.Any(x => GetNormalizedModName(x).Equals(simplifiedModName)))
                {
                    continue;
                }

                if (incompatibleMods.Any(x => x.ModName == modName))
                {
                    continue;
                }

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
