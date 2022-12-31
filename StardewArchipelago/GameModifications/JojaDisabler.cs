using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class JojaDisabler
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private Harmony _harmony;

        public JojaDisabler(IMonitor monitor, IModHelper modHelper, Harmony harmony)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _harmony = harmony;
        }

        public void DisableJojaMembership()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(JojaMart), nameof(JojaMart.answerDialogue)),
                prefix: new HarmonyMethod(typeof(JojaDisabler), nameof(JojaDisabler.AnswerDialogue_JojaMembershipPurchase_Prefix))
            );
        }

        public static bool AnswerDialogue_JojaMembershipPurchase_Prefix(JojaMart __instance, Response answer, ref bool __result)
        {
            try
            {
                if (__instance.lastQuestionKey == null || __instance.afterQuestion != null || __instance.lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey != "JojaSignUp_Yes")
                {
                    return true; // run original logic
                }

                const int memberShipPrice = 5000;
                if (Game1.player.Money >= memberShipPrice)
                {
                    JojaMart.Morris.setNewDialogue("I see you are already a member of a competing brand... \"Archipelago\"? Here at JojaMart, we believe in complete commitment to our superior services. Please cancel that membership and come back afterwards.");
                    Game1.drawDialogue(JojaMart.Morris);
                }
                else if (Game1.player.Money < memberShipPrice)
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }

                __result = true;
                return false;  // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogue_JojaMembershipPurchase_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
