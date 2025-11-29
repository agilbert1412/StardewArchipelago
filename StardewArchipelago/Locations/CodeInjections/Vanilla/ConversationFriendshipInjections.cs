using System;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class ConversationFriendshipInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void grantConversationFriendship(Farmer who, int amount = 20)
        public static void GrantConversationFriendship_TalkEvents_Postfix(NPC __instance, Farmer who, int amount = 20)
        {
            try
            {
                if (!who.friendshipData.ContainsKey(__instance.Name))
                {
                    return;
                }

                RecipeFriendshipInjections.SendFriendshipRecipeChecks(__instance, who);
                GetExtraTvRemote(__instance, who);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GrantConversationFriendship_TalkEvents_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void GetExtraTvRemote(NPC npc, Farmer player)
        {
            try
            {
                if (npc.Name != "George")
                {
                    return;
                }

                var ownsRemote = false;
                Utility.ForEachItem(x =>
                {
                    if (x.QualifiedItemId != QualifiedItemIds.ADVANCED_TV_REMOTE)
                    {
                        return true;
                    }
                    ownsRemote = true;
                    return false;
                });

                if (ownsRemote)
                {
                    return;
                }

                var animalWellBundleId = ArchipelagoJunimoNoteMenu.GetBundleId(MemeBundleNames.ANIMAL_WELL, out var communityCenter, out _);
                var bundleExists = animalWellBundleId >= 0;
                if (!bundleExists)
                {
                    return;
                }

                var hasCompletedQuest = player.hasOrWillReceiveMail("birdieQuestFinished") || _locationChecker.IsLocationChecked("Quest: The Pirate's Wife");
                var needsToCompleteQuest = !_archipelago.SlotData.ExcludeGingerIsland;

                if (hasCompletedQuest)
                {
                    var donatedAmount = ArchipelagoJunimoNoteMenu.TryDonateToBundle(MemeBundleNames.ANIMAL_WELL, "Advanced TV Remote", 1);
                    if (donatedAmount == 0)
                    {
                        return;
                    }

                    var dialogue = new Dialogue(npc, null,
                        "Remember that complicated remote you gave me? I never could quite get used to it. It was always triggering random things in the rooms of the house... I brought it to the old Community Center.");
                    npc.setNewDialogue(dialogue);
                    return;
                }
                else if (!needsToCompleteQuest)
                {
                    var remote = ItemRegistry.Create<Object>(QualifiedItemIds.ADVANCED_TV_REMOTE);

                    var dialogue = new Dialogue(npc, null,
                        "I got this fancy new remote, but I can't figure it out. It's always triggering random things in the rooms of the house. Youngsters like you like these things, you can have it.");

                    remote.specialItem = true;
                    remote.questItem.Value = true;
                    DelayedAction.functionAfterDelay(() => player.addItemByMenuIfNecessary(remote), 200);
                    npc.setNewDialogue(dialogue);
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetExtraTvRemote)}:\n{ex}");
                return;
            }
        }
    }
}
