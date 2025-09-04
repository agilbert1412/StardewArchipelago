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
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class ThrowInWaterInjections
    {
        private const string HONEYWELL_FILE = "honeywell.wav";
        private const string HONEYWELL_CUE = "honeywell";

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static GiftSender _giftSender;

        private static List<JumpingFish> _jumpingFish = new();
        private static List<(Object fish, Vector2 position, int jumpsRemaining, float delay)> _fishToJump = new();

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, GiftSender giftSender)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _giftSender = giftSender;
            var currentModFolder = _modHelper.DirectoryPath;
            var soundsFolder = "Sounds";
            var relativePathToSound = Path.Combine(currentModFolder, soundsFolder, HONEYWELL_FILE);
            var honeywellCueDefinition = new CueDefinition(HONEYWELL_CUE, SoundEffect.FromFile(relativePathToSound), 0);
            Game1.soundBank.AddCue(honeywellCueDefinition);
        }

        // public virtual bool doAction(Vector2 tileLocation, Farmer who)
        public static bool DoAction_ThrowHoneyInWell_Prefix(Building __instance, Vector2 tileLocation, Farmer who, ref bool __result)
        {
            try
            {
                if (__instance.buildingType.Value != "Well")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.daysOfConstructionLeft.Value > 0 || !__instance.occupiesTile(tileLocation))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var activeObject = who.ActiveObject;
                if (activeObject == null || activeObject.QualifiedItemId != QualifiedItemIds.HONEY)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (who.isMoving())
                {
                    Game1.haltAfterCheck = false;
                }

                var honey = who.ActiveObject;
                var quality = honey.Quality;
                var preserveType = honey.preserve.Value;
                var preserveId = honey.preservedParentSheetIndex.Value;
                ShowObjectThrownIntoBuildingAnimation(__instance, who, who.ActiveObject, () =>
                {
                    Game1.playSound(HONEYWELL_CUE);
                    ArchipelagoJunimoNoteMenu.CompleteBundleIfExists(MemeBundleNames.HONEYWELL);
                    var giftHoney = ItemRegistry.Create<Object>(ObjectIds.HONEY, 1, quality);
                    giftHoney.preserve.Set(preserveType);
                    giftHoney.preservedParentSheetIndex.Set(preserveId);
                    _giftSender.SendSilentGift("Honeywell", giftHoney).FireAndForget();
                });
                who.reduceActiveItemByOne();
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoAction_ThrowHoneyInWell_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_ThrowPollutionAndFishInWater_Prefix(GameLocation __instance, xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var isWater = CrabPot.IsValidCrabPotLocationTile(__instance, (int)tileLocation.X, (int)tileLocation.Y);
                var activeObject = who.ActiveObject;
                if (!isWater || activeObject == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var validObjects = new[] { QualifiedItemIds.TRASH, QualifiedItemIds.BROKEN_GLASSES, QualifiedItemIds.BROKEN_CD, QualifiedItemIds.SOGGY_NEWSPAPER, QualifiedItemIds.JOJA_COLA, QualifiedItemIds.BATTERY_PACK };
                if (activeObject.Category != Category.FISH && !validObjects.Contains(activeObject.QualifiedItemId))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (who.isMoving())
                {
                    Game1.haltAfterCheck = false;
                }

                var honey = who.ActiveObject;
                var quality = honey.Quality;
                var offsetX = who.FacingDirection == 1 ? 1f : (who.FacingDirection == 3 ? -1f : 0f);
                var offsetY = who.FacingDirection == 2 ? 1f : (who.FacingDirection == 0 ? -1f : 0f);
                var vectorLocation = new Vector2(tileLocation.X + offsetX, tileLocation.Y + offsetY);
                ShowObjectThrownIntoWaterAnimation(vectorLocation, who, activeObject, () =>
                {
                    var donatedAmount = ArchipelagoJunimoNoteMenu.TryDonateToBundle(MemeBundleNames.POLLUTION, activeObject.Name, 1);
                    if (donatedAmount == 0)
                    {
                        donatedAmount = ArchipelagoJunimoNoteMenu.TryDonateToBundle(MemeBundleNames.CATCH_AND_RELEASE, activeObject.Name, 1);
                    }

                    if (activeObject.Category == Category.FISH)
                    {
                        _fishToJump.Add(new(activeObject, vectorLocation, 8, 2f));
                    }
                });
                who.reduceActiveItemByOne();
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_ThrowPollutionAndFishInWater_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void UpdateWhenCurrentLocation(GameTime time)
        public static void UpdateWhenCurrentLocation_WaterWithFish_Postfix(GameLocation __instance, GameTime time)
        {
            try
            {
                for (var index = _fishToJump.Count - 1; index >= 0; index--)
                {
                    var (fish, position, jumpsRemaining, delay) = _fishToJump[index];

                    delay -= (float)time.ElapsedGameTime.TotalSeconds;
                    if (delay <= 0.0 && JumpFish(__instance, fish, position))
                    {
                        --jumpsRemaining;
                        delay = Utility.RandomFloat(1.1f, 2.1f);
                    }

                    if (jumpsRemaining > 0)
                    {
                        _fishToJump[index] = (fish, position, jumpsRemaining, delay);
                    }
                    else
                    {
                        _fishToJump.RemoveAt(index);
                    }
                }
                for (var index = 0; index < _jumpingFish.Count; ++index)
                {
                    var jumpingFish = _jumpingFish[index];
                    var elapsedGameTime = time.ElapsedGameTime;
                    var totalSeconds = elapsedGameTime.TotalSeconds;
                    if (jumpingFish.Update((float)totalSeconds))
                    {
                        _jumpingFish.RemoveAt(index);
                        --index;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UpdateWhenCurrentLocation_WaterWithFish_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual void draw(SpriteBatch b)
        public static void Draw_JumpingFish_Postfix(GameLocation __instance, SpriteBatch b)
        {
            try
            {
                for (var index = 0; index < _jumpingFish.Count; ++index)
                {
                    var jumpingFish = _jumpingFish[index];
                    jumpingFish.Draw(b);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_JumpingFish_Postfix)}:\n{ex}");
                return;
            }
        }

        public static bool JumpFish(GameLocation gameLocation, Object fish, Vector2 position)
        {
            var randomStartOffset = new Vector2(GetJumpOffset(), GetJumpOffset());
            var randomEndOffset = new Vector2(GetJumpOffset(), GetJumpOffset());
            _jumpingFish.Add(new JumpingFish(gameLocation, fish, (position + randomStartOffset) * 64f, (position + randomEndOffset) * 64f));
            return true;
        }

        private static float GetJumpOffset()
        {
            return Game1.random.NextSingle() * 1.5f - 0.75f;
        }

        private static void ShowObjectThrownIntoBuildingAnimation(Building building, Farmer who, Object whichObject, Action callback = null)
        {
            var centerTile = GetCenterTile(building);
            ShowObjectThrownIntoWaterAnimation(centerTile, who, whichObject, callback);
        }

        private static void ShowObjectThrownIntoWaterAnimation(Vector2 targetTile, Farmer who, Object whichObject, Action callback = null)
        {
            who.faceGeneralDirection(targetTile * 64f + new Vector2(32f, 32f));
            if (who.FacingDirection == 1 || who.FacingDirection == 3)
            {
                var distanceToCenterTile = Vector2.Distance(who.Position, targetTile * 64f);
                var verticalDistance = (float)(targetTile.Y * 64.0 + 32.0) - who.position.Y;
                var num3 = distanceToCenterTile - 8f;
                var y = 1f / 400f;
                var num4 = num3 * (float)Math.Sqrt(y / (2.0 * (num3 + 96.0)));
                var delay = (float)(2.0 * (num4 / (double)y)) + ((float)Math.Sqrt(num4 * (double)num4 + 2.0 * y * 96.0) - num4) / y + verticalDistance;
                var num5 = 0.0f;
                if (verticalDistance > 0.0)
                {
                    num5 = verticalDistance / 832f;
                    delay += num5 * 200f;
                }
                Game1.playSound("throwDownITem");
                var sprites = new TemporaryAnimatedSpriteList();
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(whichObject.QualifiedItemId);
                sprites.Add(new TemporaryAnimatedSprite(dataOrErrorItem.GetTextureName(), dataOrErrorItem.GetSourceRect(), who.Position + new Vector2(0.0f, -64f), false, 0.0f, Color.White)
                {
                    scale = 4f,
                    layerDepth = 1f,
                    totalNumberOfLoops = 1,
                    interval = delay,
                    motion = new Vector2((who.FacingDirection == 3 ? -1f : 1f) * (num4 - num5), (float)(-(double)num4 * 3.0 / 2.0)),
                    acceleration = new Vector2(0.0f, y),
                    timeBasedMotion = true
                });
                sprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, targetTile * 64f, false, false)
                {
                    delayBeforeAnimationStart = (int)delay,
                    layerDepth = (float)(((targetTile.Y - 0.5) * 64.0 + 2.0) / 10000.0)
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, targetTile * 64f, false, Game1.random.NextBool(), (float)(((targetTile.Y - 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, targetTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)(((targetTile.Y - 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, targetTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)(((targetTile.Y - 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                if (who.IsLocalPlayer)
                {
                    DelayedAction.playSoundAfterDelay("waterSlosh", (int)delay, who.currentLocation);
                    if (callback != null)
                        DelayedAction.functionAfterDelay(callback, (int)delay);
                }
                Game1.Multiplayer.broadcastSprites(who.currentLocation, sprites);
            }
            else
            {
                var num6 = Vector2.Distance(who.Position, targetTile * 64f);
                var num7 = Math.Abs(num6);
                if (who.FacingDirection == 0)
                {
                    num6 = -num6;
                    num7 += 64f;
                }
                var num8 = targetTile.X * 64f - who.position.X;
                var y = 1f / 400f;
                var num9 = (float)Math.Sqrt(2.0 * y * num7);
                var num10 = (float)(Math.Sqrt(2.0 * (num7 - (double)num6) / y) + num9 / (double)y) * 1.05f;
                var delay = (float)((who.FacingDirection != 0 ? num10 * 2.5f : (double)(num10 * 0.7f)) - Math.Abs(num8) / (who.FacingDirection == 0 ? 100.0 : 2.0));
                Game1.playSound("throwDownITem");
                var sprites = new TemporaryAnimatedSpriteList();
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(whichObject.QualifiedItemId);
                sprites.Add(new TemporaryAnimatedSprite(dataOrErrorItem.GetTextureName(), dataOrErrorItem.GetSourceRect(), who.Position + new Vector2(0.0f, -64f), false, 0.0f, Color.White)
                {
                    scale = 4f,
                    layerDepth = 1f,
                    totalNumberOfLoops = 1,
                    interval = delay,
                    motion = new Vector2(num8 / (who.FacingDirection == 0 ? 900f : 1000f), -num9),
                    acceleration = new Vector2(0.0f, y),
                    timeBasedMotion = true
                });
                sprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, targetTile * 64f, false, false)
                {
                    delayBeforeAnimationStart = (int)delay,
                    layerDepth = (float)(((targetTile.Y - 0.5) * 64.0 + 2.0) / 10000.0)
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, targetTile * 64f, false, Game1.random.NextBool(), (float)((targetTile.Y * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, targetTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)((targetTile.Y * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, targetTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)((targetTile.Y * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                if (who.IsLocalPlayer)
                {
                    DelayedAction.playSoundAfterDelay("waterSlosh", (int)delay, who.currentLocation);
                    if (callback != null)
                        DelayedAction.functionAfterDelay(callback, (int)delay);
                }
                Game1.Multiplayer.broadcastSprites(who.currentLocation, sprites);
            }
        }

        private static Vector2 GetCenterTile(Building building)
        {
            return new Vector2(building.tileX.Value + 1, building.tileY.Value + 1);
        }
    }

    public class JumpingFish
    {
        public Vector2 startPosition;
        public Vector2 endPosition;
        protected float _age;
        public float jumpTime = 1f;
        protected GameLocation _gameLocation;
        protected Object _fishObject;
        protected bool _flipped;
        public Vector2 position;
        public float jumpHeight;
        public float angularVelocity;
        public float angle;

        public JumpingFish(GameLocation gameLocation, Object fish, Vector2 start_position, Vector2 end_position)
        {
            this.angularVelocity = (float)((double)Utility.RandomFloat(20f, 40f) * 3.1415927410125732 / 180.0);
            this.startPosition = start_position;
            this.endPosition = end_position;
            this.position = this.startPosition;
            _gameLocation = gameLocation;
            this._fishObject = fish;
            if ((double)this.startPosition.X > (double)this.endPosition.X)
                this._flipped = true;
            this.jumpHeight = Utility.RandomFloat(75f, 100f);
            this.Splash();
        }

        public void Splash()
        {
            if (_gameLocation != Game1.currentLocation)
            {
                return;
            }

            Game1.playSound("dropItemInWater");
            Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, this.position + new Vector2(-0.5f, -0.5f) * 64f, false, false)
            {
                delayBeforeAnimationStart = 0,
                layerDepth = this.startPosition.Y / 10000f
            });
        }

        public bool Update(float time)
        {
            this._age += time;
            this.angle += this.angularVelocity * time;
            if ((double)this._age >= (double)this.jumpTime)
            {
                this._age = time;
                this.Splash();
                return true;
            }
            this.position.X = Utility.Lerp(this.startPosition.X, this.endPosition.X, this._age / this.jumpTime);
            this.position.Y = Utility.Lerp(this.startPosition.Y, this.endPosition.Y, this._age / this.jumpTime);
            return false;
        }

        public void Draw(SpriteBatch b)
        {
            var angle = this.angle;
            var effects = SpriteEffects.None;
            if (this._flipped)
            {
                effects = SpriteEffects.FlipHorizontally;
                angle *= -1f;
            }
            var num = 1f;
            var globalPosition = this.position + new Vector2(0.0f, (float)Math.Sin((double)this._age / (double)this.jumpTime * Math.PI) * -this.jumpHeight);
            var origin = new Vector2(8f, 8f);
            var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this._fishObject.QualifiedItemId);
            b.Draw(dataOrErrorItem.GetTexture(), Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle?(dataOrErrorItem.GetSourceRect()), Color.White, angle, origin, 4f * num, effects, (float)((double)this.position.Y / 10000.0 + 9.9999999747524271E-07));
            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0.0f, new Vector2((float)(Game1.shadowTexture.Bounds.Width / 2), (float)(Game1.shadowTexture.Bounds.Height / 2)), 2f, effects, (float)((double)this.position.Y / 10000.0 + 9.9999999747524271E-07));
        }
    }
}
