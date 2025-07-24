using Newtonsoft.Json.Linq;
using System;

namespace StardewArchipelago.Constants.Vanilla
{
    public class QualifiedItemIds
    {
        public const string OBJECT_QUALIFIER = "(O)";
        public const string BIG_CRAFTABLE_QUALIFIER = "(BC)";
        public const string FURNITURE_QUALIFIER = "(F)";
        public const string WEAPON_QUALIFIER = "(W)";
        public const string BOOTS_QUALIFIER = "(B)";
        public const string TOOLS_QUALIFIER = "(T)";
        public const string HAT_QUALIFIER = "(H)";
        public const string SHIRT_QUALIFIER = "(S)";
        public const string TRINKET_QUALIFIER = "(TR)";
        public const string WALLPAPER_QUALIFIER = "(WP)";
        public const string FLOORPAPER_QUALIFIER = "(FL)";
        public const string PANTS_QUALIFIER = "(P)";
        public const string MANNEQUIN_QUALIFIER = "(M)";
        public const string ZERO_QUALIFIER = "(0)"; // This apparently is a typo in the base game, and should be (O) [Object]
        public const string ARCHIPELAGO_QUALIFER = "(AP)";
        public const string MEME_BUNDLE_ITEM_QUALIFIER = "(MBI)";

        private static readonly string[] ALL_QUALIFIERS =
        {
            OBJECT_QUALIFIER, BIG_CRAFTABLE_QUALIFIER, FURNITURE_QUALIFIER, WEAPON_QUALIFIER, BOOTS_QUALIFIER, TOOLS_QUALIFIER, HAT_QUALIFIER,
            SHIRT_QUALIFIER, TRINKET_QUALIFIER, WALLPAPER_QUALIFIER, FLOORPAPER_QUALIFIER, PANTS_QUALIFIER, MANNEQUIN_QUALIFIER, ZERO_QUALIFIER,
        };

