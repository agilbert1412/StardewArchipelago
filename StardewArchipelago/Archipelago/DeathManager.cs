using System;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Archipelago
{
    public class DeathManager
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static Harmony _harmony;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private static bool _isCurrentlyReceivingDeathLink = false;

        public DeathManager(ILogger logger, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
        }

        public static void Initialize(LocationChecker locationChecker)
        {
            _locationChecker = locationChecker;
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
                    TriggerDeathBundle();
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Update_SendDeathLink_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void TriggerDeathBundle()
        {
            if (Game1.currentLocation is not CommunityCenter communityCenter)
            {
                return;
            }

            var index = -1;
            foreach (var (bundleKey, bundleData) in Game1.netWorldState.Value.BundleData)
            {
                var name = bundleData.Split("/").First();
                if (name == MemeBundleNames.DEATH)
                {
                    index = int.Parse(bundleKey.Split("/").Last());
                    break;
                }
            }
            if (index == -1)
            {
                return;
            }
            communityCenter.bundleRewards[index] = true;
            communityCenter.bundles.FieldDict[index][0] = true;
            var bundleReader = new BundleReader();
            bundleReader.CheckAllBundleLocations(_locationChecker);
        }

        public static bool PerformPassOut_SendDeathLink_Prefix(Farmer __instance)
        {
            try
            {
                if (__instance.stamina <= -15)
                {
                    SendDeathLink("Passed out");
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformPassOut_SendDeathLink_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
