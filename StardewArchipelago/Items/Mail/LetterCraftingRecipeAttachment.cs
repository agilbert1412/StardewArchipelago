using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterCraftingRecipeAttachment : LetterAttachment
    {
        public string RecipeItemName { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterCraftingRecipeAttachment(ReceivedItem apItem, string recipeItemName) : base(apItem)
        {
            RecipeItemName = recipeItemName.Replace(" ", "_");
        }

        public override string GetEmbedString()
        {
            return $"%item craftingRecipe {RecipeItemName} %%";
        }

        public override void SendToPlayer(Mailman mailman)
        {
            mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }
    }
}
