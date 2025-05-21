using StardewArchipelago.Constants.Modded;
using System.Collections.Generic;

namespace StardewArchipelago.Constants.Vanilla
{
    public static class PowerBooks
    {
        public const string PRICE_CATALOGUE = "Price Catalogue";
        public const string MAPPING_CAVE_SYSTEMS = "Mapping Cave Systems";
        public const string WAY_OF_THE_WIND_1 = "Way Of The Wind pt. 1";
        public const string WAY_OF_THE_WIND_2 = "Way Of The Wind pt. 2";
        public const string MONSTER_COMPENDIUM = "Monster Compendium";
        public const string FRIENDSHIP_101 = "Friendship 101";
        public const string JACK_BE_NIMBLE_JACK_BE_THICK = "Jack Be Nimble, Jack Be Thick";
        public const string WOODYS_SECRET = "Woody's Secret";
        public const string RACCOON_JOURNAL = "Raccoon Journal";
        public const string JEWELS_OF_THE_SEA = "Jewels Of The Sea";
        public const string DWARVISH_SAFETY_MANUAL = "Dwarvish Safety Manual";
        public const string THE_ART_O_CRABBING = "The Art O' Crabbing";
        public const string THE_ALLEYWAY_BUFFET = "The Alleyway Buffet";
        public const string THE_DIAMOND_HUNTER = "The Diamond Hunter";
        public const string BOOK_OF_MYSTERIES = "Book of Mysteries";
        public const string HORSE_THE_BOOK = "Horse: The Book";
        public const string TREASURE_APPRAISAL_GUIDE = "Treasure Appraisal Guide";
        public const string OL_SLITHERLEGS = "Ol' Slitherlegs";
        public const string ANIMAL_CATALOGUE = "Animal Catalogue";

        public const string BOOK_OF_STARS = "Book Of Stars";
        public const string ALMANAC = "Stardew Valley Almanac";
        public const string WOODCUTTER_WEEKLY = "Woodcutter's Weekly";
        public const string BAIT_AND_BOBBER = "Bait And Bobber";
        public const string MINING_MONTHLY = "Mining Monthly";
        public const string COMBAT_QUARTERLY = "Combat Quarterly";
        public const string QUEEN_OF_SAUCE_COOKBOOK = "Queen Of Sauce Cookbook";
        // Mod Books
        public const string DIGGING_LIKE_WORMS = "Digging Like Worms";

        public static readonly Dictionary<string, string> BookIdsToNames = new()
        {
            { ObjectIds.PRICE_CATALOGUE, PRICE_CATALOGUE },
            { ObjectIds.MAPPING_CAVE_SYSTEMS, MAPPING_CAVE_SYSTEMS },
            { ObjectIds.WAY_OF_THE_WIND_1, WAY_OF_THE_WIND_1 },
            { ObjectIds.WAY_OF_THE_WIND_2, WAY_OF_THE_WIND_2 },
            { ObjectIds.MONSTER_COMPENDIUM, MONSTER_COMPENDIUM },
            { ObjectIds.FRIENDSHIP_101, FRIENDSHIP_101 },
            { ObjectIds.JACK_BE_NIMBLE_JACK_BE_THICK, JACK_BE_NIMBLE_JACK_BE_THICK },
            { ObjectIds.WOODYS_SECRET, WOODYS_SECRET },
            { ObjectIds.RACCOON_JOURNAL, RACCOON_JOURNAL },
            { ObjectIds.JEWELS_OF_THE_SEA, JEWELS_OF_THE_SEA },
            { ObjectIds.DWARVISH_SAFETY_MANUAL, DWARVISH_SAFETY_MANUAL },
            { ObjectIds.THE_ART_O_CRABBING, THE_ART_O_CRABBING },
            { ObjectIds.THE_ALLEYWAY_BUFFET, THE_ALLEYWAY_BUFFET },
            { ObjectIds.THE_DIAMOND_HUNTER, THE_DIAMOND_HUNTER },
            { ObjectIds.BOOK_OF_MYSTERIES, BOOK_OF_MYSTERIES },
            { ObjectIds.HORSE_THE_BOOK, HORSE_THE_BOOK },
            { ObjectIds.TREASURE_APPRAISAL_GUIDE, TREASURE_APPRAISAL_GUIDE },
            { ObjectIds.OL_SLITHERLEGS, OL_SLITHERLEGS },
            { ObjectIds.ANIMAL_CATALOGUE, ANIMAL_CATALOGUE },
            { ObjectIds.BOOK_OF_STARS, BOOK_OF_STARS },
            { ObjectIds.STARDEW_VALLEY_ALMANAC, ALMANAC },
            { ObjectIds.WOODCUTTER_WEEKLY, WOODCUTTER_WEEKLY },
            { ObjectIds.BAIT_AND_BOBBER, BAIT_AND_BOBBER },
            { ObjectIds.MINING_MONTHLY, MINING_MONTHLY },
            { ObjectIds.COMBAT_QUARTERLY, COMBAT_QUARTERLY },
            { ObjectIds.QUEEN_OF_SAUCE_COOKBOOK, QUEEN_OF_SAUCE_COOKBOOK },
            // Mod Books
            { ModBookIds.DIGGING_LIKE_WORMS, DIGGING_LIKE_WORMS },
        };

