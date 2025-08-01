﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Constants.Locations;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutPuzzleInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static bool ReceiveLeftClick_CrackGoldenCoconut_Prefix(GeodeMenu __instance, int x, int y, bool playSound)
        {
            try
            {
                if (__instance.waitingForServerResponse || !__instance.geodeSpot.containsPoint(x, y) || __instance.heldItem == null ||
                    !Utility.IsGeode(__instance.heldItem) || Game1.player.Money < 25 || __instance.geodeAnimationTimer > 0 ||
                    (Game1.player.freeSpotsInInventory() <= 1 && __instance.heldItem.Stack > 1) || Game1.player.freeSpotsInInventory() < 1 ||
                    __instance.heldItem.QualifiedItemId != QualifiedItemIds.GOLDEN_COCONUT)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var goldenCoconutLocation = $"{Prefix.WALNUTSANITY}Open Golden Coconut";
                if (Game1.netWorldState.Value.GoldenCoconutCracked)
                {
                    // Just in case
                    _locationChecker.AddCheckedLocation(goldenCoconutLocation);
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!_locationChecker.IsLocationMissing(goldenCoconutLocation))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __instance.waitingForServerResponse = true;
                Game1.player.team.goldenCoconutMutex.RequestLock(() =>
                {
                    __instance.waitingForServerResponse = false;
                    var itemToSpawnId = IDProvider.CreateApLocationItemId(goldenCoconutLocation);
                    __instance.geodeTreasureOverride = ItemRegistry.Create(itemToSpawnId);
                    Game1.netWorldState.Value.GoldenCoconutCracked = true;
                    __instance.startGeodeCrack();
                }, () =>
                {
                    __instance.waitingForServerResponse = false;
                    __instance.startGeodeCrack();
                });

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_CrackGoldenCoconut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void SpawnBananaNutReward()
        public static bool SpawnBananaNutReward_CheckInsteadOfNuts_Prefix(IslandEast __instance)
        {
            try
            {
                if (__instance.bananaShrineNutAwarded.Value || !Game1.IsMasterGame)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                Game1.player.team.MarkCollectedNut("BananaShrine");
                __instance.bananaShrineNutAwarded.Value = true;
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Banana Altar", new Vector2(16.5f, 25f) * 64f, __instance, 0, 1280);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SpawnBananaNutReward_CheckInsteadOfNuts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void SpitTreeNut()
        public static bool SpitTreeNut_CheckInsteadOfNut_Prefix(IslandHut __instance)
        {
            try
            {
                if (__instance.treeHitLocal)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                __instance.treeHitLocal = true;
                if (Game1.currentLocation == __instance)
                {
                    Game1.playSound("boulderBreak");
                    DelayedAction.playSoundAfterDelay("croak", 300);
                    DelayedAction.playSoundAfterDelay("slimeHit", 1250);
                    DelayedAction.playSoundAfterDelay("coin", 1250);
                }
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(5, new Vector2(10f, 5f) * 64f, Color.White)
                {
                    motion = new Vector2(0.0f, -1.5f),
                    interval = 25f,
                    delayBeforeAnimationStart = 1250,
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(32, 192, 16, 32), 1250f, 1, 1, new Vector2(10f, 7f) * 64f, false, false, 0.0001f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f)
                {
                    shakeIntensity = 1f,
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(10f, 5f) * 64f, Color.White)
                {
                    motion = new Vector2(0.0f, -3f),
                    interval = 25f,
                    delayBeforeAnimationStart = 1250,
                });
                for (var index = 0; index < 5; ++index)
                {
                    __instance.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(352, 1200, 16, 16), 50f, 11, 3, new Vector2(10f, 5f) * 64f, false, false, 0.1f, 0.01f, Color.White, 4f, 0.0f, 0.0f, 0.0f)
                    {
                        motion =
                        {
                            X = Utility.RandomFloat(-3f, 3f),
                            Y = Utility.RandomFloat(-1f, -3f),
                        },
                        acceleration =
                        {
                            Y = 0.05f,
                        },
                        delayBeforeAnimationStart = 1250,
                    });
                }
                if (!Game1.IsMasterGame || __instance.treeNutObtained.Value)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                Game1.player.team.MarkCollectedNut("TreeNut");

                DelayedAction.functionAfterDelay(() => CreateLocationDebris($"{Prefix.WALNUTSANITY}Leo's Tree", new Vector2(10.5f, 7f) * 64f, __instance), 1250);
                __instance.treeNutObtained.Value = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SpitTreeNut_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void OnPuzzleFinish()
        public static bool OnPuzzleFinish_CheckInsteadOfNuts_Prefix(IslandShrine __instance)
        {
            try
            {
                if (Game1.IsMasterGame)
                {
                    CreateLocationDebris($"{Prefix.WALNUTSANITY}Gem Birds Shrine", new Vector2(24f, 19f) * 64f, __instance, -1);
                }
                if (Game1.currentLocation != __instance)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                Game1.playSound("boulderBreak");
                Game1.playSound("secret1");
                Game1.flashAlpha = 1f;
                __instance.ApplyFinishedTiles();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnPuzzleFinish_CheckInsteadOfNuts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void GiveReward()
        public static bool GiveReward_GourmandCheckInsteadOfNuts_Prefix(IslandFarmCave __instance)
        {
            try
            {
                var gourmandChecks = new[]
                {
                    $"{Prefix.WALNUTSANITY}Gourmand Frog Melon",
                    $"{Prefix.WALNUTSANITY}Gourmand Frog Wheat",
                    $"{Prefix.WALNUTSANITY}Gourmand Frog Garlic",
                };
                // CreateLocationDebris(gourmandChecks[__instance.gourmandRequestsFulfilled.Value], new Vector2(4.5f, 4f) * 64f, __instance, 1);

                ++__instance.gourmandRequestsFulfilled.Value;
                for (var i = 0; i < __instance.gourmandRequestsFulfilled.Value; i++)
                {
                    if (i >= gourmandChecks.Length)
                    {
                        continue;
                    }
                    _locationChecker.AddCheckedLocation(gourmandChecks[i]);
                }

                Game1.player.team.MarkCollectedNut($"IslandGourmand{__instance.gourmandRequestsFulfilled.Value}");
                // private NetMutex gourmandMutex
                var gourmandMutexField = _helper.Reflection.GetField<NetMutex>(__instance, "gourmandMutex");
                var gourmandMutex = gourmandMutexField.GetValue();
                gourmandMutex.ReleaseLock();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GiveReward_GourmandCheckInsteadOfNuts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void OnWhackedChanged(NetBool field, bool old_value, bool new_value)
        public static bool OnWhackedChanged_CheckInsteadOfNut_Prefix(SandDuggy __instance, NetBool field, bool old_value, bool new_value)
        {
            try
            {
                if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback() || !__instance.whacked.Value)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (Game1.IsMasterGame)
                {
                    var index = __instance.currentHoleIndex.Value;
                    if (index == -1)
                    {
                        index = 0;
                    }
                    Game1.player.team.MarkCollectedNut(nameof(SandDuggy));
                    var pixelOrigin = new Vector2(__instance.holeLocations[index].X, __instance.holeLocations[index].Y) * 64f;
                    CreateLocationDebris($"{Prefix.WALNUTSANITY}Whack A Mole", pixelOrigin, __instance.locationRef.Value, -1);
                }
                if (Game1.currentLocation != __instance.locationRef.Value)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                __instance.AnimateWhacked();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnWhackedChanged_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void UpdateWhenCurrentLocation(GameTime time)
        public static bool UpdateWhenCurrentLocation_CheckInsteadOfNuts_Prefix(IslandWestCave1 __instance, GameTime time)
        {
            try
            {
                __instance.enterValueEvent.Poll();
                if ((__instance.localPhase != 1 || __instance.currentPlaybackCrystalSequenceIndex < 0 || __instance.currentPlaybackCrystalSequenceIndex >= __instance.currentCrystalSequence.Count) && __instance.localPhase != __instance.netPhase.Value)
                {
                    __instance.localPhaseTimer = __instance.netPhaseTimer.Value;
                    __instance.localPhase = __instance.netPhase.Value;
                    __instance.currentPlaybackCrystalSequenceIndex = __instance.localPhase == 1 ? 0 : -1;
                }

                // base.UpdateWhenCurrentLocation(time);
                CallBaseUpdateWhenCurrentLocation(__instance, time);

                // protected List<IslandWestCave1.CaveCrystal> crystals
                var crystalsField = _helper.Reflection.GetField<List<IslandWestCave1.CaveCrystal>>(__instance, "crystals");
                var crystals = crystalsField.GetValue();

                foreach (var crystal in crystals)
                {
                    crystal.update();
                }
                TimeSpan elapsedGameTime;
                if (__instance.localPhaseTimer > 0.0)
                {
                    double localPhaseTimer = __instance.localPhaseTimer;
                    elapsedGameTime = time.ElapsedGameTime;
                    var totalMilliseconds = elapsedGameTime.TotalMilliseconds;
                    __instance.localPhaseTimer = (float)(localPhaseTimer - totalMilliseconds);
                    if (__instance.localPhaseTimer <= 0.0)
                    {
                        switch (__instance.localPhase)
                        {
                            case 0:
                            case 4:
                                __instance.currentPlaybackCrystalSequenceIndex = 0;
                                if (Game1.IsMasterGame)
                                {
                                    ++__instance.currentDifficulty.Value;
                                    __instance.currentCrystalSequence.Clear();
                                    __instance.currentCrystalSequenceIndex.Value = 0;
                                    if (__instance.currentDifficulty.Value > (__instance.timesFailed.Value < 8 ? 7 : 6))
                                    {
                                        __instance.netPhaseTimer.Value = 10f;
                                        __instance.netPhase.Value = 5;
                                        break;
                                    }
                                    for (var index = 0; index < __instance.currentDifficulty.Value; ++index)
                                        __instance.currentCrystalSequence.Add(Game1.random.Next(5));
                                    __instance.netPhase.Value = 1;
                                }
                                __instance.betweenNotesTimer = 600f;
                                break;
                            case 5:
                                if (Game1.currentLocation == __instance)
                                {
                                    Game1.playSound("fireball");
                                    Utility.addSmokePuff(__instance, new Vector2(5f, 1f) * 64f);
                                    Utility.addSmokePuff(__instance, new Vector2(7f, 1f) * 64f);
                                }
                                if (Game1.IsMasterGame)
                                {
                                    Game1.player.team.MarkCollectedNut("IslandWestCavePuzzle");
                                    CreateLocationDebris($"{Prefix.WALNUTSANITY}Colored Crystals", new Vector2(6f, 4f) * 64f, __instance, -1);
                                }
                                __instance.completed.Value = true;
                                if (Game1.currentLocation == __instance)
                                {
                                    __instance.addCompletionTorches();
                                    break;
                                }
                                break;
                        }
                    }
                }
                if (__instance.localPhase != 1)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                double betweenNotesTimer = __instance.betweenNotesTimer;
                elapsedGameTime = time.ElapsedGameTime;
                var totalMilliseconds1 = elapsedGameTime.TotalMilliseconds;
                __instance.betweenNotesTimer = (float)(betweenNotesTimer - totalMilliseconds1);
                if (__instance.betweenNotesTimer > 0.0 || __instance.currentCrystalSequence.Count <= 0 || __instance.currentPlaybackCrystalSequenceIndex < 0)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                var index1 = __instance.currentCrystalSequence[__instance.currentPlaybackCrystalSequenceIndex];
                if (index1 < crystals.Count)
                {
                    crystals[index1].activate();
                }
                ++__instance.currentPlaybackCrystalSequenceIndex;
                var num = __instance.currentDifficulty.Value;
                if (__instance.currentDifficulty.Value > 5)
                {
                    --num;
                    if (__instance.timesFailed.Value >= 4)
                    {
                        --num;
                    }
                    if (__instance.timesFailed.Value >= 6)
                    {
                        --num;
                    }
                    if (__instance.timesFailed.Value >= 8)
                    {
                        num = 3;
                    }
                }
                else if (__instance.timesFailed.Value >= 4 && __instance.currentDifficulty.Value > 4)
                {
                    --num;
                }
                __instance.betweenNotesTimer = 1500f / num;
                if (__instance.currentDifficulty.Value > (__instance.timesFailed.Value < 8 ? 7 : 6))
                {
                    __instance.betweenNotesTimer = 100f;
                }
                if (__instance.currentPlaybackCrystalSequenceIndex < __instance.currentCrystalSequence.Count)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                __instance.currentPlaybackCrystalSequenceIndex = -1;
                if (__instance.currentDifficulty.Value > (__instance.timesFailed.Value < 8 ? 7 : 6))
                {
                    if (!Game1.IsMasterGame)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    __instance.netPhaseTimer.Value = 1000f;
                    __instance.netPhase.Value = 5;
                }
                else
                {
                    if (!Game1.IsMasterGame)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    __instance.netPhase.Value = 2;
                    __instance.currentCrystalSequenceIndex.Value = 0;
                }


                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UpdateWhenCurrentLocation_CheckInsteadOfNuts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CallBaseUpdateWhenCurrentLocation(IslandWestCave1 crystalsCave, GameTime time)
        {
            // public override void UpdateWhenCurrentLocation(GameTime time)
            var islandLocationUpdateWhenCurrentLocationMethod = typeof(IslandLocation).GetMethod("UpdateWhenCurrentLocation", BindingFlags.Instance | BindingFlags.Public, new[] { typeof(GameTime) });
            var functionPointer = islandLocationUpdateWhenCurrentLocationMethod.MethodHandle.GetFunctionPointer();
            var baseUpdateWhenCurrentLocation = (Action<GameTime>)Activator.CreateInstance(typeof(Action<GameTime>), crystalsCave, functionPointer);
            baseUpdateWhenCurrentLocation(time);
        }

        // private void ApplyPlantRestoreLeft()
        public static bool ApplyPlantRestoreLeft_CheckInsteadOfNut_Prefix(IslandFieldOffice __instance)
        {
            try
            {
                var position = new Vector2(1.1f, 3.3f) * 64f;
                var color = new Color(0, 220, 150);
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, position, color)
                {
                    layerDepth = 1f,
                    motion = new Vector2(1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, position + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), color * 0.75f)
                {
                    scale = 0.75f,
                    flipped = true,
                    layerDepth = 1f,
                    motion = new Vector2(-1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, position + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), color * 0.75f)
                {
                    scale = 0.75f,
                    delayBeforeAnimationStart = 50,
                    layerDepth = 1f,
                    motion = new Vector2(1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, position + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), color * 0.75f)
                {
                    scale = 0.75f,
                    flipped = true,
                    delayBeforeAnimationStart = 100,
                    layerDepth = 1f,
                    motion = new Vector2(-1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, position + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(250, 100, 250) * 0.75f)
                {
                    scale = 0.75f,
                    flipped = true,
                    delayBeforeAnimationStart = 150,
                    layerDepth = 1f,
                    motion = new Vector2(0.0f, -3f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback())
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (Game1.currentLocation == __instance)
                {
                    Game1.playSound("leafrustle");
                    DelayedAction.playSoundAfterDelay("leafrustle", 150);
                }
                if (!Game1.IsMasterGame)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                Game1.player.team.MarkCollectedNut("IslandLeftPlantRestored");
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Purple Flowers Island Survey", new Vector2(1.5f, 3.3f) * 64f, __instance, 1, 256);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ApplyPlantRestoreLeft_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // private void ApplyPlantRestoreRight()
        public static bool ApplyPlantRestoreRight_CheckInsteadOfNut_Prefix(IslandFieldOffice __instance)
        {
            try
            {
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(7.5f, 3.3f) * 64f, new Color(0, 220, 150))
                {
                    layerDepth = 1f,
                    motion = new Vector2(1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
                {
                    scale = 0.75f,
                    flipped = true,
                    layerDepth = 1f,
                    motion = new Vector2(-1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8.3f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 200, 120) * 0.75f)
                {
                    scale = 0.75f,
                    delayBeforeAnimationStart = 50,
                    layerDepth = 1f,
                    motion = new Vector2(1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
                {
                    scale = 0.75f,
                    flipped = true,
                    delayBeforeAnimationStart = 100,
                    layerDepth = 1f,
                    motion = new Vector2(-1f, -4f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                __instance.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8.5f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 250, 180) * 0.75f)
                {
                    scale = 0.75f,
                    flipped = true,
                    delayBeforeAnimationStart = 150,
                    layerDepth = 1f,
                    motion = new Vector2(0.0f, -3f),
                    acceleration = new Vector2(0.0f, 0.1f),
                });
                if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback())
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (Game1.currentLocation == __instance)
                {
                    Game1.playSound("leafrustle");
                    DelayedAction.playSoundAfterDelay("leafrustle", 150);
                }
                if (!Game1.IsMasterGame)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                Game1.player.team.MarkCollectedNut("IslandRightPlantRestored");
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Purple Starfish Island Survey", new Vector2(7.5f, 3.3f) * 64f, __instance, 3, 256);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ApplyPlantRestoreRight_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public bool donatePiece(int which)
        public static bool DonatePiece_CheckInsteadOfNuts_Prefix(IslandFieldOffice __instance, int which, ref bool __result)
        {
            try
            {
                __instance.piecesDonated[which] = true;
                if (!__instance.centerSkeletonRestored.Value && __instance.isRangeAllTrue(0, 6))
                {
                    __instance.centerSkeletonRestored.Value = true;
                    _locationChecker.AddCheckedLocation($"{Prefix.WALNUTSANITY}Complete Large Animal Collection");
                    __instance.uncollectedRewards.Add(ItemRegistry.Create("(O)69"));
                    Game1.player.team.MarkCollectedNut("IslandCenterSkeletonRestored");
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (!__instance.snakeRestored.Value && __instance.isRangeAllTrue(6, 9))
                {
                    __instance.snakeRestored.Value = true;
                    _locationChecker.AddCheckedLocation($"{Prefix.WALNUTSANITY}Complete Snake Collection");
                    __instance.uncollectedRewards.Add(ItemRegistry.Create("(O)835"));
                    Game1.player.team.MarkCollectedNut("IslandSnakeRestored");
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (!__instance.batRestored.Value && __instance.piecesDonated[9])
                {
                    __instance.batRestored.Value = true;
                    _locationChecker.AddCheckedLocation($"{Prefix.WALNUTSANITY}Complete Mummified Bat Collection");
                    __instance.uncollectedRewards.Add(ItemRegistry.Create("(O)TentKit"));
                    Game1.player.team.MarkCollectedNut("IslandBatRestored");
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (!__instance.frogRestored.Value && __instance.piecesDonated[10])
                {
                    __instance.frogRestored.Value = true;
                    _locationChecker.AddCheckedLocation($"{Prefix.WALNUTSANITY}Complete Mummified Frog Collection");
                    __instance.uncollectedRewards.Add(ItemRegistry.Create("(O)926"));
                    Game1.player.team.MarkCollectedNut("IslandFrogRestored");
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __result = false;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DonatePiece_CheckInsteadOfNuts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer,
        // int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false,
        // bool skipCollisionEffects = false)
        public static bool IsCollidingPosition_CheckInsteadOfNut_Prefix(IslandNorth __instance, Rectangle position, xTile.Dimensions.Rectangle viewport,
            bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile, bool ignoreCharacterRequirement,
            bool skipCollisionEffects, ref bool __result)
        {
            try
            {
                if (!projectile || damagesFarmer != 0 || position.Bottom >= 832)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!position.Intersects(new Rectangle(3648, 576, 256, 64)) || !Game1.IsMasterGame || __instance.treeNutShot.Value)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Game1.player.team.MarkCollectedNut("TreeNutShot");
                __instance.treeNutShot.Value = true;
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Protruding Tree Walnut", new Vector2(58.5f, 11f) * 64f, __instance);

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(IsCollidingPosition_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        public static bool GetFish_CheckInsteadOfNut_Prefix(IslandSouthEast __instance, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName, ref Item __result)
        {
            try
            {
                if ((int)bobberTile.X < 18 || (int)bobberTile.X > 20 || (int)bobberTile.Y < 20 || (int)bobberTile.Y > 22)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!__instance.fishedWalnut.Value)
                {
                    Game1.player.team.MarkCollectedNut("StardropPool");
                    if (!Game1.IsMultiplayer)
                    {
                        __instance.fishedWalnut.Value = true;
                        __result = CreateLocationItem($"{Prefix.WALNUTSANITY}Starfish Tide Pool");
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    __instance.fishWalnutEvent.Fire();
                }
                else
                {
                    _locationChecker.AddCheckedLocation($"{Prefix.WALNUTSANITY}Starfish Tide Pool");
                }

                __result = null;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetFish_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void OnMermaidPuzzleSuccess()
        public static bool OnMermaidPuzzleSuccess_CheckInsteadOfNut_Prefix(IslandSouthEast __instance)
        {
            try
            {
                __instance.currentMermaidAnimation = __instance.mermaidReward;
                __instance.mermaidFrameTimer = 0.0f;
                if (Game1.currentLocation == __instance)
                {
                    Game1.playSound("yoba");
                }
                if (!Game1.IsMasterGame || __instance.mermaidPuzzleFinished.Value)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                Game1.player.team.MarkCollectedNut("Mermaid");
                __instance.mermaidPuzzleFinished.Value = true;
                CreateLocationDebris($"{Prefix.WALNUTSANITY}Mermaid Song", new Vector2(32f, 33f) * 64f, __instance);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnMermaidPuzzleSuccess_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void QuitGame()
        public static bool QuitGame_CheckInsteadOfNut_Prefix(Darts __instance)
        {
            try
            {
                __instance.unload();
                Game1.playSound("bigDeSelect");
                Game1.currentMinigame = null;
                if (__instance.currentGameState != Darts.GameState.GameOver)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (__instance.points != 0)
                {
                    if (Game1.currentLocation is not IslandSouthEastCave)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Lose"));
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var isPerfect = __instance.IsPerfectVictory();
                if (isPerfect)
                {
                    Game1.Multiplayer.globalChatInfoMessage("DartsWinPerfect", Game1.player.Name);
                }
                else
                {
                    Game1.Multiplayer.globalChatInfoMessage("DartsWin", Game1.player.Name, __instance.throwsCount.ToString());
                }
                if (Game1.currentLocation is not IslandSouthEastCave)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var victoryText = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win");
                if (isPerfect)
                {
                    victoryText = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win_Perfect");
                }
                victoryText += "#";
                var numberOfDartNutsAlreadyDropped = Game1.player.team.GetDroppedLimitedNutCount(nameof(Darts));
                if (__instance.startingDartCount == 20 && numberOfDartNutsAlreadyDropped == 0 || __instance.startingDartCount == 15 && numberOfDartNutsAlreadyDropped == 1 || __instance.startingDartCount == 10 && numberOfDartNutsAlreadyDropped == 2)
                {
                    var dialogue = victoryText + Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinPrize");
                    var dartLocations = new[]
                    {
                        $"{Prefix.WALNUTSANITY}Pirate Darts 1",
                        $"{Prefix.WALNUTSANITY}Pirate Darts 2",
                        $"{Prefix.WALNUTSANITY}Pirate Darts 3"
                    };
                    var dartLocation = dartLocations[numberOfDartNutsAlreadyDropped];
                    Game1.afterDialogues += () => CreateLocationDebris(dartLocation, new Vector2(31, 8) * 64f, Game1.currentLocation);
                    numberOfDartNutsAlreadyDropped++;
                    Game1.player.team.limitedNutDrops[nameof(Darts)] = numberOfDartNutsAlreadyDropped;
                    Game1.drawDialogueNoTyping(dialogue);
                }
                else
                {
                    var dialogue = victoryText + Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinNoPrize");
                    Game1.drawDialogueNoTyping(dialogue);
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(QuitGame_CheckInsteadOfNut_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CreateLocationDebris(string locationName, Vector2 pixelOrigin, GameLocation gameLocation, int direction = 0, int groundLevel = 0)
        {
            var item = CreateLocationItem(locationName);
            Game1.createItemDebris(item, pixelOrigin, direction, gameLocation, groundLevel);
        }

        private static Item CreateLocationItem(string locationName)
        {
            var itemId = IDProvider.CreateApLocationItemId(locationName);
            var item = ItemRegistry.Create(itemId);
            return item;
        }
    }
}
