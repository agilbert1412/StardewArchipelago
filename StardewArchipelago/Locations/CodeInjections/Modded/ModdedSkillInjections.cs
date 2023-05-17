using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{

    public static class ModdedSkillInjections
    {
        private const string _skillLocationName = "Level {0} {1}";

        private static readonly Dictionary<string, string> _skillNameToModDict = new Dictionary<string, string>
        {
            { "Magic", ModNames.MAGIC }, { "Binning", ModNames.BINNING }, { "Cooking", ModNames.COOKING },
            { "Excavation", ModNames.ARCHAEOLOGY }, { "Socializing", ModNames.SOCIALIZING }
        };

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Dictionary<Skill, double> _archipelagoExperience = new();
        private static Dictionary<Skill, int> _archipelagoSkillLevel = new();

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            AddMissingSkillsToDictionary();
        }

        public static Dictionary<int, int> GetArchipelagoExperience()
        {
            return _archipelagoExperience.ToDictionary(x => (int)x.Key, x => (int)Math.Round(x.Value));
        }

        public static Dictionary<int, int> GetArchipelagoSkillLevel()
        {
            return _archipelagoSkillLevel.ToDictionary(x => (int)x.Key, x => x.Value);
        }

        public static List<string> GetArchipelagoExperienceForPrinting()
        {
            var pattern = "{0}: Level {1} ({2}/{3} to next level)";
            var skillList = EnabledSkills();
            return _archipelagoExperience.Where(x => EnabledSkills().Contains((int)x.Key)).Select(x =>
            {
                var skillName = x.Key.ToString();
                var currentExperience = (int)Math.Round(x.Value);
                var currentLevel = GetLevel(currentExperience);
                if (currentLevel >= 10)
                {
                    return $"{skillName}: Max!";
                }

                var neededExperience = GetExperienceNeeded(currentLevel + 1);
                return string.Format(pattern, skillName, currentLevel, currentExperience, neededExperience);
            }).ToList();
        }

        public static void SetArchipelagoExperience(Dictionary<int, int> values)
        {
            if (values == null)
            {
                ResetSkillExperience();
                return;
            }

            _archipelagoExperience = values.ToDictionary(x => (Skill)x.Key, x => (double)x.Value);
        }

        public static void SetArchipelagoSkillLevel(Dictionary<int, int> values)
        {
            if (values == null)
            {
                ResetModSkillLevel();
                return;
            }

            _archipelagoSkillLevel = values.ToDictionary(x => (Skill)x.Key, x => x.Value);
        }

        public static bool GainExperience_NormalExperience_Prefix(Farmer __instance, int which, int howMuch)
        {
            try
            {
                var idMax = 4;
                if (_archipelago.SlotData.Mods.HasMod(ModNames.LUCK))
                    idMax = 5;
                if (which < 0 || which > idMax || howMuch <= 0 || !__instance.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var skill = (Skill)which;
                var experienceAmount = GetMultipliedExperience(howMuch);
                var oldExperienceLevel = _archipelagoExperience[skill];
                var newExperienceLevel = _archipelagoExperience[skill] + experienceAmount;
                _archipelagoExperience[skill] = newExperienceLevel;
                var oldLevel = GetLevel(oldExperienceLevel);
                var newLevel = GetLevel(newExperienceLevel);
                if (newLevel <= oldLevel)
                {
                    return false; // don't run original logic
                }

                switch (skill)
                {
                    case Skill.Farming:
                        __instance.farmingLevel.Value = newLevel;
                        break;
                    case Skill.Fishing:
                        __instance.fishingLevel.Value = newLevel;
                        break;
                    case Skill.Foraging:
                        __instance.foragingLevel.Value = newLevel;
                        break;
                    case Skill.Mining:
                        __instance.miningLevel.Value = newLevel;
                        break;
                    case Skill.Combat:
                        __instance.combatLevel.Value = newLevel;
                        break;
                    case Skill.Luck:
                        __instance.luckLevel.Value = newLevel;
                        break;
                }

                for (var levelUp = oldLevel + 1; levelUp <= newLevel; ++levelUp)
                {
                    __instance.newLevels.Add(new Point(which, levelUp));
                }

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GainExperience_NormalExperience_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool GainExperience_ArchipelagoExperience_Prefix(Farmer __instance, int which, int howMuch)
        {
            try
            {
                var idMax = 4;
                if (_archipelago.SlotData.Mods.HasMod(ModNames.LUCK))
                    idMax = 5;
                if (which < 0 || which > idMax || howMuch <= 0 || !__instance.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var skill = (Skill)which;
                var experienceAmount = GetMultipliedExperience(howMuch);
                var oldExperienceLevel = _archipelagoExperience[skill];
                var newExperienceLevel = _archipelagoExperience[skill] + experienceAmount;
                _archipelagoExperience[skill] = newExperienceLevel;
                var newLevel = GetLevel(_archipelagoExperience[skill]);
                for (var i = 1; i <= newLevel; i++)
                {
                    var checkedLocation = string.Format(_skillLocationName, i, skill.ToString());
                    _locationChecker.AddCheckedLocation(checkedLocation);
                }

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GainExperience_ArchipelagoExperience_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static double GetMultipliedExperience(int amount)
        {
            return amount * _archipelago.SlotData.ExperienceMultiplier;
        }

        private static int GetLevel(double experienceAmount)
        {
            return GetLevel((int)Math.Floor(experienceAmount));
        }

        private static int GetLevel(int experienceAmount)
        {
            return experienceAmount switch
            {
                < 100 => 0,
                < 380 => 1,
                < 770 => 2,
                < 1300 => 3,
                < 2150 => 4,
                < 3300 => 5,
                < 4800 => 6,
                < 6900 => 7,
                < 10000 => 8,
                < 15000 => 9,
                _ => 10
            };
        }

        private static int GetExperienceNeeded(int level)
        {
            return level switch
            {
                1 => 100,
                2 => 380,
                3 => 770,
                4 => 1300,
                5 => 2150,
                6 => 3300,
                7 => 4800,
                8 => 6900,
                9 => 10000,
                10 => 15000,
                _ => 0
            };
        }

        public static void AddMissingSkillsToDictionary()
        {
            foreach (var skill in Enum.GetValues<Skill>())
            {
                if (_archipelagoExperience.ContainsKey(skill))
                {
                    continue;
                }

                _archipelagoExperience.Add(skill, 0);
            }
        }

        public static void ResetSkillExperience()
        {
            _archipelagoExperience = new Dictionary<Skill, double>();
            AddMissingSkillsToDictionary();
        }

        public static void ResetModSkillLevel()
        {
            _archipelagoSkillLevel = new Dictionary<Skill, int>();
            foreach (var skill in Enum.GetValues<Skill>())
            {
                if (_archipelagoSkillLevel.ContainsKey(skill))
                {
                    continue;
                }

                _archipelagoSkillLevel.Add(skill, 0);
            }
        }

        public static int ModdedGetLevel(int modSkillExp)
        {
            return GetLevel(modSkillExp);
        }

        public static List<int> EnabledSkills()
        {
            var modSkillList = new List<int>() { 0, 1, 2, 3, 4 };
            if (_archipelago.SlotData.Mods.HasMod(ModNames.LUCK))
            {
                modSkillList.Add((int)Skill.Luck);
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.BINNING))
            {
                modSkillList.Add((int)Skill.Binning);
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                modSkillList.Add((int)Skill.Magic);
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.ARCHAEOLOGY))
            {
                modSkillList.Add((int)Skill.Excavation);
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.COOKING))
            {
                modSkillList.Add((int)Skill.Cooking);
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.SOCIALIZING))
            {
                modSkillList.Add((int)Skill.Socializing);
            }

            return modSkillList;
        }

        public static bool AddExperience_ArchipelagoModExperience_Prefix(Farmer __instance, string skillName, int amt)
        {
            try
            {
                var skillActualName = skillName.Split('.').Last().Replace("Skill", "");
                if (!_archipelago.SlotData.Mods.HasMod(_skillNameToModDict[skillActualName]))
                {
                    return true; // run original logic
                }

                var skill = (Skill)Enum.Parse(typeof(Skill), skillActualName);
                var experienceAmount = GetMultipliedExperience(amt);
                var oldExperienceLevel = _archipelagoExperience[skill];
                var newExperienceLevel = _archipelagoExperience[skill] + experienceAmount;
                _archipelagoExperience[skill] = newExperienceLevel;
                var newLevel = GetLevel(_archipelagoExperience[skill]);
                for (var i = 1; i <= newLevel; i++)
                {
                    if (skill == Skill.Excavation)
                    {
                        var archaeologyLocation = string.Format(_skillLocationName, i, ModNames.ARCHAEOLOGY);
                        _locationChecker.AddCheckedLocation(archaeologyLocation);
                        continue;
                    }

                    var checkedLocation = string.Format(_skillLocationName, i, skill.ToString());
                    _locationChecker.AddCheckedLocation(checkedLocation);

                }

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AddExperience_ArchipelagoModExperience_Prefix)}:\n{ex}",
                    LogLevel.Error);
                return true; // run original logic
            }
        }
    }

    public enum Skill
    {
        Farming = 0,
        Fishing = 1,
        Foraging = 2,
        Mining = 3,
        Combat = 4,
        Luck = 5,
        Magic = 6,
        Socializing = 7,
        Excavation = 8,
        Binning = 9,
        Cooking = 10
    }
}