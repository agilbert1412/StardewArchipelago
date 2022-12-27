using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewArchipelago
{
    internal class LocationsCodeInjection
    {
        private const string BACKPACK_UPGRADE_LEVEL_KEY = "Backpack_Upgrade_Level_Key";

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private static Action<long> _addCheckedLocation;

        public LocationsCodeInjection(IMonitor monitor, ArchipelagoClient archipelago, Action<long> addCheckedLocation)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _addCheckedLocation = addCheckedLocation;
        }

        public void DoAreaCompleteReward(int whichArea)
        {
            string AreaAPLocationName = "";
            switch ((Area)whichArea)
            {
                case Area.Pantry:
                    AreaAPLocationName = "Complete Pantry";
                    break;
                case Area.CraftsRoom:
                    AreaAPLocationName = "Complete Crafts Room";
                    break;
                case Area.FishTank:
                    AreaAPLocationName = "Complete Fish Tank";
                    break;
                case Area.BoilerRoom:
                    AreaAPLocationName = "Complete Boiler Room";
                    break;
                case Area.Vault:
                    AreaAPLocationName = "Complete Vault";
                    break;
                case Area.Bulletin:
                    AreaAPLocationName = "Complete Bulletin";
                    break;
            }
            var completedAreaAPLocationId = _archipelago.GetLocationId(AreaAPLocationName);
            _addCheckedLocation(completedAreaAPLocationId);
        }

        public static bool AnswerDialogueAction_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Backpack_Purchase")
                {
                    return true; // run original logic
                }

                __result = true;
                var modData = Game1.getFarm().modData;
                if (!modData.ContainsKey(BACKPACK_UPGRADE_LEVEL_KEY))
                {
                    modData.Add(BACKPACK_UPGRADE_LEVEL_KEY, "0");
                }

                if (Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] == "0" && Game1.player.Money >= 2000)
                {
                    Game1.player.Money -= 2000;
                    modData[BACKPACK_UPGRADE_LEVEL_KEY] = "1";
                    var completedAreaAPLocationId = _archipelago.GetLocationId("Backpack Upgrade 1");
                    _addCheckedLocation(completedAreaAPLocationId);
                    return false; // don't run original logic
                }

                if (Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] == "1" && Game1.player.Money >= 10000)
                {
                    Game1.player.Money -= 10000;
                    modData[BACKPACK_UPGRADE_LEVEL_KEY] = "2";
                    var completedAreaAPLocationId = _archipelago.GetLocationId("Backpack Upgrade 2");
                    _addCheckedLocation(completedAreaAPLocationId);
                    return false; // don't run original logic
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool PerformAction_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true;
                }

                var actionName = action.Split(' ').First();
                if (actionName != "BuyBackpack")
                {
                    return true; // run original logic
                }

                __result = true;

                var modData = Game1.getFarm().modData;
                if (!modData.ContainsKey(BACKPACK_UPGRADE_LEVEL_KEY))
                {
                    modData.Add(BACKPACK_UPGRADE_LEVEL_KEY, "0");
                }

                Response responsePurchaseLevel1 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000"));
                Response responsePurchaseLevel2 = new Response("Purchase", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000"));
                Response responseDontPurchase = new Response("Not", Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo"));
                if (modData[BACKPACK_UPGRADE_LEVEL_KEY] == "0")
                {
                    __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24"), new Response[2]
                    {
                        responsePurchaseLevel1,
                        responseDontPurchase
                    }, "Backpack");
                }
                else if (modData[BACKPACK_UPGRADE_LEVEL_KEY] == "1")
                {
                    __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36"), new Response[2]
                    {
                        responsePurchaseLevel2,
                        responseDontPurchase
                    }, "Backpack");
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AnswerDialogueAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
