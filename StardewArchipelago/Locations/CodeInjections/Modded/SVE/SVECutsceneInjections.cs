using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.SpecialOrders;

namespace StardewArchipelago.Locations.CodeInjections.Modded.SVE
{
    public class SVECutsceneInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private const string AURORA_EVENT = "658059254";
        private const string MORGAN_EVENT = "658078924";
        private const string RAILROAD_KEY = "Clint2Again";
        private const string RAILROAD_BOULDER_ID = "8050108";
        private const string IRIDIUM_BOMB_ID = "8050109";
        private const string LANCE_CHEST_LOCATION = "Quest: Monster Crops";
        private const string MONSTER_ERADICATION_AP_PREFIX = "Monster Eradication: ";

        private const string APPLES_NAME = "Apples";

        private static readonly List<string> voidSpirits = new()
        {
            MonsterName.SHADOW_BRUTE, MonsterName.SHADOW_SHAMAN, MonsterName.SHADOW_SNIPER, MonsterCategory.VOID_SPIRITS,
            string.Join("30 ", MonsterCategory.VOID_SPIRITS), string.Join("60 ", MonsterCategory.VOID_SPIRITS),
            string.Join("90 ", MonsterCategory.VOID_SPIRITS), string.Join("120 ", MonsterCategory.VOID_SPIRITS),
        };

        private const string DEINFEST_AP_LOCATION = "Purify an Infested Lichtung";

        private static readonly Dictionary<string, string> sveEventSpecialOrders = new()
        {
            { "8050108", "Clint2" },
            { "2551994", "Clint3" },
            { "8033859", "Lewis2" },
            { "2554903", "Robin3" },
            { "2554928", "Robin4" },
            { "7775926", "Apples" },
            { "65360183", "MarlonFay2" },
            { "65360186", "Lance" },
            { "1090506", "Krobus" },
        };

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        public static bool CheckForAction_LanceChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity,
            ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.Items.Count is <= 0 or > 1 || __instance.Items.First().Name != "Diamond Wand")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                {
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                }
                else
                {
                    __instance.performOpenChest();
                }

                var obj = __instance.Items[0];
                __instance.Items[0] = null;
                __instance.Items.RemoveAt(0);
                __result = true;

                _locationChecker.AddCheckedLocation(LANCE_CHEST_LOCATION);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForAction_LanceChest_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void endBehaviors(string[] args, GameLocation location)
        public static bool EndBehaviors_AddSpecialOrderAfterEvent_Prefix(Event __instance, string[] args, GameLocation location)
        {
            try
            {
                if (!sveEventSpecialOrders.ContainsKey(__instance.id))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                //Change the key so it doesn't get deleted
                var eventsKey = sveEventSpecialOrders[__instance.id];

                var specialOrder = SpecialOrder.GetSpecialOrder(eventsKey, null);
                Game1.player.team.specialOrders.Add(specialOrder);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(EndBehaviors_AddSpecialOrderAfterEvent_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // Railroad Boulder Special Order won't load if Iridium Bomb is sent early, so we duplicate it so the player gets it.
        // private static void UpdateSpecialOrders()
        public static bool UpdateSpecialOrders_StopDeletingSpecialOrders_Prefix()
        {
            try
            {
                return false; // we're not using this, its too strict.
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UpdateSpecialOrders_StopDeletingSpecialOrders_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // Original method runs on SaveLoaded, OnWarped, TimeChanged
        // private static void FixMonsterSlayerQuest()
        public static void FixMonsterSlayerQuest_IncludeReleaseofGoals_Postfix()
        {
            try
            {
                if (!Game1.player.eventsSeen.Contains("1090508"))
                {
                    return;
                }

                foreach (var voidSpirit in voidSpirits)
                {
                    var locationName = $"{MONSTER_ERADICATION_AP_PREFIX}{voidSpirit}";
                    if (_locationChecker.IsLocationMissing(locationName))
                    {
                        _locationChecker.AddCheckedLocation(locationName);
                    }

                    if (_locationChecker.IsLocationMissing(DEINFEST_AP_LOCATION)) // Temp, as Void Spirits are on these maps
                    {
                        _locationChecker.AddCheckedLocation(DEINFEST_AP_LOCATION);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FixMonsterSlayerQuest_IncludeReleaseofGoals_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
