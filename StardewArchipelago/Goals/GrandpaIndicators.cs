using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Goals
{
    public class GrandpaIndicators
    {
        private const int INDICATOR_SIZE = 24;

        private Vector2 _currentMousePosition;

        private readonly LogHandler _logger;
        private readonly IModHelper _modHelper;
        private readonly StardewArchipelagoClient _archipelago;

        public GrandpaIndicators(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            var preference = ModEntry.Instance.Config.ShowGrandpaShrineIndicators;
            if (preference == GrandpaShrinePreference.Never)
            {
                return;
            }
            if (preference == GrandpaShrinePreference.GrandpaGoal && _archipelago.SlotData.Goal != Goal.GrandpaEvaluation)
            {
                return;
            }
            _modHelper.Events.Display.RenderingHud += OnRenderingHud;
            _modHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        public void EvaluateGrandpaToday(Farm farm)
        {
            var score = Utility.getGrandpaScore();
            var candlesFromScore = Utility.getGrandpaCandlesFromScore(score);
            farm.grandpaScore.Value = candlesFromScore;
            for (var index = 0; index < candlesFromScore; ++index)
            {
                DelayedAction.playSoundAfterDelay("fireball", 100 * index);
            }
            farm.addGrandpaCandles();
            AddArchipelagoIconsForPoints(farm);
        }

        private void AddArchipelagoIconsForPoints(Farm farm)
        {
            var preference = ModEntry.Instance.Config.ShowGrandpaShrineIndicators;
            if (preference == GrandpaShrinePreference.Never)
            {
                return;
            }
            if (preference == GrandpaShrinePreference.GrandpaGoal && _archipelago.SlotData.Goal != Goal.GrandpaEvaluation)
            {
                return;
            }

            var whiteArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(INDICATOR_SIZE, ArchipelagoTextures.WHITE);
            var coloredArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(INDICATOR_SIZE, ArchipelagoTextures.COLOR);

            var grandpaShrinePosition = farm.GetGrandpaShrinePosition();
            var localId = 6666 + _archipelago.SlotData.Seed.GetHashCode();
            farm.removeTemporarySpritesWithIDLocal(localId);

            for (var i = 0; i < 21; i++)
            {
                var texture = whiteArchipelagoIcon;
                var textureName = $"{ArchipelagoTextures.WHITE}_{INDICATOR_SIZE}";
                if (_indicatorTestMethods[i]())
                {
                    texture = coloredArchipelagoIcon;
                    textureName = $"{ArchipelagoTextures.COLOR}_{INDICATOR_SIZE}";
                }

                var sprite = new TemporaryAnimatedSprite();
                sprite.texture = texture;
                sprite.textureName = textureName;

                var sourceRect = new Rectangle(0, 0, INDICATOR_SIZE, INDICATOR_SIZE);
                var column = i % 7;
                var row = i / 7;
                var position = GetIndicatorPosition(grandpaShrinePosition, column, row);

                sprite.currentParentTileIndex = 0;
                sprite.initialParentTileIndex = 0;
                sprite.position = position;
                sprite.flicker = false;
                sprite.flipped = false;
                sprite.drawAboveAlwaysFront = true;

                sprite.sourceRect = sourceRect;
                sprite.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
                sprite.initialPosition = position;
                sprite.alphaFade = 0.0f;
                sprite.color = Color.White;

                sprite.interval = 50f;
                sprite.totalNumberOfLoops = 99999;
                sprite.animationLength = 1;
                sprite.lightId = $"Farm_GrandpaArchipelagoScore_{i}";
                sprite.id = localId;
                sprite.lightRadius = 1f;
                sprite.scale = 1f;
                sprite.layerDepth = 0.03f;
                sprite.delayBeforeAnimationStart = i * 50;

                farm.temporarySprites.Add(sprite);
            }
        }

        private static Vector2 GetIndicatorPosition(Point grandpaShrinePosition, int column, int row)
        {
            var positionX = ((grandpaShrinePosition.X - 2) * 64) + 50 + (column * 32);
            var positionY = ((grandpaShrinePosition.Y - 3) * 64) + 8 + (row * 32);
            var position = new Vector2(positionX, positionY);
            return position;
        }

        private static bool TryGetIndicatorIndex(Point grandpaShrinePosition, Vector2 position, out int index)
        {
            index = -1;

            var originX = ((grandpaShrinePosition.X - 2) * 64) + 50;
            var originY = ((grandpaShrinePosition.Y - 3) * 64) + 8;
            var width = (7 * 32);
            var height = (3 * 32);
            if (position.X < originX || position.X > originX + width || position.Y < originY || position.Y > originY + height)
            {
                return false;
            }

            var offsetPositionX = position.X - originX;
            var offsetPositionY = position.Y - originY;
            if (offsetPositionX % 32 > 24 || offsetPositionY % 32 > 24)
            {
                return false;
            }

            var column = (int)Math.Floor(offsetPositionX / 32);
            var row = (int)Math.Floor(offsetPositionY / 32);

            index = (row * 7) + column;
            return true;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(4))
            {
                return;
            }

            if (Game1.currentLocation is not Farm)
            {
                _currentMousePosition = new Vector2(-1, -1);
                return;
            }

            _currentMousePosition = new Vector2(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY());
        }

        /// <summary>
        ///   Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this
        ///   point (e.g. because a menu is open).
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null)
            {
                return;
            }

            if (!TryGetIndicatorIndex(Game1.getFarm().GetGrandpaShrinePosition(), _currentMousePosition, out var indicatorIndex))
            {
                return;
            }

            IClickableMenu.drawHoverText(
                Game1.spriteBatch,
                string.Join('\n', _indicatorDescriptions[indicatorIndex]),
                Game1.smallFont,
                overrideX: -1,
                overrideY: -1
            );
        }

        private readonly Dictionary<int, string> _indicatorDescriptions = new()
        {
            { 0, "50,000g" },
            { 1, "100,000g" },
            { 2, "200,000g" },
            { 3, "300,000g" },
            { 4, "500,000g" },
            { 5, "1,000,000g" },
            { 6, "1,000,000g" },
            { 7, "30 Levels Total" },
            { 8, "50 Levels Total" },
            { 9, "Complete Collection" },
            { 10, "Master Angler" },
            { 11, "Full Shipment" },
            { 12, "Married with two house upgrades" },
            { 13, "5 good friends" },
            { 14, "10 good friends" },
            { 15, "Pet loves you" },
            { 16, "Community Center" },
            { 17, "Community Center Ceremony" },
            { 18, "Community Center Ceremony" },
            { 19, "Skull Key" },
            { 20, "Rusty Key" },
        };

        private readonly Dictionary<int, Func<bool>> _indicatorTestMethods = new()
        {
            { 0, () => Game1.player.totalMoneyEarned >= 50000U },
            { 1, () => Game1.player.totalMoneyEarned >= 100000U },
            { 2, () => Game1.player.totalMoneyEarned >= 200000U },
            { 3, () => Game1.player.totalMoneyEarned >= 300000U },
            { 4, () => Game1.player.totalMoneyEarned >= 500000U },
            { 5, () => Game1.player.totalMoneyEarned >= 1000000U },
            { 6, () => Game1.player.totalMoneyEarned >= 1000000U },
            { 7, () => Game1.player.Level >= 15 },
            { 8, () => Game1.player.Level >= 25 },
            { 9, () => Game1.player.achievements.Contains(5) },
            { 10, () => Game1.player.achievements.Contains(26) },
            { 11, () => Game1.player.achievements.Contains(34) },
            { 12, () => Game1.player.isMarriedOrRoommates() && Utility.getHomeOfFarmer(Game1.player).upgradeLevel >= 2 },
            { 13, () => Utility.getNumberOfFriendsWithinThisRange(Game1.player, 1975, 999999) >= 5 },
            { 14, () => Utility.getNumberOfFriendsWithinThisRange(Game1.player, 1975, 999999) >= 10 },
            { 15, () => Game1.player.mailReceived.Contains("petLoveMessage") },
            { 16, () => Game1.player.hasCompletedCommunityCenter() },
            { 17, () => Utility.HasAnyPlayerSeenEvent("191393") },
            { 18, () => Utility.HasAnyPlayerSeenEvent("191393") },
            { 19, () => Game1.player.hasSkullKey },
            { 20, () => Game1.player.hasRustyKey },
        };
    }
}