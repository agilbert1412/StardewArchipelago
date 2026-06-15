using System;
using System.Collections.Generic;
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
            { TrapItemsDifficulty.Nightmare, BuffDuration.OneMinuteRealTime },
            { TrapItemsDifficulty.Eldritch, BuffDuration.FiveMinutesRealTime },
        };

        public Dictionary<TrapItemsDifficulty, BuffDuration> DefaultDebuffDurations = new()
        {
            { TrapItemsDifficulty.NoTraps, BuffDuration.Zero },
            { TrapItemsDifficulty.Easy, BuffDuration.HalfHour },
            { TrapItemsDifficulty.Medium, BuffDuration.OneHour },
            { TrapItemsDifficulty.Hard, BuffDuration.TwoHours },
            { TrapItemsDifficulty.Hell, BuffDuration.FourHours },
            { TrapItemsDifficulty.Nightmare, BuffDuration.FiveMinutesRealTime },
            { TrapItemsDifficulty.Eldritch, BuffDuration.TwentyMinutesRealTime },
        };

        public Dictionary<TrapItemsDifficulty, double> TaxRates = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.1 },
            { TrapItemsDifficulty.Medium, 0.2 },
            { TrapItemsDifficulty.Hard, 0.4 },
            { TrapItemsDifficulty.Hell, 0.8 },
            { TrapItemsDifficulty.Nightmare, 1 },
            { TrapItemsDifficulty.Eldritch, 2 },
        };

        public Dictionary<TrapItemsDifficulty, TeleportDestination> TeleportDestinations = new()
        {
            { TrapItemsDifficulty.NoTraps, TeleportDestination.None },
            { TrapItemsDifficulty.Easy, TeleportDestination.Nearby },
            { TrapItemsDifficulty.Medium, TeleportDestination.SameMap },
            { TrapItemsDifficulty.Hard, TeleportDestination.SameMapOrHome },
            { TrapItemsDifficulty.Hell, TeleportDestination.PelicanTown },
            { TrapItemsDifficulty.Nightmare, TeleportDestination.Anywhere },
            { TrapItemsDifficulty.Eldritch, TeleportDestination.Anywhere },
        };

        public Dictionary<TrapItemsDifficulty, double> CrowAttackRate = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.05 },
            { TrapItemsDifficulty.Medium, 0.1 },
            { TrapItemsDifficulty.Hard, 0.25 },
            { TrapItemsDifficulty.Hell, 0.60 },
            { TrapItemsDifficulty.Nightmare, 1 },
            { TrapItemsDifficulty.Eldritch, 1 },
        };

        public Dictionary<TrapItemsDifficulty, CrowTargets> CrowValidTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, CrowTargets.None },
            { TrapItemsDifficulty.Easy, CrowTargets.Farm },
            { TrapItemsDifficulty.Medium, CrowTargets.Outside },
            { TrapItemsDifficulty.Hard, CrowTargets.Outside },
            { TrapItemsDifficulty.Hell, CrowTargets.Everywhere },
            { TrapItemsDifficulty.Nightmare, CrowTargets.Everywhere },
            { TrapItemsDifficulty.Eldritch, CrowTargets.Everywhere },
        };

        public Dictionary<TrapItemsDifficulty, double> ScarecrowEfficiency = new()
        {
            { TrapItemsDifficulty.NoTraps, 0.8 },
            { TrapItemsDifficulty.Easy, 0.7 },
            { TrapItemsDifficulty.Medium, 0.6 },
            { TrapItemsDifficulty.Hard, 0.5 },
            { TrapItemsDifficulty.Hell, 0.4 },
            { TrapItemsDifficulty.Nightmare, 0.3 },
            { TrapItemsDifficulty.Eldritch, 0.05 },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfMonsters = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 2 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 8 },
            { TrapItemsDifficulty.Nightmare, 12 },
            { TrapItemsDifficulty.Eldritch, 50 },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfSuperMonsters = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 1 },
            { TrapItemsDifficulty.Hard, 1 },
            { TrapItemsDifficulty.Hell, 2 },
            { TrapItemsDifficulty.Nightmare, 4 },
            { TrapItemsDifficulty.Eldritch, 12 },
        };

        public Dictionary<TrapItemsDifficulty, int> SuperMonsterStrength = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 3 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 4 },
            { TrapItemsDifficulty.Nightmare, 6 },
            { TrapItemsDifficulty.Eldritch, 10 },
        };

        public Dictionary<TrapItemsDifficulty, int> AmountOfDebris = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 20 },
            { TrapItemsDifficulty.Medium, 50 },
            { TrapItemsDifficulty.Hard, 200 },
            { TrapItemsDifficulty.Hell, 400 },
            { TrapItemsDifficulty.Nightmare, 800 },
            { TrapItemsDifficulty.Eldritch, 4000 },
        };

        public Dictionary<TrapItemsDifficulty, int> AmountOfTrees = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 14 },
            { TrapItemsDifficulty.Medium, 28 },
            { TrapItemsDifficulty.Hard, 56 },
            { TrapItemsDifficulty.Hell, 224 },
            { TrapItemsDifficulty.Nightmare, 448 },
            { TrapItemsDifficulty.Eldritch, 2000 },
        };

        public Dictionary<TrapItemsDifficulty, ShuffleTarget> ShuffleTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, ShuffleTarget.None },
            { TrapItemsDifficulty.Easy, ShuffleTarget.Self },
            { TrapItemsDifficulty.Medium, ShuffleTarget.Adjacent },
            { TrapItemsDifficulty.Hard, ShuffleTarget.Close },
            { TrapItemsDifficulty.Hell, ShuffleTarget.SameMap },
            { TrapItemsDifficulty.Nightmare, ShuffleTarget.AllMaps },
            { TrapItemsDifficulty.Eldritch, ShuffleTarget.AllMaps },
        };

        public Dictionary<TrapItemsDifficulty, double> ShuffleRate = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.8 }, // Self essentially only affects the player inventory, so we shuffle most of it
            { TrapItemsDifficulty.Medium, 0.4 },
            { TrapItemsDifficulty.Hard, 0.4 },
            { TrapItemsDifficulty.Hell, 0.5 },
            { TrapItemsDifficulty.Nightmare, 0.6 },
            { TrapItemsDifficulty.Eldritch, 1 },
        };

        public Dictionary<TrapItemsDifficulty, double> ShuffleRateToFriends = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0 },
            { TrapItemsDifficulty.Medium, 0 },
            { TrapItemsDifficulty.Hard, 0 },
            { TrapItemsDifficulty.Hell, 0.04 },
            { TrapItemsDifficulty.Nightmare, 0.1 },
            { TrapItemsDifficulty.Eldritch, 0.2 },
        };

        public Dictionary<TrapItemsDifficulty, int> ExtraSwapsAfterShuffle = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0 },
            { TrapItemsDifficulty.Medium, 0 },
            { TrapItemsDifficulty.Hard, 0 },
            { TrapItemsDifficulty.Hell, 0 },
            { TrapItemsDifficulty.Nightmare, 0 },
            { TrapItemsDifficulty.Eldritch, 100 },
        };

        public Dictionary<TrapItemsDifficulty, int> PariahFriendshipLoss = new()
        {
            { TrapItemsDifficulty.NoTraps, -0 },
            { TrapItemsDifficulty.Easy, -10 },
            { TrapItemsDifficulty.Medium, -10 },
            { TrapItemsDifficulty.Hard, -20 },
            { TrapItemsDifficulty.Hell, -50 },
            { TrapItemsDifficulty.Nightmare, -200 },
            { TrapItemsDifficulty.Eldritch, -2500 },
        };

        public Dictionary<TrapItemsDifficulty, int> PariahShunningDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0 },
            { TrapItemsDifficulty.Medium, 1 },
            { TrapItemsDifficulty.Hard, 3 },
            { TrapItemsDifficulty.Hell, 5 },
            { TrapItemsDifficulty.Nightmare, 10 },
            { TrapItemsDifficulty.Eldritch, 28 },
        };

        public Dictionary<TrapItemsDifficulty, int> PariahShunningDistance = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 5 },
            { TrapItemsDifficulty.Hard, 10 },
            { TrapItemsDifficulty.Hell, 24 },
            { TrapItemsDifficulty.Nightmare, 64 },
            { TrapItemsDifficulty.Eldritch, 256 },
        };

        public Dictionary<TrapItemsDifficulty, DroughtTarget> DroughtTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, DroughtTarget.None },
            { TrapItemsDifficulty.Easy, DroughtTarget.Soil },
            { TrapItemsDifficulty.Medium, DroughtTarget.Crops },
            { TrapItemsDifficulty.Hard, DroughtTarget.CropsIncludingInside },
            { TrapItemsDifficulty.Hell, DroughtTarget.CropsAndWateringCan },
            { TrapItemsDifficulty.Nightmare, DroughtTarget.All },
            { TrapItemsDifficulty.Eldritch, DroughtTarget.All },
        };

        public Dictionary<TrapItemsDifficulty, TimeFliesDuration> TimeFliesDurations = new()
        {
            { TrapItemsDifficulty.NoTraps, TimeFliesDuration.Zero },
            { TrapItemsDifficulty.Easy, TimeFliesDuration.OneHour },
            { TrapItemsDifficulty.Medium, TimeFliesDuration.TwoHours },
            { TrapItemsDifficulty.Hard, TimeFliesDuration.SixHours },
            { TrapItemsDifficulty.Hell, TimeFliesDuration.TwelveHours },
            { TrapItemsDifficulty.Nightmare, TimeFliesDuration.TwoDays },
            { TrapItemsDifficulty.Eldritch, TimeFliesDuration.TwoWeeks },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfBabies = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 4 },
            { TrapItemsDifficulty.Medium, 8 },
            { TrapItemsDifficulty.Hard, 16 },
            { TrapItemsDifficulty.Hell, 32 },
            { TrapItemsDifficulty.Nightmare, 128 },
            { TrapItemsDifficulty.Eldritch, 64 },
        };

        public Dictionary<TrapItemsDifficulty, int> BabiesDespawnAge = new()
        {
            { TrapItemsDifficulty.NoTraps, 1 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 1 },
            { TrapItemsDifficulty.Hard, 1 },
            { TrapItemsDifficulty.Hell, 1 },
            { TrapItemsDifficulty.Nightmare, 40 },
            { TrapItemsDifficulty.Eldritch, 80 },
        };

        public Dictionary<TrapItemsDifficulty, int> MeowBarkNumber = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 4 },
            { TrapItemsDifficulty.Medium, 8 },
            { TrapItemsDifficulty.Hard, 16 },
            { TrapItemsDifficulty.Hell, 32 },
            { TrapItemsDifficulty.Nightmare, 128 },
            { TrapItemsDifficulty.Eldritch, 128 },
        };

        public Dictionary<TrapItemsDifficulty, int> NoiseNumber = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 4 },
            { TrapItemsDifficulty.Hard, 8 },
            { TrapItemsDifficulty.Hell, 16 },
            { TrapItemsDifficulty.Nightmare, 64 },
            { TrapItemsDifficulty.Eldritch, 96 },
        };

        public Dictionary<TrapItemsDifficulty, int> DepressionTrapDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 3 },
            { TrapItemsDifficulty.Hard, 7 },
            { TrapItemsDifficulty.Hell, 14 },
            { TrapItemsDifficulty.Nightmare, 28 },
            { TrapItemsDifficulty.Eldritch, 224 },
        };

        public Dictionary<TrapItemsDifficulty, int> UngrowthDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 2 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 8 },
            { TrapItemsDifficulty.Nightmare, 14 },
            { TrapItemsDifficulty.Eldritch, 56 },
        };

        public Dictionary<TrapItemsDifficulty, int> TreeUngrowthDays = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 4 },
            { TrapItemsDifficulty.Hard, 7 },
            { TrapItemsDifficulty.Hell, 21 },
            { TrapItemsDifficulty.Nightmare, 56 },
            { TrapItemsDifficulty.Eldritch, 112 },
        };

        public Dictionary<TrapItemsDifficulty, double> InflationAmount = new()
        {
            { TrapItemsDifficulty.NoTraps, 1 },
            { TrapItemsDifficulty.Easy, 1.1 },
            { TrapItemsDifficulty.Medium, 1.25 }, // Vanilla Inflation at Clint's after a year is equivalent to 2 traps
            { TrapItemsDifficulty.Hard, 1.75 },
            { TrapItemsDifficulty.Hell, 2.25 }, // Vanilla Inflation at Robin's after a year is equivalent to 2 traps
            { TrapItemsDifficulty.Nightmare, 3 },
            { TrapItemsDifficulty.Eldritch, 7 },
        };

        public Dictionary<TrapItemsDifficulty, int> InflationSoftcapThreshold = new()
        {
            { TrapItemsDifficulty.NoTraps, 2 },
            { TrapItemsDifficulty.Easy, 4 },
            { TrapItemsDifficulty.Medium, 8 },
            { TrapItemsDifficulty.Hard, 10 },
            { TrapItemsDifficulty.Hell, 10 },
            { TrapItemsDifficulty.Nightmare, 12 },
            { TrapItemsDifficulty.Eldritch, 30 },
        };

        public Dictionary<TrapItemsDifficulty, int> ExplosionSize = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 3 }, // Cherry Bomb
            { TrapItemsDifficulty.Hard, 5 }, // Bomb
            { TrapItemsDifficulty.Hell, 7 }, // Mega Bomb
            { TrapItemsDifficulty.Nightmare, 15 }, // Good luck!
            { TrapItemsDifficulty.Eldritch, 64 },
        };

        public Dictionary<TrapItemsDifficulty, double> NudgeChance = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.1 },
            { TrapItemsDifficulty.Medium, 0.4 },
            { TrapItemsDifficulty.Hard, 1 },
            { TrapItemsDifficulty.Hell, 4 },
            { TrapItemsDifficulty.Nightmare, 8 },
            { TrapItemsDifficulty.Eldritch, 64 },
        };

        public Dictionary<TrapItemsDifficulty, int> HoardAmount = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 10 },
            { TrapItemsDifficulty.Hard, 25 },
            { TrapItemsDifficulty.Hell, 60 },
            { TrapItemsDifficulty.Nightmare, 200 },
            { TrapItemsDifficulty.Eldritch, 1600 },
        };

        public Dictionary<TrapItemsDifficulty, ButterfingersTarget> ButterfingersTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, ButterfingersTarget.None },
            { TrapItemsDifficulty.Easy, ButterfingersTarget.ActiveItem },
            { TrapItemsDifficulty.Medium, ButterfingersTarget.Hotbar },
            { TrapItemsDifficulty.Hard, ButterfingersTarget.Inventory },
            { TrapItemsDifficulty.Hell, ButterfingersTarget.Inventory },
            { TrapItemsDifficulty.Nightmare, ButterfingersTarget.InventoryAndChestsOnSameMap },
            { TrapItemsDifficulty.Eldritch, ButterfingersTarget.InventoryAndAllChests },
        };

        public Dictionary<TrapItemsDifficulty, double> ButterfingersRate = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 0.5 },
            { TrapItemsDifficulty.Hard, 0.5 },
            { TrapItemsDifficulty.Hell, 1 },
            { TrapItemsDifficulty.Nightmare, 0.5 },
            { TrapItemsDifficulty.Eldritch, 1 },
        };

        public Dictionary<TrapItemsDifficulty, int> ButterfingersExtraDrops = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0 },
            { TrapItemsDifficulty.Medium, 0 },
            { TrapItemsDifficulty.Hard, 0 },
            { TrapItemsDifficulty.Hell, 0 },
            { TrapItemsDifficulty.Nightmare, 12 },
            { TrapItemsDifficulty.Eldritch, 128 },
        };

        public Dictionary<TrapItemsDifficulty, double> SellRate = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 0.01 },
            { TrapItemsDifficulty.Medium, 0.02 },
            { TrapItemsDifficulty.Hard, 0.05 },
            { TrapItemsDifficulty.Hell, 0.1 },
            { TrapItemsDifficulty.Nightmare, 0.2 },
            { TrapItemsDifficulty.Eldritch, 0.5 },
        };

        public Dictionary<TrapItemsDifficulty, int> SellNumberCap = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 2 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 8 },
            { TrapItemsDifficulty.Nightmare, 32 },
            { TrapItemsDifficulty.Eldritch, 128 },
        };

        public Dictionary<TrapItemsDifficulty, double> SellMultiplier = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 2 },
            { TrapItemsDifficulty.Medium, 1.5 },
            { TrapItemsDifficulty.Hard, 1 },
            { TrapItemsDifficulty.Hell, 1 },
            { TrapItemsDifficulty.Nightmare, 0.5 },
            { TrapItemsDifficulty.Eldritch, 0.1 },
        };

        public Dictionary<TrapItemsDifficulty, int> EncumberAmount = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 8 },
            { TrapItemsDifficulty.Medium, 16 },
            { TrapItemsDifficulty.Hard, 32 },
            { TrapItemsDifficulty.Hell, 64 },
            { TrapItemsDifficulty.Nightmare, 256 },
            { TrapItemsDifficulty.Eldritch, 1024 },
        };

        public Dictionary<TrapItemsDifficulty, int> SpoilsNumber = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 4 },
            { TrapItemsDifficulty.Medium, 8 },
            { TrapItemsDifficulty.Hard, 16 },
            { TrapItemsDifficulty.Hell, 64 },
            { TrapItemsDifficulty.Nightmare, 256 },
            { TrapItemsDifficulty.Eldritch, 1024 },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfCows = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 10 },
            { TrapItemsDifficulty.Medium, 20 },
            { TrapItemsDifficulty.Hard, 30 },
            { TrapItemsDifficulty.Hell, 50 },
            { TrapItemsDifficulty.Nightmare, 200 },
            { TrapItemsDifficulty.Eldritch, 1000 },
        };

        public Dictionary<TrapItemsDifficulty, double> CowDespawnChancePerDay = new()
        {
            { TrapItemsDifficulty.NoTraps, 1 },
            { TrapItemsDifficulty.Easy, 0.6 },
            { TrapItemsDifficulty.Medium, 0.4 },
            { TrapItemsDifficulty.Hard, 0.2 },
            { TrapItemsDifficulty.Hell, 0.1 },
            { TrapItemsDifficulty.Nightmare, 0.05 },
            { TrapItemsDifficulty.Eldritch, 0.02 },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfFish = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 1 },
            { TrapItemsDifficulty.Hard, 1 },
            { TrapItemsDifficulty.Hell, 2 },
            { TrapItemsDifficulty.Nightmare, 4 },
            { TrapItemsDifficulty.Eldritch, 12 },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfMenus = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 1 },
            { TrapItemsDifficulty.Hard, 2 },
            { TrapItemsDifficulty.Hell, 4 },
            { TrapItemsDifficulty.Nightmare, 12 },
            { TrapItemsDifficulty.Eldritch, 32 },
        };

        public Dictionary<TrapItemsDifficulty, int> NumberOfEmotes = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 1 },
            { TrapItemsDifficulty.Medium, 2 },
            { TrapItemsDifficulty.Hard, 4 },
            { TrapItemsDifficulty.Hell, 8 },
            { TrapItemsDifficulty.Nightmare, 24 },
            { TrapItemsDifficulty.Eldritch, 80 },
        };

        public Dictionary<TrapItemsDifficulty, MakeoverTargets> MakeoverTargets = new()
        {
            { TrapItemsDifficulty.NoTraps, Traps.MakeoverTargets.None },
            { TrapItemsDifficulty.Easy, Traps.MakeoverTargets.Shirt },
            { TrapItemsDifficulty.Medium, Traps.MakeoverTargets.OutfitServices },
            { TrapItemsDifficulty.Hard, Traps.MakeoverTargets.OutfitServices | Traps.MakeoverTargets.Eyes },
            { TrapItemsDifficulty.Hell, Traps.MakeoverTargets.OutfitServices | Traps.MakeoverTargets.Eyes | Traps.MakeoverTargets.Hair },
            { TrapItemsDifficulty.Nightmare, Traps.MakeoverTargets.Clothes | Traps.MakeoverTargets.PhysicalAppearance },
            { TrapItemsDifficulty.Eldritch, Traps.MakeoverTargets.RandomizeEntireAppearance },
        };

        public Dictionary<TrapItemsDifficulty, float> EnergyToRemove = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 20 },
            { TrapItemsDifficulty.Medium, 50 },
            { TrapItemsDifficulty.Hard, 100 },
            { TrapItemsDifficulty.Hell, 200 },
            { TrapItemsDifficulty.Nightmare, 500 },
            { TrapItemsDifficulty.Eldritch, 2000 },
        };

        public Dictionary<TrapItemsDifficulty, int> HealthToRemove = new()
        {
            { TrapItemsDifficulty.NoTraps, 0 },
            { TrapItemsDifficulty.Easy, 10 },
            { TrapItemsDifficulty.Medium, 30 },
            { TrapItemsDifficulty.Hard, 75 },
            { TrapItemsDifficulty.Hell, 125 },
            { TrapItemsDifficulty.Nightmare, 250 },
            { TrapItemsDifficulty.Eldritch, 1000 },
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
        CropsAndWateringCan = 4,
        All = 5,
    }

    public enum TimeFliesDuration
    {
        Zero = 0,
        OneHour = 6,
        TwoHours = 12,
        SixHours = 36,
        TwelveHours = 72,
        TwoDays = 240,
        TwoWeeks = 1680,
    }

    public enum ButterfingersTarget
    {
        None,
        ActiveItem,
        Hotbar,
        Inventory,
        InventoryAndChestsOnSameMap,
        InventoryAndAllChests,
    }

    [Flags]
    public enum MakeoverTargets
    {
        None =        0,
        Pants =       0b00000001,
        Shirt =       0b00000010,
        Hat =         0b00000100,
        Hair =        0b00001000,
        Eyes =        0b00010000,
        Gender =      0b00100000,
        Accessory =   0b01000000,
        FollowTheme = 0b10000000,
        Clothes = Shirt | Pants | Hat,
        PhysicalAppearance = Hair | Eyes | Gender,
        OutfitServices = Clothes | FollowTheme,
        RandomizeEntireAppearance = Clothes | PhysicalAppearance | Accessory,
    }
}
