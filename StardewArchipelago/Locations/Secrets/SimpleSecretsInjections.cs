using System;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Secrets
{
    public class SimpleSecretsInjections
    {
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

        // public virtual void proceedToNextScene()
        public static void ProceedToNextScene_ForsakenSouls_Postfix(TV __instance)
        {
            try
            {
                if (GetCurrentChannelField(__instance).GetValue() == 666)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.FREE_THE_FORSAKEN_SOULS);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ProceedToNextScene_ForsakenSouls_Postfix)}:\n{ex}");
                return;
            }
        }

        private static IReflectedField<int> GetCurrentChannelField(TV tv)
        {
            // private int currentChannel;
            return _modHelper.Reflection.GetField<int>(tv, "currentChannel");
        }

        // public override void DayUpdate()
        public static void DayUpdate_SomethingForSanta_Postfix(Furniture __instance)
        {
            try
            {
                if (!Game1.IsMasterGame || Game1.season != Season.Winter || Game1.dayOfMonth != 25 || (__instance.furniture_type.Value != 11 && __instance.furniture_type.Value != 5) || __instance.heldObject.Value == null)
                {
                    return;
                }

                if (!Game1.player.mailReceived.Contains($"CookiePresent_year{Game1.year}") && !Game1.player.mailReceived.Contains($"MilkPresent_year{Game1.year}"))
                {
                    return;
                }

                if (__instance.heldObject.Value.QualifiedItemId != "(O)MysteryBox")
                {
                    return;
                }

                if (_locationChecker.IsLocationMissing(SecretsLocationNames.SOMETHING_FOR_SANTA))
                {
                    var itemToSpawnId = IDProvider.CreateApLocationItemId(SecretsLocationNames.SOMETHING_FOR_SANTA);
                    __instance.heldObject.Value = ItemRegistry.Create<StardewValley.Object>(itemToSpawnId);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_SomethingForSanta_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_LonelyStone_Postfix(MapPage __instance, int x, int y, bool playSound)
        {
            try
            {
                foreach (var clickableComponent in __instance.points.Values)
                {
                    if (!clickableComponent.containsPoint(x, y))
                    {
                        continue;
                    }
                    var name = clickableComponent.name;
                    if (name == "Beach/LonelyStone")
                    {
                        _locationChecker.AddCheckedLocation(SecretsLocationNames.ACKNOWLEDGE_THE_LONELY_STONE);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_LonelyStone_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_JungleJunimo_Postfix(SkillsPage __instance, int x, int y, bool playSound)
        {
            try
            {
                // private int timesClickedJunimo;
                var timesClickedJunimoField = _modHelper.Reflection.GetField<int>(__instance, "timesClickedJunimo");
                if (timesClickedJunimoField.GetValue() > 6)
                {
                    _locationChecker.AddCheckedLocation("Jungle Junimo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_JungleJunimo_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_SecretStatuesAndDwarfGrave_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (!who.IsLocalPlayer || __instance.ShouldIgnoreAction(action, who, tileLocation))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
                if (!ArgUtility.TryGet(action, 0, out var key1, out var error, name: "string actionType") || key1 == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (key1.Equals("HMTGF"))
                {
                    CheckSecretStatueLocation(__instance, who, QualifiedItemIds.SUPER_CUCUMBER, "??HMTGF??", "discoverMineral");

                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (key1.Equals("Starpoint"))
                {
                    if (!ArgUtility.TryGet(action, 1, out var which, out error, name: "string which"))
                    {
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }

                    if (which == "3")
                    {
                        CheckSecretStatueLocation(__instance, who, QualifiedItemIds.DUCK_MAYONNAISE, "??Pinky Lemon??", "discoverMineral");
                    }
                    else if (which == "4")
                    {
                        CheckSecretStatueLocation(__instance, who, QualifiedItemIds.STRANGE_BUN, "??Foroguemon??", "croak");
                    }
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (key1.Equals("DwarfGrave") && who.canUnderstandDwarves)
                {
                    _locationChecker.AddCheckedLocation("Galaxies Will Heed Your Cry");
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_DwarfGrave_Translated").Replace('\n', '^'));
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_SecretStatuesAndDwarfGrave_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CheckSecretStatueLocation(GameLocation gameLocation, Farmer who, string itemId, string locationName, string sound)
        {

            if (who.ActiveObject != null && who.ActiveObject.QualifiedItemId == itemId)
            {
                _locationChecker.AddCheckedLocation(locationName);
                Game1.player.reduceActiveItemByOne();
                gameLocation.localSound(sound);
                Game1.flashAlpha = 1f;
            }
        }
    }
}
