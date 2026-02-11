using System.Collections.Generic;
using StardewArchipelago.Constants.Vanilla;
using StardewValley;

namespace StardewArchipelago.Locations.Secrets
{
    public class SecretsLocationNames
    {
        public const string OLD_MASTER_CANNOLI = "Secret: Old Master Cannoli";
        public const string POT_OF_GOLD = "Secret: Pot Of Gold";
        public const string POISON_THE_GOVERNOR = "Secret: Poison The Governor";
        public const string GRANGE_DISPLAY_BRIBE = "Secret: Grange Display Bribe";
        public const string PURPLE_LETTUCE = "Secret: Purple Lettuce";
        public const string MAKE_MARNIE_LAUGH = "Secret: Make Marnie Laugh";
        public const string JUMPSCARE_LEWIS = "Secret: Jumpscare Lewis";
        public const string CONFRONT_MARNIE = "Secret: Confront Marnie";
        public const string LUCKY_PURPLE_BOBBER = "Secret: Lucky Purple Bobber";
        public const string FREE_THE_FORSAKEN_SOULS = "Secret: Free The Forsaken Souls";
        public const string SOMETHING_FOR_SANTA = "Secret: Something For Santa";
        public const string THANK_THE_DEVS = "Secret: Thank the Devs";
        public const string ANNOY_THE_MOON_MAN = "Secret: Annoy the Moon Man";
        public const string ACKNOWLEDGE_THE_LONELY_STONE = "Secret: Acknowledge the Lonely Stone";
        public const string JUNGLE_JUNIMO = "Secret: Jungle Junimo";
        public const string HMTGF = "Secret: ??HMTGF??";
        public const string PINKY_LEMON = "Secret: ??Pinky Lemon??";
        public const string FOROGUEMON = "Secret: ??Foroguemon??";
        public const string GALAXIES_WILL_HEED_YOUR_CRY = "Secret: Galaxies Will Heed Your Cry";
        public const string STRANGE_SIGHTING = "Secret: Strange Sighting";
        public const string BOAT = "Fishing Secret: 'Boat'";
        public const string DECORATIVE_TRASH_CAN = "Fishing Secret: Decorative Trash Can";
        public const string FOLIAGE_PRINT = "Fishing Secret: Foliage Print";
        public const string FROG_HAT = "Fishing Secret: Frog Hat";
        public const string GOURMAND_STATUE = "Fishing Secret: Gourmand Statue";
        public const string IRIDIUM_KROBUS = "Fishing Secret: Iridium Krobus";
        public const string LIFESAVER = "Fishing Secret: Lifesaver";
        public const string PHYSICS_101 = "Fishing Secret: 'Physics 101'";
        public const string PYRAMID_DECAL = "Fishing Secret: Pyramid Decal";
        public const string SQUIRREL_FIGURINE = "Fishing Secret: Squirrel Figurine";
        public const string VISTA = "Fishing Secret: 'Vista'";
        public const string WALL_BASKET = "Fishing Secret: Wall Basket";
        public const string SUMMON_BONE_SERPENT = "Secret: Summon Bone Serpent";
        public const string MEOWMERE = "Secret: Meowmere";
        public const string A_FAMILIAR_TUNE = "Secret: A Familiar Tune";
        public const string SEA_MONSTER_SIGHTING = "Secret: Sea Monster Sighting";
        public const string BIGFOOT = "Secret: ...Bigfoot?";
        public const string FLUBBER_EXPERIMENT = "Secret: Flubber Experiment";
        public const string SEEMS_FISHY = "Secret: Seems Fishy";
        public const string ME_ME_ME_ME = "Secret: 'Me me me me me me me me me me me me me me me me'";
        public const string NICE_TRY = "Secret: Nice Try";
        public const string ENJOY_YOUR_NEW_LIFE_HERE = "Secret: Enjoy your new life here";
        public const string WHAT_YOU_EXPECT = "Secret: 'What'd you expect?'";
        public const string SECRET_IRIDIUM_STACKMASTER_TROPHY = "Secret: Secret Iridium Stackmaster Trophy";
        public const string WHAT_KIND_OF_MONSTER_IS_THIS = "Secret: What kind of monster is this?";
        public const string MOUTH_WATERING_ALREADY = "Secret: My mouth is watering already";
        public const string LOVELY_PERFUME = "Secret: A gift of lovely perfume";
        public const string WHERE_DOES_THIS_JUICE_COME_FROM = "Secret: Where exactly does this juice come from?";
        public const string PRECIOUS_FRUIT_WHENEVER = "Secret: Obtain my precious fruit whenever you like";

