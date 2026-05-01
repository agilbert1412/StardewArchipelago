using System.Collections.Generic;
using StardewArchipelago.Archipelago.ApworldData;

namespace StardewArchipelago.Constants
{
    public class ShopIds
    {
        public const string ADVENTURE_GUILD_RECOVERY = "AdventureGuildRecovery";
        public const string ADVENTURE_SHOP = "AdventureShop";
        public const string ANIMAL_SHOP = "AnimalShop";
        public const string BLACKSMITH = "Blacksmith";
        public const string CATALOGUE = "Catalogue";
        public const string CLINT_UPGRADE = "ClintUpgrade";
        public const string PET_ADOPTION = "PetAdoption";
        public const string BOX_OFFICE = "BoxOffice";
        public const string CARPENTER = "Carpenter";
        public const string CASINO = "Casino";
        public const string CONCESSIONS = "Concessions";
        public const string DESERT_FESTIVAL_ABIGAIL = "DesertFestival_Abigail";
        public const string DESERT_FESTIVAL_MARU = "DesertFestival_Maru";
        public const string DESERT_FESTIVAL_EMILY = "DesertFestival_Emily";
        public const string DESERT_FESTIVAL_LEAH = "DesertFestival_Leah";
        public const string DESERT_FESTIVAL_HARVEY = "DesertFestival_Harvey";
        public const string DESERT_FESTIVAL_PIERRE = "DesertFestival_Pierre";
        public const string DESERT_FESTIVAL_SAM = "DesertFestival_Sam";
        public const string DESERT_FESTIVAL_SEBASTIAN = "DesertFestival_Sebastian";
        public const string DESERT_FESTIVAL_ALEX = "DesertFestival_Alex";
        public const string DESERT_FESTIVAL_CAROLINE = "DesertFestival_Caroline";
        public const string DESERT_FESTIVAL_GUS = "DesertFestival_Gus";
        public const string DESERT_FESTIVAL_ELLIOTT = "DesertFestival_Elliott";
        public const string DESERT_FESTIVAL_HALEY = "DesertFestival_Haley";
        public const string DESERT_FESTIVAL_DEMETRIUS = "DesertFestival_Demetrius";
        public const string DESERT_FESTIVAL_CLINT = "DesertFestival_Clint";
        public const string DESERT_FESTIVAL_ROBIN = "DesertFestival_Robin";
        public const string DESERT_FESTIVAL_PAM = "DesertFestival_Pam";
        public const string DESERT_FESTIVAL_PENNY = "DesertFestival_Penny";
        public const string DESERT_FESTIVAL_LEO = "DesertFestival_Leo";
        public const string DESERT_FESTIVAL_EVELYN = "DesertFestival_Evelyn";
        public const string DESERT_FESTIVAL_GEORGE = "DesertFestival_George";
        public const string DESERT_FESTIVAL_MARNIE = "DesertFestival_Marnie";
        public const string DESERT_FESTIVAL_SHANE = "DesertFestival_Shane";
        public const string DESERT_FESTIVAL_JAS = "DesertFestival_Jas";
        public const string DESERT_FESTIVAL_JODI = "DesertFestival_Jodi";
        public const string DESERT_FESTIVAL_VINCENT = "DesertFestival_Vincent";
        public const string DESERT_FESTIVAL_KENT = "DesertFestival_Kent";
        public const string DESERT_FESTIVAL_EGG_SHOP = "DesertFestival_EggShop";
        public const string DESERT_TRADE = "DesertTrade";
        public const string DWARF = "Dwarf";
        public const string FESTIVAL_DANCE_OF_THE_MOONLIGHT_JELLIES_PIERRE = "Festival_DanceOfTheMoonlightJellies_Pierre";
        public const string FESTIVAL_EGG_FESTIVAL_PIERRE = "Festival_EggFestival_Pierre";
        public const string FESTIVAL_FEAST_OF_THE_WINTER_STAR_PIERRE = "Festival_FeastOfTheWinterStar_Pierre";
        public const string FESTIVAL_FESTIVAL_OF_ICE_TRAVELING_MERCHANT = "Festival_FestivalOfIce_TravelingMerchant";
        public const string FESTIVAL_FLOWER_DANCE_PIERRE = "Festival_FlowerDance_Pierre";
        public const string FESTIVAL_LUAU_PIERRE = "Festival_Luau_Pierre";
        public const string FESTIVAL_NIGHT_MARKET_DECORATION_BOAT = "Festival_NightMarket_DecorationBoat";
        public const string FESTIVAL_NIGHT_MARKET_MAGIC_BOAT_DAY1 = "Festival_NightMarket_MagicBoat_Day1";
        public const string FESTIVAL_NIGHT_MARKET_MAGIC_BOAT_DAY2 = "Festival_NightMarket_MagicBoat_Day2";
        public const string FESTIVAL_NIGHT_MARKET_MAGIC_BOAT_DAY3 = "Festival_NightMarket_MagicBoat_Day3";
        public const string FESTIVAL_SPIRITS_EVE_PIERRE = "Festival_SpiritsEve_Pierre";
        public const string FESTIVAL_STARDEW_VALLEY_FAIR_STAR_TOKENS = "Festival_StardewValleyFair_StarTokens";
        public const string FISH_SHOP = "FishShop";
        public const string FURNITURE_CATALOGUE = "Furniture Catalogue";
        public const string HAT_MOUSE = "HatMouse";
        public const string HOSPITAL = "Hospital";
        public const string ICE_CREAM_STAND = "IceCreamStand";
        public const string ISLAND_TRADE = "IslandTrade";
        public const string JOJA = "Joja";
        public const string QI_GEM_SHOP = "QiGemShop";
        public const string RESORT_BAR = "ResortBar";
        public const string SALOON = "Saloon";
        public const string SANDY = "Sandy";
        public const string SEED_SHOP = "SeedShop";
        public const string BOOKSELLER = "Bookseller";
        public const string SHADOW_SHOP = "ShadowShop";
        public const string TRAVELER = "Traveler";
        public const string VOLCANO_SHOP = "VolcanoShop";
        public const string RACCOON = "Raccoon";
        public const string JOJA_FURNITURE_CATALOGUE = "JojaFurnitureCatalogue";
        public const string WIZARD_FURNITURE_CATALOGUE = "WizardFurnitureCatalogue";
        public const string JUNIMO_FURNITURE_CATALOGUE = "JunimoFurnitureCatalogue";
        public const string BOOKSELLER_TRADE = "BooksellerTrade";
        public const string RETRO_FURNITURE_CATALOGUE = "RetroFurnitureCatalogue";
        public const string TRASH_FURNITURE_CATALOGUE = "TrashFurnitureCatalogue";

