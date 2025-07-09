using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace StardewArchipelago.Locations.Secrets
{
    public class PurpleShortsInjections
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

        // public static void SwitchEvent(Event @event, string[] args, EventContext context)
        public static void SwitchEvent_PurpleShortsInSoup_Postfix(Event @event, string[] args, EventContext context)
        {
            try
            {
                var switchEventKey = args[1];
                var governorReactionKey = "governorReaction";
                if (!@event.isSpecificFestival("summer11") || !switchEventKey.StartsWith(governorReactionKey))
                {
                    return;
                }

                var soupScore = int.Parse(switchEventKey[governorReactionKey.Length..]);

                const int SHORTS_REACTION = 6;
                if (soupScore == SHORTS_REACTION)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.POISON_THE_GOVERNOR);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SwitchEvent_PurpleShortsInSoup_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void interpretGrangeResults()
        public static void InterpretGrangeResults_Bribe_Postfix(Event __instance)
        {
            try
            {
                if (__instance.grangeScore == -666)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.GRANGE_DISPLAY_BRIBE);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(InterpretGrangeResults_Bribe_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool tryToReceiveActiveObject(Farmer who, bool probe = false)
        public static bool TryToReceiveActiveObject_ConfrontMarnie_Prefix(NPC __instance, Farmer who, bool probe)
        {
            try
            {
                if (__instance.Name != "Marnie" || !QualifiedItemIds.IsLuckyShorts(who.ActiveObject.QualifiedItemId) || probe)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var confrontDialogue = __instance.TryGetDialogue("reject_789");
                if (confrontDialogue != null)
                {
                    __instance.setNewDialogue(confrontDialogue);
                    Game1.drawDialogue(__instance);
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.CONFRONT_MARNIE);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryToReceiveActiveObject_ConfrontMarnie_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void drawDialogue(NPC speaker)
        public static bool DrawDialogue_ShortsResponses_Prefix(NPC speaker)
        {
            try
            {
                if (speaker == null || speaker.Name != "Marnie" || speaker.CurrentDialogue.Count == 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var dialogue = speaker.CurrentDialogue.Peek();
                if (dialogue == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                SendLocationIfCorrectDialogue(speaker, dialogue, "Fair_Judged_PlayerLost_PurpleShorts", SecretsLocationNames.PURPLE_LETTUCE);
                SendLocationIfCorrectDialogue(speaker, dialogue, "reject_789", SecretsLocationNames.CONFRONT_MARNIE);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawDialogue_ShortsResponses_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public virtual bool checkAction(Farmer who, GameLocation l)
        public static bool CheckAction_ShortsReactions_Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                if (__instance.IsInvisible || __instance.isSleeping.Value || !who.CanMove)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var name = __instance.Name;
                var wearingPurpleShorts = who.pantsItem.Value?.QualifiedItemId == "(P)15";
                var showingPurpleShorts = wearingPurpleShorts && (name == "Lewis" || name == "Marnie");
                if (!showingPurpleShorts)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.yJumpVelocity != 0.0 || __instance.Sprite.CurrentAnimation != null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }


                if (name == "Lewis")
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.JUMPSCARE_LEWIS);
                }
                if (name == "Marnie")
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.MAKE_MARNIE_LAUGH);
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_ShortsReactions_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public int getBobberStyle(Farmer who)
        public static void GetBobberStyle_ShortsBobber_Postfix(FishingRod __instance, Farmer who, ref int __result)
        {
            try
            {
                if (__result == 39)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.LUCKY_PURPLE_BOBBER);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetBobberStyle_ShortsBobber_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void SendLocationIfCorrectDialogue(NPC speaker, Dialogue dialogue, string desiredDialogueKey, string location)
        {

            var desiredDialogue = speaker.TryGetDialogue(desiredDialogueKey);
            if (desiredDialogue == null || dialogue.dialogues.Count != desiredDialogue.dialogues.Count)
            {
                return;
            }

            for (var i = 0; i < dialogue.dialogues.Count; i++)
            {
                if (dialogue.dialogues[i].Text != desiredDialogue.dialogues[i].Text)
                {
                    return;
                }
            }

            _locationChecker.AddCheckedLocation(location);
        }
    }
}
