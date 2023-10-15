using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class DebugPatchInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }

        // public LetterViewerMenu(string mail, string mailTitle, bool fromCollection = false)
        public static bool LetterViewerMenuConstructor_AddLog_Prefix(LetterViewerMenu __instance, string mail, string mailTitle, bool fromCollection)
        {
            try
            {
                _monitor.Log($"About to open a Letter. [mailTitle: {mailTitle}, fromCollection: {fromCollection}, mailText: {mail}]");
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(LetterViewerMenuConstructor_AddLog_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public LetterViewerMenu(string mail, string mailTitle, bool fromCollection = false)
        public static void LetterViewerMenuConstructor_AddLog_Postfix(LetterViewerMenu __instance, string mail, string mailTitle, bool fromCollection)
        {
            try
            {
                _monitor.Log($"Finished opening letter. [mailTitle: {mailTitle}, fromCollection: {fromCollection}, mailText: {mail}]");
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(LetterViewerMenuConstructor_AddLog_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
