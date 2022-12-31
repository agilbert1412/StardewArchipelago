using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.GameModifications
{
    public class MultiSleepSelectionMenu : NumberSelectionMenu
    {
        private string _message;

        public MultiSleepSelectionMenu(string message, behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
            : base(message, behaviorOnSelection, price, minValue, maxValue, defaultNumber)
        {
            _message = message;
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);
            b.DrawString(Game1.dialogueFont, _message, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2)), Game1.textColor);
            this.okButton.draw(b);
            this.cancelButton.draw(b);
            this.leftButton.draw(b);
            this.rightButton.draw(b);
            if (this.price > 0)
            {
                var totalPrice = this.price * (this.currentValue - 1);
                var text = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", totalPrice);
                var positionX = (float)(rightButton.bounds.Right + 32 + (priceShake > 0 ? Game1.random.Next(-1, 2) : 0));
                var positionY = (float)(rightButton.bounds.Y + (priceShake > 0 ? Game1.random.Next(-1, 2) : 0));
                var position = new Vector2(positionX, positionY);
                b.DrawString(Game1.dialogueFont, text, position, totalPrice > Game1.player.Money ? Color.Red : Game1.textColor);
            }
            this.numberSelectedBox.Draw(b);
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.rightButton.containsPoint(x, y))
            {
                int num = this.currentValue + 1;
                if (num <= this.maxValue && (this.price == -1 || this.currentValue * this.price <= Game1.player.Money))
                {
                    this.rightButton.scale = this.rightButton.baseScale;
                    this.currentValue = num;
                    this.numberSelectedBox.Text = this.currentValue.ToString() ?? "";
                    Game1.playSound("smallSelect");
                }
                return;
            }
            base.receiveLeftClick(x, y, playSound);
        }
    }
}
