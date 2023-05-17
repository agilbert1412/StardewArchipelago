using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations.CodeInjections;
using StardewValley;

namespace StardewArchipelago.Items.Unlocks
{
    public class UnlockManager
    {
        private List<IUnlockManager> _specificUnlockManagers;

        public UnlockManager(ArchipelagoClient archipelago)
        {
            _specificUnlockManagers = new List<IUnlockManager>();
            _specificUnlockManagers.Add(new VanillaUnlockManager());
            if (archipelago.SlotData.ModList.Any())
            {
                _specificUnlockManagers.Add(new ModUnlockManager());
            }
        }

        public bool IsUnlock(string unlockName)
        {
            return _specificUnlockManagers.Any(specificUnlockManager => specificUnlockManager.IsUnlock(unlockName));
        }

        public LetterAttachment PerformUnlockAsLetter(ReceivedItem unlock)
        {
            foreach (var specificUnlockManager in _specificUnlockManagers)
            {
                if (specificUnlockManager.IsUnlock(unlock.ItemName))
                {
                    return specificUnlockManager.PerformUnlockAsLetter(unlock);
                }
            }

            throw new ArgumentException($"Could not perform unlock '{unlock.ItemName}'");
        }
    }
}