        public static readonly string ACORN = QualifiedObjectId(ObjectIds.ACORN);
        public static readonly string AMARANTH_SEEDS = QualifiedObjectId(ObjectIds.AMARANTH_SEEDS);
        public static readonly string AMETHYST = QualifiedObjectId(ObjectIds.AMETHYST);
        public static readonly string ANCIENT_SEEDS = QualifiedObjectId(ObjectIds.ANCIENT_SEEDS);
        public static readonly string APPLE = QualifiedObjectId(ObjectIds.APPLE);
        public static readonly string APPLE_SAPLING = QualifiedObjectId(ObjectIds.APPLE_SAPLING);
        public static readonly string APRICOT_SAPLING = QualifiedObjectId(ObjectIds.APRICOT_SAPLING);
        public static readonly string AQUAMARINE = QualifiedObjectId(ObjectIds.AQUAMARINE);
        public static readonly string ARTICHOKE_SEEDS = QualifiedObjectId(ObjectIds.ARTICHOKE_SEEDS);
        public static readonly string ARTIFACT_TROVE = QualifiedObjectId(ObjectIds.ARTIFACT_TROVE);
        public static readonly string AUTUMN_BOUNTY = QualifiedObjectId(ObjectIds.AUTUMN_BOUNTY);
        public static readonly string BAIT = QualifiedObjectId(ObjectIds.BAIT);
        public static readonly string BANANA_PUDDING = QualifiedObjectId(ObjectIds.BANANA_PUDDING);
        public static readonly string BANANA_SAPLING = QualifiedObjectId(ObjectIds.BANANA_SAPLING);
        public static readonly string BASIC_FERTILIZER = QualifiedObjectId(ObjectIds.BASIC_FERTILIZER);
        public static readonly string BASIC_RETAINING_SOIL = QualifiedObjectId(ObjectIds.BASIC_RETAINING_SOIL);
        public static readonly string BATTERY_PACK = QualifiedObjectId(ObjectIds.BATTERY_PACK);
        public static readonly string BEAN_HOTPOT = QualifiedObjectId(ObjectIds.BEAN_HOTPOT);
        public static readonly string BEAN_STARTER = QualifiedObjectId(ObjectIds.BEAN_STARTER);
        public static readonly string BEER = QualifiedObjectId(ObjectIds.BEER);
        public static readonly string BEET = QualifiedObjectId(ObjectIds.BEET);
        public static readonly string BEET_SEEDS = QualifiedObjectId(ObjectIds.BEET_SEEDS);
        public static readonly string BLACKBERRY = QualifiedObjectId(ObjectIds.BLACKBERRY);
        public static readonly string BLACKBERRY_COBBLER = QualifiedObjectId(ObjectIds.BLACKBERRY_COBBLER);
        public static readonly string BLUEBERRY_SEEDS = QualifiedObjectId(ObjectIds.BLUEBERRY_SEEDS);
        public static readonly string BOAT = QualifiedFurnitureId(FurnitureIds.BOAT);
        public static readonly string BOK_CHOY_SEEDS = QualifiedObjectId(ObjectIds.BOK_CHOY_SEEDS);
        public static readonly string BOMB = QualifiedObjectId(ObjectIds.BOMB);
        public static readonly string BONE_FRAGMENT = QualifiedObjectId(ObjectIds.BONE_FRAGMENT);
        public static readonly string BOUQUET = QualifiedObjectId(ObjectIds.BOUQUET);
        public static readonly string BREAD = QualifiedObjectId(ObjectIds.BREAD);
        public static readonly string BROKEN_CD = QualifiedObjectId(ObjectIds.BROKEN_CD);
        public static readonly string BROKEN_GLASSES = QualifiedObjectId(ObjectIds.BROKEN_GLASSES);
        public static readonly string BROWN_EGG = QualifiedObjectId(ObjectIds.BROWN_EGG);
        public static readonly string BUG_MEAT = QualifiedObjectId(ObjectIds.BUG_MEAT);
        public static readonly string CACTUS_FRUIT = QualifiedObjectId(ObjectIds.CACTUS_FRUIT);
        public static readonly string CACTUS_SEEDS = QualifiedObjectId(ObjectIds.CACTUS_SEEDS);
        public static readonly string CAULIFLOWER_SEEDS = QualifiedObjectId(ObjectIds.CAULIFLOWER_SEEDS);
        public static readonly string CAVE_JELLY = QualifiedObjectId(ObjectIds.CAVE_JELLY);
        public static readonly string CHALLENGE_BAIT = QualifiedObjectId(ObjectIds.CHALLENGE_BAIT);
        public static readonly string CHERRY_BOMB = QualifiedObjectId(ObjectIds.CHERRY_BOMB);
        public static readonly string CHERRY_SAPLING = QualifiedObjectId(ObjectIds.CHERRY_SAPLING);
        public static readonly string CHOCOLATE_CAKE = QualifiedObjectId(ObjectIds.CHOCOLATE_CAKE);
        public static readonly string CLAY = QualifiedObjectId(ObjectIds.CLAY);
        public static readonly string COAL = QualifiedObjectId(ObjectIds.COAL);
        public static readonly string COFFEE = QualifiedObjectId(ObjectIds.COFFEE);
        public static readonly string COMPLETE_BREAKFAST = QualifiedObjectId(ObjectIds.COMPLETE_BREAKFAST);
        public static readonly string COOKIES = QualifiedObjectId(ObjectIds.COOKIES);
        public static readonly string COPPER_BAR = QualifiedObjectId(ObjectIds.COPPER_BAR);
        public static readonly string COPPER_ORE = QualifiedObjectId(ObjectIds.COPPER_ORE);
        public static readonly string CORN_SEEDS = QualifiedObjectId(ObjectIds.CORN_SEEDS);
        public static readonly string CRAB_CAKES = QualifiedObjectId(ObjectIds.CRAB_CAKES);
        public static readonly string CRANBERRY_CANDY = QualifiedObjectId(ObjectIds.CRANBERRY_CANDY);
        public static readonly string CRANBERRY_SEEDS = QualifiedObjectId(ObjectIds.CRANBERRY_SEEDS);
        public static readonly string CRISPY_BASS = QualifiedObjectId(ObjectIds.CRISPY_BASS);
        public static readonly string CRYSTAL_FRUIT = QualifiedObjectId(ObjectIds.CRYSTAL_FRUIT);
        public static readonly string DECORATIVE_TRASH_CAN = QualifiedFurnitureId(FurnitureIds.DECORATIVE_TRASH_CAN);
        public static readonly string DELUXE_BAIT = QualifiedObjectId(ObjectIds.DELUXE_BAIT);
        public static readonly string DELUXE_FERTILIZER = QualifiedObjectId(ObjectIds.DELUXE_FERTILIZER);
        public static readonly string DELUXE_RETAINING_SOIL = QualifiedObjectId(ObjectIds.DELUXE_RETAINING_SOIL);
        public static readonly string DELUXE_SPEED_GRO = QualifiedObjectId(ObjectIds.DELUXE_SPEED_GRO);
        public static readonly string DIAMOND = QualifiedObjectId(ObjectIds.DIAMOND);
        public static readonly string DINOSAUR_EGG = QualifiedObjectId(ObjectIds.DINOSAUR_EGG);
        public static readonly string DRIED_FRUIT = QualifiedObjectId(ObjectIds.DRIED_FRUIT);
        public static readonly string DRIED_MUSHROOMS = QualifiedObjectId(ObjectIds.DRIED_MUSHROOMS);
        public static readonly string DRIFTWOOD = QualifiedObjectId(ObjectIds.DRIFTWOOD);
        public static readonly string DUCK_MAYONNAISE = QualifiedObjectId(ObjectIds.DUCK_MAYONNAISE);
        public static readonly string EARTH_CRYSTAL = QualifiedObjectId(ObjectIds.EARTH_CRYSTAL);
        public static readonly string EGGPLANT_SEEDS = QualifiedObjectId(ObjectIds.EGGPLANT_SEEDS);
        public static readonly string EMERALD = QualifiedObjectId(ObjectIds.EMERALD);
        public static readonly string ENERGY_TONIC = QualifiedObjectId(ObjectIds.ENERGY_TONIC);
        public static readonly string EXPLOSIVE_AMMO = QualifiedObjectId(ObjectIds.EXPLOSIVE_AMMO);
        public static readonly string FAIRY_ROSE = QualifiedObjectId(ObjectIds.FAIRY_ROSE);
        public static readonly string FAIRY_SEEDS = QualifiedObjectId(ObjectIds.FAIRY_SEEDS);
        public static readonly string FALL_SEEDS = QualifiedObjectId(ObjectIds.FALL_SEEDS);
        public static readonly string FAR_AWAY_STONE = QualifiedObjectId(ObjectIds.FAR_AWAY_STONE);
        public static readonly string FIBER = QualifiedObjectId(ObjectIds.FIBER);
        public static readonly string FIBER_SEEDS = QualifiedObjectId(ObjectIds.FIBER_SEEDS);
        public static readonly string FIDDLEHEAD_RISOTTO = QualifiedObjectId(ObjectIds.FIDDLEHEAD_RISOTTO);
        public static readonly string FIRE_QUARTZ = QualifiedObjectId(ObjectIds.FIRE_QUARTZ);
        public static readonly string FISH_TACO = QualifiedObjectId(ObjectIds.FISH_TACO);
        public static readonly string FOLIAGE_PRINT = QualifiedFurnitureId(FurnitureIds.FOLIAGE_PRINT);
        public static readonly string FOSSILIZED_SPINE = QualifiedObjectId(ObjectIds.FOSSILIZED_SPINE);
        public static readonly string FRIED_MUSHROOM = QualifiedObjectId(ObjectIds.FRIED_MUSHROOM);
        public static readonly string FROG_HAT = QualifiedHatId(HatIds.FROG_HAT);
        public static readonly string FROZEN_GEODE = QualifiedObjectId(ObjectIds.FROZEN_GEODE);
        public static readonly string FROZEN_TEAR = QualifiedObjectId(ObjectIds.FROZEN_TEAR);
        public static readonly string GALAXY_SOUL = QualifiedObjectId(ObjectIds.GALAXY_SOUL);
        public static readonly string GARLIC_SEEDS = QualifiedObjectId(ObjectIds.GARLIC_SEEDS);
        public static readonly string GEODE = QualifiedObjectId(ObjectIds.GEODE);
        public static readonly string GLAZED_YAMS = QualifiedObjectId(ObjectIds.GLAZED_YAMS);
        public static readonly string GOAT_CHEESE = QualifiedObjectId(ObjectIds.GOAT_CHEESE);
        public static readonly string GOLD_BAR = QualifiedObjectId(ObjectIds.GOLD_BAR);
        public static readonly string GOLD_LEWIS = QualifiedBigCraftableId(BigCraftableIds.GOLD_LEWIS);
        public static readonly string GOLD_ORE = QualifiedObjectId(ObjectIds.GOLD_ORE);
        public static readonly string GOLDEN_COCONUT = QualifiedObjectId(ObjectIds.GOLDEN_COCONUT);
        public static readonly string GOLDEN_EGG = QualifiedObjectId(ObjectIds.GOLDEN_EGG);
        public static readonly string GOLDEN_MYSTERY_BOX = QualifiedObjectId(ObjectIds.GOLDEN_MYSTERY_BOX);
        public static readonly string GOLDEN_PUMPKIN = QualifiedObjectId(ObjectIds.GOLDEN_PUMPKIN);
        public static readonly string GOLDEN_WALNUT = QualifiedObjectId(ObjectIds.GOLDEN_WALNUT);
        public static readonly string GOURMAND_STATUE = QualifiedFurnitureId(FurnitureIds.GOURMAND_STATUE);
        public static readonly string GRAPE = QualifiedObjectId(ObjectIds.GRAPE);
        public static readonly string GRAPE_STARTER = QualifiedObjectId(ObjectIds.GRAPE_STARTER);
        public static readonly string GRASS_STARTER = QualifiedObjectId(ObjectIds.GRASS_STARTER);
        public static readonly string GREEN_ALGAE = QualifiedObjectId(ObjectIds.GREEN_ALGAE);
        public static readonly string HARDWOOD = QualifiedObjectId(ObjectIds.HARDWOOD);
        public static readonly string HAY = QualifiedObjectId(ObjectIds.HAY);
        public static readonly string HONEY = QualifiedObjectId(ObjectIds.HONEY);
        public static readonly string HOPS_STARTER = QualifiedObjectId(ObjectIds.HOPS_STARTER);
        public static readonly string HYPER_SPEED_GRO = QualifiedObjectId(ObjectIds.HYPER_SPEED_GRO);
        public static readonly string IRIDIUM_BAR = QualifiedObjectId(ObjectIds.IRIDIUM_BAR);
        public static readonly string IRIDIUM_KROBUS = QualifiedFurnitureId(FurnitureIds.IRIDIUM_KROBUS);
        public static readonly string IRIDIUM_ORE = QualifiedObjectId(ObjectIds.IRIDIUM_ORE);
        public static readonly string IRIDIUM_SPRINKLER = QualifiedObjectId(ObjectIds.IRIDIUM_SPRINKLER);
        public static readonly string IRON_BAR = QualifiedObjectId(ObjectIds.IRON_BAR);
        public static readonly string IRON_ORE = QualifiedObjectId(ObjectIds.IRON_ORE);
        public static readonly string JADE = QualifiedObjectId(ObjectIds.JADE);
        public static readonly string JAZZ_SEEDS = QualifiedObjectId(ObjectIds.JAZZ_SEEDS);
        public static readonly string JOJA_COLA = QualifiedObjectId(ObjectIds.JOJA_COLA);
        public static readonly string JOURNAL_SCRAP = QualifiedObjectId(ObjectIds.JOURNAL_SCRAP);
        public static readonly string JUNIMO_PLUSH = QualifiedFurnitureId(FurnitureIds.JUNIMO_PLUSH);
        public static readonly string KALE_SEEDS = QualifiedObjectId(ObjectIds.KALE_SEEDS);
        public static readonly string LARGE_BROWN_EGG = QualifiedObjectId(ObjectIds.LARGE_BROWN_EGG);
        public static readonly string GOAT_MILK = QualifiedObjectId(ObjectIds.GOAT_MILK);
        public static readonly string LARGE_GOAT_MILK = QualifiedObjectId(ObjectIds.LARGE_GOAT_MILK);
        public static readonly string LARGE_MILK = QualifiedObjectId(ObjectIds.LARGE_MILK);
        public static readonly string LEEK = QualifiedObjectId(ObjectIds.LEEK);
        public static readonly string LIFESAVER = QualifiedFurnitureId(FurnitureIds.LIFESAVER);
        public static readonly string LUCKY_LUNCH = QualifiedObjectId(ObjectIds.LUCKY_LUNCH);
        public static readonly string LUCKY_PURPLE_SHORTS = QualifiedObjectId(ObjectIds.LUCKY_PURPLE_SHORTS);
        public static readonly string TRIMMED_LUCKY_PURPLE_SHORTS = QualifiedObjectId(ObjectIds.TRIMMED_LUCKY_PURPLE_SHORTS);
        public static readonly string MAGIC_BAIT = QualifiedObjectId(ObjectIds.MAGIC_BAIT);
        public static readonly string MAGIC_ROCK_CANDY = QualifiedObjectId(ObjectIds.MAGIC_ROCK_CANDY);
        public static readonly string MAGMA_GEODE = QualifiedObjectId(ObjectIds.MAGMA_GEODE);
        public static readonly string MAHOGANY_SEED = QualifiedObjectId(ObjectIds.MAHOGANY_SEED);
        public static readonly string MANGO_SAPLING = QualifiedObjectId(ObjectIds.MANGO_SAPLING);
        public static readonly string MAPLE_BAR = QualifiedObjectId(ObjectIds.MAPLE_BAR);
        public static readonly string MAPLE_SEED = QualifiedObjectId(ObjectIds.MAPLE_SEED);
        public static readonly string MEGA_BOMB = QualifiedObjectId(ObjectIds.MEGA_BOMB);
        public static readonly string MELON_SEEDS = QualifiedObjectId(ObjectIds.MELON_SEEDS);
        public static readonly string MEOWMERE = QualifiedWeaponId(WeaponIds.MEOWMERE);
        public static readonly string MILK = QualifiedObjectId(ObjectIds.MILK);
        public static readonly string MINERS_TREAT = QualifiedObjectId(ObjectIds.MINERS_TREAT);
        public static readonly string MIXED_FLOWER_SEEDS = QualifiedObjectId(ObjectIds.MIXED_FLOWER_SEEDS);
        public static readonly string MIXED_SEEDS = QualifiedObjectId(ObjectIds.MIXED_SEEDS);
        public static readonly string MONSTER_MUSK = QualifiedObjectId(ObjectIds.MONSTER_MUSK);
        public static readonly string MOSSY_SEED = QualifiedObjectId(ObjectIds.MOSSY_SEED);
        public static readonly string MUMMIFIED_BAT = QualifiedObjectId(ObjectIds.MUMMIFIED_BAT);
        public static readonly string MUSCLE_REMEDY = QualifiedObjectId(ObjectIds.MUSCLE_REMEDY);
        public static readonly string MUSHROOM_TREE_SEED = QualifiedObjectId(ObjectIds.MUSHROOM_TREE_SEED);
        public static readonly string MUTANT_CARP = QualifiedObjectId(ObjectIds.MUTANT_CARP);
        public static readonly string MYSTERY_BOX = QualifiedObjectId(ObjectIds.MYSTERY_BOX);
        public static readonly string MYSTIC_TREE_SEED = QualifiedObjectId(ObjectIds.MYSTIC_TREE_SEED);
        public static readonly string OIL = QualifiedObjectId(ObjectIds.OIL);
        public static readonly string OMNI_GEODE = QualifiedObjectId(ObjectIds.OMNI_GEODE);
        public static readonly string ORANGE_SAPLING = QualifiedObjectId(ObjectIds.ORANGE_SAPLING);
        public static readonly string ORNATE_NECKLACE = QualifiedObjectId(ObjectIds.ORNATE_NECKLACE);
        public static readonly string PANCAKES = QualifiedObjectId(ObjectIds.PANCAKES);
        public static readonly string PARSNIP = QualifiedObjectId(ObjectIds.PARSNIP);
        public static readonly string PARSNIP_SEEDS = QualifiedObjectId(ObjectIds.PARSNIP_SEEDS);
        public static readonly string PEACH_SAPLING = QualifiedObjectId(ObjectIds.PEACH_SAPLING);
        public static readonly string PEARL = QualifiedObjectId(ObjectIds.PEARL);
        public static readonly string PEPPER_POPPERS = QualifiedObjectId(ObjectIds.PEPPER_POPPERS);
        public static readonly string PEPPER_SEEDS = QualifiedObjectId(ObjectIds.PEPPER_SEEDS);
        public static readonly string PHYSICS_101 = QualifiedFurnitureId(FurnitureIds.PHYSICS_101);
        public static readonly string PICKLES = QualifiedObjectId(ObjectIds.PICKLES);
        public static readonly string PINE_CONE = QualifiedObjectId(ObjectIds.PINE_CONE);
        public static readonly string PINEAPPLE_SEEDS = QualifiedObjectId(ObjectIds.PINEAPPLE_SEEDS);
        public static readonly string PINK_CAKE = QualifiedObjectId(ObjectIds.PINK_CAKE);
        public static readonly string PIZZA = QualifiedObjectId(ObjectIds.PIZZA);
        public static readonly string PLUM_PUDDING = QualifiedObjectId(ObjectIds.PLUM_PUDDING);
        public static readonly string POMEGRANATE = QualifiedObjectId(ObjectIds.POMEGRANATE);
        public static readonly string POMEGRANATE_SAPLING = QualifiedObjectId(ObjectIds.POMEGRANATE_SAPLING);
        public static readonly string POPPY = QualifiedObjectId(ObjectIds.POPPY);
        public static readonly string POPPY_SEEDS = QualifiedObjectId(ObjectIds.POPPY_SEEDS);
        public static readonly string POT_OF_GOLD = QualifiedObjectId(ObjectIds.POT_OF_GOLD);
        public static readonly string POTATO_SEEDS = QualifiedObjectId(ObjectIds.POTATO_SEEDS);
        public static readonly string PRISMATIC_SHARD = QualifiedObjectId(ObjectIds.PRISMATIC_SHARD);
        public static readonly string PRIZE_TICKET = QualifiedObjectId(ObjectIds.PRIZE_TICKET);
        public static readonly string PUMPKIN = QualifiedObjectId(ObjectIds.PUMPKIN);
        public static readonly string PUMPKIN_PIE = QualifiedObjectId(ObjectIds.PUMPKIN_PIE);
        public static readonly string PUMPKIN_SEEDS = QualifiedObjectId(ObjectIds.PUMPKIN_SEEDS);
        public static readonly string PUMPKIN_SOUP = QualifiedObjectId(ObjectIds.PUMPKIN_SOUP);
        public static readonly string PYRAMID_DECAL = QualifiedFurnitureId(FurnitureIds.PYRAMID_DECAL);
        public static readonly string QI_BEAN = QualifiedObjectId(ObjectIds.QI_BEAN);
        public static readonly string QI_FRUIT = QualifiedObjectId(ObjectIds.QI_FRUIT);
        public static readonly string QI_SEASONING = QualifiedObjectId(ObjectIds.QI_SEASONING);
        public static readonly string QUALITY_FERTILIZER = QualifiedObjectId(ObjectIds.QUALITY_FERTILIZER);
        public static readonly string QUALITY_RETAINING_SOIL = QualifiedObjectId(ObjectIds.QUALITY_RETAINING_SOIL);
        public static readonly string QUALITY_SPRINKLER = QualifiedObjectId(ObjectIds.QUALITY_SPRINKLER);
        public static readonly string QUARTZ = QualifiedObjectId(ObjectIds.QUARTZ);
        public static readonly string RADIOACTIVE_BAR = QualifiedObjectId(ObjectIds.RADIOACTIVE_BAR);
        public static readonly string RADIOACTIVE_ORE = QualifiedObjectId(ObjectIds.RADIOACTIVE_ORE);
        public static readonly string RADISH_SEEDS = QualifiedObjectId(ObjectIds.RADISH_SEEDS);
        public static readonly string RAIN_TOTEM = QualifiedObjectId(ObjectIds.RAIN_TOTEM);
        public static readonly string RARE_SEED = QualifiedObjectId(ObjectIds.RARE_SEED);
        public static readonly string RED_CABBAGE_SEEDS = QualifiedObjectId(ObjectIds.RED_CABBAGE_SEEDS);
        public static readonly string RED_PLATE = QualifiedObjectId(ObjectIds.RED_PLATE);
        public static readonly string REFINED_QUARTZ = QualifiedObjectId(ObjectIds.REFINED_QUARTZ);
        public static readonly string RHUBARB_SEEDS = QualifiedObjectId(ObjectIds.RHUBARB_SEEDS);
        public static readonly string RICE = QualifiedObjectId(ObjectIds.RICE);
        public static readonly string RICE_SHOOT = QualifiedObjectId(ObjectIds.RICE_SHOOT);
        public static readonly string RIVER_JELLY = QualifiedObjectId(ObjectIds.RIVER_JELLY);
        public static readonly string ROASTED_HAZELNUTS = QualifiedObjectId(ObjectIds.ROASTED_HAZELNUTS);
        public static readonly string ROOTS_PLATTER = QualifiedObjectId(ObjectIds.ROOTS_PLATTER);
        public static readonly string RUBY = QualifiedObjectId(ObjectIds.RUBY);
        public static readonly string SALAD = QualifiedObjectId(ObjectIds.SALAD);
        public static readonly string SALMON_DINNER = QualifiedObjectId(ObjectIds.SALMON_DINNER);
        public static readonly string SALMONBERRY = QualifiedObjectId(ObjectIds.SALMONBERRY);
        public static readonly string SAP = QualifiedObjectId(ObjectIds.SAP);
        public static readonly string SASHIMI = QualifiedObjectId(ObjectIds.SASHIMI);
        public static readonly string SEA_JELLY = QualifiedObjectId(ObjectIds.SEA_JELLY);
        public static readonly string SEAFOAM_PUDDING = QualifiedObjectId(ObjectIds.SEAFOAM_PUDDING);
        public static readonly string SEASONAL_PLANT = "(BC)196";
        public static readonly string SEAWEED = QualifiedObjectId(ObjectIds.SEAWEED);
        public static readonly string SECRET_NOTE = QualifiedObjectId(ObjectIds.SECRET_NOTE);
        public static readonly string SMOKED_FISH = QualifiedObjectId(ObjectIds.SMOKED_FISH);
        public static readonly string SNAKE_SKULL = QualifiedObjectId(ObjectIds.SNAKE_SKULL);
        public static readonly string SOGGY_NEWSPAPER = QualifiedObjectId(ObjectIds.SOGGY_NEWSPAPER);
        public static readonly string SPAGHETTI = QualifiedObjectId(ObjectIds.SPAGHETTI);
        public static readonly string SPANGLE_SEEDS = QualifiedObjectId(ObjectIds.SPANGLE_SEEDS);
        public static readonly string SPEED_GRO = QualifiedObjectId(ObjectIds.SPEED_GRO);
        public static readonly string SPICY_EEL = QualifiedObjectId(ObjectIds.SPICY_EEL);
        public static readonly string SPRING_SEEDS = QualifiedObjectId(ObjectIds.SPRING_SEEDS);
        public static readonly string SPRINKLER = QualifiedObjectId(ObjectIds.SPRINKLER);
        public static readonly string SQUIRREL_FIGURINE = QualifiedFurnitureId(FurnitureIds.SQUIRREL_FIGURINE);
        public static readonly string STARDROP = QualifiedObjectId(ObjectIds.STARDROP);
        public static readonly string STARDROP_TEA = QualifiedObjectId(ObjectIds.STARDROP_TEA);
        public static readonly string STARFRUIT_SEEDS = QualifiedObjectId(ObjectIds.STARFRUIT_SEEDS);
        public static readonly string STONE = QualifiedObjectId(ObjectIds.STONE);
        public static readonly string STONE_JUNIMO = QualifiedBigCraftableId(BigCraftableIds.STONE_JUNIMO);
        public static readonly string STRANGE_BUN = QualifiedObjectId(ObjectIds.STRANGE_BUN);
        public static readonly string STRANGE_DOLL_GREEN = QualifiedObjectId(ObjectIds.STRANGE_DOLL_GREEN);
        public static readonly string STRAWBERRY = QualifiedObjectId(ObjectIds.STRAWBERRY);
        public static readonly string SUGAR = QualifiedObjectId(ObjectIds.SUGAR);
        public static readonly string SUMMER_SEEDS = QualifiedObjectId(ObjectIds.SUMMER_SEEDS);
        public static readonly string SUNFLOWER = QualifiedObjectId(ObjectIds.SUNFLOWER);
        public static readonly string SUNFLOWER_SEEDS = QualifiedObjectId(ObjectIds.SUNFLOWER_SEEDS);
        public static readonly string SUPER_CUCUMBER = QualifiedObjectId(ObjectIds.SUPER_CUCUMBER);
        public static readonly string TEA_SET = QualifiedObjectId(ObjectIds.TEA_SET);
        public static readonly string TOMATO_SEEDS = QualifiedObjectId(ObjectIds.TOMATO_SEEDS);
        public static readonly string TOPAZ = QualifiedObjectId(ObjectIds.TOPAZ);
        public static readonly string TORCH = QualifiedObjectId(ObjectIds.TORCH);
        public static readonly string TORTILLA = QualifiedObjectId(ObjectIds.TORTILLA);
        public static readonly string TRASH = QualifiedObjectId(ObjectIds.TRASH);
        public static readonly string TREASURE_CHEST = QualifiedObjectId(ObjectIds.TREASURE_CHEST);
        public static readonly string TRIPLE_SHOT_ESPRESSO = QualifiedObjectId(ObjectIds.TRIPLE_SHOT_ESPRESSO);
        public static readonly string TROPICAL_CURRY = QualifiedObjectId(ObjectIds.TROPICAL_CURRY);
        public static readonly string TROUT_SOUP = QualifiedObjectId(ObjectIds.TROUT_SOUP);
        public static readonly string TRUFFLE = QualifiedObjectId(ObjectIds.TRUFFLE);
        public static readonly string TULIP = QualifiedObjectId(ObjectIds.TULIP);
        public static readonly string TULIP_BULB = QualifiedObjectId(ObjectIds.TULIP_BULB);
        public static readonly string VINEGAR = QualifiedObjectId(ObjectIds.VINEGAR);
        public static readonly string VISTA = QualifiedFurnitureId(FurnitureIds.VISTA);
        public static readonly string VOID_MAYONNAISE = QualifiedObjectId(ObjectIds.VOID_MAYONNAISE);
        public static readonly string WALL_BASKET = QualifiedFurnitureId(FurnitureIds.WALL_BASKET);
        public static readonly string WALL_CACTUS = QualifiedFurnitureId(FurnitureIds.WALL_CACTUS);
        public static readonly string WARP_TOTEM_BEACH = QualifiedObjectId(ObjectIds.WARP_TOTEM_BEACH);
        public static readonly string WARP_TOTEM_DESERT = QualifiedObjectId(ObjectIds.WARP_TOTEM_DESERT);
        public static readonly string WARP_TOTEM_FARM = QualifiedObjectId(ObjectIds.WARP_TOTEM_FARM);
        public static readonly string WARP_TOTEM_ISLAND = QualifiedObjectId(ObjectIds.WARP_TOTEM_ISLAND);
        public static readonly string WARP_TOTEM_MOUNTAINS = QualifiedObjectId(ObjectIds.WARP_TOTEM_MOUNTAINS);
        public static readonly string WHEAT_FLOUR = QualifiedObjectId(ObjectIds.WHEAT_FLOUR);
        public static readonly string WHEAT_SEEDS = QualifiedObjectId(ObjectIds.WHEAT_SEEDS);
        public static readonly string WHITE_ALGAE = QualifiedObjectId(ObjectIds.WHITE_ALGAE);
        public static readonly string WINE = QualifiedObjectId(ObjectIds.WINE);
        public static readonly string WINTER_SEEDS = QualifiedObjectId(ObjectIds.WINTER_SEEDS);
        public static readonly string WOOD = QualifiedObjectId(ObjectIds.WOOD);
        public static readonly string WOOL = QualifiedObjectId(ObjectIds.WOOL);
        public static readonly string YAM_SEEDS = QualifiedObjectId(ObjectIds.YAM_SEEDS);
        public static readonly string MERMAID_PENDANT = QualifiedObjectId(ObjectIds.MERMAIDS_PENDANT);
        public static readonly string WILTED_BOUQUET = QualifiedObjectId(ObjectIds.WILTED_BOUQUET);
        public static readonly string ANCIENT_DOLL = QualifiedObjectId(ObjectIds.ANCIENT_DOLL);
        public static readonly string TUNA = QualifiedObjectId(ObjectIds.TUNA);
        public static readonly string PARTY_HAT_RED = QualifiedHatId(HatIds.PARTY_HAT_RED);
        public static readonly string PARTY_HAT_BLUE = QualifiedHatId(HatIds.PARTY_HAT_BLUE);
        public static readonly string PARTY_HAT_GREEN = QualifiedHatId(HatIds.PARTY_HAT_GREEN);
        public static readonly string COPPER_PAN_HAT = QualifiedHatId(HatIds.COPPER_PAN);
        public static readonly string STEEL_PAN_HAT = QualifiedHatId(HatIds.STEEL_PAN);
        public static readonly string GOLD_PAN_HAT = QualifiedHatId(HatIds.GOLD_PAN);
        public static readonly string IRIDIUM_PAN_HAT = QualifiedHatId(HatIds.IRIDIUM_PAN);
        public static readonly string RETURN_SCEPTER = QualifiedToolId(ToolIds.RETURN_SCEPTER);
        public static readonly string EXOTIC_DOUBLE_BED = QualifiedFurnitureId(FurnitureIds.EXOTIC_DOUBLE_BED);
        public static readonly string PIERRE_MISSING_STOCKLIST = QualifiedObjectId(ObjectIds.PIERRES_MISSING_STOCKLIST);
        public static readonly string HORSE_FLUTE = QualifiedObjectId(ObjectIds.HORSE_FLUTE);
        public static readonly string MINI_SHIPPING_BIN = QualifiedBigCraftableId(BigCraftableIds.MINI_SHIPPING_BIN);
        public static readonly string IRIDIUM_SNAKE_MILK = QualifiedObjectId(ObjectIds.IRIDIUM_SNAKE_MILK);
        public static readonly string ADVANCED_TV_REMOTE = QualifiedObjectId(ObjectIds.ADVANCED_TV_REMOTE);

