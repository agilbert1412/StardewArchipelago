using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.Modded
{
    public class JunimoShopStockModifier : BarterShopStockModifier
    {
        private static readonly string[] spring = { "spring" };
        private static readonly string[] summer = { "summer" };
        private static readonly string[] fall = { "fall" };
        private static readonly string[] summer_fall = { "summer", "fall" };
        private static readonly string[] winter = { "winter" };
        private const float INITIAL_DISCOUNT = 0.7f;
        private const float APPLES_DISCOUNT = 0.025f;

        private static readonly Dictionary<string, string> _junimoPhrase = new()
        {
            { "Orange", "Look! Me gib pretty for orange thing!" },
            { "Red", "Give old things for red gubbins!" },
            { "Grey", "I trade rocks for grey what's-its!" },
            { "Yellow", "I hab seeds, gib yellow gubbins!" },
            { "Blue", "I hab fish! You give blue pretty?" },
            { "Purple", "Rare thing?  Purple thing!  Yay!" },
        };

        public JunimoShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;
                    var orangeShop = shopsData["FlashShifter.StardewValleyExpandedCP_OrangeJunimoVendor"];
                    var purpleShop = shopsData["FlashShifter.StardewValleyExpandedCP_PurpleJunimoVendor"];
                    var redShop = shopsData["FlashShifter.StardewValleyExpandedCP_RedJunimoVendor"];
                    var blueShop = shopsData["FlashShifter.StardewValleyExpandedCP_BlueJunimoVendor"];
                    var greyShop = shopsData["FlashShifter.StardewValleyExpandedCP_GreyJunimoVendor"];
                    var yellowShop = shopsData["FlashShifter.StardewValleyExpandedCP_YellowJunimoVendor"];
                    var priceReduction = 1 - GiveApplesFriendshipDiscount();
                    var stockCount = StockBasedOnApplesFriendship();
                    GenerateBlueJunimoStock(blueShop, stockCount, priceReduction);
                    GenerateRedJunimoStock(redShop, stockCount, priceReduction);
                    GenerateGreyJunimoStock(greyShop, stockCount, priceReduction);
                    GenerateYellowJunimoStock(yellowShop, stockCount, priceReduction);
                    GenerateOrangeJunimoStock(orangeShop, stockCount, priceReduction);
                    GeneratePurpleJunimoStock(purpleShop, stockCount, priceReduction);
                },
                AssetEditPriority.Late
            );
        }

        private void GenerateBlueJunimoStock(ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Blue"];
            var fishData = DataLoader.Fish(Game1.content);
            shopData.Items.Clear();
            var blueObjects = _stardewItemManager.GetObjectsByColor("Blue").Where(x => !Constants.Modded.IgnoredModdedStrings.JojaRouteSVE.Contains(x.Name)).ToList();

            foreach (var (id, fishInfo) in fishData)
            {
                string[] fishSeasons = null;
                if (fishInfo.Split("/")[1] != "trap")
                {
                    fishSeasons = fishInfo.Split("/")[6].Split(" ");
                }
                var fishItem = _stardewItemManager.GetObjectById(id);
                var itemHashCode = fishItem.GetHashCode();
                var condition = $"SYNCED_RANDOM day junimo_shops_{itemHashCode} 0.4 @addDailyLuck, PLAYER_HAS_CAUGHT_FISH Current {id}";
                if (fishSeasons is not null)
                {
                    condition = $"{GameStateConditionProvider.CreateSeasonsCondition(fishSeasons)}, {condition}";
                }
                shopData.Items.Add(CreateBarterItem(blueObjects, fishItem, condition, offeredStock: stockCount, priceReduction: priceReduction));
            }
        }

        private void GenerateGreyJunimoStock(ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Grey"];
            var mineralObjects = _stardewItemManager.GetObjectsByType("Minerals");
            shopData.Items.Clear();
            var greyItems = _stardewItemManager.GetObjectsByColor("Grey");

            foreach (var rock in mineralObjects)
            {
                var itemHashCode = rock.GetHashCode();
                var mineralCondition = $"{GameStateCondition.FOUND_MINERAL} {rock.Id}, SYNCED_RANDOM day junimo_shops_{itemHashCode} 0.4 @addDailyLuck";
                shopData.Items.Add(CreateBarterItem(greyItems, rock, mineralCondition, offeredStock: stockCount, priceReduction: priceReduction));
            }
        }

        private void GenerateRedJunimoStock(ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Red"];
            var artifactObjects = _stardewItemManager.GetObjectsByType("Arch");
            shopData.Items.Clear();
            var redObjects = _stardewItemManager.GetObjectsByColor("Red");
            foreach (var artifact in artifactObjects)
            {
                var itemHashCode = artifact.GetHashCode();
                var artifactCondition = $"{GameStateCondition.FOUND_ARTIFACT} {artifact.Id}, SYNCED_RANDOM day junimo_shops_{itemHashCode} 0.4 @addDailyLuck";
                shopData.Items.Add(CreateBarterItem(redObjects, artifact, artifactCondition, offeredStock: stockCount, priceReduction: priceReduction));
            }
        }

        private void GenerateOrangeJunimoStock(ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Orange"];
            var orangeObjects = _stardewItemManager.GetObjectsByColor("Orange");
            foreach (var item in shopData.Items)
            {
                ReplaceCurrencyWithBarterGivenObjects(orangeObjects, item, offeredStock: stockCount, priceReduction: priceReduction);
            }
        }

        private void GeneratePurpleJunimoStock(ShopData shopData, int stockCount, double priceReduction)
        {

            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Purple"];
            var itemToKeep = shopData.Items.First(x => x.ItemId == "FlashShifter.StardewValleyExpandedCP_Super_Starfruit");
            shopData.Items.Clear();
            var purpleObjects = _stardewItemManager.GetObjectsByColor("Purple");
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectById(itemToKeep.Id), itemToKeep.Condition));
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectByName("Stardrop Tea"), overridePrice: 20000, offeredStock: stockCount, priceReduction: priceReduction));
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectByName("Dewdrop Berry"), overridePrice: 4000, offeredStock: stockCount, priceReduction: priceReduction));
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectByName("Qi Gem"), overridePrice: 10000, offeredStock: stockCount, priceReduction: priceReduction));
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectByName("Calico Egg"), overridePrice: 50, offeredStock: stockCount, priceReduction: priceReduction));
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectByName("Hardwood"), overridePrice: 500, offeredStock: stockCount, priceReduction: priceReduction));
            shopData.Items.Add(CreateBarterItem(purpleObjects, _stardewItemManager.GetObjectByName("Tea Set"), overridePrice: 50000, offeredStock: stockCount, priceReduction: priceReduction));
        }

        private void GenerateYellowJunimoStock(ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Yellow"];
            shopData.Items.Clear();
            var yellowObjects = _stardewItemManager.GetObjectsByColor("Yellow");
            AddSpringSeedsToYellowStock(yellowObjects, shopData, stockCount, priceReduction);
            AddSummerSeedsToYellowStock(yellowObjects, shopData, stockCount, priceReduction);
            AddFallSeedsToYellowStock(yellowObjects, shopData, stockCount, priceReduction);
            AddSaplingsToShop(yellowObjects, shopData, stockCount, priceReduction);
        }

        private int StockBasedOnApplesFriendship()
        {
            var applesHearts = 0;
            if (Game1.player.friendshipData.ContainsKey("Apples"))
            {
                applesHearts = Game1.player.friendshipData["Apples"].Points / 500;
            }
            return applesHearts + 1;
        }

        private double GiveApplesFriendshipDiscount()
        {
            var applesHearts = 0;
            if (Game1.player.friendshipData.ContainsKey("Apples"))
            {
                applesHearts = Game1.player.friendshipData["Apples"].Points / 250; // Get discount from being friends with Apples
            }
            var additionalDiscount = applesHearts * APPLES_DISCOUNT;
            return INITIAL_DISCOUNT + additionalDiscount;
        }

        private ShopItemData CreateJunimoSeedItem(List<StardewObject> yellowObjects, string qualifiedId, int stockCount, double priceReduction, string[] season = null)
        {
            var seedItemName = _stardewItemManager.GetItemByQualifiedId(qualifiedId).Name;
            var itemHashCode = seedItemName.GetHashCode();
            var condition = $"SYNCED_RANDOM day junimo_shops_{itemHashCode} 0.4 @addDailyLuck";
            ;
            if (season is not null)
            {
                condition = $"{GameStateConditionProvider.CreateSeasonsCondition(season)}, {condition}";
            }
            if (_archipelago.SlotData.Cropsanity == Cropsanity.Shuffled)
            {
                condition = $"{GameStateConditionProvider.CreateHasReceivedItemCondition(seedItemName)}, {condition}";
            }
            return CreateBarterItem(yellowObjects, _stardewItemManager.GetItemByQualifiedId(qualifiedId), condition, offeredStock: stockCount, priceReduction: priceReduction);
        }

        private void AddSpringSeedsToYellowStock(List<StardewObject> yellowObjects, ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.PARSNIP_SEEDS, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.BEAN_STARTER, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.CAULIFLOWER_SEEDS, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.POTATO_SEEDS, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.TULIP_BULB, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.KALE_SEEDS, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.JAZZ_SEEDS, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.GARLIC_SEEDS, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.RICE_SHOOT, stockCount, priceReduction, spring));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.RHUBARB_SEEDS, stockCount, priceReduction, spring));
        }

        private void AddSummerSeedsToYellowStock(List<StardewObject> yellowObjects, ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.MELON_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.TOMATO_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.BLUEBERRY_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.PEPPER_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.WHEAT_SEEDS, stockCount, priceReduction, summer_fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.RADISH_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.POPPY_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.SPANGLE_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.HOPS_STARTER, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.CORN_SEEDS, stockCount, priceReduction, summer_fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.SUNFLOWER_SEEDS, stockCount, priceReduction, summer_fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.RED_CABBAGE_SEEDS, stockCount, priceReduction, summer));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.STARFRUIT_SEEDS, stockCount, priceReduction, summer));
        }

        private void AddFallSeedsToYellowStock(List<StardewObject> yellowObjects, ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.PUMPKIN_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.EGGPLANT_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.BOK_CHOY_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.YAM_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.CRANBERRY_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.FAIRY_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.AMARANTH_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.GRAPE_STARTER, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.ARTICHOKE_SEEDS, stockCount, priceReduction, fall));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.BEET_SEEDS, stockCount, priceReduction, fall));
        }

        private void AddSaplingsToShop(List<StardewObject> yellowObjects, ShopData shopData, int stockCount, double priceReduction)
        {
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.CHERRY_SAPLING, stockCount, priceReduction));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.APRICOT_SAPLING, stockCount, priceReduction));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.ORANGE_SAPLING, stockCount, priceReduction));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.PEACH_SAPLING, stockCount, priceReduction));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.POMEGRANATE_SAPLING, stockCount, priceReduction));
            shopData.Items.Add(CreateJunimoSeedItem(yellowObjects, QualifiedItemIds.APPLE_SAPLING, stockCount, priceReduction));
        }
    }
}
