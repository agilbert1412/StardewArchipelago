using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;

namespace StardewArchipelago.GameModifications
{
    public class MultiSleep
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private Harmony _harmony;

        public static int DaysToSkip = 0;

        public MultiSleep(IMonitor monitor, IModHelper modHelper, Harmony harmony)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _harmony = harmony;
            DaysToSkip = 0;
            InjectMultiSleepOption();
        }

        public void InjectMultiSleepOption()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
                prefix: new HarmonyMethod(typeof(MultiSleep), nameof(MultiSleep.PerformTouchAction_Sleep_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(MultiSleep), nameof(MultiSleep.AnswerDialogueAction_SleepMany_Prefix))
            );
        }

        public static bool PerformTouchAction_Sleep_Prefix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            try
            {
                var actionStringFirstWord = fullActionString.Split(' ')[0];

                if (Game1.eventUp || actionStringFirstWord != "Sleep" || Game1.newDay || !Game1.shouldTimePass() || !Game1.player.hasMoved || Game1.player.passedOut)
                {
                    return true; // run original logic
                }

                var possibleResponses = new Response[3]
                {
                    new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y),
                    new Response("Many", "Sleep for multiple days").SetHotKey(Keys.None),
                    new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape),
                };
                
                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), possibleResponses, "Sleep", null);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformTouchAction_Sleep_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AnswerDialogueAction_SleepMany_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Sleep_Many")
                {
                    return true; // run original logic
                }

                var multiSleepMessage =
                    "How many days do you wish to sleep for?\n(Warning: Sleeping saves the game, this action cannot be undone)";
                Game1.activeClickableMenu = new NumberSelectionMenu(multiSleepMessage, (value, price, who) => SleepMany(__instance, value), minValue: 1, maxValue: 112, defaultNumber: 7, price: 0);

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_SleepMany_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void SleepMany(GameLocation instance, int numberOfDays)
        {
            DaysToSkip = numberOfDays - 1;

            var startSleepMethod = _modHelper.Reflection.GetMethod(instance, "startSleep");
            startSleepMethod.Invoke();
        }
    }
}
