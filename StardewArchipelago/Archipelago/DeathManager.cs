using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class DeathManager
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private IModHelper _modHelper;
        private readonly Harmony _harmony;

        private static bool _isCurrentlyReceivingDeathLink = false;

        public DeathManager(ILogger logger, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
        }

        public static void ReceiveDeathLink()
        {
            if (!_archipelago.DeathLink)
            {
                return;
            }

            _isCurrentlyReceivingDeathLink = true;
            foreach (var farmer in Game1.getAllFarmers())
            {
                farmer.health = 0;
            }
        }

        private static void SendDeathLink(string cause)
        {
            if (!_archipelago.DeathLink)
            {
                return;
            }

            if (_isCurrentlyReceivingDeathLink)
            {
                _isCurrentlyReceivingDeathLink = false;
                return;
            }

            _archipelago.SendDeathLink(cause);
        }

        public void HookIntoDeathlinkEvents()
        {
            HookIntoDeathEvent();
            HookIntoPassOutEvent();
        }

        private void HookIntoDeathEvent()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.Update)),
                prefix: new HarmonyMethod(typeof(DeathManager), nameof(Update_SendDeathLink_Prefix))
            );
        }

        private void HookIntoPassOutEvent()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "performPassOut"),
                prefix: new HarmonyMethod(typeof(DeathManager), nameof(PerformPassOut_SendDeathLink_Prefix))
            );
        }

        public static bool Update_SendDeathLink_Prefix(Farmer __instance, GameTime time, GameLocation location)
        {
            try
            {
                if (__instance.CanMove && __instance.health <= 0 && !Game1.killScreen && Game1.timeOfDay < 2600)
                {
                    SendDeathLink("died in combat");
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Update_SendDeathLink_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        public static bool PerformPassOut_SendDeathLink_Prefix(Farmer __instance)
        {
            try
            {
                if (__instance.stamina <= -15)
                {
                    SendDeathLink("Passed out");
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformPassOut_SendDeathLink_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
