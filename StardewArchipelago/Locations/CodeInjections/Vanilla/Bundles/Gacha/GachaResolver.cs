using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes;
using StardewValley;
using StardewValley.Delegates;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Gacha
{
    public class GachaResolver
    {
        private static readonly Color MESSAGE_COLOR = Color.Green;

        private readonly Random _random;
        private GachaRoller _roller;

        public GachaResolver(int stack)
        {
            _random = new Random();
            _roller = new GachaRoller(stack, _random);
        }

        public void PressButton(BundleButton button, int price, ArchipelagoJunimoNoteMenu menu)
        {
            if (Game1.player.Money < price)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            Game1.player.Money -= price;

            button.StartAnimation(() => RollAndGetItem(button, price, menu));
        }

        public void RollAndGetItem(BundleButton button, int price, ArchipelagoJunimoNoteMenu menu)
        {
            var tierRolled = _roller.RollTier(price);
            if (tierRolled >= 5)
            {
                SendMessageAndPlaySound(button.hoverText, $"Bundle Completion!", "discoverMineral", tierRolled);
                menu.PerformCurrencyPurchase();
                return;
            }

            var item = ResolveItem(tierRolled);
            if (item == null)
            {
                SendMessageAndPlaySound(button.hoverText, $" Nothing...", "newArtifact", tierRolled);
                return;
            }

            SendMessageAndPlaySound(button.hoverText, $" {item.Name}!", tierRolled >= 4 ? "discoverMineral" : "newArtifact", tierRolled);
            var notAdded = Game1.player.addItemToInventory(item);
            if (notAdded != null)
            {
                Game1.createItemDebris(notAdded, Game1.player.Position, 0, Game1.currentLocation);
            }
        }

        private static void SendMessageAndPlaySound(string chest, string messageResult, string sound, int rewardTier)
        {
            var message = $"Opened {chest}: {messageResult} [Tier {rewardTier}]";
            Game1.chatBox.addMessage(message, MESSAGE_COLOR);
            if (!string.IsNullOrEmpty(sound))
            {
                Game1.playSound(sound);
            }
        }

        public Item ResolveItem(int tier)
        {
            var itemsInTier = _itemsByTier[tier];
            if (!itemsInTier.Any())
            {
                return null;
            }

            var chosenItemId = itemsInTier[_random.Next(itemsInTier.Length)];
            return ItemRegistry.Create(chosenItemId);
        }

        private static readonly string[] _tier0Items = {
            QualifiedItemIds.TRASH, QualifiedItemIds.JOJA_COLA, QualifiedItemIds.DRIFTWOOD,
            QualifiedItemIds.BROKEN_CD, QualifiedItemIds.BROKEN_GLASSES, QualifiedItemIds.SOGGY_NEWSPAPER,
        };

        private static readonly string[] _tier1Items = {
            QualifiedItemIds.WOOD, QualifiedItemIds.STONE, QualifiedItemIds.FIBER,
            QualifiedItemIds.SAP, QualifiedItemIds.MIXED_SEEDS, QualifiedItemIds.TORCH,
            QualifiedItemIds.SUGAR, QualifiedItemIds.WHEAT_FLOUR, QualifiedItemIds.VINEGAR,
            QualifiedItemIds.OIL, QualifiedItemIds.RICE,
            QualifiedItemIds.GEODE, QualifiedItemIds.FROZEN_GEODE, QualifiedItemIds.MAGMA_GEODE,
            QualifiedItemIds.GRASS_STARTER, QualifiedItemIds.ACORN, QualifiedItemIds.MAPLE_SEED, QualifiedItemIds.PINE_CONE,
            QualifiedItemIds.CLAY, QualifiedItemIds.BONE_FRAGMENT,
            QualifiedItemIds.BUG_MEAT, QualifiedItemIds.BAIT,

        };

        private static readonly string[] _tier2Items = {
            QualifiedItemIds.EARTH_CRYSTAL, QualifiedItemIds.QUARTZ, QualifiedItemIds.FIRE_QUARTZ,
            QualifiedItemIds.FROZEN_TEAR, QualifiedItemIds.REFINED_QUARTZ,
            QualifiedItemIds.COAL, QualifiedItemIds.COPPER_ORE, QualifiedItemIds.IRON_ORE,
            QualifiedItemIds.SEAWEED, QualifiedItemIds.GREEN_ALGAE, QualifiedItemIds.WHITE_ALGAE,
            QualifiedItemIds.HAY, 
            QualifiedItemIds.SALAD, QualifiedItemIds.PIZZA, QualifiedItemIds.PINK_CAKE,
            QualifiedItemIds.CHOCOLATE_CAKE, QualifiedItemIds.BREAD, QualifiedItemIds.COOKIES,
            QualifiedItemIds.SPAGHETTI, QualifiedItemIds.SASHIMI, QualifiedItemIds.TORTILLA,
            QualifiedItemIds.COFFEE, QualifiedItemIds.OMNI_GEODE, QualifiedItemIds.CHERRY_BOMB,
            QualifiedItemIds.MAHOGANY_SEED,
            QualifiedItemIds.BASIC_FERTILIZER, QualifiedItemIds.QUALITY_FERTILIZER,
            QualifiedItemIds.BASIC_RETAINING_SOIL, QualifiedItemIds.QUALITY_RETAINING_SOIL,
            QualifiedItemIds.SPEED_GRO, QualifiedItemIds.DELUXE_SPEED_GRO, QualifiedItemIds.SPRINKLER,
            QualifiedItemIds.RAIN_TOTEM, QualifiedItemIds.MYSTERY_BOX, QualifiedItemIds.QI_SEASONING,
            QualifiedItemIds.CHALLENGE_BAIT, QualifiedItemIds.QI_BEAN, QualifiedItemIds.QI_FRUIT, QualifiedItemIds.FIBER_SEEDS
        };

        private static readonly string[] _tier3Items = {
            QualifiedItemIds.EMERALD, QualifiedItemIds.RUBY, QualifiedItemIds.JADE,
            QualifiedItemIds.AMETHYST, QualifiedItemIds.AQUAMARINE, QualifiedItemIds.TOPAZ,
            QualifiedItemIds.GOLD_ORE, QualifiedItemIds.IRIDIUM_ORE, QualifiedItemIds.RADIOACTIVE_ORE,
            QualifiedItemIds.COPPER_BAR, QualifiedItemIds.IRON_BAR,
            QualifiedItemIds.COMPLETE_BREAKFAST, QualifiedItemIds.TROUT_SOUP, QualifiedItemIds.RED_PLATE,
            QualifiedItemIds.FRIED_MUSHROOM, QualifiedItemIds.MAPLE_BAR, QualifiedItemIds.PEPPER_POPPERS,
            QualifiedItemIds.MINERS_TREAT, QualifiedItemIds.TRIPLE_SHOT_ESPRESSO,
            QualifiedItemIds.WARP_TOTEM_MOUNTAINS, QualifiedItemIds.WARP_TOTEM_BEACH, QualifiedItemIds.WARP_TOTEM_FARM,
            QualifiedItemIds.ARTIFACT_TROVE, QualifiedItemIds.BOMB, QualifiedItemIds.MOSSY_SEED, QualifiedItemIds.MUSHROOM_TREE_SEED,
            QualifiedItemIds.ENERGY_TONIC, QualifiedItemIds.MUSCLE_REMEDY,
            QualifiedItemIds.DELUXE_FERTILIZER, QualifiedItemIds.DELUXE_RETAINING_SOIL, QualifiedItemIds.HYPER_SPEED_GRO,
            QualifiedItemIds.EXPLOSIVE_AMMO, QualifiedItemIds.QUALITY_SPRINKLER, QualifiedItemIds.GOLDEN_MYSTERY_BOX,
            QualifiedItemIds.DELUXE_BAIT, QualifiedItemIds.MAGIC_BAIT,

        };

        private static readonly string[] _tier4Items = {
            QualifiedItemIds.DIAMOND, QualifiedItemIds.PRISMATIC_SHARD, QualifiedItemIds.LUCKY_PURPLE_SHORTS,
            QualifiedItemIds.GOLD_BAR, QualifiedItemIds.IRIDIUM_BAR, QualifiedItemIds.RADIOACTIVE_BAR,
            QualifiedItemIds.DINOSAUR_EGG, QualifiedItemIds.TREASURE_CHEST,
            QualifiedItemIds.LUCKY_LUNCH, QualifiedItemIds.SPICY_EEL, QualifiedItemIds.SEAFOAM_PUDDING,
            QualifiedItemIds.ROOTS_PLATTER, QualifiedItemIds.PUMPKIN_SOUP, QualifiedItemIds.TROPICAL_CURRY,
            QualifiedItemIds.MAGIC_ROCK_CANDY, QualifiedItemIds.BANANA_PUDDING,
            QualifiedItemIds.WARP_TOTEM_DESERT, QualifiedItemIds.WARP_TOTEM_ISLAND,
            QualifiedItemIds.MEGA_BOMB, QualifiedItemIds.MYSTIC_TREE_SEED, 
            QualifiedItemIds.TEA_SET, QualifiedItemIds.GOLDEN_PUMPKIN, QualifiedItemIds.PEARL,
            QualifiedItemIds.IRIDIUM_SPRINKLER, QualifiedItemIds.PRIZE_TICKET, QualifiedItemIds.GALAXY_SOUL,
        };

        private Dictionary<int, string[]> _itemsByTier = new()
        {
            {0, _tier0Items},
            { 1, _tier1Items },
            { 2, _tier2Items },
            { 3, _tier3Items },
            { 4, _tier4Items },
        };
    }
}
