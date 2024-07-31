using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Vanilla;
using StardewValley;
using StardewValley.Extensions;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class SecretNoteInjections
    {
        private const int MAX_SECRET_NOTES = 25;
        private const string SECRET_NOTE_ID = "(O)79";

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public Object tryToCreateUnseenSecretNote(Farmer who)
        public static void TryToCreateUnseenSecretNote_AllowSecretNotesIfStillNeedToShipThem_Postfix(GameLocation __instance, Farmer who, ref Object __result)
        {
            try
            {
                if (who == null || __result != null || !who.hasMagnifyingGlass)
                {
                    return;
                }

                var isIsland = __instance.InIslandContext();
                if (isIsland)
                {
                    var needsExtraJournalScrap = _locationChecker.IsAnyLocationNotChecked("Journal Scrap");
                    if (!needsExtraJournalScrap)
                    {
                        return;
                    }
                }
                else
                {
                    var needsExtraSecretNotes = _locationChecker.IsAnyLocationNotChecked("Secret Note");
                    if (!needsExtraSecretNotes)
                    {
                        return;
                    }
                }

                var itemId = isIsland ? QualifiedItemIds.JOURNAL_SCRAP : QualifiedItemIds.SECRET_NOTE;
                if (who.Items.ContainsId(itemId))
                {
                    return;
                }

                var unseenSecretNotes = Utility.GetUnseenSecretNotes(who, isIsland, out var totalNotes);
                if (unseenSecretNotes.Length > 0)
                {
                    return;
                }

                if (Game1.random.NextBool(GameLocation.LAST_SECRET_NOTE_CHANCE * 0.25))
                {
                    return;
                }

                var secretNote = ItemRegistry.Create<Object>(itemId);
                __result = secretNote;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryToCreateUnseenSecretNote_AllowSecretNotesIfStillNeedToShipThem_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
