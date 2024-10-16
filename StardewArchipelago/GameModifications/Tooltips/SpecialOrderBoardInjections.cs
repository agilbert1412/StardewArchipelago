﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
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

        public static void Initialize(LogHandler logger, IModHelper modHelper, ArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;

            var desiredTextureName = ArchipelagoTextures.COLOR;
            _bigArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(logger, modHelper, 48, desiredTextureName);
        }

        // public override void draw(SpriteBatch spriteBatch)
        public static void Draw_AddArchipelagoIndicators_Postfix(SpecialOrdersBoard __instance, SpriteBatch b)
        {
            try
            {
                DrawOrderIcons(__instance.leftOrder, __instance.acceptLeftQuestButton, b);
                DrawOrderIcons(__instance.rightOrder, __instance.acceptRightQuestButton, b);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddArchipelagoIndicators_Postfix)}:\n{ex}");
                return;
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
    }
}
