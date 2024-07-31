using System.Collections.Generic;

namespace StardewArchipelago.Constants.Modded
{
    public static class EventRecipes
    {
        public static readonly Dictionary<string, string> CookingRecipeEvents = new()
        {
            { EventIds.MUSHROOM_KEBAB, "Mushroom Kebab" },
            { EventIds.CRAYFISH_SOUP, "Crayfish Soup" },
            { EventIds.PEMMICAN, "Pemmican" },
            { EventIds.VOID_MINT_TEA_ALECTO, "Void Mint Tea" },
            { EventIds.VOID_MINT_TEA_WIZARD, "Void Mint Tea" },
            { EventIds.SPECIAL_PUMPKIN_SOUP, "Special Pumpkin Soup" },
        };

        public static readonly Dictionary<string, string> CraftingRecipeEvents = new()
        {
            { EventIds.GINGER_TINCTURE_ALECTO, "Ginger Tincture" },
            { EventIds.GINGER_TINCTURE_WIZARD, "Ginger Tincture" },
        };

        public static readonly string[] QuestEventsWithRecipes = {
            EventIds.CRAYFISH_SOUP, EventIds.SPECIAL_PUMPKIN_SOUP, EventIds.GINGER_TINCTURE_ALECTO, EventIds.GINGER_TINCTURE_WIZARD,
        };
    }
}
