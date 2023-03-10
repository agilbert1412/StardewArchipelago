using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications
{
    public class CharacterCustomizationArchipelago : CharacterCustomization
    {
        private const int ARCHIPELAGO_Y_OFFSET = 488;

        public TextBox IpAddressTextBox;
        public TextBox SlotNameTextBox;
        public TextBox PasswordTextBox;
        private ClickableComponent ipAddressCC;
        private ClickableComponent slotNameCC;
        private ClickableComponent passwordCC;
        private ClickableComponent ipAddressLabel;
        private ClickableComponent slotNameLabel;
        private ClickableComponent passwordLabel;

        public CharacterCustomizationArchipelago(CharacterCustomization parent)
        :base(parent.source)
        {
            height += 48;
            SetupArchipelagoFieldsPositions();
            var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
            gameWindowSizeChanged(bounds, bounds);
        }

        public sealed override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            SetupArchipelagoFieldsPositions();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            IpAddressTextBox.Draw(b);
            SlotNameTextBox.Draw(b);
            PasswordTextBox.Draw(b);

            var ipIsValid = IpIsFormattedCorrectly();

            var ipLabelColor = ipIsValid ? Game1.textColor : Color.Red;
            Utility.drawTextWithShadow(b, ipAddressLabel.name, Game1.smallFont, new Vector2(ipAddressLabel.bounds.X, ipAddressLabel.bounds.Y), ipLabelColor);

            var slotNameLabelColor = SlotNameTextBox.Text != null && SlotNameTextBox.Text.Length >= 1 ? Game1.textColor : Color.Red;
            Utility.drawTextWithShadow(b, slotNameLabel.name, Game1.smallFont, new Vector2(slotNameLabel.bounds.X, slotNameLabel.bounds.Y), slotNameLabelColor);
        }

        public bool IpIsFormattedCorrectly()
        {
            if (IpAddressTextBox.Text == null)
            {
                return false;
            }
            var ipParts = IpAddressTextBox.Text.Split(":");
            var ipIsValid = ipParts.Length == 2 && ipParts.All(x => x.Length > 0) && int.TryParse(ipParts[1], out _);

            return ipIsValid;
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.IpAddressTextBox.Hover(x, y);
            this.SlotNameTextBox.Hover(x, y);
            this.PasswordTextBox.Hover(x, y);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            IpAddressTextBox.Update();
            SlotNameTextBox.Update();
            PasswordTextBox.Update();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Tab)
            {
                if (IpAddressTextBox.Selected)
                {
                    IpAddressTextBox.Selected = false;
                    SlotNameTextBox.SelectMe();
                    return;
                }
                else if (SlotNameTextBox.Selected)
                {
                    SlotNameTextBox.Selected = false;
                    PasswordTextBox.SelectMe();
                    return;
                }
                else if (PasswordTextBox.Selected)
                {
                    PasswordTextBox.Selected = false;
                    IpAddressTextBox.SelectMe();
                    return;
                }
            }
            base.receiveKeyPress(key);
        }

        private void SetupArchipelagoFieldsPositions()
        {
            var xOffset = xPositionOnScreen + spaceToClearSideBorder + borderWidth;
            var yOffset = yPositionOnScreen + spaceToClearTopBorder + borderWidth + ARCHIPELAGO_Y_OFFSET;
            var texture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");

            SetupIpFieldPosition(xOffset, yOffset, texture);
            SetupSlotNameFieldPosition(xOffset, yOffset + 64, texture);
            SetupPasswordFieldPosition(xOffset, yOffset + 128, texture);

            skipIntroButton.setPosition(skipIntroButton.bounds.X - 240, skipIntroButton.bounds.Y + 136);
        }

        private void SetupIpFieldPosition(int xOffset, int yOffset, Texture2D texture)
        {
            var defaultserver = "archipelago.gg:";
#if DEBUG
            defaultserver = "localhost:38282";
#endif
            var xPosition = xOffset + 256 + 64 + 16;
            var yPosition = yOffset - 16;
            IpAddressTextBox = new TextBox(texture, null, Game1.smallFont, Game1.textColor)
            {
                X = xPosition,
                Y = yPosition,
            };
            IpAddressTextBox.limitWidth = false;
            IpAddressTextBox.Text = defaultserver;
            var ipAddressRectangle = new Rectangle(xPosition, yPosition, 192, 48);
            ipAddressCC = new ClickableComponent(ipAddressRectangle, "")
            {
                myID = 536536,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            var languageOffset = LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.es or LocalizedContentManager.LanguageCode.pt ? -4 : 0;
            ipAddressLabel = new ClickableComponent(new Rectangle(xOffset + languageOffset + 16 + 192 + 4, yOffset - 8, 1, 1), "Server");
            labels.Add(ipAddressLabel);
        }

        private void SetupSlotNameFieldPosition(int xOffset, int yOffset, Texture2D texture)
        {
            var xPosition = xOffset + 256 + 64 + 16;
            var yPosition = yOffset - 16;
            SlotNameTextBox = new TextBox(texture, null, Game1.smallFont, Game1.textColor)
            {
                X = xPosition,
                Y = yPosition,
            };
            SlotNameTextBox.limitWidth = false;
            SlotNameTextBox.Text = "";
            var ipAddressRectangle = new Rectangle(xPosition, yPosition, 192, 48);
            slotNameCC = new ClickableComponent(ipAddressRectangle, "")
            {
                myID = 537537,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            var languageOffset = LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.es or LocalizedContentManager.LanguageCode.pt ? -4 : 0;
            slotNameLabel = new ClickableComponent(new Rectangle(xOffset + languageOffset + 16 + 192 + 4, yOffset - 8, 1, 1), "Slot");
            labels.Add(slotNameLabel);
        }

        private void SetupPasswordFieldPosition(int xOffset, int yOffset, Texture2D texture)
        {
            var xPosition = xOffset + 256 + 64 + 16;
            var yPosition = yOffset - 16;
            PasswordTextBox = new TextBox(texture, null, Game1.smallFont, Game1.textColor)
            {
                X = xPosition,
                Y = yPosition,
            };
            PasswordTextBox.limitWidth = false;
            PasswordTextBox.Text = "";
            var ipAddressRectangle = new Rectangle(xPosition, yPosition, 192, 48);
            passwordCC = new ClickableComponent(ipAddressRectangle, "")
            {
                myID = 538538,
                upNeighborID = -99998,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
            var languageOffset = LocalizedContentManager.CurrentLanguageCode is LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.es or LocalizedContentManager.LanguageCode.pt ? -4 : 0;
            passwordLabel = new ClickableComponent(new Rectangle(xOffset + languageOffset + 16 + 192 + 4, yOffset - 8, 1, 1), "Password");
            labels.Add(passwordLabel);
        }
    }
}
