using System.Collections.Generic;
using StardewArchipelago.Stardew.Ids.Vanilla;

namespace StardewArchipelago.Constants.Vanilla
{
    public static class EventRecipes
    {
        public static readonly Dictionary<string, string> CookingRecipeEvents = new()
        {
            { EventIds.COOKIES_RECIPE, "Cookies" },
        };

        public static readonly Dictionary<string, string> CraftingRecipeEvents = new()
        {
            { EventIds.BIRDIE_QUEST_COMPLETE, "Fairy Dust" },
        };

        public static readonly string[] QuestEventsWithRecipes = { };
    }
}
