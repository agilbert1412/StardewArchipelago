using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.Festival
{
    internal class DesertFestivalInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static LocalizedContentManager _englishContentManager;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _englishContentManager = new LocalizedContentManager(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory, new CultureInfo("en-US"));
        }

        // public virtual void CollectRacePrizes()
        public static bool CollectRacePrizes_RaceWinner_Prefix(DesertFestival __instance)
        {
            try
            {
                var rewards = new List<Item>();
                if (__instance.specialRewardsCollected.ContainsKey(Game1.player.UniqueMultiplayerID) && !__instance.specialRewardsCollected[Game1.player.UniqueMultiplayerID])
                {
                    __instance.specialRewardsCollected[Game1.player.UniqueMultiplayerID] = true;
                    // No 100 eggs reward
                    // rewards.Add(ItemRegistry.Create("CalicoEgg", 100));
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.CALICO_RACE);

                for (var index = 0; index < __instance.rewardsToCollect[Game1.player.UniqueMultiplayerID]; ++index)
                {
                    rewards.Add(ItemRegistry.Create("CalicoEgg", 20));
                }
                __instance.rewardsToCollect[Game1.player.UniqueMultiplayerID] = 0;
                Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, "Rewards", canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, context: __instance);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CollectRacePrizes_RaceWinner_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public virtual void ReceiveMakeOver(int randomSeedOverride = -1)
        public static void ReceiveMakeOver_RaceWinner_Postfix(DesertFestival __instance, int randomSeedOverride)
        {
            try
            {
                _locationChecker.AddCheckedLocation(FestivalLocationNames.EMILYS_OUTFIT_SERVICES);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ReceiveMakeOver_RaceWinner_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public override bool answerDialogueAction(string question_and_answer, string[] question_params)
        public static bool AnswerDialogueAction_CactusMan_Prefix(DesertFestival __instance, string question_and_answer, string[] question_params, ref bool __result)
        {
            try
            {
                if (!question_and_answer.Equals("CactusMan_Yes", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true; // run original logic
                }

                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Intro"));
                Game1.afterDialogues += () => TryGetFreeCactus(__instance);

                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_CactusMan_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void TryGetFreeCactus(DesertFestival desertFestival)
        {
            if (Game1.player.isInventoryFull())
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
                return;
            }

            var seed = (int)(Game1.player.UniqueMultiplayerID + Game1.year);
            Game1.player.freezePause = 4000;

            // protected NetEvent1Field<int, NetInt> _revealCactusEvent = new NetEvent1Field<int, NetInt>();
            var revealCactusEventField = _modHelper.Reflection.GetField<NetEvent1Field<int, NetInt>>(desertFestival, "_revealCactusEvent");
            var revealCactusEvent = revealCactusEventField.GetValue();

            DelayedAction.functionAfterDelay(() => revealCactusEvent.Fire(seed), 1000);
            DelayedAction.functionAfterDelay(() => GetFreeCactus(desertFestival, seed), 3000);
        }

        private static void GetFreeCactus(DesertFestival desertFestival, int seed)
        {

            var random = Utility.CreateRandom(seed);
            random.Next();
            random.Next();
            random.Next();
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_" + random.Next(1, 6).ToString()));
            Game1.afterDialogues += () =>
            {
                _locationChecker.AddCheckedLocation(FestivalLocationNames.FREE_CACTUS);
                Game1.playSound("coin");
                Game1.player.mailReceived.Add(desertFestival.GetCactusMail());

                // protected NetEvent1Field<int, NetInt> _hideCactusEvent = new NetEvent1Field<int, NetInt>();
                var hideCactusEventField = _modHelper.Reflection.GetField<NetEvent1Field<int, NetInt>>(desertFestival, "_hideCactusEvent");
                var hideCactusEvent = hideCactusEventField.GetValue();
                hideCactusEvent.Fire(seed);

                Game1.player.freezePause = 100;
                if (_archipelago.HasReceivedItem(FestivalLocationNames.FREE_CACTUS))
                {
                    Game1.player.addItemToInventoryBool(new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed));
                    return;
                }
            };
        }

        // public override bool answerDialogueAction(string question_and_answer, string[] question_params)
        public static void AnswerDialogueAction_DesertChef_Postfix(DesertFestival __instance, string question_and_answer, string[] question_params, ref bool __result)
        {
            try
            {
                CheckDesertChefLocation(__instance, question_and_answer);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_DesertChef_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static void CheckDesertChefLocation(DesertFestival desertFestival, string question_and_answer)
        {
            if (!question_and_answer.StartsWith("Cook_ChoseSauce", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            // protected int _cookIngredient = -1;
            // protected int _cookSauce = -1;
            var cookIngredientField = _modHelper.Reflection.GetField<int>(desertFestival, "_cookIngredient");
            var cookSauceField = _modHelper.Reflection.GetField<int>(desertFestival, "_cookSauce");
            var cookIngredient = cookIngredientField.GetValue();
            var cookSauce = cookSauceField.GetValue();

            var foodPath = $"Strings\\1_6_Strings:Cook_DishNames_{cookIngredient}_{cookSauce}";
            var foodName = _englishContentManager.LoadString(foodPath);
            _locationChecker.AddCheckedLocation(foodName);
        }
    }
}
