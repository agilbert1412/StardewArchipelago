using System;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewArchipelago.Goals;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Archipelago
{
    public class ChatForwarder
    {
        public const string COMMAND_PREFIX = "!!";

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private Harmony _harmony;
        private static GiftHandler _giftHandler;

        public ChatForwarder(IMonitor monitor, Harmony harmony, GiftHandler giftHandler)
        {
            _monitor = monitor;
            _harmony = harmony;
            _giftHandler = giftHandler;
        }

        public void ListenToChatMessages(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;

            _harmony.Patch(
                original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage)),
                postfix: new HarmonyMethod(typeof(ChatForwarder), nameof(ReceiveChatMessage_ForwardToAp_PostFix))
            );
        }

        public static void ReceiveChatMessage_ForwardToAp_PostFix(ChatBox __instance, long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            try
            {
                if (sourceFarmer == 0 || chatKind != 0)
                {
                    return;
                }

                if (TryHandleCommand(message))
                {
                    return;
                }

                _archipelago.SendMessage(message);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ReceiveChatMessage_ForwardToAp_PostFix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static bool TryHandleCommand(string message)
        {
            if (message == null || !message.StartsWith(COMMAND_PREFIX))
            {
                return false;
            }

            var messageLower = message.ToLower();
            if (HandleGoalCommand(messageLower))
            {
                return true;
            }

            if (HandleExperienceCommand(messageLower))
            {
                return true;
            }

            if (HandleFriendshipCommand(message))
            {
                return true;
            }

            if (HandleArcadeReleaseCommand(messageLower))
            {
                return true;
            }

            if (_giftHandler.HandleGiftItemCommand(message))
            {
                return true;
            }

            if (HandleSyncCommand(messageLower))
            {
                return true;
            }

            if (HandleHelpCommand(messageLower))
            {
                return true;
            }

            if (message.StartsWith(COMMAND_PREFIX))
            {
                Game1.chatBox?.addMessage($"Unrecognized command. Use {COMMAND_PREFIX}help for a list of commands", Color.Gold);
                return true;
            }

            return false;
        }

        private static bool HandleGoalCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}goal")
            {
                return false;
            }

            var goal = GoalCodeInjection.GetGoalString();
            var goalMessage = $"Your Goal is: {goal}";
            Game1.chatBox?.addMessage(goalMessage, Color.Gold);
            return true;
        }

        private static bool HandleExperienceCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}experience")
            {
                return false;
            }

            var skillsExperiences = SkillInjections.GetArchipelagoExperienceForPrinting();
            foreach (var skill in skillsExperiences)
            {
                Game1.chatBox?.addMessage(skill, Color.Gold);
            }

            return true;
        }

        private static bool HandleFriendshipCommand(string message)
        {
            var friendshipPrefix = $"{COMMAND_PREFIX}friendship ";
            if (!message.StartsWith(friendshipPrefix))
            {
                return false;
            }

            var remainder = message.Substring(friendshipPrefix.Length);
            var name = CorrectName(remainder);
            var state = FriendshipInjections.GetArchipelagoFriendshipPointsForPrinting(name);
            Game1.chatBox?.addMessage(state, Color.Gold);

            return true;
        }

        private static string CorrectName(string enteredName)
        {
            var loweredName = enteredName.ToLower().Replace(" ", "");
            var loweredPetName = Game1.player.getPetName().ToLower().Replace(" ", "");
            if (loweredName == loweredPetName)
            {
                return Game1.player.getPetName();
            }
            switch (loweredName)
            {
                case "pet":
                case "cat":
                case "dog":
                    return Game1.player.getPetName();
                case "rasmodius":
                    return "Wizard";
                case "milf":
                    return "Robin";
                case "hobo":
                    return "Linus";
                case "josh":
                    return "Alex";
                case "bestgirl":
                    return "Abigail";
                case "gilf":
                    return "Evelyn";
                case "boomer":
                    return "George";
                case "nerd":
                    return "Maru";
                case "emo":
                    return "Sebastian";
                default:
                    return Utility.capitalizeFirstLetter(enteredName);
            }
        }

        private static bool HandleSyncCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}sync")
            {
                return false;
            }

            _archipelago.Sync();
            return true;
        }

        private static bool HandleArcadeReleaseCommand(string message)
        {
            var arcadePrefix = $"{COMMAND_PREFIX}arcade_release ";
            if (!message.StartsWith(arcadePrefix))
            {
                return false;
            }

            var remainder = message.Substring(arcadePrefix.Length);

            var isJunimoCart = IsJunimoKart(remainder);
            var isPrairieKing = IsPrairieKing(remainder);

            if (!isJunimoCart && !isPrairieKing)
            {
                Game1.chatBox?.addMessage($"Unrecognized arcade game: {remainder} (Options: JotPK, JK)", Color.Gold);
                return true;
            }

            if (isJunimoCart)
            {
                if (!_archipelago.GetAllCheckedLocations().Keys.Contains(ArcadeMachineInjections.JK_VICTORY))
                {
                    Game1.chatBox?.addMessage($"You must complete Junimo Kart before releasing it", Color.Gold);
                    return true;
                }

                Game1.chatBox?.addMessage($"Releasing all remaining checks in Junimo Kart", Color.Gold);
                ArcadeMachineInjections.ReleaseJunimoKart();
                return true;
            }

            if (isPrairieKing)
            {
                if (!_archipelago.GetAllCheckedLocations().Keys.Contains(ArcadeMachineInjections.JOTPK_VICTORY))
                {
                    Game1.chatBox?.addMessage($"You must complete Journey of the Prairie King before releasing it", Color.Gold);
                    return true;
                }

                Game1.chatBox?.addMessage($"Releasing all remaining checks in Journey of the Prairie King", Color.Gold);
                ArcadeMachineInjections.ReleasePrairieKing();
                return true;
            }

            return false;
        }

        private static bool IsJunimoKart(string remainder)
        {
            var trimmedToMinimum = remainder.Replace(" ", "").Replace("-", "").Replace("_", "").ToLower();
            return trimmedToMinimum is "jk" or "junimokart" or "junimocart" or "junimo" or "kart" or "cart";
        }

        private static bool IsPrairieKing(string remainder)
        {
            var trimmedToMinimum = remainder.Replace(" ", "").Replace("-", "").Replace("_", "").ToLower();
            return trimmedToMinimum is "jotpk" or "journeyoftheprairieking" or "journey" or "prairieking" or "prairie" or "king";
        }

        private static bool HandleHelpCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}help")
            {
                return false;
            }

            PrintCommandHelp();
            return true;
        }

        private static void PrintCommandHelp()
        {
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}help - Shows the list of client commands", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}goal - Shows your current Archipelago Goal", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}experience - Shows your current progressive skills experience levels", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}friendship [npc] - Shows your current earned friendship points with a specific npc", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}gift [slotName] - Sends your currently held item stack to a chosen player as a gift", Color.Gold);
#if DEBUG
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sync - Sends a Sync packet to the Archipelago server", Color.Gold);
#endif
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}arcade_release [game] - Releases all remaining checks in an arcade machine that you have already completed", Color.Gold);
        }
    }
}
