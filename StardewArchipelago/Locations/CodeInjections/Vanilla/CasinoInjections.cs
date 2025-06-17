using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using Microsoft.Xna.Framework.Input;
using static System.Collections.Specialized.BitVector32;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CasinoInjections
    {
        private const string STATUE_LOCATION = "Purchase Statue Of Endless Fortune";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_OfferStatueOfEndlessFortune_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (__instance.Name != "Club")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGet(action, 0, out var key1, out var error, name: "string actionType") || key1 != "ClubSeller")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var statue = "Statue of Endless Fortune";
                var question = $"Psst... I have a '{statue}' for sale... only 1,000,000g. What do you say?";
                if (_locationChecker.IsLocationMissing(STATUE_LOCATION))
                {
                    var scout = _archipelago.ScoutStardewLocation(STATUE_LOCATION, false);
                    if (scout != null)
                    {
                        question = question.Replace(statue, scout.ItemName);
                    }
                }

                var responses = new List<Response>();
                if (_locationChecker.IsLocationMissing(STATUE_LOCATION) || _archipelago.HasReceivedItem(statue))
                {
                    responses.Add(new Response("I'll", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_Yes")));
                }
                
                responses.Add(new Response("No", Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_No")));
                __instance.createQuestionDialogue(question, responses.ToArray(), "ClubSeller");
                
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_OfferStatueOfEndlessFortune_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_PurchaseStatueOfEndlessFortune_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (__instance.Name != "Club")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (questionAndAnswer == "ClubSeller_No" && _locationChecker.IsLocationMissing(STATUE_LOCATION))
                {
                    _archipelago.ScoutStardewLocation(STATUE_LOCATION, true);
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (questionAndAnswer != "ClubSeller_I'll")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = true;
                if (Game1.player.Money < 1000000)
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Club_ClubSeller_NotEnoughMoney"));
                    _archipelago.ScoutStardewLocation(STATUE_LOCATION, true); 
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                Game1.exitActiveMenu();
                Game1.player.forceCanMove();
                if (_locationChecker.IsLocationMissing(STATUE_LOCATION))
                {
                    Game1.player.Money -= 1000000;
                    _locationChecker.AddCheckedLocation(STATUE_LOCATION);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_archipelago.HasReceivedItem("Statue Of Endless Fortune"))
                {
                    Game1.player.Money -= 1000000;
                    Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)127"));
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_PurchaseStatueOfEndlessFortune_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
        
        
    }
}
