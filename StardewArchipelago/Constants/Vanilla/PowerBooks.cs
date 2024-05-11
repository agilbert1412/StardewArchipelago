using System.Collections.Generic;
using StardewArchipelago.Stardew.Ids.Vanilla;

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

        public static readonly Dictionary<string, string> BookIdsToNames = new()
        {
            { ObjectIds.PRICE_CATALOGUE, PRICE_CATALOGUE },
            { ObjectIds.MAPPING_CAVE_SYSTEMS, MAPPING_CAVE_SYSTEMS },
            { ObjectIds.WAY_OF_THE_WIND_1, WAY_OF_THE_WIND_1 },
            { ObjectIds.WAY_OF_THE_WIND_2, WAY_OF_THE_WIND_2 },
            { ObjectIds.MONSTER_COMPENDIUM, MONSTER_COMPENDIUM },
            { ObjectIds.FRIENDSHIP_101, FRIENDSHIP_101 },
            { ObjectIds.JACK_BE_NIMBLE_JACK_BE_THICK, ANIMAL_CATALOGUE },
            { ObjectIds.WOODYS_SECRET, WOODYS_SECRET },
            { ObjectIds.RACCOON_JOURNAL, RACCOON_JOURNAL },
            { ObjectIds.JEWELS_OF_THE_SEA, JEWELS_OF_THE_SEA },
            { ObjectIds.DWARVISH_SAFETY_MANUAL, DWARVISH_SAFETY_MANUAL },
            { ObjectIds.THE_ART_O_CRABBING, THE_ART_O_CRABBING },
            { ObjectIds.THE_ALLEYWAY_BUFFET, THE_ALLEYWAY_BUFFET },
            { ObjectIds.THE_DIAMOND_HUNTER, ANIMAL_CATALOGUE },
            { ObjectIds.BOOK_OF_MYSTERIES, BOOK_OF_MYSTERIES },
            { ObjectIds.HORSE_THE_BOOK, HORSE_THE_BOOK },
            { ObjectIds.TREASURE_APPRAISAL_GUIDE, TREASURE_APPRAISAL_GUIDE },
            { ObjectIds.OL_SLITHERLEGS, OL_SLITHERLEGS },
            { ObjectIds.ANIMAL_CATALOGUE, ANIMAL_CATALOGUE },
        };

        public static readonly Dictionary<string, string> BookNamesToIds = new()
        {
            { PRICE_CATALOGUE, ObjectIds.PRICE_CATALOGUE },
            { MAPPING_CAVE_SYSTEMS, ObjectIds.MAPPING_CAVE_SYSTEMS },
            { WAY_OF_THE_WIND_1, ObjectIds.WAY_OF_THE_WIND_1 },
            { WAY_OF_THE_WIND_2, ObjectIds.WAY_OF_THE_WIND_2 },
            { MONSTER_COMPENDIUM, ObjectIds.MONSTER_COMPENDIUM },
            { FRIENDSHIP_101, ObjectIds.FRIENDSHIP_101 },
            { JACK_BE_NIMBLE_JACK_BE_THICK, ObjectIds.ANIMAL_CATALOGUE },
            { WOODYS_SECRET, ObjectIds.WOODYS_SECRET },
            { RACCOON_JOURNAL, ObjectIds.RACCOON_JOURNAL },
            { JEWELS_OF_THE_SEA, ObjectIds.JEWELS_OF_THE_SEA },
            { DWARVISH_SAFETY_MANUAL, ObjectIds.DWARVISH_SAFETY_MANUAL },
            { THE_ART_O_CRABBING, ObjectIds.THE_ART_O_CRABBING },
            { THE_ALLEYWAY_BUFFET, ObjectIds.THE_ALLEYWAY_BUFFET },
            { THE_DIAMOND_HUNTER, ObjectIds.ANIMAL_CATALOGUE },
            { BOOK_OF_MYSTERIES, ObjectIds.BOOK_OF_MYSTERIES },
            { HORSE_THE_BOOK, ObjectIds.HORSE_THE_BOOK },
            { TREASURE_APPRAISAL_GUIDE, ObjectIds.TREASURE_APPRAISAL_GUIDE },
            { OL_SLITHERLEGS, ObjectIds.OL_SLITHERLEGS },
            { ANIMAL_CATALOGUE, ObjectIds.ANIMAL_CATALOGUE },
        };


        /* Books with the wrong names
        "Book_Artifact" "Treasure Appraisal Guide"
        "Book_Grass" "Ol' Slitherlegs"
        "Book_Horse" "Horse: The Book"
        "Book_WildSeeds" "Ways Of The Wild" "Raccoon Journal"
         */
    }
}
