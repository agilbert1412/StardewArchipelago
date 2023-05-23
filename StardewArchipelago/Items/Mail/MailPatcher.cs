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
                if (__instance is not LetterViewerMenu letterMenuInstance || letterMenuInstance.mailTitle == null || letterMenuInstance.isFromCollection)
                {
                    return;
                }

                var title = letterMenuInstance.mailTitle;
                if (!MailKey.TryParse(title, out var apMailKey))
                {
                    return;
                }

                var apActionName = apMailKey.LetterOpenedAction;
                var apActionParameter = apMailKey.ActionParameter;

                if (string.IsNullOrWhiteSpace(apActionName))
                {
                    return;
                }

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
