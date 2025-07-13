// Code from Skull Cavern Elevator, Decompiled with JetBrains decompiler, Then adapted for Archipelago Integration

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.GameModifications;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Runtime.CompilerServices;
using StardewArchipelago.Locations.CodeInjections.Modded;

#nullable disable
namespace SkullCavernElevator.SkullCavernElevator
{
    public class AchipelagoSkullCavernElevatorMenu : ArchipelagoMineElevatorMenu
    {
        private readonly int elevatorCostPerStep;

        public string hoverText { get; private set; }

        public AchipelagoSkullCavernElevatorMenu(int mineLevelUnlocked) : base(mineLevelUnlocked)
        {
            this.initialize(0, 0, 0, 0, true);
            this.elevatorCostPerStep = 0;
            this.hoverText = "";
            if (Game1.gameMode != 3 || Game1.player == null || Game1.eventUp)
            {
                return;
            }
            Game1.player.Halt();
            this.elevators.Clear();
            var num1 = mineLevelUnlocked / SkullCavernInjections.ELEVATOR_STEP;
            this.width = num1 > 50 ? 484 + borderWidth * 2 : Math.Min(220 + borderWidth * 2, num1 * 44 + borderWidth * 2);
            this.height = Math.Max(64 + borderWidth * 3, num1 * 44 / (this.width - borderWidth) * 44 + 64 + borderWidth * 3);
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            Game1.playSound("crystal");
            var num2 = this.width / 44 - 1;
            var x1 = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder * 3 / 4;
            var y = this.yPositionOnScreen + borderWidth + borderWidth / 3;
            this.elevators.Add(new ClickableComponent(new Rectangle(x1, y, 44, 44), string.Concat(0))
            {
                myID = 0,
                rightNeighborID = 1,
                downNeighborID = num2
            });
            var x2 = x1 + 64 - 20;
            if (x2 > this.xPositionOnScreen + this.width - borderWidth)
            {
                x2 = this.xPositionOnScreen + borderWidth + spaceToClearSideBorder * 3 / 4;
                y += 44;
            }
            for (var index = 1; index <= num1; ++index)
            {
                this.elevators.Add(new ClickableComponent(new Rectangle(x2, y, 44, 44), string.Concat(index * SkullCavernInjections.ELEVATOR_STEP))
                {
                    myID = index,
                    rightNeighborID = index % num2 == num2 - 1 ? -1 : index + 1,
                    leftNeighborID = index % num2 == 0 ? -1 : index - 1,
                    downNeighborID = index + num2,
                    upNeighborID = index - num2,
                    label = formatMoney(index * elevatorCostPerStep)
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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.isWithinBounds(x, y))
            {
                var flag = false;
                foreach (var elevator in this.elevators)
                {
                    if (elevator.containsPoint(x, y))
                    {
                        var pitch = Game1.currentLocation is MineShaft currentLocation ? new int?(currentLocation.mineLevel) : new int?();
                        var num = Convert.ToInt32(elevator.name) + 120;
                        if (pitch.GetValueOrDefault() == num & pitch.HasValue)
                        {
                            return;
                        }
                        pitch = new int?();
                        Game1.playSound("smallSelect", pitch);
                        if (Convert.ToInt32(elevator.name) == 0)
                        {
                            if (!Game1.currentLocation.Equals((object)Game1.mine))
                            {
                                return;
                            }
                            Game1.warpFarmer("SkullCave", 3, 4, 2);
                            Game1.exitActiveMenu();
                            Game1.changeMusicTrack("none");
                            flag = true;
                        }
                        else
                        {
                            if (Game1.currentLocation.Equals((object)Game1.mine) && Convert.ToInt32(elevator.name) == Game1.mine.mineLevel)
                            {
                                return;
                            }
                            if (this.elevatorCostPerStep != 0 && Game1.player.Money < extractAmount(elevator.label))
                            {
                                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"), 3));
                                Game1.exitActiveMenu();
                                return;
                            }
                            if (this.elevatorCostPerStep != 0)
                            {
                                Game1.player.Money -= extractAmount(elevator.label);
                                pitch = new int?();
                                Game1.playSound("purchase", pitch);
                            }
                            Game1.player.ridingMineElevator = true;
                            Game1.enterMine(Convert.ToInt32(elevator.name) + 120);
                            Game1.exitActiveMenu();
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    return;
                }
                base.receiveLeftClick(x, y, true);
            }
            else
            {
                Game1.exitActiveMenu();
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (var elevator in this.elevators)
            {
                b.Draw(Game1.mouseCursors, new Vector2(elevator.bounds.X - 4, elevator.bounds.Y + 4), new Rectangle?(new Rectangle(elevator.scale > 1.0 ? 267 : 256, 256, 10, 10)), Color.Black * 0.5f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
                b.Draw(Game1.mouseCursors, new Vector2(elevator.bounds.X, elevator.bounds.Y), new Rectangle?(new Rectangle(elevator.scale > 1.0 ? 267 : 256, 256, 10, 10)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);
                var position = new Vector2(elevator.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(elevator.name)) * 6, elevator.bounds.Y + 24 - NumberSprite.getHeight() / 4);
                NumberSprite.draw(Convert.ToInt32(elevator.name), b, position, Game1.CurrentMineLevel == Convert.ToInt32(elevator.name) + 120 && Game1.currentLocation.Equals((object)Game1.mine) || Convert.ToInt32(elevator.name) == 0 && !Game1.currentLocation.Equals((object)Game1.mine) ? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0);
            }
            this.drawMouse(b);
            if (this.hoverText.Length <= 0 || this.elevatorCostPerStep <= 0)
            {
                return;
            }
            drawHoverText(b, this.hoverText, Game1.smallFont);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverText = "";
            foreach (var elevator in this.elevators)
            {
                if (elevator != null && elevator.containsPoint(x, y) && elevator.myID != 0)
                {
                    this.hoverText = elevator.label;
                    break;
                }
            }
        }
        public static string formatMoney(int amount)
        {
            if (amount < 0)
            {
                amount = int.MaxValue;
            }
            var interpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
            interpolatedStringHandler.AppendLiteral("G ");
            interpolatedStringHandler.AppendFormatted<int>(amount);
            return interpolatedStringHandler.ToStringAndClear();
        }

        public static int extractAmount(string money) => int.Parse(money.Split(' ')[1]);
    }
}
