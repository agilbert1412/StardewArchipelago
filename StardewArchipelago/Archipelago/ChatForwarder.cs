using System;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.CodeInjections;
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
        private static BankHandler _bankHandler;
        private static AppearanceRandomizer _appearanceRandomizer;

        public ChatForwarder(IMonitor monitor, Harmony harmony, ArchipelagoClient archipelago, GiftHandler giftHandler, AppearanceRandomizer appearanceRandomizer)
        {
            _monitor = monitor;
            _harmony = harmony;
            _archipelago = archipelago;
            _giftHandler = giftHandler;
            _bankHandler = new BankHandler(_archipelago);
            _appearanceRandomizer = appearanceRandomizer;
        }

        public void ListenToChatMessages()
        {
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

            if (HandleArcadeReleaseCommand(messageLower))
            {
                return true;
            }

            if (_giftHandler.HandleGiftItemCommand(message))
            {
                return true;
            }

            if (_bankHandler.HandleBankCommand(message))
            {
                return true;
            }

            if (DisableAppearanceRandomizerCommand(messageLower))
            {
                return true;
            }

            if (HandleAppearanceRandomizationCommand(message))
            {
                return true;
            }

            if (HandleSyncCommand(messageLower))
            {
                return true;
            }

            if (HandleDeathlinkCommand(messageLower))
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

        private static bool HandleSyncCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}sync")
            {
                return false;
            }

            _archipelago.Sync();
            return true;
        }

        private static bool HandleDeathlinkCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}deathlink")
            {
                return false;
            }

            _archipelago.ToggleDeathlink();
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

        private static bool DisableAppearanceRandomizerCommand(string message)
        {
            if (!message.ToLower().Equals($"{COMMAND_PREFIX}sprite"))
            {
                return false;
            }

            var disabled = ModEntry.Instance._state.DisableAppearanceRandomizerOverride;
            disabled = !disabled;
            ModEntry.Instance._state.DisableAppearanceRandomizerOverride = disabled;
            var disabledString = disabled ? "off" : "on";
            Game1.chatBox?.addMessage($"Appearance Randomizer is now {disabledString}. Changes will take effect after sleeping, then reloading your game.", Color.Gold);
            return true;
        }

        private static bool HandleAppearanceRandomizationCommand(string message)
        {
            var sprite = $"{COMMAND_PREFIX}sprite ";
            if (!message.StartsWith(sprite))
            {
                return false;
            }

            var remainder = message.Substring(sprite.Length).Split(" ");
            if (remainder.Length < 1)
            {
                Game1.chatBox?.addMessage($"You need to choose an option from [Disabled|Villagers|All|Chaos]", Color.Gold);
                return true;
            }

            var setting = remainder[0].ToLower();
            AppearanceRandomization parsedSetting;
            switch (setting)
            {
                case "villagers":
                    parsedSetting = AppearanceRandomization.Villagers;
                    break;
                case "all":
                    parsedSetting = AppearanceRandomization.All;
                    break;
                case "chaos":
                    parsedSetting = AppearanceRandomization.Chaos;
                    break;
                case "disabled":
                    parsedSetting = AppearanceRandomization.Disabled;
                    break;
                default:
                    Game1.chatBox?.addMessage($"You need to choose an option from [Disabled|Villagers|All|Chaos]", Color.Gold);
                    return true;
            }

            var daily = false;
            if (remainder.Length >= 2)
            {
                bool.TryParse(remainder[1], out daily);
            }

            var dailyString = daily ? " (Daily)" : "";
            _archipelago.SlotData.AppearanceRandomization = parsedSetting;
            _archipelago.SlotData.AppearanceRandomizationDaily = daily;
            _appearanceRandomizer.ShuffleCharacterAppearances();
            Game1.chatBox?.addMessage($"Switch Randomization to {parsedSetting}{dailyString}", Color.Gold);

            return true;
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
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}gift [slotName] - Sends your currently held item stack to a chosen player as a gift", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}deathlink - Toggles Deathlink on/off. Saves when sleeping", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sprite - Enable/Disable the Appearance Randomizer", Color.Gold);
#if DEBUG
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sprite [Disabled|Villagers|All|Chaos] [daily:true/false] - Sets your appearance randomizer setting", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sync - Sends a Sync packet to the Archipelago server", Color.Gold);
#endif
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}arcade_release [game] - Releases all remaining checks in an arcade machine that you have already completed", Color.Gold);
        }
    }
}