        public static readonly Dictionary<string, string> BookNamesToIds = new()
        {
            { PRICE_CATALOGUE, ObjectIds.PRICE_CATALOGUE },
            { MAPPING_CAVE_SYSTEMS, ObjectIds.MAPPING_CAVE_SYSTEMS },
            { WAY_OF_THE_WIND_1, ObjectIds.WAY_OF_THE_WIND_1 },
            { WAY_OF_THE_WIND_2, ObjectIds.WAY_OF_THE_WIND_2 },
            { MONSTER_COMPENDIUM, ObjectIds.MONSTER_COMPENDIUM },
            { FRIENDSHIP_101, ObjectIds.FRIENDSHIP_101 },
            { JACK_BE_NIMBLE_JACK_BE_THICK, ObjectIds.JACK_BE_NIMBLE_JACK_BE_THICK },
            { WOODYS_SECRET, ObjectIds.WOODYS_SECRET },
            { RACCOON_JOURNAL, ObjectIds.RACCOON_JOURNAL },
            { JEWELS_OF_THE_SEA, ObjectIds.JEWELS_OF_THE_SEA },
            { DWARVISH_SAFETY_MANUAL, ObjectIds.DWARVISH_SAFETY_MANUAL },
            { THE_ART_O_CRABBING, ObjectIds.THE_ART_O_CRABBING },
            { THE_ALLEYWAY_BUFFET, ObjectIds.THE_ALLEYWAY_BUFFET },
            { THE_DIAMOND_HUNTER, ObjectIds.THE_DIAMOND_HUNTER },
            { BOOK_OF_MYSTERIES, ObjectIds.BOOK_OF_MYSTERIES },
            { HORSE_THE_BOOK, ObjectIds.HORSE_THE_BOOK },
            { TREASURE_APPRAISAL_GUIDE, ObjectIds.TREASURE_APPRAISAL_GUIDE },
            { OL_SLITHERLEGS, ObjectIds.OL_SLITHERLEGS },
            { ANIMAL_CATALOGUE, ObjectIds.ANIMAL_CATALOGUE },
            { BOOK_OF_STARS, ObjectIds.BOOK_OF_STARS },
            { ALMANAC, ObjectIds.STARDEW_VALLEY_ALMANAC },
            { WOODCUTTER_WEEKLY, ObjectIds.WOODCUTTER_WEEKLY },
            { BAIT_AND_BOBBER, ObjectIds.BAIT_AND_BOBBER },
            { MINING_MONTHLY, ObjectIds.MINING_MONTHLY },
            { COMBAT_QUARTERLY, ObjectIds.COMBAT_QUARTERLY },
            { QUEEN_OF_SAUCE_COOKBOOK, ObjectIds.QUEEN_OF_SAUCE_COOKBOOK },
            // Mod Books
            { DIGGING_LIKE_WORMS, ModBookIds.DIGGING_LIKE_WORMS },
        };


        /* Books with the wrong names
        "Book_Artifact" "Treasure Appraisal Guide"
        "Book_Grass" "Ol' Slitherlegs"
        "Book_Horse" "Horse: The Book"
        "Book_WildSeeds" "Ways Of The Wild" "Raccoon Journal"
         */
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
