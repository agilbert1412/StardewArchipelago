using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Mail;
using StardewValley;
using StardewArchipelago.Locations.CodeInjections.Vanilla;

namespace StardewArchipelago.Items.Unlocks
{
    public class ModSkillUnlockManager : IUnlockManager
    {
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public ModSkillUnlockManager()
        {
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            _unlockables.Add($"Magic Level", SendProgressiveMagicLevel);
            _unlockables.Add($"Binning Level", SendProgressiveBinningLevel);
            _unlockables.Add($"Cooking Level", SendProgressiveCookingLevel);
            _unlockables.Add($"Luck Level", SendProgressiveLuckLevel);
            _unlockables.Add($"Archaeology Level", SendProgressiveArchaeologyLevel);
            _unlockables.Add($"Socializing Level", SendProgressiveSocializingLevel);
        }

        public bool IsUnlock(string unlockName)
        {
            return _unlockables.ContainsKey(unlockName);
        }

        public LetterAttachment PerformUnlockAsLetter(ReceivedItem unlock)
        {
            return _unlockables[unlock.ItemName](unlock);
        }

        /*public LetterActionAttachment SendExcalibur(ReceivedItem receivedItem)
        {
            var excaliburType = AccessTools.TypeByName("DeepWoodsMod.Excalibur");
            // Time to do research!
            return new LetterActionAttachment(receivedItem, Excalibur);
        }*/

        private LetterAttachment SendProgressiveLuckLevel(ReceivedItem receivedItem)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                var experienceForLevelUp = farmer.GetExperienceToNextLevel(Skill.Luck);
                farmer.AddExperience(Skill.Luck, experienceForLevelUp);
                farmer.luckLevel.Value++;
                farmer.newLevels.Add(new Point((int)Skill.Luck, farmer.luckLevel.Value));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveMagicLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel("spacechase0.Magic");
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveBinningLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel("drbirbdev.Binning");
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveCookingLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel("spacechase0.Cooking");
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveSocializingLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel("drbirbdev.Socializing");
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveArchaeologyLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel("moonslime.Excavation");
            return new LetterInformationAttachment(receivedItem);
        }

        private void ReceiveAPLevel(string skill)
        {
            var farmer = Game1.player;
            var skillsType = AccessTools.TypeByName("SpaceCore.Skills");
            var expField = ModEntry.Instance.Helper.Reflection.GetField<Dictionary<long, Dictionary<string, int>>>(skillsType, "Exp");
            var exp = expField.GetValue();
            var myNewLevelsField = ModEntry.Instance.Helper.Reflection.GetField<List<KeyValuePair<string, int>>>(skillsType, "NewLevels");
            var myNewLevels = myNewLevelsField.GetValue();
            if (!exp.ContainsKey(farmer.UniqueMultiplayerID))
            {
                exp.Add(farmer.UniqueMultiplayerID, new Dictionary<string, int>());
            }

            if (!exp[farmer.UniqueMultiplayerID].ContainsKey(skill))
            {
                exp[farmer.UniqueMultiplayerID].Add(skill, 0);
            }

            var modOldSkillExp = exp[farmer.UniqueMultiplayerID][skill];
            var modPrevSkillLevel = SkillInjections.GetLevel(modOldSkillExp);
            var expToNextLevel = farmer.GetExperienceToNextLevel(modOldSkillExp);
            exp[farmer.UniqueMultiplayerID][skill] += expToNextLevel;
            var modNewSkillExp = exp[farmer.UniqueMultiplayerID][skill];
            var modNewSkillLevel = SkillInjections.GetLevel(modNewSkillExp);
            if (modPrevSkillLevel != modNewSkillLevel)
            {
                for (var i = modPrevSkillLevel + 1; i <= modNewSkillLevel; ++i)
                {
                    myNewLevels.Add(new KeyValuePair<string, int>(skill, i));
                }
            }
        }
    }
}
