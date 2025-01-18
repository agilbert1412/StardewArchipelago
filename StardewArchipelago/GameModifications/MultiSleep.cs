using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications
{
    public class MultiSleep
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private readonly Harmony _harmony;

        public static int DaysToSkip = 0;
        private static int _multiSleepPrice = -1;

        public MultiSleep(ILogger logger, IModHelper modHelper, Harmony harmony)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            DaysToSkip = 0;
        }

        public void InjectMultiSleepOption(SlotData slotData)
        {
            if (!slotData.EnableMultiSleep)
            {
                return;
            }

            _multiSleepPrice = slotData.MultiSleepCostPerDay;

            var performTouchActionParameters = new[] { typeof(string[]), typeof(Vector2) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), performTouchActionParameters),
                prefix: new HarmonyMethod(typeof(MultiSleep), nameof(PerformTouchAction_Sleep_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(MultiSleep), nameof(AnswerDialogueAction_SleepMany_Prefix))
            );
        }

        // public virtual void performTouchAction(string[] action, Vector2 playerStandingPosition)
        public static bool PerformTouchAction_Sleep_Prefix(GameLocation __instance, string[] action, Vector2 playerStandingPosition)
        {
            try
            {
                var actionStringFirstWord = action[0];

                if (Game1.eventUp || actionStringFirstWord != "Sleep" || Game1.newDay || !Game1.shouldTimePass() || !Game1.player.hasMoved || Game1.player.passedOut)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var possibleResponses = new Response[3]
                {
                    new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
                    new Response("Many", "Sleep for multiple days").SetHotKey(Keys.U),
                    new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape),
                };

                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), possibleResponses, "Sleep", null);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformTouchAction_Sleep_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool AnswerDialogueAction_SleepMany_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Sleep_Many")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var multiSleepMessage =
                    "How many days do you wish to sleep for?\n(Warning: Sleeping saves the game, this action cannot be undone)";
                Game1.activeClickableMenu = new MultiSleepSelectionMenu(multiSleepMessage, (value, price, who) => SleepMany(__instance, value), minValue: 1, maxValue: 112, defaultNumber: 7, price: _multiSleepPrice);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_SleepMany_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static void SleepMany(GameLocation instance, int numberOfDays)
        {
            DaysToSkip = numberOfDays - 1;
            var totalPrice = 0;
            if (_multiSleepPrice > 0)
            {
                totalPrice = _multiSleepPrice * DaysToSkip;
            }

            if (Game1.player.Money < totalPrice)
            {
                return;
            }

            Game1.player.Money -= totalPrice;

            var startSleepMethod = _modHelper.Reflection.GetMethod(instance, "startSleep");
            startSleepMethod.Invoke();
        }
    }
}