        public static readonly string CATALOGUE = QualifiedFurnitureId(FurnitureIds.CATALOGUE);
        public static readonly string FURNITURE_CATALOGUE = QualifiedFurnitureId(FurnitureIds.FURNITURE_CATALOGUE);
        public static readonly string JOJA_CATALOGUE = QualifiedFurnitureId(FurnitureIds.JOJA_CATALOGUE);
        public static readonly string JUNIMO_CATALOGUE = QualifiedFurnitureId(FurnitureIds.JUNIMO_CATALOGUE);
        public static readonly string RETRO_CATALOGUE = QualifiedFurnitureId(FurnitureIds.RETRO_CATALOGUE);
        public static readonly string TRASH_CATALOGUE = QualifiedFurnitureId(FurnitureIds.TRASH_CATALOGUE);
        public static readonly string WIZARD_CATALOGUE = QualifiedFurnitureId(FurnitureIds.WIZARD_CATALOGUE);
        public static readonly string SCYTHE = QualifiedWeaponId(WeaponIds.SCYTHE);
        public static readonly string GOLDEN_SCYTHE = QualifiedWeaponId(WeaponIds.GOLDEN_SCYTHE);
        public static readonly string IRIDIUM_SCYTHE = QualifiedWeaponId(WeaponIds.IRIDIUM_SCYTHE);

