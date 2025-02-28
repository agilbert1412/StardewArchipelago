using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.Secrets
{
    public class SecretNotesInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void onGiftGiven(NPC npc, Object item)
        public static void OnGiftGiven_GiftingNotes_Postfix(Farmer __instance, NPC npc, Object item)
        {
            try
            {
                var giftedItems = __instance.giftedItems;
                var needsAll = _archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.Difficult);
                foreach (var (secretNoteLocation, requiredGifts) in SecretsLocationNames.SECRET_NOTE_GIFT_REQUIREMENTS)
                {
                    if (!_locationChecker.IsLocationMissing(secretNoteLocation))
                    {
                        continue;
                    }

                    if (AreSecretNoteGiftsFulfilled(requiredGifts, giftedItems, needsAll))
                    {
                        _locationChecker.AddCheckedLocation(secretNoteLocation);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnGiftGiven_GiftingNotes_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool AreSecretNoteGiftsFulfilled(List<RequiredGift> requiredGifts, SerializableDictionary<string, SerializableDictionary<string, int>> giftedItems, bool needsAll)
        {
            foreach (var requiredGift in requiredGifts)
            {
                var giftedItemsToThisPerson = giftedItems[requiredGift.Npc].Where(x => x.Value > 0).Select(x => x.Key).ToHashSet();
                var giftFulfilled = needsAll ? requiredGift.Gifts.All(x => giftedItemsToThisPerson.Contains(x)) : requiredGift.Gifts.Any(x => giftedItemsToThisPerson.Contains(x));
                if (!giftFulfilled)
                {
                    return false;
                }
            }

            return true;
        }

        // public void junimoPlushCallback(Item item, Farmer who)
        public static void JunimoPlushCallback_SendCheckAndRemovePlush_Postfix(Bush __instance, Item item, Farmer who)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_13);
                who.removeFirstOfThisItemFromInventory(QualifiedItemIds.JUNIMO_PLUSH);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(JunimoPlushCallback_SendCheckAndRemovePlush_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
