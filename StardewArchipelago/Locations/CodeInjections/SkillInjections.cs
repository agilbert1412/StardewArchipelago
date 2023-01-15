using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class SkillInjections
    {
        private const string _skillLocationName = "Level {0} {1}";

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Dictionary<Skill, double> _archipelagoExperience = new();

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            InitializeSkillExperienceToZero();
        }

        public static Dictionary<int, int> GetArchipelagoExperience()
        {
            return _archipelagoExperience.ToDictionary(x => (int)x.Key, x => (int)Math.Round(x.Value));
        }

        public static void SetArchipelagoExperience(Dictionary<int, int> values)
        {
            if (values == null)
            {
                _archipelagoExperience = new Dictionary<Skill, double>();
                InitializeSkillExperienceToZero();
                return;
            }

            _archipelagoExperience = values.ToDictionary(x => (Skill)x.Key, x => (double)x.Value);
        }

        public static bool GainExperience_NormalExperience_Prefix(Farmer __instance, int which, int howMuch)
        {
            try
            {
                if (which < 0 || which > 4 || howMuch <= 0 || !__instance.IsLocalPlayer)
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
                _monitor.Log($"Failed in {nameof(GainExperience_ArchipelagoExperience_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool GainExperience_ArchipelagoExperience_Prefix(Farmer __instance, int which, int howMuch)
        {
            try
            {
                if (which < 0 || which > 4 || howMuch <= 0 || !__instance.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var skill = (Skill)which;
                var experienceAmount = GetMultipliedExperience(howMuch);
                var oldExperienceLevel = _archipelagoExperience[skill];
                var newExperienceLevel = _archipelagoExperience[skill] + experienceAmount;
                _archipelagoExperience[skill] = newExperienceLevel;
                var newLevel = GetLevel(_archipelagoExperience[skill]);
                if (newLevel < 1)
                {
                    return false; // don't run original logic
                }

                var checkedLocation = string.Format(_skillLocationName, newLevel, skill.ToString());
                _locationChecker.AddCheckedLocation(checkedLocation);
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

        private static void InitializeSkillExperienceToZero()
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
    }

    public enum Skill
    {
        Farming = 0,
        Fishing = 1,
        Foraging = 2,
        Mining = 3,
        Combat = 4,
        Luck = 5,
    }
}
