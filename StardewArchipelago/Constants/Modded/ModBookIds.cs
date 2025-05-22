using System.Collections.Generic;

namespace StardewArchipelago.Constants.Modded
{
    internal class ModBookIds
    {
        public static readonly string DIGGING_LIKE_WORMS = "moonslime.Archaeology.skill_book";
    }
}

namespace StardewArchipelago.Constants.Modded
{
    public static class ModPowerBooks
    {
        // Mod Books
        public const string DIGGING_LIKE_WORMS = "Digging Like Worms";

        public static readonly Dictionary<string, string> BookIdsToNames = new()
        {
            // Mod Books
            { ModBookIds.DIGGING_LIKE_WORMS, DIGGING_LIKE_WORMS },
        };

        public static readonly Dictionary<string, string> BookNamesToIds = new()
        {
            // Mod Books
            { DIGGING_LIKE_WORMS, ModBookIds.DIGGING_LIKE_WORMS },
        };
    }
}
