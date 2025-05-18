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

            SendMessageAndPlaySound(button.hoverText, $" {item.Name}!", "newArtifact", tierRolled);
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
            QualifiedItemIds.SAP, QualifiedItemIds.MIXED_SEEDS,
        };

        private static readonly string[] _tier2Items = {
            QualifiedItemIds.EARTH_CRYSTAL, QualifiedItemIds.QUARTZ, QualifiedItemIds.FIRE_QUARTZ,
            QualifiedItemIds.FROZEN_TEAR, 
            QualifiedItemIds.COAL, QualifiedItemIds.COPPER_ORE, QualifiedItemIds.IRON_ORE,
        };

        private static readonly string[] _tier3Items = {
            QualifiedItemIds.EMERALD, QualifiedItemIds.RUBY, QualifiedItemIds.JADE,
            QualifiedItemIds.AMETHYST, QualifiedItemIds.AQUAMARINE, QualifiedItemIds.TOPAZ,
            QualifiedItemIds.GOLD_ORE, QualifiedItemIds.IRIDIUM_ORE, QualifiedItemIds.RADIOACTIVE_ORE,
            QualifiedItemIds.COPPER_BAR, QualifiedItemIds.IRON_BAR,
        };

        private static readonly string[] _tier4Items = {
            QualifiedItemIds.DIAMOND, QualifiedItemIds.PRISMATIC_SHARD, QualifiedItemIds.LUCKY_PURPLE_SHORTS,
            QualifiedItemIds.GOLD_BAR, QualifiedItemIds.IRIDIUM_BAR, QualifiedItemIds.RADIOACTIVE_BAR,
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
