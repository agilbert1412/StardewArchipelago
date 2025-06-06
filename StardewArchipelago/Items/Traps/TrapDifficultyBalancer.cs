﻿using System.Collections.Generic;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Items.Traps
{
    public class TrapDifficultyBalancer
    {
        public Dictionary<TrapItemsDifficulty, BuffDuration> FrozenDebuffDurations = new()
        {
            { TrapItemsDifficulty.NoTraps, BuffDuration.Zero },
            { TrapItemsDifficulty.Easy, BuffDuration.TenMinutes },
            { TrapItemsDifficulty.Medium, BuffDuration.HalfHour },
            { TrapItemsDifficulty.Hard, BuffDuration.OneHour },
            { TrapItemsDifficulty.Hell, BuffDuration.TwoHours },
            { TrapItemsDifficulty.Nightmare, BuffDuration.WholeDay },
        };

        public Dictionary<TrapItemsDifficulty, BuffDuration> DefaultDebuffDurations = new()
        {
            { TrapItemsDifficulty.NoTraps, BuffDuration.Zero },
            { TrapItemsDifficulty.Easy, BuffDuration.HalfHour },
            { TrapItemsDifficulty.Medium, BuffDuration.OneHour },
            { TrapItemsDifficulty.Hard, BuffDuration.TwoHours },
            { TrapItemsDifficulty.Hell, BuffDuration.FourHours },
            { TrapItemsDifficulty.Nightmare, BuffDuration.WholeDay },
        };

        public Dictionary<TrapItemsDifficulty, double> TaxRates = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.1 },
            { TrapItemsDifficulty.Medium, 0.2 },
            { TrapItemsDifficulty.Hard, 0.4 },
            { TrapItemsDifficulty.Hell, 0.8 },
            { TrapItemsDifficulty.Nightmare, 1 },
        };

        public Dictionary<TrapItemsDifficulty, TeleportDestination> TeleportDestinations = new()
        {
            { TrapItemsDifficulty.NoTraps, TeleportDestination.None },
            { TrapItemsDifficulty.Easy, TeleportDestination.Nearby },
            { TrapItemsDifficulty.Medium, TeleportDestination.SameMap },
            { TrapItemsDifficulty.Hard, TeleportDestination.SameMapOrHome },
            { TrapItemsDifficulty.Hell, TeleportDestination.PelicanTown },
            { TrapItemsDifficulty.Nightmare, TeleportDestination.Anywhere },
        };

        public Dictionary<TrapItemsDifficulty, double> CrowAttackRate = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.05 },
            { TrapItemsDifficulty.Medium, 0.1 },
            { TrapItemsDifficulty.Hard, 0.25 },
            { TrapItemsDifficulty.Hell, 0.60 },
            { TrapItemsDifficulty.Nightmare, 1 },
        };

        public Dictionary<TrapItemsDifficulty, CrowTargets> CrowValidTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, CrowTargets.None },
            { TrapItemsDifficulty.Easy, CrowTargets.Farm },
            { TrapItemsDifficulty.Medium, CrowTargets.Outside },
            { TrapItemsDifficulty.Hard, CrowTargets.Outside },
            { TrapItemsDifficulty.Hell, CrowTargets.Everywhere },
            { TrapItemsDifficulty.Nightmare, CrowTargets.Everywhere },
        };

        public const double SCARECROW_EFFICIENCY = 0.40;

        public Dictionary<TrapItemsDifficulty, int> NumberOfMonsters = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 2 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 8 },
            { TrapItemsDifficulty.Nightmare, 12 },
        };

        public Dictionary<TrapItemsDifficulty, int> AmountOfDebris = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 20 },
            { TrapItemsDifficulty.Medium, 50 },
            { TrapItemsDifficulty.Hard, 200 },
            { TrapItemsDifficulty.Hell, 400 },
            { TrapItemsDifficulty.Nightmare, 800 },
        };

        public Dictionary<TrapItemsDifficulty, ShuffleTarget> ShuffleTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, ShuffleTarget.None },
            { TrapItemsDifficulty.Easy, ShuffleTarget.Self },
            { TrapItemsDifficulty.Medium, ShuffleTarget.Adjacent },
            { TrapItemsDifficulty.Hard, ShuffleTarget.Close },
            { TrapItemsDifficulty.Hell, ShuffleTarget.SameMap },
            { TrapItemsDifficulty.Nightmare, ShuffleTarget.AllMaps },
        };

        public Dictionary<TrapItemsDifficulty, double> ShuffleRate = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.8 }, // Self essentially only affects the player inventory, so we shuffle most of it
            { TrapItemsDifficulty.Medium, 0.4 },
            { TrapItemsDifficulty.Hard, 0.4 },
            { TrapItemsDifficulty.Hell, 0.5 },
            { TrapItemsDifficulty.Nightmare, 0.6 },
        };

        public Dictionary<TrapItemsDifficulty, double> ShuffleRateToFriends = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0 },
            { TrapItemsDifficulty.Medium, 0 },
            { TrapItemsDifficulty.Hard, 0 },
            { TrapItemsDifficulty.Hell, 0.04 },
            { TrapItemsDifficulty.Nightmare, 0.1 },
        };

        public Dictionary<TrapItemsDifficulty, int> PariahFriendshipLoss = new()
        {
            { TrapItemsDifficulty.NoTraps, -0 },
            { TrapItemsDifficulty.Easy, -10 },
            { TrapItemsDifficulty.Medium, -20 },
            { TrapItemsDifficulty.Hard, -40 },
            { TrapItemsDifficulty.Hell, -100 },
            { TrapItemsDifficulty.Nightmare, -400 },
        };

        public Dictionary<TrapItemsDifficulty, DroughtTarget> DroughtTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, DroughtTarget.None },
            { TrapItemsDifficulty.Easy, DroughtTarget.Soil },
            { TrapItemsDifficulty.Medium, DroughtTarget.Crops },
            { TrapItemsDifficulty.Hard, DroughtTarget.CropsIncludingInside },
            { TrapItemsDifficulty.Hell, DroughtTarget.CropsIncludingInside },
            { TrapItemsDifficulty.Nightmare, DroughtTarget.CropsIncludingWateringCan },
        };

        public Dictionary<TrapItemsDifficulty, TimeFliesDuration> TimeFliesDurations = new()
        {
            { TrapItemsDifficulty.NoTraps, TimeFliesDuration.Zero },
            { TrapItemsDifficulty.Easy, TimeFliesDuration.OneHour },
            { TrapItemsDifficulty.Medium, TimeFliesDuration.TwoHours },
            { TrapItemsDifficulty.Hard, TimeFliesDuration.SixHours },
            { TrapItemsDifficulty.Hell, TimeFliesDuration.TwelveHours },
            { TrapItemsDifficulty.Nightmare, TimeFliesDuration.TwoDays },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfBabies = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 4 },
            { TrapItemsDifficulty.Medium, 8 },
            { TrapItemsDifficulty.Hard, 16 },
            { TrapItemsDifficulty.Hell, 32 },
            { TrapItemsDifficulty.Nightmare, 128 },
        };

        public Dictionary<TrapItemsDifficulty, int> MeowBarkNumber = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 4 },
            { TrapItemsDifficulty.Medium, 8 },
            { TrapItemsDifficulty.Hard, 16 },
            { TrapItemsDifficulty.Hell, 32 },
            { TrapItemsDifficulty.Nightmare, 128 },
        };

        public Dictionary<TrapItemsDifficulty, int> DepressionTrapDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 3 },
            { TrapItemsDifficulty.Hard, 7 },
            { TrapItemsDifficulty.Hell, 14 },
            { TrapItemsDifficulty.Nightmare, 28 },
        };

        public Dictionary<TrapItemsDifficulty, int> UngrowthDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 2 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 8 },
            { TrapItemsDifficulty.Nightmare, 14 },
        };

        public Dictionary<TrapItemsDifficulty, int> TreeUngrowthDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 4 },
            { TrapItemsDifficulty.Hard, 7 },
            { TrapItemsDifficulty.Hell, 21 },
            { TrapItemsDifficulty.Nightmare, 56 },
        };

        public Dictionary<TrapItemsDifficulty, double> InflationAmount = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1.1 },
            { TrapItemsDifficulty.Medium, 1.25 }, // Vanilla Inflation at Clint's after a year is equivalent to 2 traps
            { TrapItemsDifficulty.Hard, 1.75 },
            { TrapItemsDifficulty.Hell, 2.25 }, // Vanilla Inflation at Robin's after a year is equivalent to 2 traps
            { TrapItemsDifficulty.Nightmare, 3 },
        };

        public Dictionary<TrapItemsDifficulty, int> ExplosionSize = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 3 }, // Cherry Bomb
            { TrapItemsDifficulty.Hard, 5 }, // Bomb
            { TrapItemsDifficulty.Hell, 7 }, // Mega Bomb
            { TrapItemsDifficulty.Nightmare, 15 }, // Good luck!
        };

        public Dictionary<TrapItemsDifficulty, double> NudgeChance = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.1 },
            { TrapItemsDifficulty.Medium, 0.4 },
            { TrapItemsDifficulty.Hard, 1 },
            { TrapItemsDifficulty.Hell, 4 },
            { TrapItemsDifficulty.Nightmare, 8 },
        };

        public Dictionary<TrapItemsDifficulty, int> HoardAmount = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 10 },
            { TrapItemsDifficulty.Hard, 25 },
            { TrapItemsDifficulty.Hell, 60 },
            { TrapItemsDifficulty.Nightmare, 200 },
        };
    }

    public enum TeleportDestination
    {
        None,
        Nearby,
        SameMap,
        SameMapOrHome,
        PelicanTown,
        Anywhere,
    }

    public enum CrowTargets
    {
        None,
        Farm,
        Outside,
        Everywhere,
    }

    public enum ShuffleTarget
    {
        None = -1,
        Self = 0,
        Adjacent = 1,
        Close = 3,
        SameMap = int.MaxValue / 10,
        AllMaps = int.MaxValue,
    }

    public enum DroughtTarget
    {
        None = 0,
        Soil = 1,
        Crops = 2,
        CropsIncludingInside = 3,
        CropsIncludingWateringCan = 4,
    }

    public enum TimeFliesDuration
    {
        Zero = 0,
        OneHour = 6,
        TwoHours = 12,
        SixHours = 36,
        TwelveHours = 72,
        TwoDays = 240,
    }
}
