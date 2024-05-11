using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations;

namespace StardewArchipelago.Items.Unlocks
{
    public class UnlockManager
    {
        private List<IUnlockManager> _specificUnlockManagers;
        private Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public UnlockManager(ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            _specificUnlockManagers = new List<IUnlockManager>();
            _specificUnlockManagers.Add(new VanillaUnlockManager(archipelago, locationChecker));
            if (archipelago.SlotData.Mods.HasModdedSkill())
            {
                _specificUnlockManagers.Add(new ModSkillUnlockManager());
            }
            if (archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                _specificUnlockManagers.Add(new MagicUnlockManager());
            }
            if (archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                _specificUnlockManagers.Add(new SVEUnlockManager());
            }

            RegisterUnlocks();
        }

        private void RegisterUnlocks()
        {
            foreach (var specificUnlockManager in _specificUnlockManagers)
            {
                specificUnlockManager.RegisterUnlocks(_unlockables);
            }
        }

        public bool IsUnlock(string unlockName)
        {
            return _unlockables.ContainsKey(unlockName);
        }

        public LetterAttachment PerformUnlockAsLetter(ReceivedItem unlock)
        {
            if (IsUnlock(unlock.ItemName))
            {
                return _unlockables[unlock.ItemName](unlock);
            }

            throw new ArgumentException($"Could not perform unlock '{unlock.ItemName}'");
        }
    }
}
