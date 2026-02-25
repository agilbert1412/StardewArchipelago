using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Extensions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using xTile.Dimensions;

namespace StardewArchipelago.GameModifications.MultiplayerVision
{
    public class VisiblePlayer
    {
#if DEBUG
        public static uint UPDATE_FREQUENCY_TICKS = 42;
        public static uint DRAW_DURATION_TICKS = 60;
        public static uint BROADCAST_APPEARANCE_SECONDS = 5;
#else
        public static uint UPDATE_FREQUENCY_TICKS = 42;
        public static uint DRAW_DURATION_TICKS = 60;
        public static uint BROADCAST_APPEARANCE_SECONDS = 60;
#endif

        public string UniqueIdentifier { get; set; }
        public string MapName { get; set; }
        public Vector2 Position { get; set; }
        public uint RemainingTicks { get; set; }
        public Vector2 Velocity { get; set; }
        public bool IsRidingHorse { get; set; }
        public int FacingDirection { get; set; }
        public float XOffset { get; set; }
        public float YOffset { get; set; }
        public bool IsGlowing { get; set; }
        public bool IsSitting { get; set; }
        public float Rotation { get; set; }

        public Farmer Farmer { get; set; }

        public PlayerAppearance Appearance { get; set; }

        public VisiblePlayer()
        {
            RemainingTicks = DRAW_DURATION_TICKS;
            Appearance = null;
            Farmer = null;
        }

        /// <summary>
        /// Draw and Update the Visible Player
        /// </summary>
        /// <param name="elapsedTicks"></param>
        /// <returns>True if the timeframe for displaying this player is done and they should now be removed</returns>
        public bool DrawAndUpdate(SpriteBatch b, uint elapsedTicks)
        {
            if (RemainingTicks <= 0)
            {
                return true;
            }

            Draw(b);

            Position = new Vector2(Position.X + (Velocity.X * elapsedTicks), Position.Y + (Velocity.Y * elapsedTicks));
            RemainingTicks -= elapsedTicks;
            return RemainingTicks <= 0;
        }

