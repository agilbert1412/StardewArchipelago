using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications
{
    public class ArchipelagoMineElevatorMenu : IClickableMenu
    {
        public const int FLOORS_PER_ELEVATOR = 5;

        public List<ClickableComponent> elevators = new();

        public ArchipelagoMineElevatorMenu(int mineLevelUnlocked)
          : base(0, 0, 0, 0, true)
        {
            var num1 = Math.Min(mineLevelUnlocked, 120) / FLOORS_PER_ELEVATOR;
            this.width = num1 > 50 ? 484 + borderWidth * 2 : Math.Min(220 + borderWidth * 2, num1 * 44 + borderWidth * 2);
            this.height = Math.Max(64 + borderWidth * 3, num1 * 44 / (this.width - borderWidth) * 44 + 64 + borderWidth * 3);
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            Game1.playSound("crystal", new int?(0));
            var num2 = this.width / 44 - 1;
            var x1 = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder * 3 / 4;
            var y = this.yPositionOnScreen + borderWidth + borderWidth / 3;
            this.elevators.Add(new ClickableComponent(new Rectangle(x1, y, 44, 44), 0.ToString() ?? "")
            {
                myID = 0,
                rightNeighborID = 1,
                downNeighborID = num2,
            });
            var x2 = x1 + 64 - 20;
            if (x2 > this.xPositionOnScreen + this.width - borderWidth)
            {
                x2 = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder * 3 / 4;
                y += 44;
            }
            for (var index = 1; index <= num1; ++index)
            {
                this.elevators.Add(new ClickableComponent(new Rectangle(x2, y, 44, 44), (index * 5).ToString() ?? "")
                {
                    myID = index,
                    rightNeighborID = index % num2 == num2 - 1 ? -1 : index + 1,
                    leftNeighborID = index % num2 == 0 ? -1 : index - 1,
                    downNeighborID = index + num2,
                    upNeighborID = index - num2,
                });
                x2 = x2 + 64 - 20;
                if (x2 > this.xPositionOnScreen + this.width - borderWidth)
                {
                    x2 = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder * 3 / 4;
                    y += 44;
                }
            }
            this.initializeUpperRightCloseButton();
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
            {
                return;
            }
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        /// <inheritdoc />
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.isWithinBounds(x, y))
            {
                foreach (var elevator in this.elevators)
                {
                    if (elevator.containsPoint(x, y))
                    {
                        Game1.playSound("smallSelect");
                        if (Convert.ToInt32(elevator.name) == 0)
                        {
                            if (!(Game1.currentLocation is MineShaft))
                            {
                                return;
                            }
                            Game1.warpFarmer("Mine", 17, 4, true);
                            Game1.exitActiveMenu();
                        }
                        else
                        {
                            if (Convert.ToInt32(elevator.name) == Game1.CurrentMineLevel)
                            {
                                return;
                            }
                            Game1.player.ridingMineElevator = true;
                            Game1.enterMine(Convert.ToInt32(elevator.name));
                            Game1.exitActiveMenu();
                        }
                    }
                }
                base.receiveLeftClick(x, y);
            }
            else
            {
                Game1.exitActiveMenu();
            }
        }

        /// <inheritdoc />
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            foreach (var elevator in this.elevators)
            {
                elevator.scale = !elevator.containsPoint(x, y) ? 1f : 2f;
            }
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen - 64 + 8, this.width + 21, this.height + 64, false, true);
            foreach (var elevator in this.elevators)
            {
                b.Draw(Game1.mouseCursors, new Vector2((float)(elevator.bounds.X - 4), (float)(elevator.bounds.Y + 4)), new Rectangle?(new Rectangle((double)elevator.scale > 1.0 ? 267 : 256, 256, 10, 10)), Color.Black * 0.5f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
                b.Draw(Game1.mouseCursors, new Vector2((float)elevator.bounds.X, (float)elevator.bounds.Y), new Rectangle?(new Rectangle((double)elevator.scale > 1.0 ? 267 : 256, 256, 10, 10)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
                var position = new Vector2((float)(elevator.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(elevator.name)) * 6), (float)(elevator.bounds.Y + 24 - NumberSprite.getHeight() / 4));
                NumberSprite.draw(Convert.ToInt32(elevator.name), b, position, Game1.CurrentMineLevel == Convert.ToInt32(elevator.name) ? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0);
            }
            base.draw(b);
            this.drawMouse(b);

            MineshaftInjections.DrawElevatorIndicators(this.elevators, b);
        }
    }
}
