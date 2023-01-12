using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Menus;

namespace StardewArchipelago.Items.Mail
{
    public class MailPatcher
    {
        private static IMonitor _monitor;
        private static LetterActions _letterActions;
        private readonly Harmony _harmony;

        public MailPatcher(IMonitor monitor, LetterActions letterActions, Harmony harmony)
        {
            _monitor = monitor;
            _letterActions = letterActions;
            _harmony = harmony;
        }

        public void PatchMailBoxForApItems()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.exitThisMenu)),
                postfix: new HarmonyMethod(typeof(MailPatcher), nameof(MailPatcher.ExitThisMenu_ApplyLetterAction_Postfix))
            );
        }

        public static void ExitThisMenu_ApplyLetterAction_Postfix(IClickableMenu __instance, bool playSound)
        {
            try
            {
                if (__instance is not LetterViewerMenu letterMenuInstance)
                {
                    return;
                }

                var title = letterMenuInstance.mailTitle;
                if (!title.StartsWith("AP|"))
                {
                    return;
                }
                
                var parts = title.Split('|');
                if (parts.Length < 4)
                {
                    return;
                }

                var apActionName = parts[2];
                var apActionParameter = parts[3];

                _letterActions.ExecuteLetterAction(apActionName, apActionParameter);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ExitThisMenu_ApplyLetterAction_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
