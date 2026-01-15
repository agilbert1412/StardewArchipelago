using KaitoKid.ArchipelagoUtilities.Net.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using System;
using System.Linq;
using StardewArchipelago.Locations;

namespace StardewArchipelago.GameModifications.Tooltips
{
    public class SpecialOrderBoardInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;
        private static Texture2D _bigArchipelagoIcon;
        public static ClickableComponent _rerollButton;
        private const string REROLL_TEXT = "Reroll";

        public static void Initialize(LogHandler logger, IModHelper modHelper, ArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;

            var desiredTextureName = ArchipelagoTextures.COLOR;
            _bigArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(48, desiredTextureName);
        }

        // public SpecialOrdersBoard(string board_type = "")
        public static void SpecialOrdersBoardConstructor_AddRerollButton_Postfix(SpecialOrdersBoard __instance, string board_type)
        {
            try
            {
                var buttonWidth = (int)Game1.dialogueFont.MeasureString(REROLL_TEXT).X + 24;
                var buttonHeight = (int)Game1.dialogueFont.MeasureString(REROLL_TEXT).Y + 24;
                var buttonX = __instance.xPositionOnScreen + __instance.width * 2 / 4 - (buttonWidth/2);
                var buttonY = __instance.yPositionOnScreen + __instance.height - 128;
                var bounds = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
                _rerollButton = new ClickableComponent(bounds, "")
                {
                    myID = 111,
                    leftNeighborID = -99998,
                    rightNeighborID = -99998,
                    upNeighborID = -99998,
                    downNeighborID = -99998
                };
                UpdateRerollButtonVisibility(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SpecialOrdersBoardConstructor_AddRerollButton_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void UpdateRerollButtonVisibility(SpecialOrdersBoard board)
        {
            _rerollButton.visible = board.acceptLeftQuestButton.visible && board.acceptRightQuestButton.visible && board.GetOrderType() != "DesertFestivalMarlon";
        }

        // public override void draw(SpriteBatch spriteBatch)
        public static void Draw_AddArchipelagoAdditions_Postfix(SpecialOrdersBoard __instance, SpriteBatch b)
        {
            try
            {
                DrawRerollButton(__instance, b);
                DrawOrderIcons(__instance.leftOrder, __instance.acceptLeftQuestButton, b);
                DrawOrderIcons(__instance.rightOrder, __instance.acceptRightQuestButton, b);
                if (Game1.options.SnappyMenus && !__instance.acceptLeftQuestButton.visible && !__instance.acceptRightQuestButton.visible && !_rerollButton.visible)
                {
                    return;
                }
                __instance.drawMouse(b);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddArchipelagoAdditions_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawRerollButton(SpecialOrdersBoard specialOrdersBoard, SpriteBatch spriteBatch)
        {
            if (_rerollButton.visible)
            {
                IClickableMenu.drawTextureBox(spriteBatch, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), _rerollButton.bounds.X, _rerollButton.bounds.Y, _rerollButton.bounds.Width, _rerollButton.bounds.Height, _rerollButton.scale > 1.0 ? Color.LightPink : Color.White, 4f * _rerollButton.scale);
                Utility.drawTextWithShadow(spriteBatch, REROLL_TEXT, Game1.dialogueFont, new Vector2(_rerollButton.bounds.X + 12, _rerollButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
            }
        }

        private static void DrawOrderIcons(SpecialOrder specialOrder, ClickableComponent acceptButton, SpriteBatch spriteBatch)
        {
            if (specialOrder == null || acceptButton == null || !acceptButton.visible)
            {
                return;
            }

            var dailyQuestCheckName = SpecialOrderInjections.GetEnglishQuestName(specialOrder.questName.Value);
            if (!_locationChecker.GetAllLocationsNotCheckedContainingWord(dailyQuestCheckName).Any())
            {
                return;
            }

            var size = 48;
            var position1 = new Vector2(acceptButton.bounds.X - size - 12, acceptButton.bounds.Y + 12);
            var position2 = new Vector2(acceptButton.bounds.X + acceptButton.bounds.Width + 12, acceptButton.bounds.Y + 12);
            var sourceRectangle = new Rectangle(0, 0, size, size);
            var color = Color.White;
            spriteBatch.Draw(_bigArchipelagoIcon, position1, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.Draw(_bigArchipelagoIcon, position2, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }

        // public override void performHoverAction(int x, int y)
        public static void PerformHoverAction_HoverRerollButton_Postfix(SpecialOrdersBoard __instance, int x, int y)
        {
            try
            {
                if (Game1.questOfTheDay == null || Game1.questOfTheDay.accepted.Value)
                {
                    var leftScale = __instance.acceptLeftQuestButton.scale;
                    __instance.acceptLeftQuestButton.scale = __instance.acceptLeftQuestButton.bounds.Contains(x, y) ? 1.5f : 1f;
                    if (__instance.acceptLeftQuestButton.scale > (double)leftScale)
                    {
                        Game1.playSound("Cowboy_gunshot");
                    }
                    var rightScale = __instance.acceptRightQuestButton.scale;
                    __instance.acceptRightQuestButton.scale = __instance.acceptRightQuestButton.bounds.Contains(x, y) ? 1.5f : 1f;
                    if (!(__instance.acceptRightQuestButton.scale <= (double)rightScale))
                    {
                        Game1.playSound("Cowboy_gunshot");
                    }
                }

                if (!_rerollButton.visible)
                {
                    return;
                }
                var scale1 = _rerollButton.scale;
                _rerollButton.scale = _rerollButton.bounds.Contains(x, y) ? 1.5f : 1f;
                if (_rerollButton.scale > (double)scale1)
                {
                    Game1.playSound("Cowboy_gunshot");
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformHoverAction_HoverRerollButton_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_ClickRerollButton_Postfix(SpecialOrdersBoard __instance, int x, int y, bool playSound)
        {
            try
            {
                if (_rerollButton.visible && _rerollButton.containsPoint(x, y))
                {
                    Game1.playSound("newArtifact");
                    SpecialOrderInjections.IncrementRerollCount();
                    SpecialOrder.UpdateAvailableSpecialOrders(__instance.GetOrderType(), true);
                    __instance.leftOrder = Game1.player.team.GetAvailableSpecialOrder(type: __instance.GetOrderType());
                    __instance.rightOrder = Game1.player.team.GetAvailableSpecialOrder(1, __instance.GetOrderType());
                    __instance.UpdateButtons();
                }

                UpdateRerollButtonVisibility(__instance);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_ClickRerollButton_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
