using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Festival
{
    public class WinterStarInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _shopReplacer = shopReplacer;
        }

        // public void chooseSecretSantaGift(Item i, Farmer who)
        public static bool ChooseSecretSantaGift_SuccessfulGift_Prefix(Event __instance, Item i, Farmer who)
        {
            try
            {
                if (i is not Object gift || _archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla)
                {
                    return true; // don't run original logic
                }

                var recipient = __instance.getActorByName(__instance.secretSantaRecipient.Name);
                var taste = (GiftTaste) recipient.getGiftTasteForThisItem(gift);

                if (_archipelago.SlotData.FestivalLocations != FestivalLocations.Hard || taste == GiftTaste.Love)
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.SECRET_SANTA);
                }
                
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ChooseSecretSantaGift_SuccessfulGift_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public void setUpPlayerControlSequence(string id)
        public static void SetUpPlayerControlSequence_ChooseSecretSantaTarget_Postfix(Event __instance, string id)
        {
            try
            {
                if (id != "christmas" || __instance.secretSantaRecipient == null)
                {
                    return;
                }

                var farmId = Game1.uniqueIDForThisGame / 2;
                var monthId = Game1.stats.DaysPlayed % 28;
                var seed = (int)farmId + monthId + (int)Game1.player.UniqueMultiplayerID;
                var r = new Random((int)seed);
                __instance.secretSantaRecipient = Utility.getRandomTownNPC(r);
                while (__instance.mySecretSanta == null || __instance.mySecretSanta.Equals((object)__instance.secretSantaRecipient) || __instance.mySecretSanta.isDivorcedFrom(__instance.farmer))
                    __instance.mySecretSanta = Utility.getRandomTownNPC(r);
                Game1.debugOutput = "Secret Santa Recipient: " + __instance.secretSantaRecipient.Name + "  My Secret Santa: " + __instance.mySecretSanta.Name;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetUpPlayerControlSequence_ChooseSecretSantaTarget_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public bool chooseResponse(Response response)
        public static void ChooseResponse_LegendOfTheWinterStar_Postfix(Dialogue __instance, Response response, ref bool __result)
        {
            try
            {
                if (__instance.speaker.Name != "Willy" || !Game1.CurrentEvent.isFestival || Game1.currentSeason != "winter" || Game1.dayOfMonth != 25)
                {
                    return;
                }

                if (response.responseKey == "quickResponse1")
                {
                    _locationChecker.AddCheckedLocation("The Legend of the Winter Star");
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ChooseResponse_LegendOfTheWinterStar_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
