using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Mail;
using StardewValley;

namespace StardewArchipelago.Items.Unlocks.Vanilla
{
    public class VillagerUnlockManager : IUnlockManager
    {
        private const string VILLAGER_SUFFIX = " Arrival";

        public VillagerUnlockManager()
        {
        }

        public void RegisterUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            RegisterVillagerUnlocks(unlocks);
        }

        private void RegisterVillagerUnlocks(IDictionary<string, Func<ReceivedItem, LetterAttachment>> unlocks)
        {
            foreach (var (characterId, characterData) in Game1.characterData)
            {
                unlocks.Add($"{characterId}{VILLAGER_SUFFIX}", (x) => SendVillagerArrivalLetter(x, characterId));
            }
        }

        private LetterAttachment SendVillagerArrivalLetter(ReceivedItem receivedItem, string characterId)
        {
            return new LetterVillagerActionAttachment(receivedItem, characterId);
        }
    }
}
