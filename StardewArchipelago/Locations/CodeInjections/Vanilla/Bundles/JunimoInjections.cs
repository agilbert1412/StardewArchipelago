using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;
using System.IO;
using System.Linq;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class JunimoInjections
    {
        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        
        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state)
        {
            _logger = logger;
            _modHelper = modHelper;
            _state = state;
        }

        // public override void draw(SpriteBatch b, float alpha = 1f)
        public static void Draw_DrawBeautifulHair_Postfix(Junimo __instance, SpriteBatch b, float alpha)
        {
            try
            {
                if (_state.Wallet.DonatedHair == BundleCurrencyManager.BALD_HAIR || __instance.IsInvisible)
                {
                    return;
                }

                var facingDirection = FacingDirection.Down;
                if (__instance.Sprite.currentFrame >= 32)
                {
                    facingDirection = FacingDirection.Up;
                }
                else if (__instance.Sprite.currentFrame >= 16)
                {
                    facingDirection = __instance.flip ? FacingDirection.Left : FacingDirection.Right;
                }
                DrawBeautifulHair(__instance, b, facingDirection);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_DrawBeautifulHair_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void draw(SpriteBatch b, float alpha = 1f)
        public static void DrawHarvester_DrawBeautifulHair_Postfix(JunimoHarvester __instance, SpriteBatch b, float alpha)
        {
            try
            {
                if (_state.Wallet.DonatedHair == BundleCurrencyManager.BALD_HAIR || __instance.IsInvisible)
                {
                    return;
                }


                var facingDirection = (FacingDirection)__instance.FacingDirection;
                if (__instance.Sprite.currentFrame >= 32)
                {
                    facingDirection = FacingDirection.Up;
                }
                else if (__instance.Sprite.currentFrame >= 16)
                {
                    facingDirection = __instance.flip ? FacingDirection.Left : FacingDirection.Right;
                }
                DrawBeautifulHair(__instance, b, facingDirection);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawHarvester_DrawBeautifulHair_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawBeautifulHair(NPC __instance, SpriteBatch b, FacingDirection facingDirection)
        {

            var farmer = Game1.player;
            var hairIndex = _state.Wallet.DonatedHair;
            var hairColor = new Color(_state.Wallet.DonatedHairColor[0], _state.Wallet.DonatedHairColor[1], _state.Wallet.DonatedHairColor[2]);
            var hairStyleMetadata = Farmer.GetHairStyleMetadata(hairIndex);
            var hairTexture = hairStyleMetadata?.texture ?? FarmerRenderer.hairStylesTexture;
            var hairSourceRect = hairStyleMetadata != null
                ? new Rectangle(hairStyleMetadata.tileX * 16, hairStyleMetadata.tileY * 16, 16, 32)
                : new Rectangle(hairIndex * 16 % FarmerRenderer.hairStylesTexture.Width, hairIndex * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);

            var position = __instance.Position;
            var origin = Vector2.Zero;
            var scale = 3.5f;
            var featureXOffsetPerFrame = 1;// FarmerRenderer.featureXOffsetPerFrame[currentFrame];
            var yOffsets = new[] { 0, 1, 2, 0, -1, -2, -2, -1 };
            var featureYOffsetPerFrame = yOffsets[(__instance.Sprite.currentFrame+5) % 8]/2;// FarmerRenderer.featureYOffsetPerFrame[currentFrame];
            var rotation = 0.0f;
            var layerDepth = 1f;

            var hairOffsetY = !farmer.IsMale || hairIndex < 16 ? (farmer.IsMale || hairIndex >= 16 ? 0 : 4) : -4;
            var offset = new Vector2(featureXOffsetPerFrame * 4, featureYOffsetPerFrame * 4 + 4 + hairOffsetY);
            var drawPosition = position + origin + __instance.drawOffset + offset;
            drawPosition = Game1.GlobalToLocal(drawPosition);
            var hairLayer = FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair);

            switch (facingDirection)
            {
                case FacingDirection.Up:
                    hairSourceRect.Offset(0, 64);
                    b.Draw(hairTexture, drawPosition, hairSourceRect, hairColor, rotation, origin, scale, SpriteEffects.None, hairLayer);
                    break;
                case FacingDirection.Right:
                    hairSourceRect.Offset(0, 32);
                    offset = new Vector2(featureXOffsetPerFrame * 4, featureYOffsetPerFrame * 4 + hairOffsetY);
                    drawPosition = position + origin + __instance.drawOffset + offset;
                    drawPosition = Game1.GlobalToLocal(drawPosition);
                    b.Draw(hairTexture, drawPosition, hairSourceRect, hairColor, rotation, origin, scale, SpriteEffects.None, hairLayer);
                    break;
                case FacingDirection.Down:
                    offset = new Vector2(featureXOffsetPerFrame * 4, featureYOffsetPerFrame * 4 + hairOffsetY);
                    drawPosition = position + origin + __instance.drawOffset + offset;
                    drawPosition = Game1.GlobalToLocal(drawPosition);
                    b.Draw(hairTexture, drawPosition, hairSourceRect, hairColor, rotation, origin, scale, SpriteEffects.None, hairLayer);
                    break;
                case FacingDirection.Left:
                    var hairFlip = true;
                    if (hairStyleMetadata != null && hairStyleMetadata.usesUniqueLeftSprite)
                    {
                        hairFlip = false;
                        hairSourceRect.Offset(0, 96);
                    }
                    else
                    {
                        hairSourceRect.Offset(0, 32);
                    }
                    offset = new Vector2(-featureXOffsetPerFrame * 4, featureYOffsetPerFrame * 4 + hairOffsetY);
                    drawPosition = position + origin + __instance.drawOffset + offset;
                    drawPosition = Game1.GlobalToLocal(drawPosition);
                    b.Draw(hairTexture, drawPosition, hairSourceRect, hairColor, rotation, origin, scale, hairFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, hairLayer);
                    break;

            }
        }
    }
}