        public static readonly string[] ALL_CATALOGUES = {
            CATALOGUE, FURNITURE_CATALOGUE, JOJA_CATALOGUE, JUNIMO_CATALOGUE,
            RETRO_CATALOGUE, TRASH_CATALOGUE, WIZARD_CATALOGUE,
        };

        public static string QualifiedObjectId(string objectId)
        {
            return QualifyId(OBJECT_QUALIFIER, objectId);
        }

        public static string QualifiedBigCraftableId(string bigCraftableId)
        {
            return QualifyId(BIG_CRAFTABLE_QUALIFIER, bigCraftableId);
        }

        public static string QualifiedWeaponId(string weaponId)
        {
            return QualifyId(WEAPON_QUALIFIER, weaponId);
        }

        public static string QualifiedFurnitureId(string furnitureId)
        {
            return QualifyId(FURNITURE_QUALIFIER, furnitureId);
        }

        public static string QualifiedHatId(string hatId)
        {
            return QualifyId(HAT_QUALIFIER, hatId);
        }

        public static string QualifiedToolId(string toolId)
        {
            return QualifyId(TOOLS_QUALIFIER, toolId);
        }

        public static string QualifyId(string qualifier, string objectId)
        {
            if (qualifier == null || objectId == null)
            {
                return null;
            }

            return $"{qualifier}{objectId}";
        }

