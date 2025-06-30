using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items.Traps
{
    public class GiftTrapManager
    {
        private readonly TrapExecutor _trapExecutor;
        private ConcurrentQueue<QueuedTrap> _trapQueue;

        private readonly Dictionary<string, Action<double, double>> _giftTraps;

        public GiftTrapManager(TrapExecutor trapExecutor)
        {
            _trapExecutor = trapExecutor;
            _giftTraps = new Dictionary<string, Action<double, double>>();
            RegisterTraps();
        }

        public void AssignTrapQueue(ConcurrentQueue<QueuedTrap> queue)
        {
            _trapQueue = queue;
        }

        private void RegisterTraps()
        {
            _giftTraps.Add(GiftFlag.Bomb, SpawnBomb);
            _giftTraps.Add(GiftFlag.Armor, GetJinxed);
            _giftTraps.Add(GiftFlag.Speed, GetSlowed);
            _giftTraps.Add(GiftFlag.Slowness, GetSlowed);
            _giftTraps.Add(GiftFlag.Cure, GetPoisoned);
            _giftTraps.Add(GiftFlag.Damage, GetWeaknessed);
            _giftTraps.Add(GiftFlag.Weapon, GetWeaknessed);
            _giftTraps.Add(GiftFlag.Fire, GetBurnt);
            _giftTraps.Add(GiftFlag.Ice, GetFrozen);
            _giftTraps.Add(GiftFlag.Light, GetDarknessed);
            _giftTraps.Add(GiftFlag.Energy, LoseEnergy);
            _giftTraps.Add(GiftFlag.Mana, LoseEnergy);
            _giftTraps.Add(GiftFlag.Heal, GetDamaged);
            _giftTraps.Add(GiftFlag.Life, GetDamaged);
            _giftTraps.Add(GiftFlag.Monster, SpawnMonster);
            _giftTraps.Add(GiftFlag.Animal, SpawnMonster);
            _giftTraps.Add("Teleport", TeleportRandomly);

            // TODO: Code more of these
            //_giftTraps.Add(GiftFlag.Grass, SpawnDebris);
            //_giftTraps.Add(GiftFlag.Seed, UngrowCrops);
            //_giftTraps.Add(GiftFlag.Wood, SpawnTree);
        }

        public void TriggerTrapForTrait(GiftTrait giftTrait, int amount)
        {
            foreach (var (flag, action) in _giftTraps)
            {
                if (!giftTrait.Trait.Equals(flag, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                for (var i = 0; i < amount; i++)
                {
                    _trapQueue.Enqueue(new QueuedGiftTrap($"Gift {flag}", action, giftTrait.Quality, giftTrait.Duration));
                }
            }
        }

        private void SpawnBomb(double quality, double duration)
        {
            var radius = GetRadiusFromGift(quality);
            var delay = GetDelayFromGift(duration);
            _trapExecutor.BombSpawner.SpawnBomb(radius, delay);
        }

        public int GetRadiusFromGift(double quality)
        {
            var radiusDifference = Math.Log(quality, 2) * 2;
            var radius = 5 + radiusDifference;
            return (int)Math.Round(Math.Max(1, Math.Min(25, radius)));
        }

        public int GetDelayFromGift(double duration)
        {
            return (int)Math.Round(duration * 100);
        }

        private void GetJinxed(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.EvilEye, quality, duration);
        }

        private void GetSlowed(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.Slimed, quality, duration, 0.5);
        }

        private void GetPoisoned(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.Nauseous, quality, duration);
        }

        private void GetWeaknessed(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.Weakness, quality, duration);
        }

        private void GetBurnt(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.GoblinsCurse, quality, duration);
        }

        private void GetFrozen(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.Frozen, quality, duration, 0.2);
        }

        private void GetDarknessed(double quality, double duration)
        {
            GetTrapGiftDebuff(Buffs.Darkness, quality, duration, 2.0);
        }

        private void GetTrapGiftDebuff(Buffs whichDebuff, double quality, double duration, double durationMultiplier = 1.0)
        {
            var debuffDuration = (int)Math.Round((int)BuffDuration.HalfHour * duration * quality * durationMultiplier);
            _trapExecutor.DebuffApplier.AddBuff(whichDebuff, debuffDuration);
        }

        private void LoseEnergy(double quality, double duration)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                var reduction = 0.1 * quality * duration;
                var remaining = 1 - reduction;
                farmer.stamina = (float)Math.Max(0, farmer.stamina * remaining);
            }
        }

        private void GetDamaged(double quality, double duration)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                var reduction = 0.1 * quality * duration;
                var remaining = 1 - reduction;
                farmer.health = Math.Max(1, (int)Math.Round(farmer.health * remaining));
            }
        }

        private void SpawnMonster(double quality, double duration)
        {
            _trapExecutor.MonsterSpawner.SpawnOneMonster(Game1.player.currentLocation, quality * duration);
        }

        private void TeleportRandomly(double quality, double duration)
        {
            var destinations = Enum.GetValues<TeleportDestination>();
            var destinationValues = destinations.Select(x => (int)x).ToArray();
            var averageDestination = ((destinationValues.Max() - destinationValues.Min()) * 0.5) + destinationValues.Min();
            var destinationScaled = averageDestination * quality * duration;
            var destinationRounded = (int)Math.Round(destinationScaled);
            var destinationClamped = Math.Clamp(destinationRounded, destinationValues.Min(), destinationValues.Max());
            var destination = (TeleportDestination)destinationClamped;
            _trapExecutor.TeleportRandomly(destination);
        }
    }
}
