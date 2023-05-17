using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Items.Unlocks
{
    public class ModUnlockManager : IUnlockManager
    {
        public const string Excalibur = "Excalibur";
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public ModUnlockManager()
        {
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            RegisterAPMods();
        }

        public void RegisterAPMods()
        {
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
            const int whichSkill = (int)Skill.Luck;
            foreach (var farmer in Game1.getAllFarmers())
            {
                var experienceForLevelUp = GetExperienceToNextLevel(farmer, whichSkill);
                farmer.experiencePoints[whichSkill] += experienceForLevelUp;
                farmer.LuckLevel = farmer.luckLevel.Value + 1;
                farmer.newLevels.Add(new Point(whichSkill, farmer.luckLevel.Value));
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveMagicLevel(ReceivedItem receivedItem)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                ReceiveAPLevel("spacechase0.Magic");
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveBinningLevel(ReceivedItem receivedItem)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                ReceiveAPLevel("drbirbdev.Binning");
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveCookingLevel(ReceivedItem receivedItem)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                ReceiveAPLevel("spacechase0.Cooking");
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveSocializingLevel(ReceivedItem receivedItem)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                ReceiveAPLevel("drbirbdev.Socializing");
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveArchaeologyLevel(ReceivedItem receivedItem)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                ReceiveAPLevel("moonslime.ExcavationSkill");
            }
            return new LetterInformationAttachment(receivedItem);
        }

        private int GetExperienceToNextLevel(Farmer farmer, int skill)
        {
            switch (farmer.experiencePoints[skill])
            {
                case < 100:
                    return 100 - farmer.experiencePoints[skill];
                case < 380:
                    return 380 - farmer.experiencePoints[skill];
                case < 770:
                    return 770 - farmer.experiencePoints[skill];
                case < 1300:
                    return 1300 - farmer.experiencePoints[skill];
                case < 2150:
                    return 2150 - farmer.experiencePoints[skill];
                case < 3300:
                    return 3300 - farmer.experiencePoints[skill];
                case < 4800:
                    return 4800 - farmer.experiencePoints[skill];
                case < 6900:
                    return 6900 - farmer.experiencePoints[skill];
                case < 10000:
                    return 10000 - farmer.experiencePoints[skill];
                case < 15000:
                    return 15000 - farmer.experiencePoints[skill];
            }

            return 0;
        }

        private int GetModdedExperienceToNextLevel(int amt)
        {
            switch (amt)
            {
                case < 100:
                    return 100 - amt;
                case < 380:
                    return 380 - amt;
                case < 770:
                    return 770 - amt;
                case < 1300:
                    return 1300 - amt;
                case < 2150:
                    return 2150 - amt;
                case < 3300:
                    return 3300 - amt;
                case < 4800:
                    return 4800 - amt;
                case < 6900:
                    return 6900 - amt;
                case < 10000:
                    return 10000 - amt;
                case < 15000:
                    return 15000 - amt;
            }

            return 0;
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
            var modPrevSkillLevel = SkillInjections.ModdedGetLevel(modOldSkillExp);
            var expToNextLevel = GetModdedExperienceToNextLevel(modOldSkillExp);
            exp[farmer.UniqueMultiplayerID][skill] += expToNextLevel;
            var modNewSkillExp = exp[farmer.UniqueMultiplayerID][skill];
            var modNewSkillLevel = SkillInjections.ModdedGetLevel(modNewSkillExp);
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