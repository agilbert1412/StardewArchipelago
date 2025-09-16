using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;
using System.IO;
using System.Linq;
using StardewArchipelago.Textures;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class HorseInjections
    {
        private const string POMNUT_FILE = "pomnut.wav";
        private const string POMNUT_CUE = "pomnut";

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Texture2D _pomnutTexture;
        private static string LastEatenItem = "";


        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            //var currentModFolder = _modHelper.DirectoryPath;
            //var soundsFolder = "Sounds";
            //var relativePathToSound = Path.Combine(currentModFolder, soundsFolder, HONEYWELL_FILE);
            //var honeywellCueDefinition = new CueDefinition(HONEYWELL_CUE, SoundEffect.FromFile(relativePathToSound), 0);
            //Game1.soundBank.AddCue(honeywellCueDefinition);

            var memeUIFolder = Path.Combine("Bundles", "UI");
            var memeAssetsPath = Path.Combine(memeUIFolder, "horse_pomnut.png");
            _pomnutTexture = TexturesLoader.GetTexture(memeAssetsPath);
        }

        // public override bool checkAction(Farmer who, GameLocation l)
        public static bool CheckAction_FeedPomnutItems_Prefix(Horse __instance, Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                if (who == null || !who.canMove || __instance.rider != null || who.mount != null || who.FarmerSprite.PauseForSingleAnimation || __instance.currentLocation != who.currentLocation)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // private int munchingCarrotTimer;
                var munchingCarrotTimerField = _modHelper.Reflection.GetField<int>(__instance, "munchingCarrotTimer");
                var munchingCarrotTimer = munchingCarrotTimerField.GetValue();

                if (munchingCarrotTimer > 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }


                var activeObject = who.ActiveObject;
                var validIds = new[] { QualifiedItemIds.POMEGRANATE, QualifiedItemIds.HAZELNUT, QualifiedItemIds.CARROT };
                if (activeObject == null || !validIds.Contains(activeObject.QualifiedItemId) || __instance.ateCarrotToday)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __instance.mutex.RequestLock(() =>
                {
                    __instance.Sprite.StopAnimation();
                    __instance.Sprite.faceDirection(__instance.FacingDirection);
                    Game1.playSound("eat");
                    DelayedAction.playSoundAfterDelay("eat", 600);
                    DelayedAction.playSoundAfterDelay("eat", 1200);
                    munchingCarrotTimerField.SetValue(1500);
                    __instance.doEmote(20, 32);
                    who.reduceActiveItemByOne();
                    __instance.ateCarrotToday = true;
                    ArchipelagoJunimoNoteMenu.TryDonateToBundle(MemeBundleNames.POMNUT, activeObject.Name, 1);
                    LastEatenItem = activeObject.QualifiedItemId;
                    __instance.Sprite.spriteTexture = _pomnutTexture;
                    __instance.mutex.ReleaseLock();
                    return;
                });

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_FeedPomnutItems_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static int _munchingCarrotTimerValue;

        // public override void draw(SpriteBatch b)
        public static bool Draw_EatingOtherItems_Prefix(Horse __instance, SpriteBatch b)
        {
            try
            {
                if (LastEatenItem != QualifiedItemIds.POMEGRANATE && LastEatenItem != QualifiedItemIds.HAZELNUT)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // private int munchingCarrotTimer;
                var munchingCarrotTimerField = _modHelper.Reflection.GetField<int>(__instance, "munchingCarrotTimer");
                _munchingCarrotTimerValue = munchingCarrotTimerField.GetValue();
                if (_munchingCarrotTimerValue <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                munchingCarrotTimerField.SetValue(0);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_EatingOtherItems_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void draw(SpriteBatch b)
        public static void Draw_EatingOtherItems_Postfix(Horse __instance, SpriteBatch b)
        {
            try
            {
                if (LastEatenItem != QualifiedItemIds.POMEGRANATE && LastEatenItem != QualifiedItemIds.HAZELNUT)
                {
                    return;
                }

                if (_munchingCarrotTimerValue <= 0)
                {
                    return;
                }

                // private int munchingCarrotTimer;
                var munchingCarrotTimerField = _modHelper.Reflection.GetField<int>(__instance, "munchingCarrotTimer");
                munchingCarrotTimerField.SetValue(_munchingCarrotTimerValue);

                var offset = new Point(0, 0); // 182, 98
                if (LastEatenItem == QualifiedItemIds.POMEGRANATE)
                {
                    offset = new Point(0, 33); // 182, 131
                }
                else if (LastEatenItem == QualifiedItemIds.HAZELNUT)
                {
                    offset = new Point(-43, 33); // 139, 131
                }

                Rectangle sourceRect;
                switch (__instance.FacingDirection)
                {
                    case 1:
                        sourceRect = new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14);
                        sourceRect = new Rectangle(sourceRect.X + offset.X, sourceRect.Y + offset.Y, sourceRect.Width, sourceRect.Height);
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(80f, -56f), sourceRect, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((__instance.Position.Y + 64.0) / 10000.0 + 1.0000000116860974E-07));
                        break;
                    case 2:
                        sourceRect = new Rectangle(170 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 112, 16, 16);
                        sourceRect = new Rectangle(sourceRect.X + offset.X, sourceRect.Y + offset.Y, sourceRect.Width, sourceRect.Height);
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(24f, -24f), sourceRect, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((__instance.Position.Y + 64.0) / 10000.0 + 1.0000000116860974E-07));
                        break;
                    case 3:
                        sourceRect = new Rectangle(179 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0) / 300 * 16, 97, 16, 14);
                        sourceRect = new Rectangle(sourceRect.X + offset.X, sourceRect.Y + offset.Y, sourceRect.Width, sourceRect.Height);
                        b.Draw(__instance.Sprite.Texture, __instance.getLocalPosition(Game1.viewport) + new Vector2(-16f, -56f), sourceRect, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, (float)((__instance.Position.Y + 64.0) / 10000.0 + 1.0000000116860974E-07));
                        break;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_EatingOtherItems_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
