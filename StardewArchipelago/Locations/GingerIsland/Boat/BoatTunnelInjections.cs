﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using Rectangle = xTile.Dimensions.Rectangle;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Locations.GingerIsland.Boat
{
    public class BoatTunnelInjections
    {
        public const string AP_BOAT_REPAIRED = "Boat Repair";
        public const string AP_TICKET_MACHINE = "Repair Ticket Machine";
        public const string AP_BOAT_HULL = "Repair Boat Hull";
        public const string AP_BOAT_ANCHOR = "Repair Boat Anchor";

        public const string MAIL_FIXED_BOAT = "willyBoatFixed";
        public const string MAIL_FIXED_BOAT_TICKET_MACHINE = "willyBoatTicketMachine";
        public const string MAIL_FIXED_BOAT_HULL = "willyBoatHull";
        public const string MAIL_FIXED_BOAT_ANCHOR = "willyBoatAnchor";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_BoatRepairAndUsage_Prefix(BoatTunnel __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tileProperty = __instance.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
                if (tileProperty == "BoatTicket")
                {
                    InteractWithTicketMachine(__instance, who);
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (tileLocation.X == 6 && tileLocation.Y == 8)
                {
                    InteractWithHull(__instance, who);
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (tileLocation.X == 8 && tileLocation.Y == 10)
                {
                    InteractWithAnchor(__instance, who);
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_BoatRepairAndUsage_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override bool answerDialogue(Response answer)
        public static bool AnswerDialogue_BoatRepairAndUsage_Prefix(BoatTunnel __instance, Response answer, ref bool __result)
        {
            try
            {
                if (__instance.lastQuestionKey == null || __instance.afterQuestion != null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var dialogueKey = __instance.lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey;

                switch (dialogueKey)
                {
                    case "WillyBoatDonateIridium_Yes":
                        DonateIridium();
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case "WillyBoatDonateHardwood_Yes":
                        DonateHardwood();
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case "WillyBoatDonateBatteries_Yes":
                        DonateBatteries();
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case "Boat_Yes":
                        PurchaseBoatTicket(__instance);
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    default:
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogue_BoatRepairAndUsage_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void PurchaseBoatTicket(BoatTunnel __instance)
        {
            var ticketPrice = __instance.TicketPrice;
            if (Game1.player.Money >= ticketPrice)
            {
                Game1.player.Money -= ticketPrice;
                __instance.StartDeparture();
            }
            else if (Game1.player.Money < ticketPrice)
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
            }
        }

        private static void DonateBatteries()
        {
            Game1.player.Items.ReduceId(ObjectIds.BATTERY_PACK, 5);
            DelayedAction.playSoundAfterDelay("openBox", 600);
            Game1.addMailForTomorrow(MAIL_FIXED_BOAT_TICKET_MACHINE, true, true);
            _locationChecker.AddCheckedLocation(AP_TICKET_MACHINE);
        }

        private static void DonateHardwood()
        {
            Game1.player.Items.ReduceId(ObjectIds.HARDWOOD, 200);
            DelayedAction.playSoundAfterDelay("Ship", 600);
            Game1.addMailForTomorrow(MAIL_FIXED_BOAT_HULL, true, true);
            _locationChecker.AddCheckedLocation(AP_BOAT_HULL);
        }

        private static void DonateIridium()
        {
            Game1.player.Items.ReduceId(ObjectIds.IRIDIUM_BAR, 5);
            DelayedAction.playSoundAfterDelay("clank", 600);
            DelayedAction.playSoundAfterDelay("clank", 1200);
            DelayedAction.playSoundAfterDelay("clank", 1800);
            Game1.addMailForTomorrow(MAIL_FIXED_BOAT_ANCHOR, true, true);
            _locationChecker.AddCheckedLocation(AP_BOAT_ANCHOR);
        }

        private static void InteractWithTicketMachine(BoatTunnel __instance, Farmer player)
        {
            var ticketMachineFixed = _locationChecker.IsLocationChecked(AP_TICKET_MACHINE);
            if (ticketMachineFixed && !Game1.MasterPlayer.hasOrWillReceiveMail(MAIL_FIXED_BOAT_TICKET_MACHINE))
            {
                Game1.addMailForTomorrow(MAIL_FIXED_BOAT_TICKET_MACHINE, true, true);
            }

            if (!ticketMachineFixed && player.Items.ContainsId(ObjectIds.BATTERY_PACK, 5))
            {
                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateBatteries"),
                    __instance.createYesNoResponses(), "WillyBoatDonateBatteries");
                return;
            }

            var boatIsFunctional = _archipelago.HasReceivedItem(AP_BOAT_REPAIRED);
            if (boatIsFunctional)
            {
                CreateBuyTicketDialogue(__instance);
                return;
            }

            if (!ticketMachineFixed)
            {
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateBatteriesHint"));
                return;
            }

            // Maybe could hint where the boat repair is?
        }

        private static void CreateBuyTicketDialogue(BoatTunnel __instance)
        {
            if (Game1.player.isRidingHorse() && Game1.player.mount != null)
            {
                Game1.player.mount.checkAction(Game1.player, __instance);
                return;
            }

            if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.es)
            {
                __instance.createQuestionDialogueWithCustomWidth(
                    Game1.content.LoadString("Strings\\Locations:BuyTicket",
                        __instance.TicketPrice), __instance.createYesNoResponses(), "Boat");
                return;
            }

            __instance.createQuestionDialogue(
                Game1.content.LoadString("Strings\\Locations:BuyTicket",
                    __instance.TicketPrice), __instance.createYesNoResponses(), "Boat");
        }

        private static void InteractWithHull(BoatTunnel __instance, Farmer player)
        {
            var hullFixed = _locationChecker.IsLocationChecked(AP_BOAT_HULL);
            if (hullFixed)
            {
                if (!Game1.MasterPlayer.hasOrWillReceiveMail(MAIL_FIXED_BOAT_HULL))
                {
                    Game1.addMailForTomorrow(MAIL_FIXED_BOAT_HULL, true, true);
                }
                return;
            }

            if (player.Items.ContainsId(ObjectIds.HARDWOOD, 200))
            {
                __instance.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateHardwood"),
                    __instance.createYesNoResponses(), "WillyBoatDonateHardwood");
                return;
            }

            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateHardwoodHint"));
        }

        private static void InteractWithAnchor(BoatTunnel __instance, Farmer player)
        {
            var anchorFixed = _locationChecker.IsLocationChecked(AP_BOAT_ANCHOR);
            if (anchorFixed)
            {
                if (!Game1.MasterPlayer.hasOrWillReceiveMail(MAIL_FIXED_BOAT_ANCHOR))
                {
                    Game1.addMailForTomorrow(MAIL_FIXED_BOAT_ANCHOR, true, true);
                }
                return;
            }

            if (player.Items.ContainsId(ObjectIds.IRIDIUM_BAR, 5))
            {
                __instance.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateIridium"),
                    __instance.createYesNoResponses(), "WillyBoatDonateIridium");
                return;
            }

            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateIridiumHint"));
        }

        // public override void draw(SpriteBatch spriteBatch)
        public static void Draw_DrawBoatSectionsBasedOnTasksCompleted_Postfix(BoatTunnel __instance, SpriteBatch b)
        {
            try
            {
                if (!Game1.MasterPlayer.hasOrWillReceiveMail(MAIL_FIXED_BOAT) || Game1.farmEvent != null)
                {
                    return;
                }

                var ticketMachineFixed = _locationChecker.IsLocationChecked(AP_TICKET_MACHINE);
                var hullFixed = _locationChecker.IsLocationChecked(AP_BOAT_HULL);
                var anchorFixed = _locationChecker.IsLocationChecked(AP_BOAT_ANCHOR);

                if (ticketMachineFixed && hullFixed && anchorFixed)
                {
                    return;
                }

                if (Game1.eventUp)
                {
                    return;
                }

                DrawRepairMarkers(b, hullFixed, ticketMachineFixed, anchorFixed);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_DrawBoatSectionsBasedOnTasksCompleted_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawRepairMarkers(SpriteBatch spriteBatch, bool hullFixed, bool ticketMachineFixed, bool anchorFixed)
        {
            var timeSinWave = (float)(4.0 * Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2));
            var sourceRectangle = new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8);
            var origin = new Vector2(1f, 4f);
            var color = Color.White;
            var scale = 4f + Math.Max(0.0f, (float)(0.25 - timeSinWave / 4.0));
            if (!hullFixed)
            {
                var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(416f, 456f + timeSinWave));
                spriteBatch.Draw(Game1.mouseCursors, position, sourceRectangle, color, 0.0f, origin, scale, SpriteEffects.None, 1f);
            }

            if (!ticketMachineFixed)
            {
                var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(288f, 520f + timeSinWave));
                spriteBatch.Draw(Game1.mouseCursors, position, sourceRectangle, color, 0.0f, origin, scale, SpriteEffects.None, 1f);
            }

            if (!anchorFixed)
            {
                var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 520f + timeSinWave));
                spriteBatch.Draw(Game1.mouseCursors, position, sourceRectangle, color, 0.0f, origin, scale, SpriteEffects.None, 1f);
            }
        }
    }
}
