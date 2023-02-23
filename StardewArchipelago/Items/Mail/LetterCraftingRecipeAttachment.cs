using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Items.Mail
{
    public class LetterCraftingRecipeAttachment : LetterAttachment
    {
        public string RecipeItemName { get; private set; }
        public int AttachmentAmount { get; private set; }

        public LetterCraftingRecipeAttachment(ReceivedItem apItem, string recipeItemName) : base(apItem)
        {
            RecipeItemName = recipeItemName.Replace(" ", "_");
        }

        public override string GetEmbedString()
        {
            return $"%item craftingRecipe {RecipeItemName} %%";
        }

        public override void SendToPlayer(Mailman _mailman)
        {
            _mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
