using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace StardewArchipelago.Items.Mail
{
    public class LetterVillagerActionAttachment : LetterActionAttachment
    {
        public string CharacterId { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterVillagerActionAttachment(ReceivedItem apItem, string characterId) : base(apItem, LetterActionsKeys.VillagerArrival, characterId)
        {
            CharacterId = characterId;
        }
    }
}
