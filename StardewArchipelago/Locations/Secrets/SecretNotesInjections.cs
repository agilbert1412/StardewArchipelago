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
using StardewArchipelago.Extensions;
using StardewArchipelago.Locations.Festival;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.GarbageCans;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.Layers;
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

                bool ItemIsGifted(string itemId) => giftedItemsToThisPerson.Contains(itemId);

                var giftFulfilled = needsAll ? requiredGift.Gifts.All(ItemIsGifted) : requiredGift.Gifts.Any(ItemIsGifted);
                if (!giftFulfilled)
                {
                    return false;
                }
            }

            return true;
        }

        // public LetterViewerMenu(int secretNoteIndex)
        public static void LetterViewerMenuConstructor_ReadSecretNote_Postfix(LetterViewerMenu __instance, int secretNoteIndex)
        {
            try
            {
                if (secretNoteIndex != 11)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_11);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(LetterViewerMenuConstructor_ReadSecretNote_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool TryGetGarbageItem(string id, double dailyLuck, out Item item, out GarbageCanItemData selected, out Random garbageRandom, Action<string> logError = null)
        public static void TryGetGarbageItem_TrashCanSpecials_Postfix(GameLocation __instance, string id, double dailyLuck, Item item, GarbageCanItemData selected, Random garbageRandom, Action<string> logError, ref bool __result)
        {
            try
            {
                if (__instance is not Town town || item == null)
                {
                    return;
                }

                var validPairs = new Dictionary<string, List<string>>
                {
                    { "Saloon", new List<string> { Game1.dishOfTheDay.QualifiedItemId } },
                    { "Evelyn", new List<string> { QualifiedItemIds.COOKIES } },
                    { "Blacksmith", new List<string> { QualifiedItemIds.COAL, QualifiedItemIds.COPPER_ORE, QualifiedItemIds.IRON_ORE } },
                    { "Museum", new List<string> { QualifiedItemIds.GEODE, QualifiedItemIds.OMNI_GEODE } }
                };

                if (validPairs.ContainsKey(id))
                {
                    var items = validPairs[id];
                    if (items.Contains(item.QualifiedItemId))
                    {
                        _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_12);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryGetGarbageItem_TrashCanSpecials_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool performToolAction(Tool t)
        public static void PerformToolAction_StoneJunimo_Postfix(Object __instance, Tool t)
        {
            try
            {
                if (__instance.QualifiedItemId != QualifiedItemIds.STONE_JUNIMO || !t.isHeavyHitter() || __instance.Location is not Town)
                {
                    return;
                }

                if (__instance.TileLocation.X.IsApproximately(57) && __instance.TileLocation.Y.IsApproximately(16))
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_14);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformToolAction_StoneJunimo_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void playClamTone(int which, Farmer who)
        public static void PlayClamTone_SongFinished_Postfix(MermaidHouse __instance, int which, Farmer who)
        {
            try
            {
                var pearlRecipientField = _modHelper.Reflection.GetField<Farmer>(__instance, "pearlRecipient");
                var pearlRecipient = pearlRecipientField.GetValue();
                if (pearlRecipient == null || pearlRecipient != Game1.player)
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_15);
                pearlRecipientField.SetValue(null);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlayClamTone_SongFinished_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        public static bool CheckForBuriedItem_TreasureChest_Prefix(Railroad __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who, ref string __result)
        {
            try
            {
                __result = "";
                const int noteNumber = 16;
                var expectedTile = new Point(12, 38);
                const string secretNoteMail = "SecretNote16_done";
                var apLocation = SecretsLocationNames.SECRET_NOTE_16;
                return CheckForBuriedSecretNoteLocation(__instance, who, xLocation, yLocation, noteNumber, expectedTile, secretNoteMail, apLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForBuriedItem_TreasureChest_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        public static bool CheckForBuriedItem_GreenDoll_Prefix(Town __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who, ref string __result)
        {
            try
            {
                __result = "";
                const int noteNumber = 17;
                var expectedTile = new Point(98, 5);
                const string secretNoteMail = "SecretNote17_done";
                var apLocation = SecretsLocationNames.SECRET_NOTE_17;
                return CheckForBuriedSecretNoteLocation(__instance, who, xLocation, yLocation, noteNumber, expectedTile, secretNoteMail, apLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForBuriedItem_GreenDoll_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        public static bool CheckForBuriedItem_YellowDoll_Prefix(Desert __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who, ref string __result)
        {
            try
            {
                __result = "";
                const int noteNumber = 18;
                var expectedTile = new Point(40, 55);
                const string secretNoteMail = "SecretNote18_done";
                var apLocation = SecretsLocationNames.SECRET_NOTE_18;
                return CheckForBuriedSecretNoteLocation(__instance, who, xLocation, yLocation, noteNumber, expectedTile, secretNoteMail, apLocation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForBuriedItem_YellowDoll_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool CheckForBuriedSecretNoteLocation(GameLocation gameLocation, Farmer who, int xLocation, int yLocation, int noteNumber, Point expectedTile, string secretNoteMail, string apLocation)
        {
            if (!who.secretNotesSeen.Contains(noteNumber) || xLocation != expectedTile.X || yLocation != expectedTile.Y)
            {
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            if (!who.mailReceived.Add(secretNoteMail))
            {
                _locationChecker.AddCheckedLocation(apLocation);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            var itemId = IDProvider.CreateApLocationItemId(apLocation);
            var item = ItemRegistry.Create(itemId);
            Game1.createItemDebris(item, new Vector2(xLocation, yLocation) * 64f, -1, gameLocation);
            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_FindGoldLewis_Prefix(Town __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (who.mount != null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var layer = __instance.map.RequireLayer("Buildings");
                if (layer.GetTileIndexAt(tileLocation, nameof(Town)) != 599)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!Game1.player.secretNotesSeen.Contains(19) || !Game1.player.mailReceived.Add("SecretNote19_done"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                DelayedAction.playSoundAfterDelay("newArtifact", 250);
                _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_19_PART_1);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_FindGoldLewis_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual void rot()
        public static void Rot_GoldLewisFound_Postfix(Object __instance)
        {
            try
            {
                if (__instance.QualifiedItemId == QualifiedItemIds.GOLD_LEWIS)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_19_PART_2);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Rot_GoldLewisFound_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_SpecialCharmPurchase_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {

                if (questionAndAnswer == null || questionAndAnswer != "specialCharmQuestion_Yes")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (Game1.player.Items.ContainsId("(O)446"))
                {
                    Game1.player.holdUpItemThenMessage((Item)new SpecialItem(3));
                    Game1.player.removeFirstOfThisItemFromInventory("446");
                    // Game1.player.hasSpecialCharm = true;
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_20);
                    Game1.player.mailReceived.Add("SecretNote20_done");
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Town_specialCharmNoFoot"));
                }

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_FindGoldLewis_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void initiateMarnieLewisBush()
        public static void InitiateMarnieLewisBush_BushShaken_Postfix(Town __instance)
        {
            try
            {
                _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_21);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Rot_GoldLewisFound_Postfix)}:\n{ex}");
                return;
            }
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
