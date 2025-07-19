using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;
using StardewValley.Buildings;
using StardewValley.Extensions;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using StardewArchipelago.Archipelago.Gifting;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class WellInjections
    {
        private const string HONEYWELL_FILE = "honeywell.wav";
        private const string HONEYWELL_CUE = "honeywell";

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static GiftSender _giftSender;


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
                ShowObjectThrownIntoWaterAnimation(__instance, who, who.ActiveObject, () =>
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

        private static void ShowObjectThrownIntoWaterAnimation(Building building, Farmer who, Object whichObject, Action callback = null)
        {
            var centerTile = GetCenterTile(building);
            who.faceGeneralDirection(centerTile * 64f + new Vector2(32f, 32f));
            if (who.FacingDirection == 1 || who.FacingDirection == 3)
            {
                var distanceToCenterTile = Vector2.Distance(who.Position, centerTile * 64f);
                var verticalDistance = (float)(centerTile.Y * 64.0 + 32.0) - who.position.Y;
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
                sprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, centerTile * 64f, false, false)
                {
                    delayBeforeAnimationStart = (int)delay,
                    layerDepth = (float)(((building.tileY.Value + 0.5) * 64.0 + 2.0) / 10000.0)
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, centerTile * 64f, false, Game1.random.NextBool(), (float)(((building.tileY.Value + 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, centerTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)(((building.tileY.Value + 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, centerTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)(((building.tileY.Value + 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
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
                var num6 = Vector2.Distance(who.Position, centerTile * 64f);
                var num7 = Math.Abs(num6);
                if (who.FacingDirection == 0)
                {
                    num6 = -num6;
                    num7 += 64f;
                }
                var num8 = centerTile.X * 64f - who.position.X;
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
                sprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, centerTile * 64f, false, false)
                {
                    delayBeforeAnimationStart = (int)delay,
                    layerDepth = (float)(((building.tileY.Value + 0.5) * 64.0 + 2.0) / 10000.0)
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, centerTile * 64f, false, Game1.random.NextBool(), (float)(((building.tileY.Value + 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, centerTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)(((building.tileY.Value + 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
                {
                    delayBeforeAnimationStart = (int)delay
                });
                sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, centerTile * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), false, Game1.random.NextBool(), (float)(((building.tileY.Value + 0.5) * 64.0 + 1.0) / 10000.0), 0.01f, Color.White, 0.75f, 3f / 1000f, 0.0f, 0.0f)
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
}
