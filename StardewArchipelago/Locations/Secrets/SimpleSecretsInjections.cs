using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
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
    }
}
