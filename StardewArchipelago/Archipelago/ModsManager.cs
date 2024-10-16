﻿using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Modded;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago
{

    public class ModsManager
    {
        private readonly ILogger _logger;
        private readonly List<string> _activeMods;
        private readonly VersionValidator _versionValidator;

        public ModsManager(ILogger logger, List<string> activeMods)
        {
            _logger = logger;
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

        public bool IsModStateCorrect(IModHelper modHelper, out string errorMessage)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            errorMessage = $"The slot you are connecting to has been created expecting modded content,\r\nbut not all expected mods are installed and active.";
            var valid = true;
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
                valid = false;
                errorMessage += $"{Environment.NewLine}\tMod: {modName}, expected version: {desiredVersion}, current Version: {existingVersion}";
            }

            return valid;
        }

        public bool IsPatcherStateCorrect(IModHelper modHelper, out string errorMessage)
        {
            var loadedModData = modHelper.ModRegistry.GetAll().ToList();
            errorMessage = $"The slot you are connecting to requires a content patcher,\r\n mod, but not all expected mods are installed and active.";
            var valid = true;
            foreach (var modName in _activeMods)
            {
                if (!ModVersions.CPVersions.ContainsKey(modName))
                {
                    continue;
                }
                var requirement = ModVersions.CPVersions[modName];
                if (IsModActiveAndCorrectVersion(loadedModData, requirement.ContentPatcherMod, requirement.ContentPatcherVersion, out var existingVersion))
                {
                    continue;
                }

                valid = false;
                errorMessage += $"{Environment.NewLine}\tMod: {requirement.ContentPatcherMod}, expected version: {requirement.ContentPatcherVersion}, current Version: {existingVersion}";
            }

            return valid;
        }

        private bool IsModActiveAndCorrectVersion(List<IModInfo> loadedModData, string desiredModName, string desiredVersion, out string existingVersion)
        {
            var normalizedDesiredModName = GetNormalizedModName(desiredModName);
            foreach (var modInfo in loadedModData)
            {
                var modName = GetNormalizedModName(modInfo.Manifest.Name);
                if (!modName.Equals(normalizedDesiredModName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                existingVersion = modInfo.Manifest.Version.ToString();
                return _versionValidator.IsVersionCorrect(existingVersion, desiredVersion);
            }

            existingVersion = "[NOT FOUND]";
            return false;
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
