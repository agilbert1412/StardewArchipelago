using System;
using System.Linq;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items.Traps
{
    public class BuffApplier
    {
        private static TrapsStateDto _permanentState;
        public BuffApplier(TrapsStateDto permanentState)
        {
            _permanentState = permanentState;
        }

        public bool IsBuff(string buffName, out Buffs buff)
        {
            buffName = buffName.Replace("_", " ");

            if (buffName.EndsWith(" Enzyme"))
            {
                buffName = buffName[..^" Enzyme".Length];
            }

            if (Enum.TryParse(buffName, out buff))
            {
                return true;
            }

            switch (buffName)
            {
                case "Max Stamina":
                case "Max Energy":
                    buff = Buffs.MaxStamina;
                    return true;
                case "Magnetism":
                case "Magnet":
                case "Magnetic":
                case "Magnetic Radius":
                    buff = Buffs.MagneticRadius;
                    return true;
                case "Squid Ink":
                case "Squid Ink Ravioli":
                    buff = Buffs.SquidInkRavioli;
                    return true;
                case "Monster Musk":
                    buff = Buffs.SpawnMonsters;
                    return true;
                case "Oil Of Garlic":
                case "Oil of Garlic":
                    buff = Buffs.AvoidMonsters;
                    return true;
                case "Tipsy":
                    buff = Buffs.Tipsy;
                    return true;
            }

            return false;
        }

        public void AddBuff(Buffs whichBuff, BuffDuration duration)
        {
            if (duration == BuffDuration.Zero)
            {
                return;
            }

            AddBuff(whichBuff, (int)duration, IsRealTime(duration));
        }

        public static bool IsRealTime(BuffDuration duration)
        {
            return duration is BuffDuration.OneMinuteRealTime or BuffDuration.FiveMinutesRealTime or BuffDuration.TwentyMinutesRealTime;
        }

        public void AddBuff(Buffs whichBuff, int duration, bool realTime)
        {
            if (duration <= 0)
            {
                return;
            }

            var buff = new Buff(((int)whichBuff).ToString());
            buff.millisecondsDuration = duration;
            buff.totalMillisecondsDuration = duration;

            switch (whichBuff)
            {
                case Buffs.Farming:
                    buff.effects.FarmingLevel.Value += 1;
                    break;
                case Buffs.Foraging:
                    buff.effects.ForagingLevel.Value += 1;
                    break;
                case Buffs.Fishing:
                    buff.effects.FishingLevel.Value += 1;
                    break;
                case Buffs.Mining:
                    buff.effects.MiningLevel.Value += 1;
                    break;
                case Buffs.Attack:
                    buff.effects.Attack.Value += 1;
                    break;
                case Buffs.Defense:
                    buff.effects.Defense.Value += 1;
                    break;
                case Buffs.Speed:
                    buff.effects.Speed.Value += 1;
                    break;
                case Buffs.Luck:
                    buff.effects.LuckLevel.Value += 1;
                    break;
                case Buffs.MagneticRadius:
                    buff.effects.MagneticRadius.Value += 1;
                    break;
                case Buffs.MaxStamina:
                    buff.effects.MaxStamina.Value += 1;
                    break;
            }

            Game1.player.applyBuff(buff);
            if (realTime)
            {
                if (!_permanentState.CurrentBuffs.ContainsKey(whichBuff))
                {
                    _permanentState.CurrentBuffs.Add(whichBuff, 0);
                }

                _permanentState.CurrentBuffs[whichBuff] = Math.Max(_permanentState.CurrentBuffs[whichBuff], duration);
            }
        }

        public void SaveBuffs()
        {
            foreach (var realTimeBuff in _permanentState.CurrentBuffs.Keys.ToArray())
            {
                SaveBuff(realTimeBuff);
            }
        }

        private static void SaveBuff(Buffs realTimeBuff)
        {
            foreach (var (appliedBuffName, appliedBuff) in Game1.player.buffs.AppliedBuffs)
            {
                if (appliedBuff.id.Equals(((int)realTimeBuff).ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    var timeRemaining = appliedBuff.millisecondsDuration;
                    _permanentState.CurrentBuffs[realTimeBuff] = timeRemaining;
                    return;
                }
            }

            _permanentState.CurrentBuffs.Remove(realTimeBuff);
        }

        public void LoadBuffs()
        {
            foreach (var realTimeBuff in _permanentState.CurrentBuffs.Keys.ToArray())
            {
                LoadBuff(realTimeBuff);
            }
        }

        private void LoadBuff(Buffs realTimeBuff)
        {
            foreach (var (appliedBuffName, appliedBuff) in Game1.player.buffs.AppliedBuffs)
            {
                if (appliedBuff.id.Equals(((int)realTimeBuff).ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    appliedBuff.millisecondsDuration = Math.Max(appliedBuff.millisecondsDuration, _permanentState.CurrentBuffs[realTimeBuff]);
                    return;
                }
            }

            AddBuff(realTimeBuff, _permanentState.CurrentBuffs[realTimeBuff], true);
        }
    }
}
