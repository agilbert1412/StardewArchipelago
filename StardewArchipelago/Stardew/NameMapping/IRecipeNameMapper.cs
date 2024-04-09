namespace StardewArchipelago.Stardew.NameMapping
{
    public interface IRecipeNameMapper
    {
        string GetItemName(string recipeName);
        string GetRecipeName(string itemName);
        public bool RecipeNeedsMapping(string itemName);
    }
}
