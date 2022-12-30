using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Netcode;
using StardewArchipelago.Locations;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewArchipelago.Archipelago
{
    public class DeathManager
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private IModHelper _modHelper;
        private Harmony _harmony;

        private static bool _isCurrentlyReceivingDeathLink = false;

        public DeathManager(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, Harmony harmony)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _modHelper = modHelper;
            _harmony = harmony;
            HookIntoDeathlinkEvents();
        }

        public static void ReceiveDeathLink()
        {
            _isCurrentlyReceivingDeathLink = true;
            foreach (var farmer in Game1.getAllFarmers())
            {
                farmer.health = 0;
            }
        }

        private static void SendDeathLink()
        {
            if (_isCurrentlyReceivingDeathLink)
            {
                _isCurrentlyReceivingDeathLink = false;
                return;
            }

            _archipelago.SendDeathLink(Game1.player.Name);
        }

        public void HookIntoDeathlinkEvents()
        {
            HookIntoDeathEvent();
            HookIntoPassOutEvent();
        }

        private void HookIntoDeathEvent()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playSound)),
                prefix: new HarmonyMethod(typeof(DeathManager), nameof(DeathManager.PlaySound_Death_Prefix))
            );
        }

        private void HookIntoPassOutEvent()
        {
            var farmer = Game1.player.passedOut;
            var passOutEventField = _modHelper.Reflection.GetField<NetEvent0>(farmer, "passOutEvent");
            
            passOutEventField.GetValue().onEvent += SendDeathLink;
        }

        public static bool PlaySound_Death_Prefix(GameLocation __instance, string audioName, NetAudio.SoundContext soundContext = NetAudio.SoundContext.Default)
        {
            try
            {
                if (audioName != "death")
                {
                    return true; // run original logic
                }

                SendDeathLink();
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PlaySound_Death_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
