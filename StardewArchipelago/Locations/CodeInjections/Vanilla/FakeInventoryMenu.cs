// Decompiled with JetBrains decompiler
// Type: StardewValley.Menus.InventoryMenu
// Assembly: Stardew Valley, Version=1.6.15.24356, Culture=neutral, PublicKeyToken=null
// MVID: AA1F513A-94F0-4EF6-A35F-2D5B4A3721BF
// Assembly location: d:\programs\steam\steamapps\common\Stardew Valley\Stardew Valley.dll
// XML documentation location: d:\programs\steam\steamapps\common\Stardew Valley\Stardew Valley.xml

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class FakeInventoryMenu : IClickableMenu
    {
        public const int region_inventorySlot0 = 0;
        public const int region_inventorySlot1 = 1;
        public const int region_inventorySlot2 = 2;
        public const int region_inventorySlot3 = 3;
        public const int region_inventorySlot4 = 4;
        public const int region_inventorySlot5 = 5;
        public const int region_inventorySlot6 = 6;
        public const int region_inventorySlot7 = 7;
        public const int region_dropButton = 107;
        public const int region_inventoryArea = 9000;
        public string hoverText = "";
        public string hoverTitle = "";
        public string descriptionTitle = "";
        public string descriptionText = "";
        public List<ClickableComponent> inventory = new List<ClickableComponent>();
        protected Dictionary<int, double> _iconShakeTimer = new Dictionary<int, double>();
        public IList<Item> actualInventory;
        public InventoryMenu.highlightThisItem highlightMethod;
        public ItemGrabMenu.behaviorOnItemSelect onAddItem;
        public bool playerInventory;
        public bool drawSlots;
        public bool showGrayedOutSlots;
        public int capacity;
        public int rows;
        public int horizontalGap;
        public int verticalGap;
        public ClickableComponent dropItemInvisibleButton;
        public string moveItemSound = "dwop";

        public FakeInventoryMenu(
          int xPosition,
          int yPosition,
          bool playerInventory,
          IList<Item> actualInventory = null,
          InventoryMenu.highlightThisItem highlightMethod = null,
          int capacity = -1,
          int rows = 3,
          int horizontalGap = 0,
          int verticalGap = 0,
          bool drawSlots = true)
          : base(xPosition, yPosition, 64 * ((capacity == -1 ? 36 : capacity) / rows), 64 * rows + 16)
        {
            this.drawSlots = drawSlots;
            this.horizontalGap = horizontalGap;
            this.verticalGap = verticalGap;
            this.rows = rows;
            this.capacity = capacity == -1 ? 36 : capacity;
            this.playerInventory = playerInventory;
            this.actualInventory = actualInventory;
            if (actualInventory == null)
            {
                this.actualInventory = (IList<Item>)Game1.player.Items;
            }
            for (int index = 0; index < Game1.player.maxItems.Value; ++index)
            {
                if (Game1.player.Items.Count <= index)
                {
                    Game1.player.Items.Add((Item)null);
                }
            }
            for (int index = 0; index < this.capacity; ++index)
            {
                int num = !playerInventory ? (index >= this.capacity - this.capacity / rows ? -99998 : index + this.capacity / rows) : (index >= this.actualInventory.Count - this.capacity / rows ? (index >= this.actualInventory.Count - 3 || this.actualInventory.Count < 36 ? (index % 12 < 2 ? 102 : 101) : -99998) : index + this.capacity / rows);
                this.inventory.Add(new ClickableComponent(new Rectangle(xPosition + index % (this.capacity / rows) * 64 + horizontalGap * (index % (this.capacity / rows)), this.yPositionOnScreen + index / (this.capacity / rows) * (64 + verticalGap) + (index / (this.capacity / rows) - 1) * 4 - (index > this.capacity / rows || !playerInventory || verticalGap != 0 ? 0 : 12), 64, 64), index.ToString() ?? "")
                {
                    myID = index,
                    leftNeighborID = index % (this.capacity / rows) != 0 ? index - 1 : 107,
                    rightNeighborID = (index + 1) % (this.capacity / rows) != 0 ? index + 1 : 106,
                    downNeighborID = num,
                    upNeighborID = index < this.capacity / rows ? 12340 + index : index - this.capacity / rows,
                    region = 9000,
                    upNeighborImmutable = true,
                    downNeighborImmutable = true,
                    leftNeighborImmutable = true,
                    rightNeighborImmutable = true
                });
            }
            this.highlightMethod = highlightMethod;
            if (highlightMethod == null)
            {
                this.highlightMethod = new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems);
            }
            this.dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPosition - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, this.yPositionOnScreen - 12, 64, 64), "")
            {
                myID = playerInventory ? 107 : -500,
                rightNeighborID = 0
            };
            foreach (ClickableComponent clickableComponent in this.GetBorder(InventoryMenu.BorderSide.Top))
                clickableComponent.upNeighborImmutable = false;
            foreach (ClickableComponent clickableComponent in this.GetBorder(InventoryMenu.BorderSide.Bottom))
                clickableComponent.downNeighborImmutable = false;
            foreach (ClickableComponent clickableComponent in this.GetBorder(InventoryMenu.BorderSide.Left))
                clickableComponent.leftNeighborImmutable = false;
            foreach (ClickableComponent clickableComponent in this.GetBorder(InventoryMenu.BorderSide.Right))
                clickableComponent.rightNeighborImmutable = false;
        }

        public List<ClickableComponent> GetBorder(InventoryMenu.BorderSide side)
        {
            List<ClickableComponent> border = new List<ClickableComponent>();
            int num = this.capacity / this.rows;
            switch (side)
            {
                case InventoryMenu.BorderSide.Top:
                    for (int index = 0; index < this.inventory.Count; ++index)
                    {
                        if (index < num)
                        {
                            border.Add(this.inventory[index]);
                        }
                    }
                    break;
                case InventoryMenu.BorderSide.Left:
                    for (int index = 0; index < this.inventory.Count; ++index)
                    {
                        if (index % num == 0)
                        {
                            border.Add(this.inventory[index]);
                        }
                    }
                    break;
                case InventoryMenu.BorderSide.Right:
                    for (int index = 0; index < this.inventory.Count; ++index)
                    {
                        if (index % num == num - 1)
                        {
                            border.Add(this.inventory[index]);
                        }
                    }
                    break;
                case InventoryMenu.BorderSide.Bottom:
                    for (int index = 0; index < this.inventory.Count; ++index)
                    {
                        if (index >= this.actualInventory.Count - num)
                        {
                            border.Add(this.inventory[index]);
                        }
                    }
                    break;
            }
            return border;
        }

        public static bool highlightAllItems(Item i) => true;

        public static bool highlightNoItems(Item i) => false;

        public void SetPosition(int x, int y)
        {
            this.movePosition(-this.xPositionOnScreen, -this.yPositionOnScreen);
            this.movePosition(x, y);
        }

        public void movePosition(int x, int y)
        {
            this.xPositionOnScreen += x;
            this.yPositionOnScreen += y;
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                clickableComponent.bounds.X += x;
                clickableComponent.bounds.Y += y;
            }
            this.dropItemInvisibleButton.bounds.X += x;
            this.dropItemInvisibleButton.bounds.Y += y;
        }

        public void ShakeItem(Item item) => this.ShakeItem(this.actualInventory.IndexOf(item));

        public void ShakeItem(int index)
        {
            if (index < 0 || index >= this.inventory.Count)
            {
                return;
            }
            this._iconShakeTimer[index] = Game1.currentGameTime.TotalGameTime.TotalSeconds + 0.5;
        }

        public Item tryToAddItem(Item toPlace, string sound = "coin")
        {
            if (toPlace == null)
            {
                return (Item)null;
            }
            int stack = toPlace.Stack;
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                int int32 = Convert.ToInt32(clickableComponent.name);
                Item i = int32 < this.actualInventory.Count ? this.actualInventory[int32] : (Item)null;
                if (i != null && this.highlightMethod(i) && i.canStackWith((ISalable)toPlace))
                {
                    int amount = toPlace.Stack - i.addToStack(toPlace);
                    if (toPlace.ConsumeStack(amount) == null)
                    {
                        try
                        {
                            Game1.playSound(sound);
                            ItemGrabMenu.behaviorOnItemSelect onAddItem = this.onAddItem;
                            if (onAddItem != null)
                            {
                                onAddItem(toPlace, this.playerInventory ? Game1.player : (Farmer)null);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        return (Item)null;
                    }
                }
            }
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                int int32 = Convert.ToInt32(clickableComponent.name);
                Item obj = int32 < this.actualInventory.Count ? this.actualInventory[int32] : (Item)null;
                if (int32 < this.actualInventory.Count && obj == null)
                {
                    if (!string.IsNullOrEmpty(sound))
                    {
                        try
                        {
                            Game1.playSound(sound);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    return Utility.addItemToInventory(toPlace, int32, this.actualInventory, this.onAddItem);
                }
            }
            if (toPlace.Stack < stack)
            {
                Game1.playSound(sound);
            }
            return toPlace;
        }

        public int getInventoryPositionOfClick(int x, int y)
        {
            for (int index = 0; index < this.inventory.Count; ++index)
            {
                if (this.inventory[index] != null && this.inventory[index].bounds.Contains(x, y))
                {
                    return Convert.ToInt32(this.inventory[index].name);
                }
            }
            return -1;
        }

        public Item leftClick(int x, int y, Item toPlace, bool playSound = true)
        {
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                if (clickableComponent.containsPoint(x, y))
                {
                    int int32 = Convert.ToInt32(clickableComponent.name);
                    if (int32 < this.actualInventory.Count && (this.actualInventory[int32] == null || this.highlightMethod(this.actualInventory[int32]) || this.actualInventory[int32].canStackWith((ISalable)toPlace)))
                    {
                        if (this.actualInventory[int32] != null)
                        {
                            if (toPlace != null)
                            {
                                if (playSound)
                                {
                                    Game1.playSound("stoneStep");
                                }
                                return Utility.addItemToInventory(toPlace, int32, this.actualInventory, this.onAddItem);
                            }
                            if (playSound)
                            {
                                Game1.playSound(this.moveItemSound);
                            }
                            return Utility.removeItemFromInventory(int32, this.actualInventory);
                        }
                        if (toPlace != null)
                        {
                            if (playSound)
                            {
                                Game1.playSound("stoneStep");
                            }
                            return Utility.addItemToInventory(toPlace, int32, this.actualInventory, this.onAddItem);
                        }
                    }
                }
            }
            return toPlace;
        }

        public Vector2 snapToClickableComponent(int x, int y)
        {
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                if (clickableComponent.containsPoint(x, y))
                {
                    return new Vector2((float)clickableComponent.bounds.X, (float)clickableComponent.bounds.Y);
                }
            }
            return new Vector2((float)x, (float)y);
        }

        public Item getItemAt(int x, int y)
        {
            foreach (ClickableComponent c in this.inventory)
            {
                if (c.containsPoint(x, y))
                {
                    return this.getItemFromClickableComponent(c);
                }
            }
            return (Item)null;
        }

        public Item getItemFromClickableComponent(ClickableComponent c)
        {
            if (c != null)
            {
                int int32 = Convert.ToInt32(c.name);
                if (int32 < this.actualInventory.Count)
                {
                    return this.actualInventory[int32];
                }
            }
            return (Item)null;
        }

        public Item rightClick(
          int x,
          int y,
          Item toAddTo,
          bool playSound = true,
          bool onlyCheckToolAttachments = false)
        {
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                int int32 = Convert.ToInt32(clickableComponent.name);
                Item i = int32 < this.actualInventory.Count ? this.actualInventory[int32] : (Item)null;
                if (clickableComponent.containsPoint(x, y) && int32 < this.actualInventory.Count && (i == null || this.highlightMethod(i)) && i != null)
                {
                    if (i is Tool tool && (toAddTo == null || toAddTo is StardewValley.Object) && tool.canThisBeAttached((StardewValley.Object)toAddTo))
                    {
                        return (Item)tool.attach((StardewValley.Object)toAddTo);
                    }
                    if (onlyCheckToolAttachments)
                    {
                        return toAddTo;
                    }
                    if (toAddTo == null)
                    {
                        if (i.maximumStackSize() != -1)
                        {
                            if (int32 == Game1.player.CurrentToolIndex && i.Stack == 1)
                            {
                                i.actionWhenStopBeingHeld(Game1.player);
                            }
                            Item one = i.getOne();
                            Item obj = one;
                            int num;
                            if (i.Stack > 1)
                            {
                                if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[1]
                                {
                  new InputButton(Keys.LeftShift)
                                }))
                                {
                                    num = (int)Math.Ceiling((double)i.Stack / 2.0);
                                    goto label_15;
                                }
                            }
                            num = 1;
                        label_15:
                            obj.Stack = num;
                            this.actualInventory[int32] = i.ConsumeStack(one.Stack);
                            if (playSound)
                            {
                                Game1.playSound(this.moveItemSound);
                            }
                            return one;
                        }
                    }
                    else if (i.canStackWith((ISalable)toAddTo) && toAddTo.Stack < toAddTo.maximumStackSize())
                    {
                        if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[1]
                        {
              new InputButton(Keys.LeftShift)
                        }))
                        {
                            int val2 = (int)Math.Ceiling((double)i.Stack / 2.0);
                            int amount = Math.Min(toAddTo.maximumStackSize() - toAddTo.Stack, val2);
                            toAddTo.Stack += amount;
                            this.actualInventory[int32] = i.ConsumeStack(amount);
                        }
                        else
                        {
                            ++toAddTo.Stack;
                            this.actualInventory[int32] = i.ConsumeStack(1);
                        }
                        if (playSound)
                        {
                            Game1.playSound(this.moveItemSound);
                        }
                        if (this.actualInventory[int32] == null && int32 == Game1.player.CurrentToolIndex)
                        {
                            i.actionWhenStopBeingHeld(Game1.player);
                        }
                        return toAddTo;
                    }
                }
            }
            return toAddTo;
        }

        public Item hover(int x, int y, Item heldItem)
        {
            this.descriptionText = "";
            this.descriptionTitle = "";
            this.hoverText = "";
            this.hoverTitle = "";
            Item obj = (Item)null;
            foreach (ClickableComponent clickableComponent in this.inventory)
            {
                int int32 = Convert.ToInt32(clickableComponent.name);
                clickableComponent.scale = Math.Max(1f, clickableComponent.scale - 0.025f);
                if (clickableComponent.containsPoint(x, y) && int32 < this.actualInventory.Count && (this.actualInventory[int32] == null || this.highlightMethod(this.actualInventory[int32])) && int32 < this.actualInventory.Count && this.actualInventory[int32] != null)
                {
                    this.descriptionTitle = this.actualInventory[int32].DisplayName;
                    this.descriptionText = Environment.NewLine + this.actualInventory[int32].getDescription();
                    clickableComponent.scale = Math.Min(clickableComponent.scale + 0.05f, 1.1f);
                    string hoverBoxText = this.actualInventory[int32].getHoverBoxText(heldItem);
                    if (hoverBoxText != null)
                    {
                        this.hoverText = hoverBoxText;
                        this.hoverTitle = this.actualInventory[int32].DisplayName;
                    }
                    else
                    {
                        this.hoverText = this.actualInventory[int32].getDescription();
                        this.hoverTitle = this.actualInventory[int32].DisplayName;
                    }
                    if (obj == null)
                    {
                        obj = this.actualInventory[int32];
                    }
                }
            }
            if (obj is StardewValley.Object o && Game1.RequireLocation<CommunityCenter>("CommunityCenter").couldThisIngredienteBeUsedInABundle(o))
            {
                GameMenu.bundleItemHovered = true;
            }
            return obj;
        }

        public override void setUpForGamePadMode()
        {
            base.setUpForGamePadMode();
            List<ClickableComponent> inventory = this.inventory;
            // ISSUE: explicit non-virtual call
            //if ((inventory != null ? (__nonvirtual(inventory.Count) > 0 ? 1 : 0) : 0) == 0)
            //    return;
            Game1.setMousePosition(this.inventory[0].bounds.Right - this.inventory[0].bounds.Width / 8, this.inventory[0].bounds.Bottom - this.inventory[0].bounds.Height / 8);
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b) => this.draw(b, -1, -1, -1);

        /// <inheritdoc />
        public override void draw(SpriteBatch b, int red, int green, int blue)
        {
            for (int key = 0; key < this.inventory.Count; ++key)
            {
                double num;
                if (this._iconShakeTimer.TryGetValue(key, out num) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= num)
                {
                    this._iconShakeTimer.Remove(key);
                }
            }
            Color color = red == -1 ? Color.White : new Color((int)Utility.Lerp((float)red, (float)Math.Min((int)byte.MaxValue, red + 150), 0.65f), (int)Utility.Lerp((float)green, (float)Math.Min((int)byte.MaxValue, green + 150), 0.65f), (int)Utility.Lerp((float)blue, (float)Math.Min((int)byte.MaxValue, blue + 150), 0.65f));
            Texture2D texture = red == -1 ? Game1.menuTexture : Game1.uncoloredMenuTexture;
            if (this.drawSlots)
            {
                for (int index = 0; index < this.capacity; ++index)
                {
                    Vector2 position = new Vector2((float)(this.xPositionOnScreen + index % (this.capacity / this.rows) * 64 + this.horizontalGap * (index % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + index / (this.capacity / this.rows) * (64 + this.verticalGap) + (index / (this.capacity / this.rows) - 1) * 4 - (index >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0 ? 0 : 12)));
                    b.Draw(texture, position, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), color, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
                    if ((this.playerInventory || this.showGrayedOutSlots) && index >= Game1.player.maxItems.Value)
                    {
                        b.Draw(texture, position, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57)), color * 0.5f, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
                    }
                    if (!Game1.options.gamepadControls && index < 12 && this.playerInventory)
                    {
                        string str;
                        switch (index)
                        {
                            case 9:
                                str = "0";
                                break;
                            case 10:
                                str = "-";
                                break;
                            case 11:
                                str = "=";
                                break;
                            default:
                                str = (index + 1).ToString() ?? "";
                                break;
                        }
                        string text = str;
                        Vector2 vector2 = Game1.tinyFont.MeasureString(text);
                        b.DrawString(Game1.tinyFont, text, position + new Vector2((float)(32.0 - (double)vector2.X / 2.0), -vector2.Y), index == Game1.player.CurrentToolIndex ? Color.Red : Color.DimGray);
                    }
                }
                for (int index = 0; index < this.capacity; ++index)
                {
                    Vector2 location = new Vector2((float)(this.xPositionOnScreen + index % (this.capacity / this.rows) * 64 + this.horizontalGap * (index % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + index / (this.capacity / this.rows) * (64 + this.verticalGap) + (index / (this.capacity / this.rows) - 1) * 4 - (index >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0 ? 0 : 12)));
                    if (this.actualInventory.Count > index && this.actualInventory[index] != null)
                    {
                        bool drawShadow = this.highlightMethod(this.actualInventory[index]);
                        if (this._iconShakeTimer.ContainsKey(index))
                        {
                            location += 1f * new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
                        }
                        this.actualInventory[index].drawInMenu(b, location, this.inventory.Count > index ? this.inventory[index].scale : 1f, !this.highlightMethod(this.actualInventory[index]) ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, drawShadow);
                    }
                }
            }
            else
            {
                for (int index = 0; index < this.capacity; ++index)
                {
                    Vector2 location = new Vector2((float)(this.xPositionOnScreen + index % (this.capacity / this.rows) * 64 + this.horizontalGap * (index % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + index / (this.capacity / this.rows) * (64 + this.verticalGap) + (index / (this.capacity / this.rows) - 1) * 4 - (index >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0 ? 0 : 12)));
                    if (this.actualInventory.Count > index && this.actualInventory[index] != null)
                    {
                        bool drawShadow = this.highlightMethod(this.actualInventory[index]);
                        if (this._iconShakeTimer.ContainsKey(index))
                        {
                            location += 1f * new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
                        }
                        this.actualInventory[index].drawInMenu(b, location, this.inventory.Count > index ? this.inventory[index].scale : 1f, !drawShadow ? 0.25f : 1f, 0.865f, StackDrawType.Draw, Color.White, drawShadow);
                    }
                }
            }
        }

        public List<Vector2> GetSlotDrawPositions()
        {
            List<Vector2> slotDrawPositions = new List<Vector2>();
            for (int index = 0; index < this.capacity; ++index)
                slotDrawPositions.Add(new Vector2((float)(this.xPositionOnScreen + index % (this.capacity / this.rows) * 64 + this.horizontalGap * (index % (this.capacity / this.rows))), (float)(this.yPositionOnScreen + index / (this.capacity / this.rows) * (64 + this.verticalGap) + (index / (this.capacity / this.rows) - 1) * 4 - (index >= this.capacity / this.rows || !this.playerInventory || this.verticalGap != 0 ? 0 : 12))));
            return slotDrawPositions;
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
        }

        public delegate bool highlightThisItem(Item i);

        public enum BorderSide
        {
            Top,
            Left,
            Right,
            Bottom,
        }
    }
}
