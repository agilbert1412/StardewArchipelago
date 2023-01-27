using System;
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
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private Harmony _harmony;

        public ChatForwarder(IMonitor monitor, Harmony harmony)
        {
            _monitor = monitor;
            _harmony = harmony;
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
            if (message == null || !message.StartsWith("!"))
            {
                return false;
            }

            var messageLower = message.ToLower();
            if (messageLower == "!goal")
            {
                var goal = GoalCodeInjection.GetGoalString();
                var goalMessage = $"Your Goal is: {goal}";
                Game1.chatBox?.addMessage(goalMessage, Color.Gold);
                return true;
            }

            if (messageLower == "!experience")
            {
                var skillsExperiences = SkillInjections.GetArchipelagoExperienceForPrinting();
                foreach (var skill in skillsExperiences)
                {
                    Game1.chatBox?.addMessage(skill, Color.Gold);
                }
                return true;
            }

            return false;
        }
    }
}
