using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications
{
    public class JojaDisabler
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private readonly Harmony _harmony;

        public JojaDisabler(ILogger logger, IModHelper modHelper, Harmony harmony)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
        }

        public void DisableJojaRouteShortcuts()
        {
            DisableJojaMembership();
            DisablePerfectionWaivers();
        }

        public void DisableJojaMembership()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(JojaMart), nameof(JojaMart.answerDialogue)),
                prefix: new HarmonyMethod(typeof(JojaDisabler), nameof(AnswerDialogue_JojaMembershipPurchase_Prefix))
            );
        }

        public void DisablePerfectionWaivers()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(JojaDisabler), nameof(AnswerDialogueAction_PerfectionWaiverPurchase_Prefix))
            );
        }

        // public override bool answerDialogue(Response answer)
        public static bool AnswerDialogue_JojaMembershipPurchase_Prefix(JojaMart __instance, Response answer, ref bool __result)
        {
            try
            {
                if (__instance.lastQuestionKey == null || __instance.afterQuestion != null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var response = ArgUtility.SplitBySpaceAndGet(__instance.lastQuestionKey, 0) + "_" + answer.responseKey;
                if (!response.Equals("JojaSignUp_Yes", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                const int membershipPrice = 5000;
                if (Game1.player.Money >= membershipPrice)
                {
                    const string competingBrandText = "I see you are already a member of a competing brand... \"Archipelago\"? Here at JojaMart, we believe in complete commitment to our superior services. Please cancel that membership and come back afterwards.";
                    Game1.drawObjectDialogue(competingBrandText);
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogue_JojaMembershipPurchase_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_PerfectionWaiverPurchase_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Fizz_Yes")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (Game1.player.Money >= 500000)
                {
                    __instance.getCharacterFromName("Fizz")?.shake(500);
                    const string waiversWontWorkText = "Wait a minute... these perfection stats are not the ones I'm used to... What is this? Archiperfection? My waivers won't work on this!";
                    Game1.drawObjectDialogue(waiversWontWorkText);
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_PerfectionWaiverPurchase_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
