using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Archipelago
{
    public class ChatForwarder
    {
        private static IMonitor _monitor;
        private Harmony _harmony;

        public ChatForwarder(IMonitor monitor, Harmony harmony)
        {
            _monitor = monitor;
            _harmony = harmony;
        }

        public void ListenToChatMessages()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.receiveChatMessage)),
                postfix: new HarmonyMethod(typeof(ChatForwarder), nameof(ChatForwarder.ReceiveChatMessage_ForwardToAp_PostFix))
            );
        }

        public static void ReceiveChatMessage_ForwardToAp_PostFix(ChatBox __instance, long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            try
            {
                if (!message.StartsWith("!"))
                {
                    return;
                }


            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ReceiveChatMessage_ForwardToAp_PostFix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
