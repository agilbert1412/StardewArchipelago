using StardewArchipelago.Constants.Vanilla;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class LingoHandler
    {
        private readonly ArchipelagoJunimoNoteMenu _archipelagoJunimoNoteMenu;
        private readonly ArchipelagoBundle _bundle;

        private List<ClickableTextureComponent> _slots;
        private List<string> _drawnItemIds;
        private List<LingoPuzzleType> _puzzleTypes;

        public LingoHandler(ArchipelagoJunimoNoteMenu archipelagoJunimoNoteMenu, ArchipelagoBundle bundle)
        {
            _archipelagoJunimoNoteMenu = archipelagoJunimoNoteMenu;
            _bundle = bundle;
        }

        public void UpdateIngredientSlots()
        {
            _slots = _archipelagoJunimoNoteMenu.IngredientSlots.ToList();
            _puzzleTypes = new List<LingoPuzzleType>();
            _drawnItemIds = new List<string>();

            for (var index = 0; index < _slots.Count; ++index)
            {
                var ingredient = _bundle.Ingredients[index];
                var ingredientBox = _archipelagoJunimoNoteMenu.IngredientList[index];
                var ingredientSlot = _slots[index];

                var puzzleType = GetLingoPuzzleType(ingredient);
                _puzzleTypes.Add(puzzleType);
                var height = GetLingoHeight(puzzleType);
                var color = GetLingoColor(puzzleType);
                var itemId = GetLingoItemIdToDraw(ingredient);
                _drawnItemIds.Add(itemId);

                SetupLingoIngredient(ingredientBox, puzzleType, height, color);
                SetupLingoIngredientSlot(ingredientSlot, puzzleType, height, color);
            }

            var depositedItems = new List<Item>();
            foreach (var ingredientSlot in _slots)
            {
                if (ingredientSlot.item != null)
                {
                    depositedItems.Add(ingredientSlot.item);
                    ingredientSlot.item = null;
                }
            }

            foreach (var depositedItem in depositedItems)
            {
                PutItemInCorrectSlot(depositedItem);
            }
        }

        private void PutItemInCorrectSlot(Item depositedItem)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var ingredientSlot = _slots[i];
                if (CanSlotAcceptItem(ingredientSlot, depositedItem))
                {
                    ingredientSlot.item = depositedItem;
                    ingredientSlot.visible = true;
                    return;
                }
            }

            throw new Exception($"Deposited item {depositedItem.Name} does not fit in any slot");
        }

        private void SetupLingoIngredient(ClickableTextureComponent ingredientBox, LingoPuzzleType puzzleType, int height, Color color)
        {
            ingredientBox.bounds.Y = _archipelagoJunimoNoteMenu.yPositionOnScreen + JunimoNoteMenuRemake.INGREDIENT_SLOTS_CENTER_Y - height;
            ingredientBox.texture = _archipelagoJunimoNoteMenu.MemeTexture;
            ingredientBox.sourceRect = new Rectangle(1, 231, 16, 24);
        }

        private void SetupLingoIngredientSlot(ClickableTextureComponent ingredientSlot, LingoPuzzleType puzzleType, int height, Color color)
        {
            var highlightOffset = 11;
            ingredientSlot.bounds.X += 4;
            ingredientSlot.bounds.Y = _archipelagoJunimoNoteMenu.yPositionOnScreen + JunimoNoteMenuRemake.INGREDIENT_SLOTS_CENTER_Y - height + (highlightOffset * 4) + 4;
            //ingredientSlot.bounds.Width -= 2;
            //ingredientSlot.bounds.Height -= 2;
            ingredientSlot.texture = _archipelagoJunimoNoteMenu.MemeTexture;
            ingredientSlot.sourceRect = new Rectangle(110, 232 + highlightOffset, 14, 22 - highlightOffset);
            ingredientSlot.visible = false;
        }

        public Item GetLingoItemToDraw(BundleIngredientDescription ingredient)
        {
            var itemId = GetLingoItemIdToDraw(ingredient);
            return GetLingoItemToDraw(itemId);
        }

        public Item GetLingoItemToDraw(string lingoItemId)
        {
            if (lingoItemId == null)
            {
                return null;
            }
            return ItemRegistry.Create(lingoItemId);
        }

        public string GetLingoItemIdToDraw(BundleIngredientDescription ingredient)
        {
            var lingoHintMappings = new Dictionary<string, string>()
            {
                { QualifiedItemIds.BAIT, null },
                { QualifiedItemIds.SHORTS, QualifiedItemIds.SHIRT },
                { QualifiedItemIds.SHIRT, QualifiedItemIds.SHORTS },
                { QualifiedItemIds.ACORN, QualifiedItemIds.CORN },
                { QualifiedItemIds.CORN, QualifiedItemIds.ACORN },
                { QualifiedItemIds.CORAL, QualifiedItemIds.COAL },
                { QualifiedItemIds.COAL, QualifiedItemIds.CORAL },
                { QualifiedItemIds.ANCIENT_SEEDS, QualifiedItemIds.ANCIENT_SEED },
                { QualifiedItemIds.ANCIENT_SEED, QualifiedItemIds.ANCIENT_SEEDS },
                { QualifiedItemIds.BLUEBERRY, QualifiedItemIds.BLUEBERRY_TART },
                { QualifiedItemIds.BLACKBERRY, QualifiedItemIds.BLACKBERRY_COBBLER },
                { QualifiedItemIds.CARP, QualifiedItemIds.CARP_SURPRISE },
                { QualifiedItemIds.SALMON, QualifiedItemIds.SALMON_DINNER },
                { QualifiedItemIds.RHUBARB, QualifiedItemIds.RHUBARB_PIE },
                { QualifiedItemIds.EEL, QualifiedItemIds.SPICY_EEL },
                { QualifiedItemIds.RICE, QualifiedItemIds.RICE_PUDDING },
                { QualifiedItemIds.PUMPKIN, QualifiedItemIds.PUMPKIN_PIE },
                { QualifiedItemIds.CRANBERRIES, QualifiedItemIds.CRANBERRY_CANDY },
                { QualifiedItemIds.SHRIMP, QualifiedItemIds.SHRIMP_COCKTAIL },
                { QualifiedItemIds.COPPER_BAR, QualifiedItemIds.COPPER_ORE },
                { QualifiedItemIds.IRON_BAR, QualifiedItemIds.IRON_ORE },
                { QualifiedItemIds.GOLD_BAR, QualifiedItemIds.GOLD_ORE },
                { QualifiedItemIds.IRIDIUM_BAR, QualifiedItemIds.IRIDIUM_ORE },
                { QualifiedItemIds.RADIOACTIVE_BAR, QualifiedItemIds.RADIOACTIVE_ORE },
            };

            if (lingoHintMappings.ContainsKey(ingredient.id))
            {
                return lingoHintMappings[ingredient.id];
            }
            return ingredient.id;
        }

        public LingoPuzzleType GetLingoPuzzleType(BundleIngredientDescription ingredient)
        {
            var homophoneIds = new[] { QualifiedItemIds.SHORTS, QualifiedItemIds.SHIRT };
            var addIds = new[] { QualifiedItemIds.ACORN, QualifiedItemIds.CORAL, QualifiedItemIds.ANCIENT_SEEDS, QualifiedItemIds.BAIT };
            var subtractIds = new[] { QualifiedItemIds.CORN, QualifiedItemIds.COAL, QualifiedItemIds.ANCIENT_SEED };
            var lesserIds = new[]
            {
                QualifiedItemIds.BLUEBERRY, QualifiedItemIds.BLACKBERRY, QualifiedItemIds.CARP, QualifiedItemIds.SALMON, QualifiedItemIds.RHUBARB,
                QualifiedItemIds.EEL, QualifiedItemIds.RICE, QualifiedItemIds.PUMPKIN, QualifiedItemIds.CRANBERRIES, QualifiedItemIds.SHRIMP
            };
            var greaterIds = new[] { QualifiedItemIds.COPPER_BAR, QualifiedItemIds.IRON_BAR, QualifiedItemIds.GOLD_BAR, QualifiedItemIds.IRIDIUM_BAR, QualifiedItemIds.RADIOACTIVE_BAR };

            if (homophoneIds.Contains(ingredient.id))
            {
                return LingoPuzzleType.Homophone;
            }

            if (subtractIds.Contains(ingredient.id))
            {
                return LingoPuzzleType.SubtractLetter;
            }

            if (addIds.Contains(ingredient.id))
            {
                return LingoPuzzleType.AddLetter;
            }

            if (lesserIds.Contains(ingredient.id))
            {
                return LingoPuzzleType.Lesser;
            }

            if (greaterIds.Contains(ingredient.id))
            {
                return LingoPuzzleType.Greater;
            }

            return LingoPuzzleType.Normal;
        }

        public int GetLingoHeight(LingoPuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case LingoPuzzleType.Homophone:
                    return 240; //Top
                case LingoPuzzleType.Normal:
                case LingoPuzzleType.AddLetter:
                case LingoPuzzleType.SubtractLetter:
                    return 132; //Middle
                case LingoPuzzleType.Lesser:
                case LingoPuzzleType.Greater:
                    return 24; //Bottom
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        public Color GetLingoColor(LingoPuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case LingoPuzzleType.Normal:
                case LingoPuzzleType.Homophone:
                    return Color.White;
                case LingoPuzzleType.AddLetter:
                case LingoPuzzleType.Greater:
                    return Color.Blue;
                case LingoPuzzleType.SubtractLetter:
                case LingoPuzzleType.Lesser:
                    return Color.Red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        public bool CanSlotAcceptItem(ClickableTextureComponent slot, Item item)
        {
            var slotIndex = _slots.IndexOf(slot);
            var puzzleType = _puzzleTypes[slotIndex];
            var drawnItemId = _drawnItemIds[slotIndex];
            var drawnItem = ItemRegistry.Create(drawnItemId);
            var drawnItemName = drawnItem.Name;
            var ingredientDescription = _bundle.Ingredients[slotIndex];
            var itemName = item.Name;
            var ingredient = ItemRegistry.Create(ingredientDescription.id);
            var ingredientName = ingredient.Name;

            switch (puzzleType)
            {

                case LingoPuzzleType.Normal:
                    return item.QualifiedItemId == drawnItemId;
                case LingoPuzzleType.Homophone:
                    return item.QualifiedItemId == ingredientDescription.id;
                case LingoPuzzleType.AddLetter:
                    return IsSameWithExtraLetter(itemName, drawnItemName);
                case LingoPuzzleType.SubtractLetter:
                    return IsSameWithExtraLetter(drawnItemName, itemName);
                case LingoPuzzleType.Lesser:
                    return item.QualifiedItemId == ingredientDescription.id;
                case LingoPuzzleType.Greater:
                    return item.QualifiedItemId == ingredientDescription.id;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsSameWithExtraLetter(string longerName, string shorterName)
        {
            if (longerName.Length != shorterName.Length + 1)
            {
                return false;
            }

            for (var i = 0; i < longerName.Length; i++)
            {
                var longerNameWithoutChar = longerName.Substring(0, i) + longerName.Substring(i + 1);
                if (longerNameWithoutChar.Equals(shorterName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum LingoPuzzleType
    {
        Normal,
        Homophone,
        AddLetter,
        SubtractLetter,
        Lesser,
        Greater,
    }
}
