using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications
{
    public class InformationDialog : IClickableMenu
    {
        public const int region_okButton = 101;
        protected string message;
        public ClickableTextureComponent okButton;
        protected ConfirmationDialog.behavior onClickOk;
        private bool active = true;

        public InformationDialog(
          string message,
          ConfirmationDialog.behavior onClickOkBehavior = null)
          : base(Game1.uiViewport.Width / 2 - (int)Game1.dialogueFont.MeasureString(message).X / 2 - borderWidth, Game1.uiViewport.Height / 2 - (int)Game1.dialogueFont.MeasureString(message).Y / 2, (int)Game1.dialogueFont.MeasureString(message).X + borderWidth * 2, (int)Game1.dialogueFont.MeasureString(message).Y + borderWidth * 2 + 160)
        {
            onClickOk = onClickOkBehavior ?? CloseDialog;
            var titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
            this.message = Game1.parseText(message, Game1.dialogueFont, Math.Min(titleSafeArea.Width - 64, width));
            okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 128 - 4, yPositionOnScreen + height - borderWidth - spaceToClearTopBorder + 21, 64, 64), (string)null, (string)null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
            okButton.myID = 101;
            okButton.rightNeighborID = 102;
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            okButton.setPosition(xPositionOnScreen + width - borderWidth - spaceToClearSideBorder - 128 - 4, yPositionOnScreen + height - borderWidth - spaceToClearTopBorder + 21);
        }

        public virtual void CloseDialog(Farmer who)
        {
            if (Game1.activeClickableMenu is TitleMenu titleMenu)
            {
                titleMenu.backButtonPressed();
            }
            else
            {
                Game1.exitActiveMenu();
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(102);
            snapCursorToCurrentSnappedComponent();
        }

        public void Confirm()
        {
            if (onClickOk != null)
                onClickOk(Game1.player);
            if (active)
                Game1.playSound("smallSelect");
            active = false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!active)
            {
                return;
            }
            if (okButton.containsPoint(x, y))
            {
                Confirm();
            }
        }

        public override void performHoverAction(int x, int y)
        {
            if (okButton.containsPoint(x, y))
                okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.2f);
            else
                okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
        }

        public override void draw(SpriteBatch b)
        {
            if (!active)
                return;
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            b.DrawString(Game1.dialogueFont, message, new Vector2((float)(xPositionOnScreen + borderWidth), (float)(yPositionOnScreen + spaceToClearTopBorder + borderWidth / 2)), Game1.textColor);
            okButton.draw(b);
            drawMouse(b);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public delegate void behavior(Farmer who);
    }
}
