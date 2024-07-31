using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Items.Mail;

namespace StardewArchipelago.Items.Unlocks.Modded
{
    public class ModUnlockManager : IUnlockManager
    {
        private readonly List<IUnlockManager> _childUnlockManagers;

        public ModUnlockManager(StardewArchipelagoClient archipelago)
        {
            _childUnlockManagers = new List<IUnlockManager>();

            if (archipelago.SlotData.Mods.HasModdedSkill())
            {
                _childUnlockManagers.Add(new ModSkillUnlockManager());
            }
            if (archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                _childUnlockManagers.Add(new MagicUnlockManager());
            }
            if (archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                _childUnlockManagers.Add(new SVEUnlockManager());
            }
        }

        public void RegisterUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            foreach (var childUnlockManager in _childUnlockManagers)
            {
                childUnlockManager.RegisterUnlocks(unlocks);
            }
        }
    }
}
