using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace StardewArchipelago.Items.Mail
{
    public class LetterCookingRecipeAttachment : LetterActionAttachment
    {
        public string RecipeItemName { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterCookingRecipeAttachment(ReceivedItem apItem, string recipeItemName) : base(apItem, LetterActionsKeys.LearnCookingRecipe, recipeItemName)
        {
            RecipeItemName = recipeItemName;
        }
    }
}
