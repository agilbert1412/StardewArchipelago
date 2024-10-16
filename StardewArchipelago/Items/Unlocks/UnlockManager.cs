﻿using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Unlocks.Modded;
using StardewArchipelago.Items.Unlocks.Vanilla;

namespace StardewArchipelago.Items.Unlocks
{
    public class UnlockManager
    {
        private readonly List<IUnlockManager> _specificUnlockManagers;
        private readonly Dictionary<string, Func<ReceivedItem, LetterAttachment>> _unlockables;

        public UnlockManager(StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _unlockables = new Dictionary<string, Func<ReceivedItem, LetterAttachment>>();
            _specificUnlockManagers = new List<IUnlockManager>
            {
                new VanillaUnlockManager(archipelago, locationChecker),
            };

            if (archipelago.SlotData.Mods.IsModded)
            {
                _specificUnlockManagers.Add(new ModUnlockManager(archipelago));
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
