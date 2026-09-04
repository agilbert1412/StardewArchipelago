using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Items.Traps;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class PhoneInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static WeaponsManager _weaponsManager;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, WeaponsManager weaponsManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _weaponsManager = weaponsManager;
        }

        public static void Initialize(JojaLocationChecker jojaLocationChecker)
        {
            _jojaLocationChecker = jojaLocationChecker;
        }

        // public void CallAdventureGuild()
        public static bool CallAdventureGuild_AllowRecovery_Prefix(DefaultPhoneHandler __instance)
        {
            try
            {
                Game1.currentLocation.playShopPhoneNumberSounds("AdventureGuild");
                Game1.player.freezePause = 4950;
                DelayedAction.functionAfterDelay(() =>
                {
                    Game1.playSound("bigSelect");
                    var character = Game1.getCharacterFromName("Marlon");
                    if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
                    {
                        Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_AlreadyRecovering");
                    }
                    else
                    {
                        Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_Open");
                        Game1.afterDialogues += () =>
                        {
                            var equipmentsToRecover = _weaponsManager.GetEquipmentsForSale(IDProvider.ARCHIPELAGO_EQUIPMENTS_RECOVERY);
                            if (equipmentsToRecover.Any())
                            {
                                Game1.player.forceCanMove();
                                Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon");
                            }
                            else
                            {
                                Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_NoDeathItems");
                            }
                        };
                    }
                }, 4950);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CallAdventureGuild_AllowRecovery_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        //public string CheckForIncomingCall(Random random)
        public static void CheckForIncomingCall_AdjustCalls_Postfix(DefaultPhoneHandler __instance, Random random, ref string __result)
        {
            try
            {
                if (TryReplaceCallWithJojaAd(random, ref __result))
                {
                    return;
                }

                RemoveCallIfInvalid(ref __result);

                if (TryReplaceCallWithLoanAd(random, ref __result))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForIncomingCall_AdjustCalls_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool TryReplaceCallWithLoanAd(Random random, ref string __result)
        {
            if (!string.IsNullOrWhiteSpace(__result) || Game1.player.Money >= 5000)
            {
                return false;
            }

            // Weird math. The poorer you are, the closer to 40% this gets. Exponential (inverted), so every dollar makes less of a difference than the last
            var chanceOfAd = Game1.player.Money == 0 ? 0.2 : (0.1 * (1 - (Math.Log10(Game1.player.Money) / 4)));
            if (random.NextDouble() < chanceOfAd)
            {
                __result = JojaConstants.JOJA_LOAN_INCOMING_CALL;
                return true;
            }

            return false;
        }

        private static bool TryReplaceCallWithJojaAd(Random random, ref string __result)
        {
            if (!string.IsNullOrWhiteSpace(__result) || _jojaLocationChecker == null)
            {
                return false;
            }

            var chanceOfAd = (_jojaLocationChecker.GetPercentCheckedLocationsByJoja() * 0.25) + 0.02;
            if (random.NextDouble() < chanceOfAd)
            {
                __result = JojaConstants.JOJA_INCOMING_CALL;
                return true;
            }

            return false;
        }

        private static void RemoveCallIfInvalid(ref string __result)
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.Villagers) || string.IsNullOrWhiteSpace(__result))
            {
                return;
            }

            var calls = DataLoader.IncomingPhoneCalls(Game1.content);
            var thisCall = calls[__result];
            var npc = thisCall.FromNpc;
            if (string.IsNullOrWhiteSpace(npc))
            {
                return;
            }

            if (!Game1.characterData.ContainsKey(npc))
            {
                return;
            }

            var arrivalItem = VillagerExistenceInjections.GetArrivalItem(npc);
            if (_archipelago.HasReceivedItem(arrivalItem))
            {
                return;
            }

            __result = null;
        }

        // public static Action GetIncomingCallAction(string callId)
        public static bool GetIncomingCallAction_JojaLoanIncomingCall_Prefix(string callId, ref Action __result)
        {
            try
            {
                if (callId != JojaConstants.JOJA_LOAN_INCOMING_CALL)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var dialogueAction = () =>
                {
                    var speaker = JojapocalypseShopPatcher.Morris;
                    speaker = new NPC(speaker.Sprite, Vector2.Zero, "", 0, speaker.Name, speaker.Portrait, false);
                    speaker.displayName = speaker.displayName;

                    var dialoguePart1 = "Hello! This is your Joja Capital customer service representative." +
                                             " We have noticed that your account is pretty low on funds.";
                    var maxAmount = DebtManager.MAX_LOAN_AMOUNT;
                    var dialoguePart2 = "We wanted to remind you that Joja offers Loans at any time, no questions asked!" +
                                        $" You have been preapproved for a loan of up to {maxAmount}g.";
                    var dialoguePart3 = $"You can type '!!loan x' in chat to immediately request a loan to your account.";

                    var dialogue1 = new Dialogue(speaker, nameof(dialoguePart1), dialoguePart1);
                    var dialogue2 = new Dialogue(speaker, nameof(dialoguePart2), dialoguePart2);
                    var dialogue3 = new Dialogue(speaker, nameof(dialoguePart3), dialoguePart3);

                    dialogue1.onFinish = () =>
                    {
                        speaker.CurrentDialogue.Clear();
                        speaker.CurrentDialogue.Push(dialogue2);
                        Game1.drawDialogue(speaker);
                    };

                    dialogue2.onFinish = () =>
                    {
                        speaker.CurrentDialogue.Clear();
                        speaker.CurrentDialogue.Push(dialogue3);
                        Game1.drawDialogue(speaker);
                    };

                    speaker.CurrentDialogue.Clear();
                    speaker.CurrentDialogue.Push(dialogue1);
                    Game1.drawDialogue(speaker);
                };

                __result = dialogueAction;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetIncomingCallAction_JojaLoanIncomingCall_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
