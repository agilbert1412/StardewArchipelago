using StardewValley.Buildings;
using StardewValley.Monsters;
using StardewValley;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Archipelago.Gifting.Net.Traits;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using StardewModdingAPI;

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
            // _giftTraps.Add(GiftFlag.Armor, GetJinxed);

            // TODO: Code more of these
            //_giftTraps.Add(GiftFlag.Speed, GetSlowed);
            //_giftTraps.Add(GiftFlag.Slowness, GetSlowed);
            //_giftTraps.Add(GiftFlag.Cure, GetPoisoned);
            //_giftTraps.Add(GiftFlag.Energy, LoseEnergy);
            //_giftTraps.Add(GiftFlag.Mana, LoseEnergy);
            //_giftTraps.Add(GiftFlag.Heal, GetDamaged);
            //_giftTraps.Add(GiftFlag.Life, GetDamaged);
            //_giftTraps.Add(GiftFlag.Damage, GetWeaknessed);
            //_giftTraps.Add(GiftFlag.Weapon, GetWeaknessed);
            //_giftTraps.Add(GiftFlag.Fire, GetBurnt);
            //_giftTraps.Add(GiftFlag.Grass, SpawnDebris);
            //_giftTraps.Add(GiftFlag.Ice, GetFrozen);
            //_giftTraps.Add(GiftFlag.Light, GetDarknessed);
            //_giftTraps.Add(GiftFlag.Monster, SpawnMonster);
            //_giftTraps.Add(GiftFlag.Animal, SpawnMonster);
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
                    _trapQueue.Enqueue(new QueuedGiftTrap($"Gift {flag}", action));
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
            throw new NotImplementedException();
        }
    }
}
