using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Items.Mail;
using StardewValley;
using System;
using System.Collections.Generic;

namespace StardewArchipelago.Items.Unlocks.Vanilla
{
    public class VillagerUnlockManager : IUnlockManager
    {

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
                var apName = VillagerExistenceInjections.GetArrivalItem(characterId);
                unlocks.Add(apName, (x) => SendVillagerArrivalLetter(x, characterId));
            }
        }

        private LetterAttachment SendVillagerArrivalLetter(ReceivedItem receivedItem, string characterId)
        {
            return new LetterVillagerActionAttachment(receivedItem, characterId);
        }
    }
}
