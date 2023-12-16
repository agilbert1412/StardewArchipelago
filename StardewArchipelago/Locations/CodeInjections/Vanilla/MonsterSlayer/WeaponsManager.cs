using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer
{
    public class WeaponsManager
    {
        public const string TYPE_SWORD = "Sword";
        public const string TYPE_CLUB = "Club";
        public const string TYPE_DAGGER = "Dagger";
        private ModsManager _modsManager;
        private Dictionary<int, List<int>> weightsByTotal = new Dictionary<int, List<int>>(){
            {5, new List<int>{1, 2, 3, 2, 1}},
            {6, new List<int>{1, 2, 3, 3, 2, 1}}
        };
        public Dictionary<string, Dictionary<int, List<StardewItem>>> WeaponsByCategoryByTier { get; private set; }        
        public Dictionary<int, List<StardewItem>> WeaponsByTier { get; private set; }
        public Dictionary<int, List<StardewItem>> BootsByTier { get; private set; }
        public Dictionary<int, List<StardewItem>> SlingshotsByTier { get; private set; }

        public WeaponsManager(StardewItemManager itemManager, ModsManager modsManager)
        {
            _modsManager = modsManager;
            InitializeWeapons(itemManager);
            InitializeBoots(itemManager);
            InitializeSlingshots(itemManager);
        }

        private void InitializeSlingshots(StardewItemManager itemManager)
        {
            SlingshotsByTier = new Dictionary<int, List<StardewItem>>()
            {
                { 1, new List<StardewItem> { itemManager.GetWeaponByName("Slingshot") } },
                { 2, new List<StardewItem> { itemManager.GetWeaponByName("Master Slingshot") } }
            };
        }

        private void InitializeWeapons(StardewItemManager itemManager)
        {
            WeaponsByCategoryByTier = new Dictionary<string, Dictionary<int, List<StardewItem>>>();
            WeaponsByTier = new Dictionary<int, List<StardewItem>>();
            var WeaponsByTierAndType = new Dictionary<string, Dictionary<StardewWeapon, int>>();
            var totalProgressiveWeapons = _modsManager.HasMod(ModNames.SVE) ? 6:5;
            var weightList = weightsByTotal[totalProgressiveWeapons];
            var weightTotal = weightList.Sum();
            var weapons = itemManager.GetAllWeapons();
            foreach (var weapon in weapons)
            {
                if (weapon.PrepareForGivingToFarmer() is not MeleeWeapon stardewWeapon)
                {
                    continue;
                }

                var weaponLevel = stardewWeapon.getItemLevel();
                var type = weapon.Type switch
                {
                    1 => TYPE_DAGGER,
                    2 => TYPE_CLUB,
                    _ => TYPE_SWORD,
                };

                if (!WeaponsByTierAndType.ContainsKey(type))
            {
                WeaponsByTierAndType.Add(type, new Dictionary<StardewWeapon, int>());
            }
                WeaponsByTierAndType[type][weapon] = weaponLevel;
            }
            foreach (var weapon in WeaponsByTierAndType)
            { 
                var weaponCountList = new List<int>{};
                var typeWeapons = weapon.Value.OrderBy(kvp => kvp.Value);
                var typeWeaponCount = typeWeapons.Count();
                foreach (var weight in weightList)
                {
                    weaponCountList.Add(weight * typeWeaponCount / weightTotal);
                }
                if (weaponCountList.Sum() < typeWeaponCount) // Throw extra by rounding error into the third entry
                {
                    weaponCountList[2] += typeWeaponCount - weaponCountList.Sum();
                }
                var tier = 0;
                var startingValue = 0;
                for (var j = 1; j <= totalProgressiveWeapons; j++)
                {
                    var countByTier = weaponCountList[j-1];
                    for (var i = 1; i <= countByTier; i++)
                    {
                        var currentWeapon = typeWeapons.ElementAt(i + startingValue - 1);
                        AddToWeapons(currentWeapon.Key, weapon.Key, tier);
                    }
                    tier += 1;
                    startingValue += weaponCountList[j-1];
                }
            }
        }

        private void AddToWeapons(StardewWeapon weapon, string category, int tier)
        {
            if (!WeaponsByTier.ContainsKey(tier))
            {
                WeaponsByTier.Add(tier, new List<StardewItem>());
            }

            if (!WeaponsByCategoryByTier.ContainsKey(category))
            {
                WeaponsByCategoryByTier.Add(category, new Dictionary<int, List<StardewItem>>());
            }

            if (!WeaponsByCategoryByTier[category].ContainsKey(tier))
            {
                WeaponsByCategoryByTier[category].Add(tier, new List<StardewItem>());
            }

            WeaponsByTier[tier].Add(weapon);
            WeaponsByCategoryByTier[category][tier].Add(weapon);
        }

        private void InitializeBoots(StardewItemManager itemManager)
        {
            BootsByTier = new Dictionary<int, List<StardewItem>>();
            var boots = itemManager.GetAllBoots();
            foreach (var boot in boots)
            {
                var stardewBoots = (Boots)boot.PrepareForGivingToFarmer();
                var bootsLevel = stardewBoots.defenseBonus.Value + stardewBoots.immunityBonus.Value;
                var tier = bootsLevel switch
                {
                    <= 2 => 1,
                    <= 5 => 2,
                    <= 8 => 3,
                    _ => 4,
                };

                AddToBoots(boot, tier);
            }
            BootsByTier.Add(5, new List<StardewItem>());
        }

        private void AddToBoots(StardewBoots boot, int tier)
        {
            if (!BootsByTier.ContainsKey(tier))
            {
                BootsByTier.Add(tier, new List<StardewItem>());
            }

            BootsByTier[tier].Add(boot);
        }
    }
}
