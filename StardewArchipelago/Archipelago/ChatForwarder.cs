﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Goals;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Arcade;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Archipelago
{
    public class ChatForwarder
    {
        public const string COMMAND_PREFIX = "!!";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private readonly Harmony _harmony;
        private static IGiftHandler _giftHandler;
        private static GoalManager _goalManager;
        private static TileSanityManager _tileSanityManager;
        private static BankHandler _bankHandler;
        private static PlayerUnstucker _playerUnstucker;

        private static string _lastCommand;

        public ChatForwarder(ILogger logger, IMonitor monitor, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago, IGiftHandler giftHandler, GoalManager goalManager, TileChooser tileChooser, TileSanityManager tileSanityManager)
        {
            _logger = logger;
            _helper = helper;
            _harmony = harmony;
            _archipelago = archipelago;
            _giftHandler = giftHandler;
            _goalManager = goalManager;
            _tileSanityManager = tileSanityManager;
            _playerUnstucker = new PlayerUnstucker(tileChooser);
            _bankHandler = new BankHandler(_archipelago);
            _lastCommand = null;
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

                var messagesField = _helper.Reflection.GetField<List<ChatMessage>>(__instance, "messages");
                var messages = messagesField.GetValue();
                messages.RemoveAt(messages.Count - 1);
                _archipelago.SendMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveChatMessage_ForwardToAp_PostFix)}:\n{ex}");
            }
        }

        private static bool TryHandleCommand(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || !message.StartsWith(COMMAND_PREFIX))
            {
                return false;
            }

            var messageLower = message.ToLower();
            if (HandleReCommand(messageLower))
            {
                return true;
            }
            if (HandleGoalCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }
            if (HandleVanillaGoalCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleExperienceCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleFriendshipCommand(message))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleArcadeReleaseCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (_giftHandler.HandleGiftItemCommand(message))
            {
                _lastCommand = message;
                return true;
            }

            if (_bankHandler.HandleBankCommand(message))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleHideEmptyLettersCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleOverrideSpriteRandomizerCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleUnstuckCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleSleepCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandleSyncCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (HandlePrankCommand(messageLower))
            {
                _lastCommand = message;
                return true;
            }

            if (_tileSanityManager.HandleTilesanityCommands(messageLower))
            {
                _lastCommand = message;
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

        private static bool HandleReCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}re" && message != $"{COMMAND_PREFIX}!" && message != $"{COMMAND_PREFIX}redo")
            {
                return false;
            }


            return TryHandleCommand(_lastCommand);
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

        private static bool HandleVanillaGoalCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}vanilla_goal")
            {
                return false;
            }

            var goal = GoalCodeInjection.GetGoalString();
            var goalMessage = $"Checking the vanilla completion criteria for goal: {goal}";
            Game1.chatBox?.addMessage(goalMessage, Color.Gold);
            _goalManager.CheckGoalCompletion(true);
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
                case "himbo":
                    return "Alex";
                case "bimbo":
                    return "Haley";
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
                case "chicken":
                    return "Harvey";
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

        private static bool HandlePrankCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}fish" &&
                message != $"{COMMAND_PREFIX}prank" &&
                message != $"{COMMAND_PREFIX}stop" &&
                message != $"{COMMAND_PREFIX}zelda" &&
                message != $"{COMMAND_PREFIX}april" &&
                message != $"{COMMAND_PREFIX}fool" &&
                message != $"{COMMAND_PREFIX}fools" &&
                message != $"{COMMAND_PREFIX}aprilfool" &&
                message != $"{COMMAND_PREFIX}aprilsfool")
            {
                return false;
            }

            ZeldaAnimationInjections.TogglePrank();
            return true;
        }

        private static bool HandleHideEmptyLettersCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}letters")
            {
                return false;
            }

            var currentSetting = ModEntry.Instance.Config.HideEmptyArchipelagoLetters;
            var newSetting = !currentSetting;
            var status = newSetting ? "hidden" : "visible";
            ModEntry.Instance.Config.HideEmptyArchipelagoLetters = newSetting;
            Game1.chatBox?.addMessage($"Empty archipelago letters are now {status}. Changes will take effect when opening your mailbox", Color.Gold);
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
                if (!_archipelago.GetAllCheckedLocations().Keys.Contains(JunimoKartInjections.JK_VICTORY))
                {
                    Game1.chatBox?.addMessage($"You must complete Junimo Kart before releasing it", Color.Gold);
                    return true;
                }

                Game1.chatBox?.addMessage($"Releasing all remaining checks in Junimo Kart", Color.Gold);
                JunimoKartInjections.ReleaseJunimoKart();
                return true;
            }

            if (isPrairieKing)
            {
                if (!_archipelago.GetAllCheckedLocations().Keys.Contains(JotPKInjections.JOTPK_VICTORY))
                {
                    Game1.chatBox?.addMessage($"You must complete Journey of the Prairie King before releasing it", Color.Gold);
                    return true;
                }

                Game1.chatBox?.addMessage($"Releasing all remaining checks in Journey of the Prairie King", Color.Gold);
                JotPKInjections.ReleasePrairieKing();
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

        private static bool HandleOverrideSpriteRandomizerCommand(string message)
        {
            if (!message.ToLower().StartsWith($"{COMMAND_PREFIX}sprite"))
            {
                return false;
            }

            var split = message.Split(' ');
            if (split.Length == 1)
            {
                Game1.chatBox?.addMessage($"Sprite Randomizer is currently at `{ModEntry.Instance.Config.SpriteRandomizer}` [Disabled|Enabled|Chaos]", Color.Gold);
                return true;
            }
            if (split.Length != 2)
            {
                Game1.chatBox?.addMessage($"Usage: `!!sprite [Disabled|Enabled|Chaos]`", Color.Gold);
                return true;
            }

            var choice = split[1].ToLower();
            switch (choice)
            {
                case "disabled":
                    ModEntry.Instance.Config.SpriteRandomizer = AppearanceRandomization.Disabled;
                    Game1.chatBox?.addMessage($"Sprite Randomizer is now disabled.", Color.Gold);
                    break;
                case "enabled":
                    ModEntry.Instance.Config.SpriteRandomizer = AppearanceRandomization.Enabled;
                    Game1.chatBox?.addMessage($"Sprite Randomizer is now enabled.", Color.Gold);
                    break;
                case "chaos":
                    ModEntry.Instance.Config.SpriteRandomizer = AppearanceRandomization.Chaos;
                    Game1.chatBox?.addMessage($"Sprite Randomizer is now enabled in chaos mode.", Color.Gold);
                    break;
                default:
                    Game1.chatBox?.addMessage($"Usage: `!!sprite [Disabled|Enabled|Chaos]`", Color.Gold);
                    break;

            }

            AppearanceRandomizer.GenerateSeededShuffledAppearances();
            AppearanceRandomizer.RefreshAllNPCs();
            return true;
        }

        private static bool HandleUnstuckCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}unstuck")
            {
                return false;
            }

            var success = _playerUnstucker.Unstuck();
            var response = success
                ? $"You have been moved back inbounds, be more careful next time"
                : $"Could not find suitable location to move. Consider !!sleep to end your day";
            Game1.chatBox?.addMessage(response, Color.Gold);

            return true;
        }

        private static bool HandleSleepCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}sleep")
            {
                return false;
            }

            SetLastBedToFarmhouse();
            Game1.player.startToPassOut();
            return true;
        }

        private static void SetLastBedToFarmhouse()
        {
            try
            {
                var location = Game1.getLocationFromName("FarmHouse");
                if (location is not FarmHouse farmhouse)
                {
                    return;
                }

                Game1.player.lastSleepLocation.Set(farmhouse.NameOrUniqueName);
                var bedSpot = farmhouse.GetPlayerBedSpot();
                Game1.player.lastSleepPoint.Set(bedSpot);
                var bed = farmhouse.GetBed();
                if (bed == null)
                {
                    return;
                }

                Game1.player.mostRecentBed = bed.TileLocation;
                Game1.player.currentLocation.locationContextId = "Default";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed at setting last bed. Error: {ex}");
            }
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
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}bank [deposit|withdraw] [amount] - Deposit or withdraw money from your shared bank account", Color.Gold);
            if (_archipelago.SlotData.Gifting)
            {
                Game1.chatBox?.addMessage($"{COMMAND_PREFIX}gift [slotName] - Sends your currently held item stack to a chosen player as a gift", Color.Gold);
            }
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}letters - Toggle Hiding Empty Archipelago Letters", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}unstuck - Nudge your character if you are stuck in a wall", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sleep - Immediately pass out, ending the day", Color.Gold);
#if DEBUG
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sprite - Enable/Disable the sprite randomizer", Color.Gold);
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}sync - Sends a Sync packet to the Archipelago server", Color.Gold);
#endif
            Game1.chatBox?.addMessage($"{COMMAND_PREFIX}arcade_release [game] - Releases all remaining checks in an arcade machine that you have already completed", Color.Gold);

#if TILESANITY
            if (_archipelago.SlotData.Tilesanity != Tilesanity.Nope)
            {
                Game1.chatBox?.addMessage($"{COMMAND_PREFIX}where - Shows where you are and pointing at", Color.Gold);
                Game1.chatBox?.addMessage($"{COMMAND_PREFIX}tilesanity_ui - Toggles the tilesanity UI", Color.Gold);
                Game1.chatBox?.addMessage($"{COMMAND_PREFIX}tilesanity_ui_black - Toggles the tilesanity UI, but for Kaito", Color.Gold);
            }
#endif
        }
    }
}
