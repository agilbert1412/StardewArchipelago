using System.Collections.Generic;
using StardewValley;

namespace StardewArchipelago.Locations.Festival
{
    public class FestivalLocationNames
    {
        public static readonly string EGG_FESTIVAL = FestivalIdentifier(Season.Spring, 13);
        public static readonly string FLOWER_DANCE = FestivalIdentifier(Season.Spring, 24);
        public static readonly string LUAU = FestivalIdentifier(Season.Summer, 11);
        public static readonly string MOONLIGHT_JELLIES = FestivalIdentifier(Season.Summer, 28);
        public static readonly string FAIR = FestivalIdentifier(Season.Fall, 16);
        public static readonly string SPIRIT_EVE = FestivalIdentifier(Season.Fall, 27);
        public static readonly string FESTIVAL_OF_ICE = FestivalIdentifier(Season.Winter, 8);
        public static readonly string FEAST_OF_THE_WINTER_STAR = FestivalIdentifier(Season.Winter, 25);
        public const string NIGHT_MARKET_ALL = "Night Market All";
        public const string NIGHT_MARKET_15 = "Night Market 15";
        public const string NIGHT_MARKET_16 = "Night Market 16";
        public const string NIGHT_MARKET_17 = "Night Market 17";


        public const string EGG_HUNT = "Egg Hunt Victory";
        public const string STRAWBERRY_SEEDS = "Egg Festival: Strawberry Seeds";
        public const string DANCE_WITH_SOMEONE = "Dance with someone";
        public const string RARECROW_5 = "Rarecrow #5 (Woman)";
        public const string TUB_O_FLOWERS_RECIPE = "Tub o' Flowers Recipe";
        public const string LUAU_SOUP = "Luau Soup";
        public const string WATCH_MOONLIGHT_JELLIES = "Dance of the Moonlight Jellies";
        public const string MOONLIGHT_JELLIES_BANNER = "Moonlight Jellies Banner";
        public const string STARPORT_DECAL = "Starport Decal";
        public const string STRENGTH_GAME = "Smashing Stone";
        public const string GRANGE_DISPLAY = "Grange Display";
        public const string RARECROW_1 = "Rarecrow #1 (Turnip Head)";
        public const string FAIR_STARDROP = "Fair Stardrop";
        public const string GOLDEN_PUMPKIN = "Spirit's Eve Maze";
        public const string RARECROW_2 = "Rarecrow #2 (Witch)";
        public const string JACK_O_LANTERN_RECIPE = "Jack-O-Lantern Recipe";
        public const string FISHING_COMPETITION = "Win Fishing Competition";
        public const string RARECROW_4 = "Rarecrow #4 (Snowman)";
        public const string MERMAID_PEARL = "Mermaid Pearl";
        public const string CONE_HAT = "Cone Hat";
        public const string IRIDIUM_FIREPLACE = "Iridium Fireplace";
        public const string RARECROW_7 = "Rarecrow #7 (Tanuki)";
        public const string RARECROW_8 = "Rarecrow #8 (Tribal Mask)";
        public const string RARECROW_3 = "Rarecrow #3 (Alien)";
        public const string RARECROW_6 = "Rarecrow #6 (Dwarf)";
        public const string LUPINI_YEAR_1_PAINTING_1 = "Lupini: Red Eagle";
        public const string LUPINI_YEAR_1_PAINTING_2 = "Lupini: Portrait Of A Mermaid";
        public const string LUPINI_YEAR_1_PAINTING_3 = "Lupini: Solar Kingdom";
        public const string LUPINI_YEAR_2_PAINTING_1 = "Lupini: Clouds";
        public const string LUPINI_YEAR_2_PAINTING_2 = "Lupini: 1000 Years From Now";
        public const string LUPINI_YEAR_2_PAINTING_3 = "Lupini: Three Trees";
        public const string LUPINI_YEAR_3_PAINTING_1 = "Lupini: The Serpent";
        public const string LUPINI_YEAR_3_PAINTING_2 = "Lupini: 'Tropical Fish #173'";
        public const string LUPINI_YEAR_3_PAINTING_3 = "Lupini: Land Of Clay";
        public const string LEGEND_OF_THE_WINTER_STAR = "The Legend of the Winter Star";
        public const string SECRET_SANTA = "Secret Santa";

        public static readonly Dictionary<string, string[]> LocationsByFestival = new()
        {
            { EGG_FESTIVAL, new[] { EGG_HUNT, STRAWBERRY_SEEDS } },
            { FLOWER_DANCE, new[] { DANCE_WITH_SOMEONE, RARECROW_5, TUB_O_FLOWERS_RECIPE } },
            { LUAU, new[] { LUAU_SOUP } },
            { MOONLIGHT_JELLIES, new[] { WATCH_MOONLIGHT_JELLIES, MOONLIGHT_JELLIES_BANNER, STARPORT_DECAL } },
            { FAIR, new[] { STRENGTH_GAME, RARECROW_1, FAIR_STARDROP, GRANGE_DISPLAY } },
            { SPIRIT_EVE, new[] { GOLDEN_PUMPKIN, RARECROW_2, JACK_O_LANTERN_RECIPE } },
            { FESTIVAL_OF_ICE, new[] { FISHING_COMPETITION, RARECROW_4 } },
            { FEAST_OF_THE_WINTER_STAR, new[] { LEGEND_OF_THE_WINTER_STAR, SECRET_SANTA } },
            { NIGHT_MARKET_ALL, new[] { MERMAID_PEARL, CONE_HAT, IRIDIUM_FIREPLACE, RARECROW_7, RARECROW_8 } },
            { NIGHT_MARKET_15, new[] { LUPINI_YEAR_1_PAINTING_1, LUPINI_YEAR_2_PAINTING_1, LUPINI_YEAR_3_PAINTING_1 } },
            { NIGHT_MARKET_16, new[] { LUPINI_YEAR_1_PAINTING_2, LUPINI_YEAR_2_PAINTING_2, LUPINI_YEAR_3_PAINTING_2 } },
            { NIGHT_MARKET_17, new[] { LUPINI_YEAR_1_PAINTING_3, LUPINI_YEAR_2_PAINTING_3, LUPINI_YEAR_3_PAINTING_3 } },
        };

        public static string FestivalIdentifier(Season season, int day)
        {
            return $"{season} {day}";
        }
    }
}
