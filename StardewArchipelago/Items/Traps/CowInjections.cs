using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.FarmAnimals;
using System;

namespace StardewArchipelago.Items.Traps
{
    internal class CowInjections
    {
        public const string INVISIBLE_COW_KEY = "InvisibleCow";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static TrapDifficultyBalancer _difficultyBalancer;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _difficultyBalancer = new TrapDifficultyBalancer();
        }

        public static bool Draw_InvisibleCow_Prefix(FarmAnimal __instance, SpriteBatch b)
        {
            try
            {
                if (!__instance.modData.ContainsKey(INVISIBLE_COW_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var vector2 = new Vector2(0.0f, __instance.yJumpOffset);
                var boundingBox = __instance.GetBoundingBox();
                var animalData = __instance.GetAnimalData();
                var isSwimming = __instance.IsActuallySwimming();
                var isBaby = __instance.isBaby();
                var shadow = animalData?.GetShadow(isBaby, isSwimming);
                if ((shadow != null ? (shadow.Visible ? 1 : 0) : 1) != 0)
                {
                    int? nullable1;
                    if (shadow == null)
                    {
                        nullable1 = null;
                    }
                    else
                    {
                        ref var local = ref shadow.Offset;
                        nullable1 = local.HasValue ? new int?(local.GetValueOrDefault().X) : null;
                    }
                    var valueOrDefault1 = nullable1.GetValueOrDefault();
                    int? nullable2;
                    if (shadow == null)
                    {
                        nullable2 = null;
                    }
                    else
                    {
                        ref var local = ref shadow.Offset;
                        nullable2 = local.HasValue ? new int?(local.GetValueOrDefault().Y) : null;
                    }
                    var valueOrDefault2 = nullable2.GetValueOrDefault();
                    if (isSwimming)
                    {
                        var scale = (float)(shadow?.Scale ?? (isBaby ? 2.5 : 3.5));
                        var globalPosition = new Vector2(__instance.Position.X + valueOrDefault1, __instance.Position.Y - 24f + valueOrDefault2);
                        __instance.Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, globalPosition), scale, 0.5f);
                        var num = (int)((Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + __instance.bobOffset) + 0.5) * 3.0);
                        vector2.Y += num;
                    }
                    else
                    {
                        var scale = (float)(shadow?.Scale ?? (isBaby ? 3.0 : 4.0));
                        var globalPosition = new Vector2(__instance.Position.X + valueOrDefault1, __instance.Position.Y - 24f + valueOrDefault2);
                        __instance.Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, globalPosition), scale);
                    }
                }
                vector2.Y += __instance.yJumpOffset;
                var layerDepth = (float)((boundingBox.Center.Y + 4 + __instance.Position.X / 20000.0) / 10000.0);

                var minTransparency = 0;
                var maxTransparency = 128;
                var tickDelay = 4;
                var transparencyRange = maxTransparency - minTransparency;
                var transparencyThisFrame = 0;
                if (transparencyRange >= 1)
                {
                    transparencyThisFrame = (int)(((Game1.ticks / tickDelay) + (__instance.myID.Value % transparencyRange)) % (transparencyRange * 2));
                    if (transparencyThisFrame > transparencyRange)
                    {
                        transparencyThisFrame = transparencyRange - (transparencyThisFrame - transparencyRange);
                    }

                    transparencyThisFrame += minTransparency;
                }

                if (transparencyThisFrame == 0)
                {
                    SetRandomPosition(__instance, __instance.currentLocation);
                }

                transparencyThisFrame = Math.Max(0, transparencyThisFrame - 20);
                var tone = Math.Min(64, transparencyThisFrame * 2);

                var color = __instance.hitGlowTimer > 0 ? Color.Red : new Color(tone, tone, tone, transparencyThisFrame);
                __instance.Sprite.draw(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, __instance.Position - new Vector2(0.0f, 24f) + vector2)), layerDepth, 0, 0, color, __instance.FacingDirection == 3, 4f);

                if (!__instance.isEmoting)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                var num1 = __instance.Sprite.SpriteWidth / 2 * 4 - 32 + (animalData != null ? animalData.EmoteOffset.X : 0);
                var num2 = (animalData != null ? animalData.EmoteOffset.Y : 0) - 64;
                var local1 = Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.Position.X + vector2.X + num1, __instance.Position.Y + vector2.Y + num2));
                b.Draw(Game1.emoteSpriteSheet, local1, new Rectangle(__instance.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, __instance.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, boundingBox.Bottom / 10000f);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_InvisibleCow_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

        }

        // public void dayUpdate(GameLocation environment)
        public static bool DayUpdate_InvisibleCow_Prefix(FarmAnimal __instance, GameLocation environment)
        {
            try
            {
                if (!__instance.modData.ContainsKey(INVISIBLE_COW_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var difficulty = _archipelago.SlotData.TrapItemsDifficulty;
                var chanceToDespawnPerDay = _difficultyBalancer.CowDespawnChancePerDay[difficulty];
                if (__instance.wasPet.Value)
                {
                    chanceToDespawnPerDay += 0.2;
                }
                else if (__instance.wasAutoPet.Value)
                {
                    chanceToDespawnPerDay += 0.05;
                }
                if (Game1.random.NextDouble() < chanceToDespawnPerDay)
                {
                    if (__instance.homeInterior is AnimalHouse animalHouse)
                    {
                        animalHouse.animalsThatLiveHere.Remove(__instance.myID.Value);
                    }

                    __instance.currentLocation.Animals.Remove(__instance.myID.Value);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __instance.StopAllActions();
                __instance.health.Value = 3;
                var flag1 = false;
                __instance.wasPet.Value = false;
                __instance.wasAutoPet.Value = false;
                ++__instance.daysOwned.Value;
                __instance.reload(__instance.homeInterior);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_InvisibleCow_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

        }

        // farmerPushing()
        public static bool FarmerPushing_TakeLongerToReact_Prefix(FarmAnimal __instance)
        {
            try
            {
                if (!__instance.modData.ContainsKey(INVISIBLE_COW_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                ++__instance.pushAccumulator;
                if (__instance.pushAccumulator <= 180)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                // __instance.doFarmerPushEvent.Fire(Game1.player.FacingDirection);
                var cowBoundingBox = __instance.GetBoundingBox();
                var farmerOppositeDirection = Utility.GetOppositeFacingDirection(Game1.player.FacingDirection);
                var temporaryTile = Utility.ExpandRectangle(cowBoundingBox, farmerOppositeDirection, 6);
                temporaryTile.Inflate(-(temporaryTile.Width / 8), -(temporaryTile.Height / 8));
                Game1.player.TemporaryPassableTiles.Add(temporaryTile);
                __instance.pushAccumulator = 0;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FarmerPushing_TakeLongerToReact_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

        }

        // public void doFarmerPush(int direction)

        // public void setRandomPosition(GameLocation location)
        public static bool SetRandomPosition_DontLookForProduceArea_Prefix(FarmAnimal __instance, GameLocation location)
        {
            try
            {
                if (!__instance.modData.ContainsKey(INVISIBLE_COW_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                SetRandomPosition(__instance, location);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetRandomPosition_DontLookForProduceArea_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

        }

        private static void SetRandomPosition(FarmAnimal cow, GameLocation location)
        {
            cow.StopAllActions();
            // Rectangle parsed;
            //if (!location.TryGetMapPropertyAs("ProduceArea", out parsed, true))
            //    return;
            var tile = location.getRandomTile();
            cow.Position = new Vector2(tile.X * 64f, tile.Y * 64f);
            var num = 0;
            while (cow.Position.Equals(Vector2.Zero) || location.Objects.ContainsKey(cow.Position) || location.isCollidingPosition(cow.GetBoundingBox(), Game1.viewport, false, 0, false, cow))
            {
                tile = location.getRandomTile();
                cow.Position = new Vector2(tile.X * 64f, tile.Y * 64f); ++num;
                if (num > 64)
                {
                    break;
                }
            }

            cow.SleepIfNecessary();
        }

        // public void pet(Farmer who, bool is_auto_pet = false)
        public static bool Pet_DontShowMenu_Prefix(FarmAnimal __instance, Farmer who, bool is_auto_pet)
        {
            try
            {
                if (!__instance.modData.ContainsKey(INVISIBLE_COW_KEY))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var autoPetFriendship = 7;
                if (__instance.wasAutoPet.Value)
                {
                    __instance.friendshipTowardFarmer.Value = Math.Min(1000, __instance.friendshipTowardFarmer.Value + autoPetFriendship);
                }
                else if (is_auto_pet)
                {
                    __instance.friendshipTowardFarmer.Value = Math.Min(1000, __instance.friendshipTowardFarmer.Value + (15 - autoPetFriendship));
                }
                else
                {
                    __instance.friendshipTowardFarmer.Value = Math.Min(1000, __instance.friendshipTowardFarmer.Value + 15);
                }
                if (is_auto_pet)
                {
                    __instance.wasAutoPet.Value = true;
                }
                else
                {
                    __instance.wasPet.Value = true;
                }

                var animalData = __instance.GetAnimalData();
                var happinessDrain = animalData != null ? animalData.HappinessDrain : 0;
                if (!is_auto_pet)
                {
                    if (animalData != null && animalData.ProfessionForHappinessBoost >= 0 && who.professions.Contains(animalData.ProfessionForHappinessBoost))
                    {
                        __instance.friendshipTowardFarmer.Value = Math.Min(1000, __instance.friendshipTowardFarmer.Value + 15);
                        __instance.happiness.Value = (byte)Math.Min(byte.MaxValue, __instance.happiness.Value + Math.Max(5, 30 + happinessDrain));
                    }
                    var num2 = 20;
                    if (__instance.wasAutoPet.Value)
                    {
                        num2 = 32;
                    }
                    __instance.doEmote(__instance.moodMessage.Value == 4 ? 12 : num2);
                }
                __instance.happiness.Value = (byte)Math.Min(byte.MaxValue, __instance.happiness.Value + Math.Max(5, 30 + happinessDrain));
                if (!is_auto_pet)
                {
                    __instance.makeSound();
                    // who.gainExperience(0, 5);
                }
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Pet_DontShowMenu_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

        }
    }
}
