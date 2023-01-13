using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class SkillsInjections
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
        }

        public static Dictionary<int, int> GetArchipelagoExperience()
        {
            return _archipelagoExperience.ToDictionary(x => (int)x.Key, x => (int)Math.Round(x.Value));
        }

        public static void SetArchipelagoExperience(Dictionary<int, int> values)
        {
            _archipelagoExperience = values.ToDictionary(x => (Skill)x.Key, x => (double)x.Value);
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
                var experienceAmount = howMuch * 1.0;
                var oldExperienceLevel = _archipelagoExperience[skill];
                var newExperienceLevel = _archipelagoExperience[skill] + experienceAmount;
                _archipelagoExperience[skill] = newExperienceLevel;
                var newLevel = GetLevel(_archipelagoExperience[skill]);
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
