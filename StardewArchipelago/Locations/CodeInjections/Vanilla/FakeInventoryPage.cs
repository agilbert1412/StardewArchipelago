// Decompiled with JetBrains decompiler
// Type: StardewValley.Menus.InventoryPage
// Assembly: Stardew Valley, Version=1.6.15.24356, Culture=neutral, PublicKeyToken=null
// MVID: AA1F513A-94F0-4EF6-A35F-2D5B4A3721BF
// Assembly location: d:\programs\steam\steamapps\common\Stardew Valley\Stardew Valley.dll
// XML documentation location: d:\programs\steam\steamapps\common\Stardew Valley\Stardew Valley.xml

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class FakeInventoryPage : IClickableMenu
    {
        public const int region_inventory = 100;
        public const int region_hat = 101;
        public const int region_ring1 = 102;
        public const int region_ring2 = 103;
        public const int region_boots = 104;
        public const int region_trashCan = 105;
        public const int region_organizeButton = 106;
        public const int region_accessory = 107;
        public const int region_shirt = 108;
        public const int region_pants = 109;
        public const int region_shoes = 110;
        public const int region_trinkets = 120;
        public FakeInventoryMenu inventory;
        public string hoverText = "";
        public string hoverTitle = "";
        public int hoverAmount;
        public Item hoveredItem;
        public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();
        public ClickableComponent portrait;
        public ClickableTextureComponent trashCan;
        public ClickableTextureComponent organizeButton;
        private float trashCanLidRotation;
        public ClickableTextureComponent junimoNoteIcon;
        private int junimoNotePulser;
        protected Pet _pet;
        protected Horse _horse;

        public FakeInventoryPage(int x, int y, int width, int height)
          : base(x, y, width, height)
        {
            this.inventory = new FakeInventoryMenu(this.xPositionOnScreen + spaceToClearSideBorder + borderWidth, this.yPositionOnScreen + spaceToClearTopBorder + borderWidth, true);
            var num1 = Game1.player.stats.Get("trinketSlots") > 0U ? 1 : 0;
            var num2 = num1 != 0 ? 120 : 105;
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Left Ring")
            {
                myID = 102,
                downNeighborID = 103,
                upNeighborID = Game1.player.MaxItems - 12,
                rightNeighborID = 101,
                fullyImmutable = false
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Right Ring")
            {
                myID = 103,
                upNeighborID = 102,
                downNeighborID = 104,
                rightNeighborID = 108,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Boots")
            {
                myID = 104,
                upNeighborID = 103,
                rightNeighborID = 109,
                fullyImmutable = true
            });
            this.portrait = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 192 - 8 - 64 + 32, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256 - 8 + 64, 64, 96), "32");
            var textureComponent1 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width / 3 + 576 + 32, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 192 + 64, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f);
            textureComponent1.myID = 105;
            textureComponent1.upNeighborID = 106;
            textureComponent1.leftNeighborID = 101;
            this.trashCan = textureComponent1;
            var textureComponent2 = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + width, this.yPositionOnScreen + height / 3 - 64 + 8, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f);
            textureComponent2.myID = 106;
            textureComponent2.downNeighborID = 105;
            textureComponent2.leftNeighborID = 11;
            textureComponent2.upNeighborID = 898;
            this.organizeButton = textureComponent2;
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 208, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Hat")
            {
                myID = 101,
                leftNeighborID = 102,
                downNeighborID = 108,
                upNeighborID = Game1.player.MaxItems - 9,
                rightNeighborID = num2,
                fullyImmutable = false
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 208, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Shirt")
            {
                myID = 108,
                upNeighborID = 101,
                downNeighborID = 109,
                rightNeighborID = num2,
                leftNeighborID = 103,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 208, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Pants")
            {
                myID = 109,
                upNeighborID = 108,
                rightNeighborID = num2,
                leftNeighborID = 104,
                fullyImmutable = true
            });
            if (num1 != 0)
            {
                Farmer.MaximumTrinkets = 1;
                for (var index = 0; index < Farmer.MaximumTrinkets; ++index)
                {
                    var clickableComponent = new ClickableComponent(new Rectangle(this.xPositionOnScreen + 48 + 280, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 4 + (4 + index) * 64 - 12, 64, 64), "Trinket")
                    {
                        myID = 120 + index,
                        upNeighborID = Game1.player.MaxItems - 8,
                        rightNeighborID = 105,
                        leftNeighborID = -99998,
                        fullyImmutable = true
                    };
                    if (index < Farmer.MaximumTrinkets - 1)
                    {
                        clickableComponent.downNeighborID = -99998;
                    }
                    this.equipmentIcons.Add(clickableComponent);
                }
            }
            if (ShouldShowJunimoNoteIcon())
            {
                var textureComponent3 = new ClickableTextureComponent("", new Rectangle(this.xPositionOnScreen + width, this.yPositionOnScreen + 96, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f);
                textureComponent3.myID = 898;
                textureComponent3.leftNeighborID = 11;
                textureComponent3.downNeighborID = 106;
                this.junimoNoteIcon = textureComponent3;
            }
            this._pet = Game1.GetCharacterOfType<Pet>();
            this._horse = Game1.getCharacterFromName<Horse>(Game1.player.horseName.Value, false);
            if (this._horse != null || !Game1.player.isRidingHorse() || !Game1.player.mount.Name.Equals(Game1.player.horseName.Value))
            {
                return;
            }
            this._horse = Game1.player.mount;
        }

        public static bool ShouldShowJunimoNoteIcon()
        {
            if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || Game1.player.hasOrWillReceiveMail(JojaConstants.MEMBERSHIP_MAIL))
            {
                return false;
            }
            if (!Game1.MasterPlayer.hasCompletedCommunityCenter())
            {
                return true;
            }
            return Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater");
        }

        protected virtual bool checkHeldItem(Func<Item, bool> f = null) => f == null ? Game1.player.CursorSlotItem != null : f(Game1.player.CursorSlotItem);

        protected virtual Item takeHeldItem()
        {
            var cursorSlotItem = Game1.player.CursorSlotItem;
            Game1.player.CursorSlotItem = (Item)null;
            return cursorSlotItem;
        }

        protected virtual void setHeldItem(Item item)
        {
            item?.onDetachedFromParent();
            Game1.player.CursorSlotItem = item;
        }

        /// <inheritdoc />
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (Game1.isAnyGamePadButtonBeingPressed() && Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.checkHeldItem())
            {
                Game1.setMousePosition(this.trashCan.bounds.Center);
            }
            if (key == Keys.Delete && this.checkHeldItem((Func<Item, bool>)(i => i != null && i.canBeTrashed())))
            {
                Utility.trashItem(this.takeHeldItem());
            }
            if (Game1.options.doesInputListContain(Game1.options.inventorySlot1, key))
            {
                Game1.player.CurrentToolIndex = 0;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot2, key))
            {
                Game1.player.CurrentToolIndex = 1;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot3, key))
            {
                Game1.player.CurrentToolIndex = 2;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot4, key))
            {
                Game1.player.CurrentToolIndex = 3;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot5, key))
            {
                Game1.player.CurrentToolIndex = 4;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot6, key))
            {
                Game1.player.CurrentToolIndex = 5;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot7, key))
            {
                Game1.player.CurrentToolIndex = 6;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot8, key))
            {
                Game1.player.CurrentToolIndex = 7;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot9, key))
            {
                Game1.player.CurrentToolIndex = 8;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot10, key))
            {
                Game1.player.CurrentToolIndex = 9;
                Game1.playSound("toolSwap");
            }
            else if (Game1.options.doesInputListContain(Game1.options.inventorySlot11, key))
            {
                Game1.player.CurrentToolIndex = 10;
                Game1.playSound("toolSwap");
            }
            else
            {
                if (!Game1.options.doesInputListContain(Game1.options.inventorySlot12, key))
                {
                    return;
                }
                Game1.player.CurrentToolIndex = 11;
                Game1.playSound("toolSwap");
            }
        }

        public override void setUpForGamePadMode()
        {
            base.setUpForGamePadMode();
            this.inventory?.setUpForGamePadMode();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (var equipmentIcon in this.equipmentIcons)
            {
                if (equipmentIcon.containsPoint(x, y))
                {
                    var newItem = Utility.PerformSpecialItemPlaceReplacement(Game1.player.CursorSlotItem);
                    var flag = newItem == null;
                    var name = equipmentIcon.name;
                    if (name != null)
                    {
                        switch (name.Length)
                        {
                            case 3:
                                if (name == "Hat" && (newItem == null || newItem is Hat))
                                {
                                    this.setHeldItem(Utility.PerformSpecialItemGrabReplacement((Item)Game1.player.Equip<Hat>((Hat)newItem, Game1.player.hat)));
                                    if (Game1.player.hat.Value != null)
                                    {
                                        Game1.playSound("grassyStep");
                                        goto label_43;
                                    }
                                    else if (this.checkHeldItem())
                                    {
                                        Game1.playSound("dwop");
                                        goto label_43;
                                    }
                                    else
                                    {
                                        goto label_43;
                                    }
                                }
                                else
                                {
                                    goto label_43;
                                }
                            case 5:
                                switch (name[0])
                                {
                                    case 'B':
                                        if (name == "Boots" && (newItem == null || newItem is Boots))
                                        {
                                            this.setHeldItem(Utility.PerformSpecialItemGrabReplacement((Item)Game1.player.Equip<Boots>((Boots)newItem, Game1.player.boots)));
                                            if (Game1.player.boots.Value != null)
                                            {
                                                Game1.playSound("sandyStep");
                                                DelayedAction.playSoundAfterDelay("sandyStep", 150);
                                                goto label_43;
                                            }
                                            else if (this.checkHeldItem())
                                            {
                                                Game1.playSound("dwop");
                                                goto label_43;
                                            }
                                            else
                                            {
                                                goto label_43;
                                            }
                                        }
                                        else
                                        {
                                            goto label_43;
                                        }
                                    case 'P':
                                        if (name == "Pants" && (newItem == null || (newItem is Clothing clothing1 ? (clothing1.clothesType.Value == Clothing.ClothesType.PANTS ? 1 : 0) : 0) != 0))
                                        {
                                            this.setHeldItem(Utility.PerformSpecialItemGrabReplacement((Item)Game1.player.Equip<Clothing>((Clothing)newItem, Game1.player.pantsItem)));
                                            if (Game1.player.pantsItem.Value != null)
                                            {
                                                Game1.playSound("sandyStep");
                                                goto label_43;
                                            }
                                            else if (this.checkHeldItem())
                                            {
                                                Game1.playSound("dwop");
                                                goto label_43;
                                            }
                                            else
                                            {
                                                goto label_43;
                                            }
                                        }
                                        else
                                        {
                                            goto label_43;
                                        }
                                    case 'S':
                                        if (name == "Shirt" && (newItem == null || (newItem is Clothing clothing2 ? (clothing2.clothesType.Value == Clothing.ClothesType.SHIRT ? 1 : 0) : 0) != 0))
                                        {
                                            this.setHeldItem(Utility.PerformSpecialItemGrabReplacement((Item)Game1.player.Equip<Clothing>((Clothing)newItem, Game1.player.shirtItem)));
                                            if (Game1.player.shirtItem.Value != null)
                                            {
                                                Game1.playSound("sandyStep");
                                                goto label_43;
                                            }
                                            else if (this.checkHeldItem())
                                            {
                                                Game1.playSound("dwop");
                                                goto label_43;
                                            }
                                            else
                                            {
                                                goto label_43;
                                            }
                                        }
                                        else
                                        {
                                            goto label_43;
                                        }
                                    default:
                                        goto label_43;
                                }
                            case 7:
                                if (name == "Trinket" && Game1.player.stats.Get("trinketSlots") > 0U && this.checkHeldItem((Func<Item, bool>)(i => i == null || i is Trinket)))
                                {
                                    var index = equipmentIcon.myID - 120;
                                    var heldItem1 = (Trinket)this.takeHeldItem();
                                    var heldItem2 = (Trinket)null;
                                    if (Game1.player.trinketItems.Count > index)
                                    {
                                        heldItem2 = Game1.player.trinketItems[index];
                                    }
                                    this.setHeldItem(Utility.PerformSpecialItemGrabReplacement((Item)heldItem2));
                                    while (Game1.player.trinketItems.Count <= index)
                                        Game1.player.trinketItems.Add((Trinket)null);
                                    Game1.player.trinketItems[index] = heldItem1;
                                    if (Game1.player.trinketItems[index] != null)
                                    {
                                        Game1.playSound("clank");
                                        goto label_43;
                                    }
                                    else if (this.checkHeldItem())
                                    {
                                        Game1.playSound("dwop");
                                        goto label_43;
                                    }
                                    else
                                    {
                                        goto label_43;
                                    }
                                }
                                else
                                {
                                    goto label_43;
                                }
                            case 9:
                                if (name == "Left Ring")
                                {
                                    break;
                                }
                                goto label_43;
                            case 10:
                                if (name == "Right Ring")
                                {
                                    break;
                                }
                                goto label_43;
                            default:
                                goto label_43;
                        }
                        if (newItem == null || newItem is Ring)
                        {
                            var slot = equipmentIcon.name == "Left Ring" ? Game1.player.leftRing : Game1.player.rightRing;
                            this.setHeldItem(Utility.PerformSpecialItemGrabReplacement((Item)Game1.player.Equip<Ring>((Ring)newItem, slot)));
                            if (Game1.player.leftRing.Value != null)
                            {
                                Game1.playSound("crit");
                            }
                            else if (this.checkHeldItem())
                            {
                                Game1.playSound("dwop");
                            }
                        }
                    }
                label_43:
                    if (flag && this.checkHeldItem() && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
                    {
                        for (var i = 0; i < Game1.player.Items.Count; i++)
                        {
                            if (Game1.player.Items[i] == null || this.checkHeldItem((Func<Item, bool>)(item => Game1.player.Items[i].canStackWith((ISalable)item))))
                            {
                                if (Game1.player.CurrentToolIndex == i && this.checkHeldItem())
                                {
                                    Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
                                }
                                this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory));
                                if (Game1.player.CurrentToolIndex == i && this.checkHeldItem())
                                {
                                    Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
                                }
                                Game1.playSound("stoneStep");
                                return;
                            }
                        }
                    }
                }
            }
            this.setHeldItem(this.inventory.leftClick(x, y, this.takeHeldItem(), !Game1.oldKBState.IsKeyDown(Keys.LeftShift)));
            if (this.checkHeldItem((Func<Item, bool>)(i => i?.QualifiedItemId == "(O)434")))
            {
                Game1.playSound("smallSelect");
                Game1.player.eatObject(this.takeHeldItem() as StardewValley.Object, true);
                Game1.exitActiveMenu();
            }
            else if (this.checkHeldItem() && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
            {
                if (this.checkHeldItem((Func<Item, bool>)(i => i is Ring)))
                {
                    if (Game1.player.leftRing.Value == null)
                    {
                        Game1.player.Equip<Ring>(this.takeHeldItem() as Ring, Game1.player.leftRing);
                        Game1.playSound("crit");
                        return;
                    }
                    if (Game1.player.rightRing.Value == null)
                    {
                        Game1.player.Equip<Ring>(this.takeHeldItem() as Ring, Game1.player.rightRing);
                        Game1.playSound("crit");
                        return;
                    }
                }
                else if (this.checkHeldItem((Func<Item, bool>)(i => i is Hat)))
                {
                    if (Game1.player.hat.Value == null)
                    {
                        Game1.player.Equip<Hat>(this.takeHeldItem() as Hat, Game1.player.hat);
                        Game1.playSound("grassyStep");
                        return;
                    }
                }
                else if (this.checkHeldItem((Func<Item, bool>)(i => i is Boots)))
                {
                    if (Game1.player.boots.Value == null)
                    {
                        Game1.player.Equip<Boots>(this.takeHeldItem() as Boots, Game1.player.boots);
                        Game1.playSound("sandyStep");
                        DelayedAction.playSoundAfterDelay("sandyStep", 150);
                        return;
                    }
                }
                else if (this.checkHeldItem((Func<Item, bool>)(i => i is Clothing clothing4 && clothing4.clothesType.Value == Clothing.ClothesType.SHIRT)))
                {
                    if (Game1.player.shirtItem.Value == null)
                    {
                        Game1.player.Equip<Clothing>(this.takeHeldItem() as Clothing, Game1.player.shirtItem);
                        Game1.playSound("sandyStep");
                        DelayedAction.playSoundAfterDelay("sandyStep", 150);
                        return;
                    }
                }
                else if (this.checkHeldItem((Func<Item, bool>)(i => i is Clothing clothing3 && clothing3.clothesType.Value == Clothing.ClothesType.PANTS)))
                {
                    if (Game1.player.pantsItem.Value == null)
                    {
                        Game1.player.Equip<Clothing>(this.takeHeldItem() as Clothing, Game1.player.pantsItem);
                        Game1.playSound("sandyStep");
                        DelayedAction.playSoundAfterDelay("sandyStep", 150);
                        return;
                    }
                }
                else if (this.checkHeldItem((Func<Item, bool>)(i => i is Trinket)) && Game1.player.stats.Get("trinketSlots") > 0U)
                {
                    var flag = false;
                    for (var index = 0; index < Game1.player.trinketItems.Count; ++index)
                    {
                        if (Game1.player.trinketItems[index] == null)
                        {
                            Game1.player.trinketItems[index] = this.takeHeldItem() as Trinket;
                            flag = true;
                            break;
                        }
                    }
                    if (Game1.player.trinketItems.Count < Farmer.MaximumTrinkets)
                    {
                        Game1.player.trinketItems.Add(this.takeHeldItem() as Trinket);
                        flag = true;
                    }
                    if (flag)
                    {
                        Game1.playSound("clank");
                        return;
                    }
                }
                if (this.inventory.getInventoryPositionOfClick(x, y) >= 12)
                {
                    for (var i = 0; i < 12; i++)
                    {
                        if (Game1.player.Items[i] == null || this.checkHeldItem((Func<Item, bool>)(item => Game1.player.Items[i].canStackWith((ISalable)item))))
                        {
                            if (Game1.player.CurrentToolIndex == i && this.checkHeldItem())
                            {
                                Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
                            }
                            this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory));
                            if (this.checkHeldItem())
                            {
                                Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
                            }
                            Game1.playSound("stoneStep");
                            return;
                        }
                    }
                }
                else if (this.inventory.getInventoryPositionOfClick(x, y) < 12)
                {
                    for (var i = 12; i < Game1.player.Items.Count; i++)
                    {
                        if (Game1.player.Items[i] == null || this.checkHeldItem((Func<Item, bool>)(item => Game1.player.Items[i].canStackWith((ISalable)item))))
                        {
                            if (Game1.player.CurrentToolIndex == i && this.checkHeldItem())
                            {
                                Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
                            }
                            this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory));
                            if (this.checkHeldItem())
                            {
                                Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
                            }
                            Game1.playSound("stoneStep");
                            return;
                        }
                    }
                }
            }
            if (this.portrait.containsPoint(x, y))
            {
                this.portrait.name = this.portrait.name.Equals("32") ? "8" : "32";
            }
            if (this.trashCan.containsPoint(x, y) && this.checkHeldItem((Func<Item, bool>)(i => i != null && i.canBeTrashed())))
            {
                Utility.trashItem(this.takeHeldItem());
                if (Game1.options.SnappyMenus)
                {
                    this.snapCursorToCurrentSnappedComponent();
                }
            }
            else if (!this.isWithinBounds(x, y) && this.checkHeldItem((Func<Item, bool>)(i => i != null && i.canBeTrashed())))
            {
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection).DroppedByPlayerID.Value = Game1.player.UniqueMultiplayerID;
            }
            if (this.organizeButton != null && this.organizeButton.containsPoint(x, y))
            {
                ItemGrabMenu.organizeItemsInList((IList<Item>)Game1.player.Items);
                Game1.playSound("Ship");
            }
            if (this.junimoNoteIcon == null || !this.junimoNoteIcon.containsPoint(x, y) || !this.readyToClose())
            {
                return;
            }
            Game1.activeClickableMenu = (IClickableMenu)new JunimoNoteMenu(true)
            {
                gameMenuTabToReturnTo = GameMenu.inventoryTab
            };
        }

        /// <inheritdoc />
        public override void receiveGamePadButton(Buttons button)
        {
            if (button != Buttons.Back || this.organizeButton == null)
            {
                return;
            }
            ItemGrabMenu.organizeItemsInList((IList<Item>)Game1.player.Items);
            Game1.playSound("Ship");
        }

        /// <inheritdoc />
        public override void receiveRightClick(int x, int y, bool playSound = true) => this.setHeldItem(this.inventory.rightClick(x, y, this.takeHeldItem()));

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
            this.hoverAmount = -1;
            this.hoveredItem = this.inventory.hover(x, y, Game1.player.CursorSlotItem);
            this.hoverText = this.inventory.hoverText;
            this.hoverTitle = this.inventory.hoverTitle;
            foreach (var equipmentIcon in this.equipmentIcons)
            {
                if (equipmentIcon.containsPoint(x, y))
                {
                    var name = equipmentIcon.name;
                    if (name != null)
                    {
                        switch (name.Length)
                        {
                            case 3:
                                if (name == "Hat" && Game1.player.hat.Value != null)
                                {
                                    this.hoveredItem = (Item)Game1.player.hat.Value;
                                    this.hoverText = Game1.player.hat.Value.getDescription();
                                    this.hoverTitle = Game1.player.hat.Value.DisplayName;
                                    break;
                                }
                                break;
                            case 5:
                                switch (name[0])
                                {
                                    case 'B':
                                        if (name == "Boots" && Game1.player.boots.Value != null)
                                        {
                                            this.hoveredItem = (Item)Game1.player.boots.Value;
                                            this.hoverText = Game1.player.boots.Value.getDescription();
                                            this.hoverTitle = Game1.player.boots.Value.DisplayName;
                                            break;
                                        }
                                        break;
                                    case 'P':
                                        if (name == "Pants" && Game1.player.pantsItem.Value != null)
                                        {
                                            this.hoveredItem = (Item)Game1.player.pantsItem.Value;
                                            this.hoverText = Game1.player.pantsItem.Value.getDescription();
                                            this.hoverTitle = Game1.player.pantsItem.Value.DisplayName;
                                            break;
                                        }
                                        break;
                                    case 'S':
                                        if (name == "Shirt" && Game1.player.shirtItem.Value != null)
                                        {
                                            this.hoveredItem = (Item)Game1.player.shirtItem.Value;
                                            this.hoverText = Game1.player.shirtItem.Value.getDescription();
                                            this.hoverTitle = Game1.player.shirtItem.Value.DisplayName;
                                            break;
                                        }
                                        break;
                                }
                                break;
                            case 7:
                                if (name == "Trinket" && Game1.player.trinketItems.Count == 1 && Game1.player.trinketItems[0] != null)
                                {
                                    this.hoveredItem = (Item)Game1.player.trinketItems[0];
                                    this.hoverText = Game1.player.trinketItems[0].getDescription();
                                    this.hoverTitle = Game1.player.trinketItems[0].DisplayName;
                                    break;
                                }
                                break;
                            case 9:
                                if (name == "Left Ring" && Game1.player.leftRing.Value != null)
                                {
                                    this.hoveredItem = (Item)Game1.player.leftRing.Value;
                                    this.hoverText = Game1.player.leftRing.Value.getDescription();
                                    this.hoverTitle = Game1.player.leftRing.Value.DisplayName;
                                    break;
                                }
                                break;
                            case 10:
                                if (name == "Right Ring" && Game1.player.rightRing.Value != null)
                                {
                                    this.hoveredItem = (Item)Game1.player.rightRing.Value;
                                    this.hoverText = Game1.player.rightRing.Value.getDescription();
                                    this.hoverTitle = Game1.player.rightRing.Value.DisplayName;
                                    break;
                                }
                                break;
                        }
                    }
                    equipmentIcon.scale = Math.Min(equipmentIcon.scale + 0.05f, 1.1f);
                }
                equipmentIcon.scale = Math.Max(1f, equipmentIcon.scale - 0.025f);
            }
            if (this.portrait.containsPoint(x, y))
            {
                this.portrait.scale += 0.2f;
                this.hoverText = Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", (object)Game1.player.Level) + Environment.NewLine + Game1.player.getTitle();
            }
            else
            {
                this.portrait.scale = 0.0f;
            }
            if (this.trashCan.containsPoint(x, y))
            {
                if ((double)this.trashCanLidRotation <= 0.0)
                {
                    Game1.playSound("trashcanlid");
                }
                this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + (float)Math.PI / 48f, 1.57079637f);
                if (this.checkHeldItem() && Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player) > 0)
                {
                    this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                    this.hoverAmount = Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player);
                }
            }
            else if ((double)this.trashCanLidRotation != 0.0)
            {
                this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - 0.1308997f, 0.0f);
                if ((double)this.trashCanLidRotation == 0.0)
                {
                    Game1.playSound("thudStep");
                }
            }
            if (this.organizeButton != null)
            {
                this.organizeButton.tryHover(x, y);
                if (this.organizeButton.containsPoint(x, y))
                {
                    this.hoverText = this.organizeButton.hoverText;
                }
            }
            if (this.junimoNoteIcon == null)
            {
                return;
            }
            this.junimoNoteIcon.tryHover(x, y);
            if (this.junimoNoteIcon.containsPoint(x, y))
            {
                this.hoverText = this.junimoNoteIcon.hoverText;
            }
            if (GameMenu.bundleItemHovered)
            {
                this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale + (float)Math.Sin((double)this.junimoNotePulser / 100.0) / 4f;
                this.junimoNotePulser += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                this.junimoNotePulser = 0;
                this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override bool readyToClose() => !this.checkHeldItem();

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            this.drawHorizontalPartition(b, this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 192);
            this.inventory.draw(b);
            foreach (var equipmentIcon in this.equipmentIcons)
            {
                var name = equipmentIcon.name;
                if (name != null)
                {
                    switch (name.Length)
                    {
                        case 3:
                            if (name == "Hat")
                            {
                                if (Game1.player.hat.Value != null)
                                {
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                    Game1.player.hat.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale, 1f, 0.866f, StackDrawType.Hide);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42)), Color.White);
                                continue;
                            }
                            continue;
                        case 5:
                            switch (name[0])
                            {
                                case 'B':
                                    if (name == "Boots")
                                    {
                                        if (Game1.player.boots.Value != null)
                                        {
                                            b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                            Game1.player.boots.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                            continue;
                                        }
                                        b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40)), Color.White);
                                        continue;
                                    }
                                    continue;
                                case 'P':
                                    if (name == "Pants")
                                    {
                                        if (Game1.player.pantsItem.Value != null)
                                        {
                                            b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                            Game1.player.pantsItem.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                            continue;
                                        }
                                        b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68)), Color.White);
                                        continue;
                                    }
                                    continue;
                                case 'S':
                                    if (name == "Shirt")
                                    {
                                        if (Game1.player.shirtItem.Value != null)
                                        {
                                            b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                            Game1.player.shirtItem.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                            continue;
                                        }
                                        b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69)), Color.White);
                                        continue;
                                    }
                                    continue;
                                default:
                                    continue;
                            }
                        case 7:
                            if (name == "Trinket")
                            {
                                var index = equipmentIcon.myID - 120;
                                if (Game1.player.trinketItems.Count > index && Game1.player.trinketItems[index] != null)
                                {
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                    Game1.player.trinketItems[index].drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 70)), Color.White);
                                continue;
                            }
                            continue;
                        case 9:
                            if (name == "Left Ring")
                            {
                                if (Game1.player.leftRing.Value != null)
                                {
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                    Game1.player.leftRing.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41)), Color.White);
                                continue;
                            }
                            continue;
                        case 10:
                            if (name == "Right Ring")
                            {
                                if (Game1.player.rightRing.Value != null)
                                {
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10)), Color.White);
                                    Game1.player.rightRing.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41)), Color.White);
                                continue;
                            }
                            continue;
                        default:
                            continue;
                    }
                }
            }
            b.Draw(Game1.timeOfDay >= 1900 ? Game1.nightbg : Game1.daybg, new Vector2((float)(this.xPositionOnScreen + 192 - 64 - 8), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256 - 8)), Color.White);
            FarmerRenderer.isDrawingForUI = true;
            Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, false, false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(this.xPositionOnScreen + 192 - 8 - 32), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 320 - 32 - 8)), Vector2.Zero, 0.8f, 2, Color.White, 0.0f, 1f, Game1.player);
            if (Game1.timeOfDay >= 1900)
            {
                Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, false, false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(this.xPositionOnScreen + 192 - 8 - 32), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 320 - 32 - 8)), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0.0f, 1f, Game1.player);
            }
            FarmerRenderer.isDrawingForUI = false;
            Utility.drawTextWithShadow(b, Game1.player.Name, Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + 192 - 8) - Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f, (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 448 + 8)), Game1.textColor);
            var num = 32f;
            var text1 = Game1.content.LoadString("Strings\\UI:Inventory_FarmName", (object)Game1.player.farmName);
            Utility.drawTextWithShadow(b, text1, Game1.dialogueFont, new Vector2((float)((double)this.xPositionOnScreen + (double)num + 512.0 + 32.0 - (double)Game1.dialogueFont.MeasureString(text1).X / 2.0), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 256 + 4)), Game1.textColor);
            var text2 = Game1.content.LoadString("Strings\\UI:Inventory_CurrentFunds" + (Game1.player.useSeparateWallets ? "_Separate" : ""), (object)Utility.getNumberWithCommas(Game1.player.Money));
            Utility.drawTextWithShadow(b, text2, Game1.dialogueFont, new Vector2((float)((double)this.xPositionOnScreen + (double)num + 512.0 + 32.0 - (double)Game1.dialogueFont.MeasureString(text2).X / 2.0), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 320 + 4)), Game1.textColor);
            var text3 = Game1.content.LoadString("Strings\\UI:Inventory_TotalEarnings" + (Game1.player.useSeparateWallets ? "_Separate" : ""), (object)Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned));
            Utility.drawTextWithShadow(b, text3, Game1.dialogueFont, new Vector2((float)((double)this.xPositionOnScreen + (double)num + 512.0 + 32.0 - (double)Game1.dialogueFont.MeasureString(text3).X / 2.0), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 384)), Game1.textColor);
            Utility.drawTextWithShadow(b, Utility.getDateString(), Game1.dialogueFont, new Vector2((float)((double)this.xPositionOnScreen + (double)num + 512.0 + 32.0 - (double)Game1.dialogueFont.MeasureString(Utility.getDateString()).X / 2.0), (float)(this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 448)), Game1.textColor * 0.8f);
            this.organizeButton?.draw(b);
            this.trashCan.draw(b);
            b.Draw(Game1.mouseCursors, new Vector2((float)(this.trashCan.bounds.X + 60), (float)(this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            if (this.checkHeldItem())
            {
                Game1.player.CursorSlotItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
            }
            if (!string.IsNullOrEmpty(this.hoverText))
            {
                if (this.hoverAmount > 0)
                {
                    drawToolTip(b, this.hoverText, this.hoverTitle, (Item)null, true, moneyAmountToShowAtBottom: this.hoverAmount);
                }
                else
                {
                    drawToolTip(b, this.hoverText, this.hoverTitle, this.hoveredItem, this.checkHeldItem());
                }
            }
            this.junimoNoteIcon?.draw(b);
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            this.setHeldItem(Game1.player.addItemToInventory(this.takeHeldItem()));
            if (!this.checkHeldItem())
            {
                return;
            }
            Game1.playSound("throwDownITem");
            Game1.createItemDebris(this.takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection);
        }
    }
}
