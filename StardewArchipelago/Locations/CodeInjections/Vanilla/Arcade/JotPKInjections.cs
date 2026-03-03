using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using KaitoKid.Utilities.Interfaces;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Serialization;

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
        private static ArchipelagoStateDto _state;
        private static int _bootsLevel;
        private static int _gunLevel;
        private static int _ammoLevel;
        private static int _bootsItemOffered = -1;
        private static int _gunItemOffered = -1;
        private static int _ammoItemOffered = -1;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, ArchipelagoStateDto state)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _state = state;
        }

        // public void showPrairieKingMenu()
        public static bool ShowPrairieKingMenu_AlwaysShowMenu_Prefix(GameLocation __instance)
        {
            try
            {
                var shouldOfferContinue = Game1.player.jotpkProgress.Value != null;
                var canStartAhead = _state.MaxJotPKLevelBeaten >= 5;
                if (!shouldOfferContinue && !canStartAhead)
                {
                    Game1.currentMinigame = new AbigailGame();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var answerChoices = new List<Response>();

                if (shouldOfferContinue)
                {
                    answerChoices.Add(new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue")));
                }

                answerChoices.Add(new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame")));
                answerChoices.Add(new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit")));

                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu"), answerChoices.ToArray(), "CowboyGame");
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShowPrairieKingMenu_AlwaysShowMenu_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_JotPKStartFromLevels_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (questionAndAnswer == "CowboyGame_NewGame")
                {
                    PlayerPressedNewGame();
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (questionAndAnswer.StartsWith("CowboyGame_Stage"))
                {
                    PlayerPressedNewGameSpecificStage(int.Parse(questionAndAnswer.Substring("CowboyGame_Stage".Length)));
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_JotPKStartFromLevels_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void PlayerPressedNewGame()
        {
            var canStartAhead = _state.MaxJotPKLevelBeaten >= 5;
            if (!canStartAhead)
            {
                Game1.player.jotpkProgress.Value = null;
                Game1.currentMinigame = new AbigailGame();
                return;
            }

            var answerChoices = new List<Response>();

            answerChoices.Add(new Response("Stage1", "Stage 1 - Prairie"));
            if (_state.MaxJotPKLevelBeaten >= 5)
            {
                answerChoices.Add(new Response("Stage2", "Stage 2 - Forest"));
            }
            if (_state.MaxJotPKLevelBeaten >= 9)
            {
                answerChoices.Add(new Response("Stage3", "Stage 3 - Graveyard"));
            }

            Game1.player.currentLocation.createQuestionDialogue("Start From?", answerChoices.ToArray(), "CowboyGame");
        }

        private static void PlayerPressedNewGameSpecificStage(int stage)
        {
            if (stage <= 1)
            {
                Game1.player.jotpkProgress.Value = null;
            }
            else if (stage == 2)
            {
                var progress = new AbigailGame.JOTPKProgress();
                progress.world.Set(2);
                progress.whichRound.Set(0);
                progress.whichWave.Set(5);
                progress.waveTimer.Set(80000);
                progress.monsterChances.Set(GetMonsterChances(progress.whichRound.Value, progress.whichWave.Value));
                Game1.player.jotpkProgress.Value = progress;
            }
            else
            {
                var progress = new AbigailGame.JOTPKProgress();
                progress.world.Set(1);
                progress.whichRound.Set(0);
                progress.whichWave.Set(9);
                progress.waveTimer.Set(80000);
                progress.monsterChances.Set(GetMonsterChances(progress.whichRound.Value, progress.whichWave.Value));
                Game1.player.jotpkProgress.Value = progress;
            }

            Game1.currentMinigame = new AbigailGame();
        }

        private static List<Vector2> GetMonsterChances(int whichRound, int whichWave)
        {
            var monsterChances = new List<Vector2>()
            {
                new(0.014f, 0.4f),
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero,
                Vector2.Zero
            };
            switch (whichWave)
            {
                case 1:
                case 2:
                case 3:
                    monsterChances[0] = new Vector2(monsterChances[0].X + 1f / 1000f, monsterChances[0].Y + 0.02f);
                    if (whichWave > 1)
                    {
                        monsterChances[2] = new Vector2(monsterChances[2].X + 1f / 1000f, monsterChances[2].Y + 0.01f);
                    }
                    monsterChances[6] = new Vector2(monsterChances[6].X + 1f / 1000f, monsterChances[6].Y + 0.01f);
                    if (whichRound > 0)
                    {
                        monsterChances[4] = new Vector2(1f / 500f, 0.1f);
                    }
                    break;
                case 4:
                case 5:
                case 6:
                case 7:
                    if (monsterChances[5].Equals(Vector2.Zero))
                    {
                        monsterChances[5] = new Vector2(0.01f, 0.15f);
                        if (whichRound > 0)
                        {
                            monsterChances[5] = new Vector2((float)(0.0099999997764825821 + whichRound * 0.0040000001899898052), (float)(0.15000000596046448 + whichRound * 0.039999999105930328));
                        }
                    }
                    monsterChances[0] = Vector2.Zero;
                    monsterChances[6] = Vector2.Zero;
                    monsterChances[2] = new Vector2(monsterChances[2].X + 1f / 500f, monsterChances[2].Y + 0.02f);
                    monsterChances[5] = new Vector2(monsterChances[5].X + 1f / 1000f, monsterChances[5].Y + 0.02f);
                    monsterChances[1] = new Vector2(monsterChances[1].X + 0.0018f, monsterChances[1].Y + 0.08f);
                    if (whichRound > 0)
                    {
                        monsterChances[4] = new Vector2(1f / 1000f, 0.1f);
                    }
                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                    monsterChances[5] = Vector2.Zero;
                    monsterChances[1] = Vector2.Zero;
                    monsterChances[2] = Vector2.Zero;
                    if (monsterChances[3].Equals(Vector2.Zero))
                    {
                        monsterChances[3] = new Vector2(0.012f, 0.4f);
                        if (whichRound > 0)
                        {
                            monsterChances[3] = new Vector2((float)(0.012000000104308128 + whichRound * 0.004999999888241291), (float)(0.40000000596046448 + whichRound * 0.075000002980232239));
                        }
                    }
                    if (monsterChances[4].Equals(Vector2.Zero))
                    {
                        monsterChances[4] = new Vector2(3f / 1000f, 0.1f);
                    }
                    monsterChances[3] = new Vector2(monsterChances[3].X + 1f / 500f, monsterChances[3].Y + 0.05f);
                    monsterChances[4] = new Vector2(monsterChances[4].X + 0.0015f, monsterChances[4].Y + 0.04f);
                    if (whichWave == 11)
                    {
                        monsterChances[4] = new Vector2(monsterChances[4].X + 0.01f, monsterChances[4].Y + 0.04f);
                        monsterChances[3] = new Vector2(monsterChances[3].X - 0.01f, monsterChances[3].Y + 0.04f);
                    }
                    break;
            }

            return monsterChances;
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

                string whichCowboyWasBeaten;
                if (which == -1)
                {
                    whichCowboyWasBeaten = JOTPK_COWBOY_1;
                    _state.MaxJotPKLevelBeaten = Math.Max(_state.MaxJotPKLevelBeaten, 5);
                }
                else
                {
                    whichCowboyWasBeaten = JOTPK_COWBOY_2;
                    _state.MaxJotPKLevelBeaten = Math.Max(_state.MaxJotPKLevelBeaten, 9);
                }
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
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StartShoppingLevel_ShopBasedOnSentChecks_PostFix)}:\n{ex}");
            }
        }

        private static int _coinsLastTick = -1;

        public static void Tick_Shopping_PostFix(AbigailGame __instance, GameTime time, ref bool __result)
        {
            try
            {
                if (time.TotalGameTime.Ticks % 300 == 0)
                {
                    _logger.LogWarning($"Playing JotPK. World: {AbigailGame.world}, Round: {__instance.whichRound}, Wave: {AbigailGame.whichWave}");
                    var level = AbigailGame.whichWave;
                    _state.MaxJotPKLevelBeaten = Math.Max(_state.MaxJotPKLevelBeaten, level);
                }


                if (_archipelago.SlotData.ArcadeMachineLocations != ArcadeLocations.FullShuffling)
                {
                    return;
                }

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
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Tick_Shopping_PostFix)}:\n{ex}");
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
                AssignStartingEquipment(__instance);

                if (Game1.player.jotpkProgress.Value != null)
                {
                    return;
                }

                var easyMode = _archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.VictoriesEasy;
                var extraLives = easyMode ? 2 : _archipelago.GetReceivedItemCount(JOTPK_EXTRA_LIFE);
                extraLives = Math.Max(0, Math.Min(2, extraLives));
                __instance.lives += extraLives;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AbigailGameCtor_Equipments_Postfix)}:\n{ex}");
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
