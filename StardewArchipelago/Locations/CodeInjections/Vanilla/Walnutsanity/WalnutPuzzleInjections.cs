using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutPuzzleInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
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
                    return true; // run original logic
                }

                var goldenCoconutLocation = $"Open Golden Coconut";
                if (Game1.netWorldState.Value.GoldenCoconutCracked && _locationChecker.IsLocationMissing(goldenCoconutLocation))
                {
                    // Just in case
                    _locationChecker.AddCheckedLocation(goldenCoconutLocation);
                    return true; // run original logic
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

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ReceiveLeftClick_CrackGoldenCoconut_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public virtual void SpawnBananaNutReward()
        public static bool SpawnBananaNutReward_CheckInsteadOfNuts_Prefix(IslandEast __instance)
        {
            try
            {
                if (__instance.bananaShrineNutAwarded.Value || !Game1.IsMasterGame)
                {
                    return false; // don't run original logic
                }
                Game1.player.team.MarkCollectedNut("BananaShrine");
                __instance.bananaShrineNutAwarded.Value = true;
                CreateLocationDebris("Banana Altar", new Vector2(16.5f, 25f) * 64f, __instance, 0, 1280);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SpawnBananaNutReward_CheckInsteadOfNuts_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public virtual void SpitTreeNut()
        public static bool SpitTreeNut_CheckInsteadOfNut_Prefix(IslandHut __instance)
        {
            try
            {
                if (__instance.treeHitLocal)
                {
                    return false; // don't run original logic
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
                    return false; // don't run original logic
                }
                Game1.player.team.MarkCollectedNut("TreeNut");
                
                DelayedAction.functionAfterDelay(() => CreateLocationDebris("Leo's Tree", new Vector2(10.5f, 7f) * 64f, __instance), 1250);
                __instance.treeNutObtained.Value = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SpitTreeNut_CheckInsteadOfNut_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public void OnPuzzleFinish()
        public static bool OnPuzzleFinish_CheckInsteadOfNuts_Prefix(IslandShrine __instance)
        {
            try
            {
                if (Game1.IsMasterGame)
                {
                    CreateLocationDebris("Gem Birds Shrine", new Vector2(24f, 19f) * 64f, __instance, -1);
                }
                if (Game1.currentLocation != __instance)
                {
                    return false; // don't run original logic
                }
                Game1.playSound("boulderBreak");
                Game1.playSound("secret1");
                Game1.flashAlpha = 1f;
                __instance.ApplyFinishedTiles();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(OnPuzzleFinish_CheckInsteadOfNuts_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public virtual void GiveReward()
        public static bool GiveReward_CheckInsteadOfNuts_Prefix(IslandFarmCave __instance)
        {
            try
            {
                var gourmandChecks = new[] { "Gourmand Frog Melon", "Gourmand Frog Wheat", "Gourmand Frog Garlic" };
                CreateLocationDebris(gourmandChecks[__instance.gourmandRequestsFulfilled.Value], new Vector2(24f, 19f) * 64f, __instance, -1);
                ++__instance.gourmandRequestsFulfilled.Value;
                Game1.player.team.MarkCollectedNut($"IslandGourmand{__instance.gourmandRequestsFulfilled.Value}");
                // private NetMutex gourmandMutex
                var gourmandMutexField = _helper.Reflection.GetField<NetMutex>(__instance, "gourmandMutex");
                var gourmandMutex = gourmandMutexField.GetValue();
                gourmandMutex.ReleaseLock();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GiveReward_CheckInsteadOfNuts_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void CreateLocationDebris(string locationName, Vector2 pixelOrigin, GameLocation gameLocation, int direction = 0, int groundLevel = 0)
        {
            var itemId = IDProvider.CreateApLocationItemId(locationName);
            Game1.createItemDebris(ItemRegistry.Create(itemId), pixelOrigin, direction, gameLocation, groundLevel);
        }
    }
}
