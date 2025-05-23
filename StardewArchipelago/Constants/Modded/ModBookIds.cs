using System.Collections.Generic;

namespace StardewArchipelago.Constants.Modded
{
    internal class ModBookIds
    {
        public static readonly string DIGGING_LIKE_WORMS = "moonslime.Archaeology.skill_book";

        public static readonly Dictionary<string, string> ModBookIdsToNames = new()
        {
            { ModBookIds.DIGGING_LIKE_WORMS, DIGGING_LIKE_WORMS },
        };

        public static readonly Dictionary<string, string> ModBookNamesToIds = new()
        {
            { DIGGING_LIKE_WORMS, ModBookIds.DIGGING_LIKE_WORMS },
        };
    }
}
