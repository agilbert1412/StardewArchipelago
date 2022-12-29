using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Stardew
{
    public class Weapon : StardewItem
    {
        public int MinDamage { get; }
        public int MaxDamage { get; }
        public double KnockBack { get; }
        public double Speed { get; }
        public double AddedPrecision { get; }
        public double AddedDefence { get; }
        public int Type { get; }
        public int BaseMineLevel { get; }
        public int MinMineLevel { get; }
        public double AddedAoe { get; }
        public double CriticalChance { get; }
        public double CriticalDamage { get; }


        public Weapon(int id, string name, string description, int minDamage, int maxDamage, double knockBack, double speed, double addedPrecision, double addedDefence, int type, int baseMineLevel, int minMineLevel, double addedAoe, double criticalChance, double criticalDamage, string displayName)
        : base(id, name, /* TODO */1, displayName, description)
        {
            MinDamage = minDamage;
            MaxDamage = maxDamage;
            KnockBack = knockBack;
            Speed = speed;
            AddedPrecision = addedPrecision;
            AddedDefence = addedDefence;
            Type = type;
            BaseMineLevel = baseMineLevel;
            MinMineLevel = minMineLevel;
            AddedAoe = addedAoe;
            CriticalChance = criticalChance;
            CriticalDamage = criticalDamage;
        }

        public override Item PrepareForGivingToFarmer(int amount = 1)
        {
            return new MeleeWeapon(Id);
        }

        public override void GiveToFarmer(Farmer farmer, int amount = 1)
        {
            var weapon = PrepareForGivingToFarmer();
            farmer.addItemByMenuIfNecessary(weapon);
        }
    }
}
