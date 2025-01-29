using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;

namespace StardewArchipelago.GameModifications.MultiSleep
{
    public class MultiSleepManager
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private readonly Harmony _harmony;

        public static MultiSleepBehavior _currentMultiSleep;
        public static MultiSleepBehavior CurrentMultiSleep => _currentMultiSleep;
        private static int _multiSleepPrice = 0;

        public MultiSleepManager(ILogger logger, IModHelper modHelper, Harmony harmony)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _currentMultiSleep = new DontMultiSleepBehavior();
        }

        public static bool TryDoMultiSleepOnDayStarted()
        {
            if (!_currentMultiSleep.ShouldKeepSleeping())
            {
                _currentMultiSleep = new DontMultiSleepBehavior();
                return false;
            }

            if (Game1.player.Money < _multiSleepPrice)
            {
                Game1.drawObjectDialogue($"Cannot afford to continue multisleeping. Cost: {_multiSleepPrice}g/day");
                _currentMultiSleep = new DontMultiSleepBehavior();
                return false;
            }

            Game1.player.Money -= _multiSleepPrice;
            _currentMultiSleep.KeepSleeping();
            return true;
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
                prefix: new HarmonyMethod(typeof(MultiSleepManager), nameof(PerformTouchAction_Sleep_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(MultiSleepManager), nameof(AnswerDialogueAction_SleepMany_Prefix))
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

                var possibleResponses = new List<Response>();
                possibleResponses.Add(new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y));
                possibleResponses.Add(new Response("Many", "Sleep for multiple days").SetHotKey(Keys.U));
                possibleResponses.Add(new Response("Until", "Sleep until..."));
                possibleResponses.Add(new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape));

                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), possibleResponses.ToArray(), "Sleep", null);
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
                if (questionAndAnswer == "Sleep_Many")
                {
                    ShowMultisleepDialogue(__instance);
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (questionAndAnswer == "Sleep_Until")
                {
                    ShowSleepUntilDialogue(__instance);
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (questionAndAnswer.StartsWith("SleepUntil_"))
                {
                    InitiateSleepUntil(__instance, questionAndAnswer.Split("_").Last());
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_SleepMany_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ShowMultisleepDialogue(GameLocation __instance)
        {
            var multiSleepMessage =
                "How many days do you wish to sleep for?\n(Warning: Sleeping saves the game, this action cannot be undone)";
            Game1.activeClickableMenu = new MultiSleepSelectionMenu(multiSleepMessage, (value, price, who) => SleepMany(__instance, value), minValue: 1, maxValue: 112, defaultNumber: 7, price: _multiSleepPrice);
        }

        private static void ShowSleepUntilDialogue(GameLocation __instance)
        {
            var multiSleepMessage =
                "Sleep Until...\n(Warning: Sleeping saves the game, this action cannot be undone)";

            var possibleResponses = new List<Response>();
            if (Game1.season != Season.Winter)
            {
                possibleResponses.Add(new Response(MultiSleepUntilBehavior.RAIN, "Rain"));
                possibleResponses.Add(new Response(MultiSleepUntilBehavior.STORM, "Storm"));
            }
            possibleResponses.Add(new Response(MultiSleepUntilBehavior.FESTIVAL, "Festival"));
            possibleResponses.Add(new Response(MultiSleepUntilBehavior.BIRTHDAY, "Birthday"));
            if (TravelingMerchantInjections.HasAnyTravelingMerchantDay())
            {
                possibleResponses.Add(new Response(MultiSleepUntilBehavior.TRAVELING_CART, "Traveling Cart"));
            }
            possibleResponses.Add(new Response(MultiSleepUntilBehavior.END_OF_MONTH, "End of month"));
            possibleResponses.Add(new Response("Cancel", "Nevermind").SetHotKey(Keys.Escape));

            __instance.createQuestionDialogue(multiSleepMessage, possibleResponses.ToArray(), "SleepUntil", null);
        }

        public static void InitiateSleepUntil(GameLocation instance, string untilKey)
        {
            switch (untilKey)
            {
                case MultiSleepUntilBehavior.RAIN:
                case MultiSleepUntilBehavior.STORM:
                case MultiSleepUntilBehavior.FESTIVAL:
                case MultiSleepUntilBehavior.BIRTHDAY:
                case MultiSleepUntilBehavior.TRAVELING_CART:
                    _currentMultiSleep = new MultiSleepUntilBehavior(untilKey);
                    StartSleep(instance);
                    return;
                case MultiSleepUntilBehavior.END_OF_MONTH:
                    SleepMany(instance, 28 - Game1.dayOfMonth);
                    return;

                // case "Cancel":
                default:
                    _currentMultiSleep = new DontMultiSleepBehavior();
                    return;
            }
        }

        public static void SleepMany(GameLocation instance, int numberOfDays)
        {
            var daysToSkip = numberOfDays - 1;
            SetDaysToSkip(daysToSkip);
            StartSleep(instance);
        }

        public static void SetDaysToSkip(int numDays)
        {
            _currentMultiSleep = new MultiSleepDaysBehavior(numDays);
        }

        private static void StartSleep(GameLocation instance)
        {
            var startSleepMethod = _modHelper.Reflection.GetMethod(instance, "startSleep");
            startSleepMethod.Invoke();
        }
    }
}
