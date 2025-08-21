// Decompiled with JetBrains decompiler
// Type: StardewValley.Menus.InventoryPage
// Assembly: Stardew Valley, Version=1.6.15.24356, Culture=neutral, PublicKeyToken=null
// MVID: AA1F513A-94F0-4EF6-A35F-2D5B4A3721BF
// Assembly location: d:\programs\steam\steamapps\common\Stardew Valley\Stardew Valley.dll
// XML documentation location: d:\programs\steam\steamapps\common\Stardew Valley\Stardew Valley.xml

#nullable disable
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants.Modded;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ClothesMenu : IClickableMenu
    {
        public const int region_hat = 101;
        public const int region_ring1 = 102;
        public const int region_ring2 = 103;
        public const int region_boots = 104;
        public const int region_shirt = 108;
        public const int region_pants = 109;
        public string hoverText = "";
        public string hoverTitle = "";
        public int hoverAmount;
        public Item hoveredItem;
        public List<ClickableComponent> equipmentIcons = new();
        public ClickableComponent portrait;

        public ClothesMenu(IModHelper modHelper, int x, int y, int width, int height)
          : base(x, y, width, height)
        {
            // With Bigger Backpack, the whole menu is slightly different! Gotta make these buttons fit!
            var hasBiggerBackpack = modHelper.ModRegistry.IsLoaded(ModUniqueIds.UniqueIds[ModNames.BIGGER_BACKPACK]);
            var startXOffset = 0;
            var startYOffset = 0;
            var characterXOffset = 0;
            var rightColumnXOffset = 0;
            var offsetPerRowOffset = 0;
            if (hasBiggerBackpack)
            {
                startXOffset = 24;
                startYOffset = 20 + 12;
                characterXOffset = 8;
                rightColumnXOffset = 8;
                offsetPerRowOffset = 2;
            }

            var startX = this.xPositionOnScreen + 64 + 4 - startXOffset;
            var startY = this.yPositionOnScreen + borderWidth + spaceToClearTopBorder + 128 - 40 - startYOffset;

            var leftColumnX = startX;
            var offsetPerColumn = 64;
            var characterX = leftColumnX + offsetPerColumn + 14 - characterXOffset;
            var characterY = startY + 8;
            var rightColumnX = characterX + (offsetPerColumn*2) + 14 - rightColumnXOffset;
            var offsetPerRow = 64 + 10 - offsetPerRowOffset;

            var firstRowY = startY;
            var secondRowY = firstRowY + offsetPerRow;
            var thirdRowY = secondRowY + offsetPerRow;

            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(leftColumnX, firstRowY, 64, 64), "Left Ring")
            {
                myID = region_ring1,
                downNeighborID = region_ring2,
                rightNeighborID = region_hat,
                fullyImmutable = false
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(leftColumnX, secondRowY, 64, 64), "Right Ring")
            {
                myID = region_ring2,
                upNeighborID = region_ring1,
                downNeighborID = region_boots,
                rightNeighborID = region_shirt,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(leftColumnX, thirdRowY, 64, 64), "Boots")
            {
                myID = region_boots,
                upNeighborID = region_ring2,
                rightNeighborID = region_pants,
                fullyImmutable = true
            });
            this.portrait = new ClickableComponent(new Rectangle(characterX, characterY, 64, 96), "32");
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(rightColumnX, firstRowY, 64, 64), "Hat")
            {
                myID = region_hat,
                leftNeighborID = region_ring1,
                downNeighborID = region_shirt,
                fullyImmutable = false
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(rightColumnX, secondRowY, 64, 64), "Shirt")
            {
                myID = region_shirt,
                upNeighborID = region_hat,
                downNeighborID = region_pants,
                leftNeighborID = region_ring2,
                fullyImmutable = true
            });
            this.equipmentIcons.Add(new ClickableComponent(new Rectangle(rightColumnX, thirdRowY, 64, 64), "Pants")
            {
                myID = region_pants,
                upNeighborID = region_shirt,
                leftNeighborID = region_boots,
                fullyImmutable = true
            });
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
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
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                    Game1.player.hat.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale, 1f, 0.866f, StackDrawType.Hide);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42), Color.White);
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
                                            b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                            Game1.player.boots.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                            continue;
                                        }
                                        b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40), Color.White);
                                        continue;
                                    }
                                    continue;
                                case 'P':
                                    if (name == "Pants")
                                    {
                                        if (Game1.player.pantsItem.Value != null)
                                        {
                                            b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                            Game1.player.pantsItem.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                            continue;
                                        }
                                        b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68), Color.White);
                                        continue;
                                    }
                                    continue;
                                case 'S':
                                    if (name == "Shirt")
                                    {
                                        if (Game1.player.shirtItem.Value != null)
                                        {
                                            b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                            Game1.player.shirtItem.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                            continue;
                                        }
                                        b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69), Color.White);
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
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                    Game1.player.trinketItems[index].drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 70), Color.White);
                                continue;
                            }
                            continue;
                        case 9:
                            if (name == "Left Ring")
                            {
                                if (Game1.player.leftRing.Value != null)
                                {
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                    Game1.player.leftRing.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
                                continue;
                            }
                            continue;
                        case 10:
                            if (name == "Right Ring")
                            {
                                if (Game1.player.rightRing.Value != null)
                                {
                                    b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
                                    Game1.player.rightRing.Value.drawInMenu(b, new Vector2((float)equipmentIcon.bounds.X, (float)equipmentIcon.bounds.Y), equipmentIcon.scale);
                                    continue;
                                }
                                b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
                                continue;
                            }
                            continue;
                        default:
                            continue;
                    }
                }
            }
            b.Draw(Game1.timeOfDay >= 1900 ? Game1.nightbg : Game1.daybg, new Vector2((float)(this.portrait.bounds.X), (float)(this.portrait.bounds.Y)), Color.White);
            FarmerRenderer.isDrawingForUI = true;
            Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, false, false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(this.portrait.bounds.X + 32), (float)(this.portrait.bounds.Y + 32)), Vector2.Zero, 0.8f, 2, Color.White, 0.0f, 1f, Game1.player);
            if (Game1.timeOfDay >= 1900)
                Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, false, false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)(this.portrait.bounds.X + 32), (float)(this.portrait.bounds.Y + 32)), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0.0f, 1f, Game1.player);
            FarmerRenderer.isDrawingForUI = false;
        }

        public Item leftClick(int x, int y, Item toPlace, bool playSound = true)
        {
            foreach (var equipmentIcon in this.equipmentIcons)
            {
                if (!equipmentIcon.containsPoint(x, y))
                {
                    continue;
                }

                if (toPlace != null)
                {
                    if (equipmentIcon.item == null)
                    {
                        if (equipmentIcon.name == "Hat" && toPlace is Hat hat)
                        {
                            return Game1.player.Equip(hat, Game1.player.hat);
                        }
                        else if (equipmentIcon.name == "Boots" && toPlace is Boots boots)
                        {
                            return Game1.player.Equip(boots, Game1.player.boots);
                        }
                        else if (equipmentIcon.name == "Pants" && toPlace is Clothing pants && pants.clothesType.Value == Clothing.ClothesType.PANTS)
                        {
                            return Game1.player.Equip(pants, Game1.player.pantsItem);
                        }
                        else if (equipmentIcon.name == "Shirt" && toPlace is Clothing shirt && shirt.clothesType.Value == Clothing.ClothesType.SHIRT)
                        {
                            return Game1.player.Equip(shirt, Game1.player.shirtItem);
                        }
                        else if (equipmentIcon.name == "Left Ring" && toPlace is Ring leftRing)
                        {
                            return Game1.player.Equip(leftRing, Game1.player.leftRing);
                        }
                        else if (equipmentIcon.name == "Right Ring" && toPlace is Ring rightRing)
                        {
                            return Game1.player.Equip(rightRing, Game1.player.rightRing);
                        }
                        else
                        {
                            return toPlace;
                        }
                    }
                }
                else
                {
                    if (equipmentIcon.name == "Hat")
                    {
                        return Game1.player.Equip(null, Game1.player.hat);
                    }
                    else if (equipmentIcon.name == "Boots")
                    {
                        return Game1.player.Equip(null, Game1.player.boots);
                    }
                    else if (equipmentIcon.name == "Pants")
                    {
                        return Game1.player.Equip(null, Game1.player.pantsItem);
                    }
                    else if (equipmentIcon.name == "Shirt")
                    {
                        return Game1.player.Equip(null, Game1.player.shirtItem);
                    }
                    else if (equipmentIcon.name == "Left Ring")
                    {
                        return Game1.player.Equip(null, Game1.player.leftRing);
                    }
                    else if (equipmentIcon.name == "Right Ring")
                    {
                        return Game1.player.Equip(null, Game1.player.rightRing);
                    }
                    return null;
                }
            }
            return toPlace;
        }
    }
}
