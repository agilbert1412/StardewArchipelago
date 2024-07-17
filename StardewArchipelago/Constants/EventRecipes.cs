using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Constants
{
    public static class EventRecipes
    {
        public static readonly Dictionary<string, string> CookingRecipeEvents = Vanilla.EventRecipes.CookingRecipeEvents.Union(Modded.EventRecipes.CookingRecipeEvents).ToDictionary(x => x.Key, x => x.Value);

        public static readonly Dictionary<string, string> CraftingRecipeEvents = Vanilla.EventRecipes.CraftingRecipeEvents.Union(Modded.EventRecipes.CraftingRecipeEvents).ToDictionary(x => x.Key, x => x.Value);

        public static readonly List<string> QuestEventsWithRecipes = Vanilla.EventRecipes.QuestEventsWithRecipes.Union(Modded.EventRecipes.QuestEventsWithRecipes).ToList();
    }
}
