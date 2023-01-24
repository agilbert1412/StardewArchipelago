using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
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
                postfix: new HarmonyMethod(typeof(ChatForwarder), nameof(ChatForwarder.ReceiveChatMessage_ForwardToAp_PostFix))
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
            if (message == null)
            {
                return false;
            }

            if (message.ToLower() == "!goal")
            {
                var goal = _archipelago.SlotData.Goal switch
                {
                    Goal.GrandpaEvaluation => "Complete Grandpa's Evaluation with a score of at least 12 (4 candles)",
                    Goal.BottomOfMines => "Reach Floor 120 in the Pelican Town Mineshaft",
                    Goal.CommunityCenter => "Complete the Community Center",
                    _ => throw new NotImplementedException()
                };

                var goalMessage = $"Your Goal is: {goal}";
                Game1.chatBox?.addMessage(goalMessage, Color.Gold);
                return true;
            }

            return false;
        }
    }
}
