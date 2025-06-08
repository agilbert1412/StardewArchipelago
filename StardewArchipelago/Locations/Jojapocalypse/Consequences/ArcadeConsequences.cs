using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using static StardewValley.Minigames.AbigailGame;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class ArcadeConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public bool tick(GameTime time)
        public static bool Tick_IncreaseJotPKTimescale_Prefix(AbigailGame __instance, ref GameTime time, ref bool __result)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                time = new GameTime(time.TotalGameTime * timeScaleFactor, time.ElapsedGameTime * timeScaleFactor, time.IsRunningSlowly);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Tick_IncreaseJotPKTimescale_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public float getMovementSpeed(float speed, int directions)
        public static void GetMovementSpeed_IncreaseJotPKTimescale_Postfix(AbigailGame __instance, float speed, int directions, ref float __result)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return;
                }

                __result *= timeScaleFactor;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetMovementSpeed_IncreaseJotPKTimescale_Postfix)}:\n{ex}");
                return;
            }
        }

        // public CowboyMonster(int which, int health, int speed, Point position)
        public static void CowboyMonsterConstructor1_IncreaseJotPKTimescale_Postfix(CowboyMonster __instance, int which, int health, int speed, Point position)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return;
                }

                __instance.speed = (int)Math.Round(__instance.speed * timeScaleFactor);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CowboyMonsterConstructor1_IncreaseJotPKTimescale_Postfix)}:\n{ex}");
                return;
            }
        }

        // public public CowboyMonster(int which, Point position)
        public static void CowboyMonsterConstructor2_IncreaseJotPKTimescale_Postfix(CowboyMonster __instance, int which, Point position)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return;
                }

                __instance.speed = (int)Math.Round(__instance.speed * timeScaleFactor);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CowboyMonsterConstructor2_IncreaseJotPKTimescale_Postfix)}:\n{ex}");
                return;
            }
        }

        // public CowboyBullet(Point position, Point motion, int damage)
        public static void CowboyBulletConstructor1_IncreaseJotPKTimescale_Postfix(CowboyBullet __instance, Point position, Point motion, int damage)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return;
                }

                __instance.motion = new Point((int)Math.Round(__instance.motion.X * timeScaleFactor), (int)Math.Round(__instance.motion.Y * timeScaleFactor));
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CowboyBulletConstructor1_IncreaseJotPKTimescale_Postfix)}:\n{ex}");
                return;
            }
        }

        // public CowboyBullet(Point position, int direction, int damage)
        public static void CowboyBulletConstructor2_IncreaseJotPKTimescale_Postfix(CowboyBullet __instance, Point position, int direction, int damage)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return;
                }

                __instance.motion = new Point((int)Math.Round(__instance.motion.X * timeScaleFactor), (int)Math.Round(__instance.motion.Y * timeScaleFactor)) ;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CowboyBulletConstructor2_IncreaseJotPKTimescale_Postfix)}:\n{ex}");
                return;
            }
        }

        // public bool tick(GameTime time)
        public static bool Tick_IncreaseJunimoKartTimescale_Prefix(MineCart __instance, ref GameTime time, ref bool __result)
        {
            try
            {
                if (!TryCalculateTimescaleFactor(out var timeScaleFactor))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                time = new GameTime(time.TotalGameTime * timeScaleFactor, time.ElapsedGameTime * timeScaleFactor, time.IsRunningSlowly);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Tick_IncreaseJunimoKartTimescale_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool TryCalculateTimescaleFactor(out float timeScaleFactor)
        {
            var numberArcadePurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.JOTPK) +
                                        _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.JUNIMO_KART);
            if (numberArcadePurchased <= 0)
            {
                timeScaleFactor = 1;
                return false;
            }

            timeScaleFactor = (float)Math.Pow(1.05, numberArcadePurchased);
            return true;
        }
    }
}
