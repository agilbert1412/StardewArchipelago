using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Items.Mail;

namespace StardewArchipelago.Items.Unlocks
{
    public interface IUnlockManager
    {
        public void RegisterUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks);
    }
}
