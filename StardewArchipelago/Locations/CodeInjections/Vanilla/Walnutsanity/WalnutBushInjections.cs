﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutBushInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Texture2D _bushtexture;

        public static void Initialize(LogHandler logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _bushtexture = ArchipelagoTextures.GetArchipelagoBush(logger, helper);
            Utility.ForEachLocation((x) => SetupWalnutsanityBushes(x), true, true);
            _archipelago.ScoutWalnutLocations(_bushNameMap.Values);
        }

        // public string GetShakeOffItem()
        public static bool GetShakeOffItem_ReplaceWalnutWithCheck_Prefix(Bush __instance, ref string __result)
        {
            try
            {
                if (__instance.size.Value != 4)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var bushId = $"Bush_{__instance.Location.Name}_{__instance.Tile.X}_{__instance.Tile.Y}";

                if (!_bushNameMap.ContainsKey(bushId))
                {
                    throw new Exception($"Bush '{bushId}' Could not be mapped to an Archipelago location!");
                }

                __result = IDProvider.CreateApLocationItemId(_bushNameMap[bushId]);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetShakeOffItem_ReplaceWalnutWithCheck_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void setUpSourceRect()
        public static bool SetUpSourceRect_UseArchipelagoTexture_Prefix(Bush __instance)
        {
            try
            {
                if (__instance.size.Value != 4)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                SetUpSourceRectForWalnutsanityBush(__instance);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetUpSourceRect_UseArchipelagoTexture_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void draw(SpriteBatch spriteBatch)
        public static bool Draw_UseArchipelagoTexture_Prefix(Bush __instance, SpriteBatch spriteBatch)
        {
            try
            {
                if (__instance.size.Value != 4)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var tile = __instance.Tile;
                var effects = __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (__instance.drawShadow.Value)
                {
                    var shadowPosition = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((tile.X + 0.5) * 64.0 - 51.0), (float)(tile.Y * 64.0 - 16.0)));
                    spriteBatch.Draw(Game1.mouseCursors, shadowPosition, Bush.shadowSourceRect, Color.White, 0.0f, Vector2.Zero, 4f, effects, 1E-06f);
                }
                var globalPosition = new Vector2(tile.X * 64f + 64, (float)((tile.Y + 1.0) * 64.0));
                var position = Game1.GlobalToLocal(Game1.viewport, globalPosition);
                var sourceRectangle = new Rectangle?(__instance.sourceRect.Value);
                var layerDepth = (float)((__instance.getBoundingBox().Center.Y + 48) / 10000.0 - tile.X / 1000000.0);
                // private float shakeRotation;
                var shakeRotationField = _helper.Reflection.GetField<float>(__instance, "shakeRotation");
                var shakeRotation = shakeRotationField.GetValue();
                spriteBatch.Draw(_bushtexture, position, sourceRectangle, Color.White, shakeRotation, new Vector2(16, 32f), 4f, effects, layerDepth);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_UseArchipelagoTexture_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool SetupWalnutsanityBushes(GameLocation gameLocation)
        {
            if (gameLocation is not IslandLocation || !_archipelago.SlotData.Walnutsanity.HasFlag(Archipelago.SlotData.SlotEnums.Walnutsanity.Bushes))
            {
                return true;
            }

            foreach (var largeTerrainFeature in gameLocation.largeTerrainFeatures)
            {
                if (largeTerrainFeature is not Bush bush || bush.size.Value != 4)
                {
                    continue;
                }

                SetUpSourceRectForWalnutsanityBush(bush);
            }

            return true;
        }

        private static void SetUpSourceRectForWalnutsanityBush(Bush bush)
        {
            bush.sourceRect.Value = new Rectangle(bush.tileSheetOffset.Value * 32, 0, 32, 32);
        }

        private static readonly Dictionary<string, string> _bushNameMap = new()
        {
            { "Bush_IslandEast_17_37", "Jungle Bush" },
            { "Bush_IslandShrine_23_34", "Gem Birds Bush" },
            { "Bush_CaptainRoom_2_4", "Shipwreck Bush" },
            { "Bush_IslandWest_38_56", "Bush Behind Coconut Tree" },
            { "Bush_IslandWest_25_30", "Walnut Room Bush" },
            { "Bush_IslandWest_15_3", "Coast Bush" },
            { "Bush_IslandWest_31_24", "Bush Behind Mahogany Tree" },
            { "Bush_IslandWest_54_18", "Below Colored Crystals Cave Bush" },
            { "Bush_IslandWest_64_30", "Cliff Edge Bush" },
            { "Bush_IslandWest_104_3", "Farm Parrot Express Bush" },
            { "Bush_IslandWest_75_29", "Farmhouse Cliff Bush" },
            { "Bush_IslandNorth_9_84", "Grove Bush" },
            { "Bush_IslandNorth_4_42", "Above Dig Site Bush" },
            { "Bush_IslandNorth_45_38", "Above Field Office Bush 1" },
            { "Bush_IslandNorth_47_40", "Above Field Office Bush 2" },
            { "Bush_IslandNorth_56_27", "Bush Behind Volcano Tree" },
            { "Bush_IslandNorth_20_26", "Hidden Passage Bush" },
            { "Bush_IslandNorth_13_33", "Secret Beach Bush 1" },
            { "Bush_IslandNorth_5_30", "Secret Beach Bush 2" },
            { "Bush_Caldera_28_36", "Forge Entrance Bush" },
            { "Bush_Caldera_9_34", "Forge Exit Bush" },
            { "Bush_IslandSouth_31_5", "Cliff Over Island South Bush" },
        };
    }
}