        private void Draw(SpriteBatch b)
        {
            if (Appearance == null || string.IsNullOrWhiteSpace(MapName) || !MapName.Equals(Game1.currentLocation.Name))
            {
                return;
            }

            InitializeFarmer();

            if (Velocity.X > 0)
            {
                Farmer.movementDirections.Add(1);
            }
            else if (Velocity.X < 0)
            {
                Farmer.movementDirections.Add(3);
            }
            if (Velocity.Y > 0)
            {
                Farmer.movementDirections.Add(2);
            }
            else if (Velocity.Y < 0)
            {
                Farmer.movementDirections.Add(0);
            }
            Farmer.Position = Position;
            //Farmer.FarmerSprite.currentAnimationIndex = CurrentAnimationIndex;
            // Farmer.FarmerSprite.CurrentAnimationFrame.flip = Flip;
            Farmer.FacingDirection = FacingDirection;
            Farmer.FarmerSprite.faceDirection(FacingDirection);
            Farmer.running = Math.Sqrt(Math.Pow(Velocity.X, 2) + Math.Pow(Velocity.Y, 2)) >= 5;
            Farmer.updateMovementAnimation(Game1.currentGameTime);
            var drawLayer = Game1.player.getDrawLayer();
            if (IsRidingHorse)
            {
                //this.mount.SyncPositionToRider();
                //this.mount.draw(b);
                if (FacingDirection == 3 || FacingDirection == 1)
                {
                    drawLayer += 1f / 625f;
                }
            }

            var layerDepth = (double)FarmerRenderer.GetLayerDepth(0.0f, FarmerRenderer.FarmerSpriteLayers.MAX);
            var origin = new Vector2(XOffset, (float)(((double)YOffset + 128.0 - (Farmer.GetBoundingBox().Height / 2)) / 4.0 + 4.0));
            var tile = Game1.currentLocation.Map.RequireLayer("Buildings").PickTile(new Location((int)Farmer.Position.X, (int)Farmer.Position.Y), Game1.viewport.Size);
            var num1 = (float)(layerDepth * 1.0);
            var num2 = (float)(layerDepth * 2.0);
            if (IsGlowing)
            {
                if (Farmer.coloredBorder)
                {
                    b.Draw(Farmer.Sprite.Texture, new Vector2(Farmer.getLocalPosition(Game1.viewport).X - 4f, Farmer.getLocalPosition(Game1.viewport).Y - 4f), new Microsoft.Xna.Framework.Rectangle?(Farmer.Sprite.SourceRect), Farmer.glowingColor * Farmer.glowingTransparency, 0.0f, Vector2.Zero, 1.1f, SpriteEffects.None, drawLayer + num1);
                }
                else
                {
                    Farmer.FarmerRenderer.draw(b, Farmer.FarmerSprite, Farmer.FarmerSprite.SourceRect, Farmer.getLocalPosition(Game1.viewport) + Farmer.jitter + new Vector2(0.0f, (float)Farmer.yJumpOffset), origin, drawLayer + num1, Farmer.glowingColor * Farmer.glowingTransparency, Rotation, Farmer);
                }
            }
            var nullable = tile?.TileIndexProperties.ContainsKey("Shadow");
            var mask = new Color(255, 255, 255, 255);
            if ((!nullable.HasValue ? 0 : (nullable.GetValueOrDefault() ? 1 : 0)) == 0)
            {
                if (IsSitting || !Game1.shouldTimePass() || !Farmer.temporarilyInvincible || !Farmer.flashDuringThisTemporaryInvincibility || Farmer.temporaryInvincibilityTimer % 100 < 50)
                {
                    Farmer.FarmerRenderer.draw(b, Farmer.FarmerSprite, Farmer.FarmerSprite.SourceRect, Farmer.getLocalPosition(Game1.viewport) + Farmer.jitter + new Vector2(0.0f, (float)Farmer.yJumpOffset), origin, drawLayer, mask, Rotation, Farmer);
                }
            }
            else
            {
                Farmer.FarmerRenderer.draw(b, Farmer.FarmerSprite, Farmer.FarmerSprite.SourceRect, Farmer.getLocalPosition(Game1.viewport), origin, drawLayer, mask, Rotation, Farmer);
                Farmer.FarmerRenderer.draw(b, Farmer.FarmerSprite, Farmer.FarmerSprite.SourceRect, Farmer.getLocalPosition(Game1.viewport), origin, drawLayer + num2, Color.Black * 0.25f, Rotation, Farmer);
            }

            if (Farmer.isRafting)
            {
                b.Draw(Game1.toolSpriteSheet, Farmer.getLocalPosition(Game1.viewport) + new Vector2(0.0f, YOffset), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.toolSpriteSheet, 1)), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, FarmerRenderer.GetLayerDepth(drawLayer, FarmerRenderer.FarmerSpriteLayers.ToolUp));
            }
            if (Game1.activeClickableMenu == null && !Game1.eventUp && Farmer.IsLocalPlayer && Farmer.CurrentTool != null && (Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.options.alwaysShowToolHitLocation) && Farmer.CurrentTool.doesShowTileLocationMarker() && (!Game1.options.hideToolHitLocationWhenInMotion || !Farmer.isMoving()))
            {
                var target_position = Utility.PointToVector2(Game1.getMousePosition()) + new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
                var local = Game1.GlobalToLocal(Game1.viewport, Utility.clampToTile(Farmer.GetToolLocation(target_position)));
                b.Draw(Game1.mouseCursors, local, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29)), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, local.Y / 10000f);
            }
            if (Farmer.IsEmoting)
            {
                Vector2 localPosition = Farmer.getLocalPosition(Game1.viewport);
                localPosition.Y -= 160f;
                b.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(Farmer.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, Farmer.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, drawLayer);
            }
            if (Farmer.ActiveObject != null && Farmer.IsCarrying())
            {
                Game1.drawPlayerHeldObject(Farmer);
            }
            Farmer.sparklingText?.draw(b, Game1.GlobalToLocal(Game1.viewport, Farmer.Position + new Vector2((float)(32.0 - (double)Farmer.sparklingText.textWidth / 2.0), (float)sbyte.MinValue)));
            if (Farmer.UsingTool && Farmer.CurrentTool != null)
            {
                Game1.drawTool(Farmer);
            }
            foreach (Companion companion in Farmer.companions)
            {
                companion.Draw(b);
            }
        }
        private void InitializeFarmer()
        {
            if (Farmer != null)
            {
                return;
            }

            // For Debug
            //if (Velocity.X > 0)
            //{
            //    Position = new Vector2(Position.X - 128, Position.Y);
            //}
            //else if (Velocity.X < 0)
            //{
            //    Position = new Vector2(Position.X + 128, Position.Y);
            //}
            //if (Velocity.Y > 0)
            //{
            //    Position = new Vector2(Position.X, Position.Y - 128);
            //}
            //else if (Velocity.Y < 0)
            //{
            //    Position = new Vector2(Position.X, Position.Y + 128);
            //}
            //Appearance.Hair = 52; // Bald Hair
            //Appearance.Skin = 3;
            //Appearance.CurrentEyes = 3;
            //Appearance.ShirtId = "3";
            //Appearance.PantsId = "3";
            //Appearance.HatId = 3;

            Farmer = Game1.player.CreateFakeEventFarmer();
            // Farmer.FarmerSprite.CurrentAnimation = Game1.player.FarmerSprite.CurrentAnimation.Select(x => x.DeepClone()).ToList();
            Farmer.Gender = Appearance.IsMale ? Gender.Male : Gender.Female;

            Farmer.changeSkinColor(Appearance.Skin);
            Farmer.changeHairStyle(Appearance.Hair);
            var hairColor = new Color(Appearance.HairColorRed, Appearance.HairColorGreen, Appearance.HairColorBlue);
            Farmer.changeHairColor(hairColor);
            Farmer.currentEyes = 0;
            var eyeColor = new Color(Appearance.EyeColorRed, Appearance.EyeColorGreen, Appearance.EyeColorBlue);
            Farmer.changeEyeColor(eyeColor);
            Farmer.changeAccessory(Appearance.Accessory);

            if (string.IsNullOrWhiteSpace(Appearance.HatId))
            {
                Farmer.Equip(null, Farmer.hat);
            }
            else
            {
                Farmer.Equip(ItemRegistry.Create<Hat>("(H)" + Appearance.HatId), Farmer.hat);
            }
            Farmer.changeShirt(Appearance.ShirtId);

            if (string.IsNullOrWhiteSpace(Appearance.PantsId))
            {
                Farmer.Equip(null, Farmer.pantsItem);
            }
            else
            {
                Farmer.Equip(ItemRegistry.Create<Clothing>("(P)" + Appearance.PantsId), Farmer.pantsItem);
            }
            Farmer.changePantStyle(Appearance.PantsId);
            var pantsColor = new Color(Appearance.PantsColorRed, Appearance.PantsColorGreen, Appearance.PantsColorBlue);
            Farmer.changePantsColor(pantsColor);
            Farmer.changeShoeColor(Appearance.ShoesId);
        }
    }
}