        public const string SECRET_NOTE_1 = "Secret Note #1: A Page From Abigail's Diary";
        public const string SECRET_NOTE_2 = "Secret Note #2: Sam's Holiday Shopping List";
        public const string SECRET_NOTE_3 = "Secret Note #3: Leah's Perfect Dinner";
        public const string SECRET_NOTE_4 = "Secret Note #4: Maru's Greatest Invention Yet";
        public const string SECRET_NOTE_5 = "Secret Note #5: Penny gets everyone something they love";
        public const string SECRET_NOTE_6 = "Secret Note #6: Stardrop Saloon Special Orders";
        public const string SECRET_NOTE_7 = "Secret Note #7: Older Bachelors In Town";
        public const string SECRET_NOTE_8 = "Secret Note #8: To Haley And Emily";
        public const string SECRET_NOTE_9 = "Secret Note #9: Alex's Strength Training Diet";
        public const string SECRET_NOTE_10 = "Secret Note #10: Cryptic Note";
        public const string SECRET_NOTE_11 = "Secret Note #11: Marnie's Memory";
        public const string SECRET_NOTE_12 = "Secret Note #12: Good Things In Garbage Cans";
        public const string SECRET_NOTE_13 = "Secret Note #13: Junimo Plush";
        public const string SECRET_NOTE_14 = "Secret Note #14: Stone Junimo";
        public const string SECRET_NOTE_15 = "Secret Note #15: Mermaid Show";
        public const string SECRET_NOTE_16 = "Secret Note #16: Treasure Chest";
        public const string SECRET_NOTE_17 = "Secret Note #17: Green Strange Doll";
        public const string SECRET_NOTE_18 = "Secret Note #18: Yellow Strange Doll";
        public const string SECRET_NOTE_19_PART_1 = "Secret Note #19: Solid Gold Lewis";
        public const string SECRET_NOTE_19_PART_2 = "Secret Note #19: In Town For All To See";
        public const string SECRET_NOTE_20 = "Secret Note #20: Special Charm";
        public const string SECRET_NOTE_21 = "Secret Note #21: A Date In Nature";
        public const string SECRET_NOTE_22 = "Secret Note #22: The Mysterious Qi";
        public const string SECRET_NOTE_23 = "Secret Note #23: Strange Note";
        public const string SECRET_NOTE_24 = "Secret Note #24: M. Jasper's Book On Junimos";
        public const string SECRET_NOTE_25 = "Secret Note #25: Ornate Necklace";
        public const string SECRET_NOTE_26 = "Secret Note #26: Ancient Farming Secrets";
        public const string SECRET_NOTE_27 = "Secret Note #27: A Compendium Of My Greatest Discoveries";

        public static readonly Dictionary<string, string> FISHABLE_QUALIFIED_ITEM_IDS_TO_LOCATIONS = new()
        {
            { QualifiedItemIds.BOAT, BOAT },
            { QualifiedItemIds.DECORATIVE_TRASH_CAN, DECORATIVE_TRASH_CAN },
            { QualifiedItemIds.FOLIAGE_PRINT, FOLIAGE_PRINT },
            { QualifiedItemIds.FROG_HAT, FROG_HAT },
            { QualifiedItemIds.GOURMAND_STATUE, GOURMAND_STATUE },
            { QualifiedItemIds.IRIDIUM_KROBUS, IRIDIUM_KROBUS },
            { QualifiedItemIds.LIFESAVER, LIFESAVER },
            { QualifiedItemIds.PHYSICS_101, PHYSICS_101 },
            { QualifiedItemIds.PYRAMID_DECAL, PYRAMID_DECAL },
            { QualifiedItemIds.SQUIRREL_FIGURINE, SQUIRREL_FIGURINE },
            { QualifiedItemIds.VISTA, VISTA },
            { QualifiedItemIds.WALL_BASKET, WALL_BASKET },
        };

