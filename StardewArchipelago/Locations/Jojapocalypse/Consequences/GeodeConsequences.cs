using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.Jojapocalypse.Consequences
{
    internal class GeodeConsequences
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public static Item getTreasureFromGeode(Item geode)
        public static bool GetTreasureFromGeode_SometimesTrash_Prefix(Item geode, ref Item __result)
        {
            try
            {
                var numberDonationsPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.MUSEUM_DONATIONS);
                var numberMilestonesPurchased = _jojaLocationChecker.CountCheckedLocationsWithTag(LocationTag.MUSEUM_MILESTONES);

                var seedA = geode.QualifiedItemId.Contains("MysteryBox") ? Game1.stats.Get("MysteryBoxesOpened") : (double)Game1.stats.GeodesCracked;
                var seedB = (int)Game1.player.UniqueMultiplayerID / 2;

                if (numberDonationsPurchased > 0 && JojapocalypseConsequencesPatcher.RollConsequenceChance(0.025, numberDonationsPurchased, seedA, seedB))
                {
                    __result = ChooseTrashReplacementItem();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (numberMilestonesPurchased > 0 && JojapocalypseConsequencesPatcher.RollConsequenceChance(0.075, numberMilestonesPurchased, seedA, seedB))
                {
                    __result = ChooseTrashReplacementItem();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }


                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetTreasureFromGeode_SometimesTrash_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static Item ChooseTrashReplacementItem()
        {
            var trashItems = new[] { QualifiedItemIds.JOJA_COLA, QualifiedItemIds.TRASH, QualifiedItemIds.SOGGY_NEWSPAPER, QualifiedItemIds.BROKEN_CD, QualifiedItemIds.BROKEN_GLASSES };
            var chosenReplacement = trashItems[new Random().Next(trashItems.Length)];
            var replacementItem = ItemRegistry.Create(chosenReplacement);
            return replacementItem;
        }
    }
}
