using System;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items.Traps
{
    public class BuffApplier
    {
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

            AddBuff(whichBuff, (int)duration);
        }

        public void AddBuff(Buffs whichBuff, int duration)
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
        }
    }
}