        public static readonly Dictionary<string, List<RequiredGift>> SECRET_NOTE_GIFT_REQUIREMENTS = new()
        {
            { SECRET_NOTE_1, new List<RequiredGift> { new(NPCNames.ABIGAIL, ObjectIds.PUMPKIN, ObjectIds.AMETHYST, ObjectIds.CHOCOLATE_CAKE, ObjectIds.SPICY_EEL, ObjectIds.BLACKBERRY_COBBLER) } },
            {
                SECRET_NOTE_2, new List<RequiredGift>
                {
                    new(NPCNames.SEBASTIAN, ObjectIds.FROZEN_TEAR, ObjectIds.SASHIMI),
                    new(NPCNames.PENNY, ObjectIds.EMERALD, ObjectIds.POPPY),
                    new(NPCNames.VINCENT, ObjectIds.GRAPE, ObjectIds.CRANBERRY_CANDY),
                    new(NPCNames.JODI, ObjectIds.CRISPY_BASS, ObjectIds.PANCAKES),
                    new(NPCNames.KENT, ObjectIds.FIDDLEHEAD_RISOTTO, ObjectIds.ROASTED_HAZELNUTS),
                    new(NPCNames.SAM, ObjectIds.CACTUS_FRUIT, ObjectIds.MAPLE_BAR, ObjectIds.PIZZA)
                }
            },
            { SECRET_NOTE_3, new List<RequiredGift> { new(NPCNames.LEAH, ObjectIds.SALAD, ObjectIds.GOAT_CHEESE, ObjectIds.TRUFFLE, ObjectIds.WINE) } },
            { SECRET_NOTE_4, new List<RequiredGift> { new(NPCNames.MARU, ObjectIds.GOLD_BAR, ObjectIds.IRIDIUM_BAR, ObjectIds.BATTERY_PACK, ObjectIds.DIAMOND, ObjectIds.STRAWBERRY) } },
            {
                SECRET_NOTE_5, new List<RequiredGift>
                {
                    new(NPCNames.PAM, ObjectIds.PARSNIP, ObjectIds.GLAZED_YAMS),
                    new(NPCNames.JAS, ObjectIds.FAIRY_ROSE, ObjectIds.PLUM_PUDDING),
                    new(NPCNames.VINCENT, ObjectIds.PINK_CAKE, ObjectIds.GRAPE),
                    new(NPCNames.GEORGE, ObjectIds.LEEK, ObjectIds.FRIED_MUSHROOM),
                    new(NPCNames.EVELYN, ObjectIds.BEET, ObjectIds.TULIP)
                }
            },
            {
                SECRET_NOTE_6, new List<RequiredGift>
                {
                    new(NPCNames.LEWIS, ObjectIds.AUTUMN_BOUNTY),
                    new(NPCNames.MARNIE, ObjectIds.PUMPKIN_PIE),
                    new(NPCNames.DEMETRIUS, ObjectIds.BEAN_HOTPOT),
                    new(NPCNames.CAROLINE, ObjectIds.FISH_TACO)
                }
            },
            {
                SECRET_NOTE_7, new List<RequiredGift>
                {
                    new(NPCNames.HARVEY, ObjectIds.COFFEE, ObjectIds.PICKLES),
                    new(NPCNames.ELLIOTT, ObjectIds.CRAB_CAKES, ObjectIds.POMEGRANATE),
                    new(NPCNames.SHANE, ObjectIds.BEER, ObjectIds.PIZZA, ObjectIds.PEPPER_POPPERS)
                }
            },
            {
                SECRET_NOTE_8, new List<RequiredGift>
                {
                    new(NPCNames.HALEY, ObjectIds.PINK_CAKE, ObjectIds.SUNFLOWER),
                    new(NPCNames.EMILY, ObjectIds.AMETHYST, ObjectIds.AQUAMARINE, ObjectIds.EMERALD, ObjectIds.JADE, ObjectIds.RUBY, ObjectIds.TOPAZ, ObjectIds.WOOL)
                }
            },
            { SECRET_NOTE_9, new List<RequiredGift> { new(NPCNames.ALEX, ObjectIds.COMPLETE_BREAKFAST, ObjectIds.SALMON_DINNER) } },
        };

        public static readonly List<SecretData> SECRET_DATES = new List<SecretData>()
        {
            new(POT_OF_GOLD, Season.Spring, 17),
            new(POISON_THE_GOVERNOR, Season.Summer, 11),
            new(GRANGE_DISPLAY_BRIBE, Season.Fall, 16),
            new(PURPLE_LETTUCE, Season.Fall, 16),
            new(SOMETHING_FOR_SANTA, Season.Winter, 24),
            new(ANNOY_THE_MOON_MAN, 27),
            new(FREE_THE_FORSAKEN_SOULS, Season.Fall, 26),

            new(SECRET_NOTE_13, 28),
            new(SECRET_NOTE_14, Season.Spring, 2),
            new(SECRET_NOTE_15, Season.Winter, new[] { 15, 16, 17 }),
        };
    }

    public class RequiredGift
    {
        public string Npc { get; private set; }
        public List<string> Gifts { get; private set; }

        public RequiredGift(string npc, params string[] gifts)
        {
            Npc = npc;
            Gifts = new List<string>(gifts);
        }
    }

    public class SecretData
    {
        public string Name { get; }
        public Season[] Seasons { get; }
        public int[] Days { get; }

        public SecretData(string name, int day) : this(name, new[] { Season.Spring, Season.Summer, Season.Fall, Season.Winter }, new[] { day })
        {
        }

        public SecretData(string name, Season season, int day) : this(name, new[] { season }, new[] { day })
        {
        }

        public SecretData(string name, Season[] seasons, int day) : this(name, seasons, new[] { day })
        {
        }

        public SecretData(string name, Season season, int[] days) : this(name, new[] { season }, days)
        {
        }

        public SecretData(string name, Season[] seasons, int[] days)
        {
            Name = name;
            Seasons = seasons;
            Days = days;
        }
    }
}