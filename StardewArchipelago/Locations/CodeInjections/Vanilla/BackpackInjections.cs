using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants.Modded;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class BackpackInjections
    {
        private const int UNINITIALIZED = -1;
        private const string LARGE_PACK = "Large Pack";
        private const string DELUXE_PACK = "Deluxe Pack";
        private const string PREMIUM_PACK = "Premium Pack";
        private const string PROGRESSIVE_BACKPACK = "Progressive Backpack";

        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private static uint _dayLastUpdateBackpackDisplay;
        private static int _maxItemsForBackpackDisplay;
        private static int _realMaxItems;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _realMaxItems = UNINITIALIZED;
            UpdateMaxItemsForBackpackDisplay();
        }

        private static void UpdateMaxItemsForBackpackDisplay()
        {
            var numReceivedBackpacks = _archipelago.GetReceivedItemCount(PROGRESSIVE_BACKPACK);
            if (_locationChecker.IsLocationMissing(LARGE_PACK))
            {
                _maxItemsForBackpackDisplay = 12;
            }
            else if (_locationChecker.IsLocationMissing(DELUXE_PACK) && numReceivedBackpacks >= 1)
            {
                _maxItemsForBackpackDisplay = 24;
            }
            else if (_locationChecker.IsLocationMissing(PREMIUM_PACK) && numReceivedBackpacks >= 2)
            {
                _maxItemsForBackpackDisplay = 36;
            }
            else
            {
                _maxItemsForBackpackDisplay = 48;
            }

            _dayLastUpdateBackpackDisplay = Game1.stats.DaysPlayed;
        }

        public static bool AnswerDialogueAction_BackPackPurchase_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Backpack_Purchase")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = true;

                if (_locationChecker.IsLocationNotChecked(LARGE_PACK) && Game1.player.Money >= 2000)
                {
                    Game1.player.Money -= 2000;
                    _locationChecker.AddCheckedLocation(LARGE_PACK);
                    UpdateMaxItemsForBackpackDisplay();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationNotChecked(DELUXE_PACK) && Game1.player.Money >= 10000)
                {
                    Game1.player.Money -= 10000;
                    _locationChecker.AddCheckedLocation(DELUXE_PACK);
                    UpdateMaxItemsForBackpackDisplay();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationMissing(PREMIUM_PACK) && Game1.player.Money >= 50000)
                {
                    Game1.player.Money -= 50000;
                    _locationChecker.AddCheckedLocation(PREMIUM_PACK);
                    UpdateMaxItemsForBackpackDisplay();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_BackPackPurchase_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_BuyBackpack_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGet(action, 0, out var actionName, out _, name: "string actionType"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (actionName == "BuyBackpack")
                {
                    BuyBackPackArchipelago(__instance, out __result);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_BuyBackpack_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void BuyBackPackArchipelago(GameLocation __instance, out bool __result)
        {
            __result = true;
            var numReceivedBackpacks = _archipelago.GetReceivedItemCount(PROGRESSIVE_BACKPACK);

            var responsePurchaseLevel1 = new Response("Purchase",
                Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
            var responsePurchaseLevel2 = new Response("Purchase",
                Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
            var responseDontPurchase = new Response("Not",
                Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
            if (_locationChecker.IsLocationNotChecked(LARGE_PACK))
            {
                __instance.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"),
                    new Response[2]
                    {
                        responsePurchaseLevel1,
                        responseDontPurchase,
                    }, "Backpack");
            }
            else if (_locationChecker.IsLocationNotChecked(DELUXE_PACK) && numReceivedBackpacks >= 1)
            {
                __instance.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"),
                    new Response[2]
                    {
                        responsePurchaseLevel2,
                        responseDontPurchase,
                    }, "Backpack");
            }
            else if (_archipelago.SlotData.Mods.HasMod(ModNames.BIGGER_BACKPACK) && _locationChecker.IsLocationMissing(PREMIUM_PACK) && numReceivedBackpacks >= 2)
            {
                var yes = new Response("Purchase", "Purchase (50,000g)");
                var no = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
                var resps = new Response[] { yes, no };
                __instance.createQuestionDialogue("Backpack Upgrade -- 48 slots", resps, "Backpack");
            }
        }

        // public override void draw(SpriteBatch b)
        public static bool Draw_SeedShopBackpack_Prefix(SeedShop __instance, SpriteBatch b)
        {
            try
            {
                if (Game1.stats.DaysPlayed != _dayLastUpdateBackpackDisplay)
                {
                    UpdateMaxItemsForBackpackDisplay();
                }

                if (_realMaxItems == UNINITIALIZED)
                {
                    _realMaxItems = Game1.player.MaxItems;
                }

                Game1.player.MaxItems = _maxItemsForBackpackDisplay;
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_SeedShopBackpack_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void draw(SpriteBatch b)
        public static void Draw_SeedShopBackpack_Postfix(SeedShop __instance, SpriteBatch b)
        {
            try
            {
                if (_realMaxItems != UNINITIALIZED)
                {
                    Game1.player.MaxItems = _realMaxItems;
                }
                _realMaxItems = UNINITIALIZED;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_SeedShopBackpack_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