        public static string UnqualifyId(string id)
        {
            return UnqualifyId(id, out _);
        }

        public static string UnqualifyId(string id, out string removedQualifier)
        {
            removedQualifier = string.Empty;
            if (id == null)
            {
                return null;
            }

            foreach (var qualifier in ALL_QUALIFIERS)
            {
                if (!IsType(id, qualifier))
                {
                    continue;
                }

                removedQualifier = id[..qualifier.Length];
                return id[qualifier.Length..];
            }

            if (id.StartsWith("("))
            {
                ModEntry.Instance.Logger.LogDebug($"Tried to unqualify Id '{id}', but couldn't figure out the qualifier!");
            }

            return id;
        }

        public static bool IsObject(string itemId)
        {
            return IsType(itemId, OBJECT_QUALIFIER);
        }

        public static bool IsBigCraftable(string itemId)
        {
            return IsType(itemId, BIG_CRAFTABLE_QUALIFIER);
        }

        public static bool IsHat(string itemId)
        {
            return IsType(itemId, HAT_QUALIFIER);
        }

        public static bool IsFurniture(string itemId)
        {
            return IsType(itemId, FURNITURE_QUALIFIER);
        }

        private static bool IsType(string itemId, string qualifier)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            return itemId.StartsWith(qualifier, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsLuckyShorts(string qualifiedId)
        {
            return qualifiedId == LUCKY_PURPLE_SHORTS || qualifiedId == TRIMMED_LUCKY_PURPLE_SHORTS;
        }
    }
}
