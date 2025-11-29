using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Arcade
{
    public static class JotPKInjections
    {
        private const string JOTPK_BOOTS_1 = "JotPK: Boots 1";
        private const string JOTPK_BOOTS_2 = "JotPK: Boots 2";
        private const string JOTPK_GUN_1 = "JotPK: Gun 1";
        private const string JOTPK_GUN_2 = "JotPK: Gun 2";
        private const string JOTPK_GUN_3 = "JotPK: Gun 3";
        private const string JOTPK_SUPER_GUN = "JotPK: Super Gun";
        private const string JOTPK_AMMO_1 = "JotPK: Ammo 1";
        private const string JOTPK_AMMO_2 = "JotPK: Ammo 2";
        private const string JOTPK_AMMO_3 = "JotPK: Ammo 3";
        private const string JOTPK_COWBOY_1 = "JotPK: Cowboy 1";
        private const string JOTPK_COWBOY_2 = "JotPK: Cowboy 2";
        public const string JOTPK_VICTORY = "Journey of the Prairie King Victory";

        private const string JOTPK_DROP_RATE = "JotPK: Increased Drop Rate";
        private const string JOTPK_PROGRESSIVE_BOOTS = "JotPK: Progressive Boots";
        private const string JOTPK_PROGRESSIVE_GUN = "JotPK: Progressive Gun";
        private const string JOTPK_PROGRESSIVE_AMMO = "JotPK: Progressive Ammo";
        private const string JOTPK_EXTRA_LIFE = "JotPK: Extra Life";

        private const int BOOTS_1 = 3;
        private const int BOOTS_2 = 4;
        private const int GUN_1 = 0;
        private const int GUN_2 = 1;
        private const int GUN_3 = 2;
        private const int SUPER_GUN = 9;
        private const int AMMO_1 = 6;
        private const int AMMO_2 = 7;
        private const int AMMO_3 = 8;
        private const int EXTRA_LIFE = 5;
        private const int SHERIFF_BADGE = 10;

        private static readonly string[] JOTPK_ALL_LOCATIONS =
        {
            JOTPK_BOOTS_1, JOTPK_BOOTS_2, JOTPK_GUN_1, JOTPK_GUN_2, JOTPK_GUN_3, JOTPK_SUPER_GUN, JOTPK_AMMO_1,
            JOTPK_AMMO_2, JOTPK_AMMO_3, JOTPK_COWBOY_1, JOTPK_COWBOY_2, JOTPK_VICTORY,
        };

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static int _bootsLevel;
        private static int _gunLevel;
        private static int _ammoLevel;
        private static int _bootsItemOffered = -1;
        private static int _gunItemOffered = -1;
        private static int _ammoItemOffered = -1;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool GetLootDrop_ExtraLoot_Prefix(AbigailGame.CowboyMonster __instance, ref int __result)
        {
            try
            {
                if (__instance.type == 6 && __instance.special)
                {
                    __result = -1;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var easyMode = _archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.VictoriesEasy;
                var receivedDropRate = _archipelago.HasReceivedItem(JOTPK_DROP_RATE);
                var increasedDropRate = easyMode || receivedDropRate;

                var moneyDropChance = increasedDropRate ? 0.1 : 0.05;
                if (Game1.random.NextDouble() < moneyDropChance)
                {
                    var type0Mob5CoinChance = increasedDropRate ? 0.02 : 0.01;
                    var otherMob5CoinChance = increasedDropRate ? 0.2 : 0.1;
                    var mobIsType0 = __instance.type != 0;
                    __result = mobIsType0 && Game1.random.NextDouble() < type0Mob5CoinChance || Game1.random.NextDouble() < otherMob5CoinChance ? 1 : 0;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                // 90% Chance of dropping nothing (Original: 95%)
                var dropNothingChance = increasedDropRate ? 0.9 : 0.95;
                if (Game1.random.NextDouble() <= dropNothingChance)
                {
                    __result = -1;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                // 15% Chance of dropping a 6 or 7 item
                if (Game1.random.NextDouble() < 0.15)
                {
                    __result = Game1.random.Next(6, 8);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                // 7% Chance of dropping a 10 item
                if (Game1.random.NextDouble() < 0.07)
                {
                    __result = 10;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                // Item from 2 to 9
                var lootDrop = Game1.random.Next(2, 10);
                if (lootDrop == 5 && Game1.random.NextDouble() < 0.4)
                {
                    // 40% of 5s get rolled again
                    lootDrop = Game1.random.Next(2, 10);
                }
                __result = lootDrop;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetLootDrop_ExtraLoot_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool UsePowerup_PrairieKingBossBeaten_Prefix(AbigailGame __instance, int which)
        {
            try
            {
                if (__instance.activePowerups.ContainsKey(which) || which > -1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (which == -3)
                {
                    _locationChecker.AddCheckedLocation(JOTPK_VICTORY);
                    Game1.chatBox?.addMessage("You can now type '!!arcade_release jotpk' to release all remaining Prairie King checks", Color.Green);
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var whichCowboyWasBeaten = which == -1 ? JOTPK_COWBOY_1 : JOTPK_COWBOY_2;
                _locationChecker.AddCheckedLocation(whichCowboyWasBeaten);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UsePowerup_PrairieKingBossBeaten_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static void StartShoppingLevel_ShopBasedOnSentChecks_PostFix(AbigailGame __instance)
        {
            try
            {
                _bootsItemOffered = GetBootsItemToOffer();
                _gunItemOffered = GetGunItemToOffer();
                _ammoItemOffered = GetAmmoItemToOffer();

                __instance.storeItems.Clear();
                __instance.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), _bootsItemOffered);
                __instance.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), _gunItemOffered);
                __instance.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), _ammoItemOffered);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StartShoppingLevel_ShopBasedOnSentChecks_PostFix)}:\n{ex}");
                return;
            }
        }

        private static int _coinsLastTick = -1;

        public static void Tick_Shopping_PostFix(AbigailGame __instance, GameTime time, ref bool __result)
        {
            try
            {
                if (CheckBootsPurchaseLocation(__instance))
                {
                    return;
                }
                if (CheckSuperGunPurchaseLocation(__instance))
                {
                    return;
                }
                if (CheckAmmoPurchaseLocation(__instance))
                {
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Tick_Shopping_PostFix)}:\n{ex}");
                return;
            }
            finally
            {
                _coinsLastTick = __instance.coins;
            }
        }

        private static bool CheckBootsPurchaseLocation(AbigailGame __instance)
        {
            if (__instance.runSpeedLevel == _bootsLevel)
            {
                return false;
            }

            switch (_bootsItemOffered)
            {
                case BOOTS_1:
                    _locationChecker.AddCheckedLocation(JOTPK_BOOTS_1);
                    break;
                case BOOTS_2:
                    _locationChecker.AddCheckedLocation(JOTPK_BOOTS_2);
                    break;
            }

            AssignStartingEquipment(__instance);
            return true;
        }

        private static bool CheckSuperGunPurchaseLocation(AbigailGame __instance)
        {
            if (__instance.coins == _coinsLastTick - 99) // This is the only way I found to detect the purchase of the super gun check
            {
                _locationChecker.AddCheckedLocation(JOTPK_SUPER_GUN);
                AssignStartingEquipment(__instance);
            }

            var instanceGun = __instance.fireSpeedLevel + (__instance.spreadPistol ? 1 : 0);
            if (instanceGun == _gunLevel)
            {
                return false;
            }

            switch (_gunItemOffered)
            {
                case GUN_1:
                    _locationChecker.AddCheckedLocation(JOTPK_GUN_1);
                    break;
                case GUN_2:
                    _locationChecker.AddCheckedLocation(JOTPK_GUN_2);
                    break;
                case GUN_3:
                    _locationChecker.AddCheckedLocation(JOTPK_GUN_3);
                    break;
                case SUPER_GUN:
                    _locationChecker.AddCheckedLocation(JOTPK_SUPER_GUN);
                    break;
            }

            AssignStartingEquipment(__instance);
            return true;
        }

        private static bool CheckAmmoPurchaseLocation(AbigailGame __instance)
        {
            if (__instance.ammoLevel == _ammoLevel)
            {
                return false;
            }

            switch (_ammoItemOffered)
            {
                case AMMO_1:
                    _locationChecker.AddCheckedLocation(JOTPK_AMMO_1);
                    break;
                case AMMO_2:
                    _locationChecker.AddCheckedLocation(JOTPK_AMMO_2);
                    break;
                case AMMO_3:
                    _locationChecker.AddCheckedLocation(JOTPK_AMMO_3);
                    break;
            }

            AssignStartingEquipment(__instance);
            return true;
        }

        // public AbigailGame(NPC abigail = null)
        public static void AbigailGameCtor_Equipments_Postfix(AbigailGame __instance, NPC abigail)
        {
            try
            {
                if (Game1.player.jotpkProgress.Value != null)
                {
                    return;
                }

                AssignStartingEquipment(__instance);

                var easyMode = _archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.VictoriesEasy;
                var extraLives = easyMode ? 2 : _archipelago.GetReceivedItemCount(JOTPK_EXTRA_LIFE);
                extraLives = Math.Max(0, Math.Min(2, extraLives));
                __instance.lives += extraLives;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AbigailGameCtor_Equipments_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void AssignStartingEquipment(AbigailGame __instance)
        {
            var easyMode = _archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.VictoriesEasy;

            _bootsLevel = easyMode ? 1 : _archipelago.GetReceivedItemCount(JOTPK_PROGRESSIVE_BOOTS);
            _bootsLevel = Math.Max(0, Math.Min(2, _bootsLevel));

            _gunLevel = easyMode ? 1 : _archipelago.GetReceivedItemCount(JOTPK_PROGRESSIVE_GUN);
            _gunLevel = Math.Max(0, Math.Min(4, _gunLevel));

            _ammoLevel = easyMode ? 1 : _archipelago.GetReceivedItemCount(JOTPK_PROGRESSIVE_AMMO);
            _ammoLevel = Math.Max(0, Math.Min(3, _ammoLevel));

            __instance.runSpeedLevel = _bootsLevel;
            __instance.fireSpeedLevel = _gunLevel == 4 ? 3 : _gunLevel;
            __instance.spreadPistol = _gunLevel == 4;
            __instance.ammoLevel = _ammoLevel;
            __instance.bulletDamage = 1 + _ammoLevel;

            _bootsItemOffered = -1;
            _gunItemOffered = -1;
            _ammoItemOffered = -1;
        }

        private static int GetBootsItemToOffer()
        {
            var missingBoots1 = _locationChecker.IsLocationNotChecked(JOTPK_BOOTS_1);
            var missingBoots2 = _locationChecker.IsLocationNotChecked(JOTPK_BOOTS_2);
            var bootsItemOffered = missingBoots1 ? BOOTS_1 : missingBoots2 ? BOOTS_2 : EXTRA_LIFE;
            return bootsItemOffered;
        }

        private static int GetGunItemToOffer()
        {
            var missingGun1 = _locationChecker.IsLocationNotChecked(JOTPK_GUN_1);
            var missingGun2 = _locationChecker.IsLocationNotChecked(JOTPK_GUN_2);
            var missingGun3 = _locationChecker.IsLocationNotChecked(JOTPK_GUN_3);
            var missingSuperGun = _locationChecker.IsLocationNotChecked(JOTPK_SUPER_GUN);
            var gunItemOffered = missingGun1 ? GUN_1 : missingGun2 ? GUN_2 : missingGun3 ? GUN_3 : missingSuperGun ? SUPER_GUN : SHERIFF_BADGE;
            return gunItemOffered;
        }

        private static int GetAmmoItemToOffer()
        {
            var missingAmmo1 = _locationChecker.IsLocationNotChecked(JOTPK_AMMO_1);
            var missingAmmo2 = _locationChecker.IsLocationNotChecked(JOTPK_AMMO_2);
            var missingAmmo3 = _locationChecker.IsLocationNotChecked(JOTPK_AMMO_3);
            var ammoItemOffered = missingAmmo1 ? AMMO_1 : missingAmmo2 ? AMMO_2 : missingAmmo3 ? AMMO_3 : SHERIFF_BADGE;
            return ammoItemOffered;
        }

        public static void ReleasePrairieKing()
        {
            foreach (var prairieKingLocation in JOTPK_ALL_LOCATIONS)
            {
                _locationChecker.AddCheckedLocation(prairieKingLocation);
            }
        }
    }

    public enum Powerup
    {
        SingleCoin = 0,
        FiveCoins = 1,
        ExtraLife = 8,
    }
}
