using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FishingRodInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void skipEvent()
        public static bool SkipEvent_BambooPole_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.BAMBOO_POLE)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                EventInjections.BaseSkipEvent(__instance, CheckBambooPoleLocation);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_BambooPole_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void skipEvent()
        public static bool SkipEvent_WillyFishingLesson_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != EventIds.WILLY_FISHING_LESSON)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                EventInjections.BaseSkipEvent(__instance, CheckFishingLessonLocations);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_WillyFishingLesson_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_BambooPole_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.BAMBOO_POLE || args.Length <= 1 || args[1].ToLower() != "rod")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckBambooPoleLocation();

                if (Game1.activeClickableMenu == null)
                {
                    @event.CurrentCommand++;
                }
                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_BambooPole_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_WillyFishingLesson_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.WILLY_FISHING_LESSON)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckFishingLessonLocations();

                if (Game1.activeClickableMenu == null)
                {
                    @event.CurrentCommand++;
                }
                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_WillyFishingLesson_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void GainSkill(Event @event, string[] args, EventContext context)
        public static bool GainSkill_WillyFishingLesson_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (@event.id != EventIds.WILLY_FISHING_LESSON)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                CheckFishingLessonLocations();

                @event.CurrentCommand++;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GainSkill_WillyFishingLesson_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckBambooPoleLocation()
        {
            _locationChecker.AddCheckedLocation("Bamboo Pole Cutscene");
        }

        private static void CheckFishingLessonLocations()
        {
            _locationChecker.AddCheckedLocation("Level 1 Fishing");
            _locationChecker.AddCheckedLocation("Purchase Training Rod");
        }

        // public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        public static bool DoFunction_DebugFishing_Prefix(FishingRod __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            try
            {
                return MethodPrefix.RUN_ORIGINAL_METHOD;
                who = who ?? __instance.lastUser;
                if (__instance.fishCaught || !who.IsLocalPlayer && (__instance.isReeling || __instance.isFishing || __instance.pullingOutOfWater))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                __instance.hasDoneFucntionYet = true;
                var bobberTile = new Vector2(__instance.bobber.X / 64f, __instance.bobber.Y / 64f);
                var x1 = (int)bobberTile.X;
                var y1 = (int)bobberTile.Y;

                BaseDoFunction(__instance, location, x, y, who);

                if (__instance.doneWithAnimation)
                {
                    who.canReleaseTool = true;
                }
                if (Game1.isAnyGamePadButtonBeingPressed())
                {
                    Game1.lastCursorMotionWasMouse = false;
                }
                if (!__instance.isFishing && !__instance.castedButBobberStillInAir && !__instance.pullingOutOfWater && !__instance.isNibbling && !__instance.hit && !__instance.showingTreasure)
                {
                    if (!Game1.eventUp && who.IsLocalPlayer && !__instance.hasEnchantmentOfType<EfficientToolEnchantment>())
                    {
                        var stamina = who.Stamina;
                        who.Stamina -= (float)(8.0 - who.FishingLevel * 0.10000000149011612);
                        who.checkForExhaustion(stamina);
                    }
                    if (location.canFishHere() && location.isTileFishable(x1, y1))
                    {
                        __instance.clearWaterDistance = FishingRod.distanceToLand((int)(__instance.bobber.X / 64.0), (int)(__instance.bobber.Y / 64.0), who.currentLocation);
                        __instance.isFishing = true;
                        location.temporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, new Vector2(__instance.bobber.X - 32f, __instance.bobber.Y - 32f), false, false));
                        if (who.IsLocalPlayer)
                        {
                            if (__instance.PlayUseSounds)
                            {
                                location.playSound("dropItemInWater", new Vector2?(bobberTile));
                            }
                            ++Game1.stats.TimesFished;
                        }
                        __instance.timeUntilFishingBite = 500f; // __instance.calculateTimeUntilFishingBite(bobberTile, true, who);
                        if (location.fishSplashPoint != null)
                        {
                            var flag = location.fishFrenzyFish.Value != null && !location.fishFrenzyFish.Equals("");
                            var rectangle = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
                            if (flag)
                            {
                                rectangle.Inflate(32, 32);
                            }
                            if (new Rectangle((int)__instance.bobber.X - 32, (int)__instance.bobber.Y - 32, 64, 64).Intersects(rectangle))
                            {
                                __instance.timeUntilFishingBite /= flag ? 2f : 4f;
                                location.temporarySprites.Add(new TemporaryAnimatedSprite(10, __instance.bobber.Value - new Vector2(32f, 32f), Color.Cyan));
                            }
                        }
                        who.UsingTool = true;
                        who.canMove = false;
                    }
                    else
                    {
                        if (__instance.doneWithAnimation)
                        {
                            who.UsingTool = false;
                        }
                        if (!__instance.doneWithAnimation)
                        {
                            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                        }
                        who.canMove = true;
                    }
                }
                else
                {
                    if (__instance.isCasting || __instance.pullingOutOfWater)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    var fromFishPond = location.isTileBuildingFishable((int)bobberTile.X, (int)bobberTile.Y);
                    who.FarmerSprite.PauseForSingleAnimation = false;
                    var result = who.FacingDirection;
                    switch (result)
                    {
                        case 0:
                            who.FarmerSprite.animateBackwardsOnce(299, 35f);
                            break;
                        case 1:
                            who.FarmerSprite.animateBackwardsOnce(300, 35f);
                            break;
                        case 2:
                            who.FarmerSprite.animateBackwardsOnce(301, 35f);
                            break;
                        case 3:
                            who.FarmerSprite.animateBackwardsOnce(302, 35f);
                            break;
                    }
                    if (__instance.isNibbling)
                    {
                        var bait = __instance.GetBait();
                        var num = bait != null ? bait.Price / 10.0 : 0.0;
                        var flag1 = false;
                        if (location.fishSplashPoint != null)
                        {
                            flag1 = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64).Intersects(new Rectangle((int)__instance.bobber.X - 80, (int)__instance.bobber.Y - 80, 64, 64));
                        }

                        var baitId = bait?.QualifiedItemId;
                        var waterDepth = __instance.clearWaterDistance + (flag1 ? 1 : 0);
                        var baitPotency = num + (flag1 ? 0.4 : 0.0);

                        var fish1 = new List<string>();
                        var fish2 = new List<string>();
                        var fish3 = new List<string>();
                        var fish4 = new List<string>();
                        for (var i = 0; i < 1000; i++)
                        {
                            fish1.Add(location.getFish(__instance.fishingNibbleAccumulator, baitId, 1, who, baitPotency, bobberTile).Name);
                            fish2.Add(location.getFish(__instance.fishingNibbleAccumulator, baitId, 2, who, baitPotency, bobberTile).Name);
                            fish3.Add(location.getFish(__instance.fishingNibbleAccumulator, baitId, 3, who, baitPotency, bobberTile).Name);
                            fish4.Add(location.getFish(__instance.fishingNibbleAccumulator, baitId, 4, who, baitPotency, bobberTile).Name);
                        }

                        var o = location.getFish(__instance.fishingNibbleAccumulator, baitId, 1, who, baitPotency, bobberTile);
                        var o2 = location.getFish(__instance.fishingNibbleAccumulator, baitId, waterDepth, who, baitPotency, bobberTile);
                        var o3 = location.getFish(__instance.fishingNibbleAccumulator, baitId, waterDepth, who, baitPotency, bobberTile);
                        var o4 = location.getFish(__instance.fishingNibbleAccumulator, baitId, waterDepth, who, baitPotency, bobberTile);
                        var o5 = location.getFish(__instance.fishingNibbleAccumulator, baitId, waterDepth, who, baitPotency, bobberTile);
                        if (o == null || ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId).IsErrorItem)
                        {
                            result = Game1.random.Next(167, 173);
                            o = ItemRegistry.Create("(O)" + result.ToString());
                        }
                        if ((o is StardewValley.Object @object ? (@object.scale.X == 1.0 ? 1 : 0) : 0) != 0)
                        {
                            __instance.favBait = true;
                        }
                        var dictionary = DataLoader.Fish(Game1.content);
                        var flag2 = false;
                        if (!o.HasTypeObject())
                        {
                            flag2 = true;
                        }
                        else
                        {
                            string str;
                            if (dictionary.TryGetValue(o.ItemId, out str))
                            {
                                if (!int.TryParse(str.Split('/')[1], out result))
                                {
                                    flag2 = true;
                                }
                            }
                            else
                            {
                                flag2 = true;
                            }
                        }
                        __instance.lastCatchWasJunk = false;
                        var qualifiedItemId = o.QualifiedItemId;
                        bool flag3;
                        if (qualifiedItemId != null)
                        {
                            switch (qualifiedItemId.Length)
                            {
                                case 5:
                                    switch (qualifiedItemId[4])
                                    {
                                        case '3':
                                            if (!(qualifiedItemId == "(O)73"))
                                            {
                                                flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            }
                                            break;
                                        case '9':
                                            if (qualifiedItemId == "(O)79")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        default:
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                    }
                                    break;
                                case 6:
                                    switch (qualifiedItemId[5])
                                    {
                                        case '0':
                                            if (qualifiedItemId == "(O)890" || qualifiedItemId == "(O)820")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '1':
                                            if (qualifiedItemId == "(O)821")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '2':
                                            if (qualifiedItemId == "(O)152" || qualifiedItemId == "(O)842" || qualifiedItemId == "(O)822")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '3':
                                            if (qualifiedItemId == "(O)153" || qualifiedItemId == "(O)823")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '4':
                                            if (qualifiedItemId == "(O)824")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '5':
                                            if (qualifiedItemId == "(O)825")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '6':
                                            if (qualifiedItemId == "(O)826")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '7':
                                            if (qualifiedItemId == "(O)157" || qualifiedItemId == "(O)797" || qualifiedItemId == "(O)827")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        case '8':
                                            if (qualifiedItemId == "(O)828")
                                            {
                                                break;
                                            }
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                        default:
                                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                            break;
                                    }
                                    break;
                                default:
                                    flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                                    break;
                            }
                            flag3 = true;
                        }
                        else
                        {
                            flag3 = o.Category == -20 || o.QualifiedItemId == GameLocation.CAROLINES_NECKLACE_ITEM_QID;
                        }

                        if (flag3 | fromFishPond | flag2)
                        {
                            __instance.lastCatchWasJunk = true;
                            __instance.pullFishFromWater(o.QualifiedItemId, -1, 0, 0, false, false, fromFishPond, o.SetFlagOnPickup, false, 1);
                        }
                        else
                        {
                            if (__instance.hit || !who.IsLocalPlayer)
                            {
                                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                            }
                            __instance.hit = true;
                            Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(612, 1913, 74, 30), 1500f, 1, 0, Game1.GlobalToLocal(Game1.viewport, __instance.bobber.Value + new Vector2(-140f, -160f)), false, false, 1f, 0.005f, Color.White, 4f, 0.075f, 0.0f, 0.0f, true)
                            {
                                scaleChangeChange = -0.005f,
                                motion = new Vector2(0.0f, -0.1f),
                                endFunction = _ => __instance.startMinigameEndFunction(o),
                                id = 987654321,
                            });
                            if (!__instance.PlayUseSounds)
                            {
                                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                            }
                            who.playNearbySoundLocal("FishHit");
                        }
                    }
                    else
                    {
                        if (fromFishPond && Game1.timeOfDay < 2600)
                        {
                            var fish = location.getFish(-1f, null, -1, who, -1.0, bobberTile);
                            if (fish != null)
                            {
                                __instance.pullFishFromWater(fish.QualifiedItemId, -1, 0, 0, false, false, true, null, false, 1);
                                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                            }
                        }
                        if (__instance.PlayUseSounds && who.IsLocalPlayer)
                        {
                            location.playSound("pullItemFromWater", new Vector2?(bobberTile));
                        }
                        __instance.isFishing = false;
                        __instance.pullingOutOfWater = true;
                        var standingPixel = who.StandingPixel;
                        if (who.FacingDirection == 1 || who.FacingDirection == 3)
                        {
                            double num1 = Math.Abs(__instance.bobber.X - standingPixel.X);
                            var y2 = 0.005f;
                            double num2 = y2;
                            var num3 = -(float)Math.Sqrt(num1 * num2 / 2.0);
                            var animationInterval = (float)(2.0 * (Math.Abs(num3 - 0.5f) / (double)y2)) * 1.2f;
                            __instance.animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, __instance.getBobberStyle(who), 16, 32) with
                            {
                                Height = 16,
                            }, animationInterval, 1, 0, __instance.bobber.Value + new Vector2(-32f, -48f), false, false, standingPixel.Y / 10000f, 0.0f, Color.White, 4f, 0.0f, 0.0f, Game1.random.Next(-20, 20) / 100f)
                            {
                                motion = new Vector2((who.FacingDirection == 3 ? -1f : 1f) * (num3 + 0.2f), num3 - 0.8f),
                                acceleration = new Vector2(0.0f, y2),
                                endFunction = __instance.donefishingEndFunction,
                                timeBasedMotion = true,
                                alphaFade = 1f / 1000f,
                                flipped = who.FacingDirection == 1 && __instance.flipCurrentBobberWhenFacingRight(),
                            });
                        }
                        else
                        {
                            var num4 = __instance.bobber.Y - standingPixel.Y;
                            var num5 = Math.Abs(num4 + 256f);
                            var y3 = 0.005f;
                            var num6 = (float)Math.Sqrt(2.0 * y3 * num5);
                            var animationInterval = (float)(Math.Sqrt(2.0 * (num5 - (double)num4) / y3) + num6 / (double)y3);
                            __instance.animations.Add(new TemporaryAnimatedSprite("TileSheets\\bobbers", Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, __instance.getBobberStyle(who), 16, 32) with
                            {
                                Height = 16,
                            }, animationInterval, 1, 0, __instance.bobber.Value + new Vector2(-32f, -48f), false, false, __instance.bobber.Y / 10000f, 0.0f, Color.White, 4f, 0.0f, 0.0f, Game1.random.Next(-20, 20) / 100f)
                            {
                                motion = new Vector2((float)((who.StandingPixel.X - (double)__instance.bobber.Value.X) / 800.0), -num6),
                                acceleration = new Vector2(0.0f, y3),
                                endFunction = __instance.donefishingEndFunction,
                                timeBasedMotion = true,
                                alphaFade = 1f / 1000f,
                            });
                        }
                        who.UsingTool = true;
                        who.canReleaseTool = false;
                    }
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoFunction_DebugFishing_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void BaseDoFunction(FishingRod rod, GameLocation location, int x, int y, Farmer who)
        {
            // Base.DoFunction()
            rod.lastUser = who;
            Game1.recentMultiplayerRandom = Utility.CreateRandom((short)Game1.random.Next(short.MinValue, 32768));
            if (rod.isHeavyHitter())
            {
                Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 4.0), 100 + Game1.random.Next(50));
                location.damageMonster(new Rectangle(x - 32, y - 32, 64, 64), rod.upgradeLevel.Value + 1, (rod.upgradeLevel.Value + 1) * 3, false, who);
            }
            return;
            // End of Base.DoFunction()
        }
    }
}
