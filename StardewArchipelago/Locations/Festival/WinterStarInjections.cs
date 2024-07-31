using System;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using Object = StardewValley.Object;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.Festival
{
    public class WinterStarInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Random _lastProvidedRandom;
        private static Random _random = null;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
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
                var taste = (GiftTaste)recipient.getGiftTasteForThisItem(gift);

                if (_archipelago.SlotData.FestivalLocations != FestivalLocations.Hard || taste == GiftTaste.Love)
                {
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.SECRET_SANTA);
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChooseSecretSantaGift_SuccessfulGift_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        // public static NPC GetRandomWinterStarParticipant(Func<string, bool> shouldIgnoreNpc = null)
        public static bool GetRandomWinterStarParticipant_ChooseBasedOnMonthNotYear_Prefix(Func<string, bool> ignoreNpc, ref NPC __result)
        {
            try
            {
                var random = Utility.CreateRandom((int)(Game1.uniqueIDForThisGame / 2UL), (int)(Game1.stats.DaysPlayed / 28), (double)Game1.player.UniqueMultiplayerID);
                __result = Utility.GetRandomNpc((name, data) => IsWinterStarParticipant(name, data, ignoreNpc), random);

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRandomWinterStarParticipant_ChooseBasedOnMonthNotYear_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }

        private static bool IsWinterStarParticipant(string name, CharacterData data, Func<string, bool> shouldIgnoreNpc)
        {
            if (shouldIgnoreNpc != null && shouldIgnoreNpc(name))
            {
                return false;
            }
            return data.WinterStarParticipant == null ? data.HomeRegion == "Town" : GameStateQuery.CheckConditions(data.WinterStarParticipant);
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
                    _locationChecker.AddCheckedLocation(FestivalLocationNames.LEGEND_OF_THE_WINTER_STAR);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChooseResponse_LegendOfTheWinterStar_Postfix)}:\n{ex}");
                return;
            }
        }

        internal static void OnFestivalsRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla)
            {
                return;
            }

            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/winter25"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var festivalData = asset.AsDictionary<string, string>().Data;
                    const string willyYear2Key = "Willy_y2";
                    if (festivalData.ContainsKey(willyYear2Key))
                    {
                        festivalData.Remove(willyYear2Key);
                    }
                },
                AssetEditPriority.Late
            );
        }
    }
}
