using System;
using System.Collections.Generic;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Items.Unlocks.Modded
{
    public class ModSkillUnlockManager : IUnlockManager
    {
        private readonly IModHelper _modHelper;

        public ModSkillUnlockManager(IModHelper modHelper)
        {
            _modHelper = modHelper;
        }

        public void RegisterUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            unlocks.Add($"Magic Level", SendProgressiveMagicLevel);
            unlocks.Add($"Binning Level", SendProgressiveBinningLevel);
            unlocks.Add($"Cooking Level", SendProgressiveCookingLevel);
            unlocks.Add($"Luck Level", SendProgressiveLuckLevel);
            unlocks.Add($"Archaeology Level", SendProgressiveArchaeologyLevel);
            unlocks.Add($"Socializing Level", SendProgressiveSocializingLevel);
        }

        /*public LetterActionAttachment SendExcalibur(ReceivedItem receivedItem)
        {
            var excaliburType = AccessTools.TypeByName("DeepWoodsMod.Excalibur");
            // Time to do research!
            return new LetterActionAttachment(receivedItem, Excalibur);
        }*/

        private LetterAttachment SendProgressiveLuckLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel(ArchipelagoSkillIds.LUCK);
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveMagicLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel(ArchipelagoSkillIds.MAGIC);
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveBinningLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel(ArchipelagoSkillIds.BINNING);
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveCookingLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel(ArchipelagoSkillIds.COOKING);
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveSocializingLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel(ArchipelagoSkillIds.SOCIALIZING);
            return new LetterInformationAttachment(receivedItem);
        }

        private LetterAttachment SendProgressiveArchaeologyLevel(ReceivedItem receivedItem)
        {
            ReceiveAPLevel(ArchipelagoSkillIds.ARCHAEOLOGY);
            return new LetterInformationAttachment(receivedItem);
        }

        private void ReceiveAPLevel(string skill)
        {
            var farmer = Game1.player;
            var skillsType = AccessTools.TypeByName("SpaceCore.Skills");

            var exp = GetSpaceCoreExp(skillsType);
            var myNewLevels = GetSpaceCoreNewlevels(skillsType);
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

        private Dictionary<long, Dictionary<string, int>> GetSpaceCoreExp(Type skillsType)
        {
            // There was an experience refactor in 1.27
            if (IsSpaceCoreOlderThan_1_27())
            {
                return GetSpaceCoreExp_1_26(skillsType);
            }

            var state = GetSpaceCoreSkillsState(skillsType);

            // public Dictionary<long, Dictionary<string, int>> Exp
            var expField = ModEntry.Instance.Helper.Reflection.GetField<Dictionary<long, Dictionary<string, int>>>(state, "Exp");

            var exp = expField.GetValue();
            return exp;
        }

        private static Dictionary<long, Dictionary<string, int>> GetSpaceCoreExp_1_26(Type skillsType)
        {
            var expField = ModEntry.Instance.Helper.Reflection.GetField<Dictionary<long, Dictionary<string, int>>>(skillsType, "Exp");
            var exp = expField.GetValue();
            return exp;
        }

        public List<KeyValuePair<string, int>> GetSpaceCoreNewlevels(Type skillsType)
        {
            // There was an experience refactor in 1.27
            if (IsSpaceCoreOlderThan_1_27())
            {
                return GetSpaceCoreNewlevels_1_26(skillsType);
            }

            var state = GetSpaceCoreSkillsState(skillsType);

            // public Dictionary<long, Dictionary<string, int>> Exp
            var myNewLevelsField = ModEntry.Instance.Helper.Reflection.GetField<List<KeyValuePair<string, int>>>(state, "NewLevels");

            var exp = myNewLevelsField.GetValue();
            return exp;
        }

        private static List<KeyValuePair<string, int>> GetSpaceCoreNewlevels_1_26(Type skillsType)
        {
            var myNewLevelsField = ModEntry.Instance.Helper.Reflection.GetField<List<KeyValuePair<string, int>>>(skillsType, "NewLevels");
            var myNewLevels = myNewLevelsField.GetValue();
            return myNewLevels;
        }

        private object GetSpaceCoreSkillsState(Type skillsType)
        {
            // internal class SkillState
            // var skillStateType = AccessTools.TypeByName("SpaceCore.Skills+SkillState");

            // internal static SkillState State => _State.Value;
            var stateProperty = ModEntry.Instance.Helper.Reflection.GetProperty<object>(skillsType, "State");
            var state = stateProperty.GetValue();
            return state;
        }

        private bool IsSpaceCoreOlderThan_1_27()
        {
            var spaceCoreMod = _modHelper.ModRegistry.Get("spacechase0.SpaceCore");
            if (spaceCoreMod == null)
            {
                throw new Exception("SpaceCore mod could not be found while attempting to patch modded experience");
            }

            var version = spaceCoreMod.Manifest.Version;

            return version.MajorVersion <= 1 && version.MinorVersion <= 26;
        }
    }
}