        public static readonly Dictionary<string, List<string>> ShopIdsToApworldNames = new()
        {
            { SEED_SHOP, new() { ShopNames.PIERRES_GENERAL_STORE } },
            { FESTIVAL_EGG_FESTIVAL_PIERRE, new() { ShopNames.EGG_FESTIVAL } },
            { TRAVELER, new() { ShopNames.TRAVELING_CART } },
            { SALOON, new() { ShopNames.SALOON } },
            { BOOKSELLER, new() { ShopNames.BOOKSELLER_RARE_BOOKS, ShopNames.BOOKSELLER_PERMANENT_BOOKS, ShopNames.BOOKSELLER_EXPERIENCE_BOOKS } },
            { ANIMAL_SHOP, new() { ShopNames.MARNIES_RANCH } },
            { DWARF, new() { ShopNames.MINES_DWARF_SHOP } },
            { CARPENTER, new() { ShopNames.CARPENTER_SHOP } },
            // { __, new() { ShopNames.MOVIE_THEATER }},
            { SHADOW_SHOP, new() { ShopNames.SEWER } },
            { FISH_SHOP, new() { ShopNames.WILLYS_FISH_SHOP } },
            { HOSPITAL, new() { ShopNames.HOSPITAL } },
            { QI_GEM_SHOP, new() { ShopNames.QIS_WALNUT_ROOM } },
            { BOX_OFFICE, new() { ShopNames.TICKET_STAND } },
            { ICE_CREAM_STAND, new() { ShopNames.TOWN } },
            { SANDY, new() { ShopNames.OASIS } },
            { FESTIVAL_FLOWER_DANCE_PIERRE, new() { ShopNames.FLOWER_DANCE } },
            { FESTIVAL_DANCE_OF_THE_MOONLIGHT_JELLIES_PIERRE, new() { ShopNames.DANCE_OF_THE_MOONLIGHT_JELLIES } },
            { FESTIVAL_STARDEW_VALLEY_FAIR_STAR_TOKENS, new() { ShopNames.STARDEW_VALLEY_FAIR } },
            { FESTIVAL_SPIRITS_EVE_PIERRE, new() { ShopNames.SPIRITS_EVE } },
            { FESTIVAL_FESTIVAL_OF_ICE_TRAVELING_MERCHANT, new() { ShopNames.FESTIVAL_OF_ICE } },
            // { FESTIVAL_NIGHT_MARKET_MAGIC_BOAT_DAY1, new() { ShopNames.NIGHT_MARKET }},
            // { FESTIVAL_NIGHT_MARKET_MAGIC_BOAT_DAY2, new() { ShopNames.NIGHT_MARKET }},
            // { FESTIVAL_NIGHT_MARKET_MAGIC_BOAT_DAY3, new() { ShopNames.NIGHT_MARKET }},
            // { __, new() { ShopNames.LOST_ITEMS_SHOP }},
            { RESORT_BAR, new() { ShopNames.ISLAND_RESORT } },
            { VOLCANO_SHOP, new() { ShopNames.VOLCANO_DWARF_SHOP } },
            { CASINO, new() { ShopNames.CASINO } },
            { DESERT_FESTIVAL_EGG_SHOP, new() { ShopNames.DESERT_FESTIVAL } },
            { ADVENTURE_SHOP, new() { ShopNames.ADVENTURERS_GUILD } },
            // { CLINT_UPGRADE, new() { ShopNames.CLINTS_BLACKSMITH }},
            { RACCOON, new() { ShopNames.RACCOON_SHOP_AFTER_1_REQUEST, ShopNames.RACCOON_SHOP_AFTER_2_REQUESTS } },
            { ISLAND_TRADE, new() { ShopNames.ISLAND_TRADER } },
            { DESERT_TRADE, new() { ShopNames.DESERT } },
        };
    }
}
