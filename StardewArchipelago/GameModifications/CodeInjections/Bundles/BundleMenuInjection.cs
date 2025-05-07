using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Extensions;
using StardewValley.Menus;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections.Bundles
{
    public class BundleMenuInjection
    {
        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static LocationChecker _locationChecker;
        private static BundleReader _bundleReader;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
            _bundleReader = bundleReader;
        }

        // public void checkBundle(int area)
        public static bool CheckBundle_UseRemakeMenuInCC_Prefix(CommunityCenter __instance, int area)
        {
            try
            {
                __instance.bundleMutexes[area].RequestLock(() => Game1.activeClickableMenu = new ArchipelagoJunimoNoteMenu(area, __instance.bundlesDict()));
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckBundle_UseRemakeMenuInCC_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void checkBundle()
        public static bool CheckBundle_UseRemakeMenuInJojaMart_Prefix(AbandonedJojaMart __instance)
        {
            try
            {
                __instance.bundleMutex.RequestLock(() => Game1.activeClickableMenu = new ArchipelagoJunimoNoteMenu(6, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict()));
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckBundle_UseRemakeMenuInJojaMart_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static IClickableMenu activeClickableMenu
        public static bool SetActiveClickableMenu_UseJunimoNoteMenuRemake_Prefix(ref IClickableMenu value)
        {
            try
            {
                if (value is not JunimoNoteMenu junimoNoteMenu)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                value = new ArchipelagoJunimoNoteMenu(junimoNoteMenu.fromGameMenu, junimoNoteMenu.whichArea, junimoNoteMenu.fromThisMenu);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetActiveClickableMenu_UseJunimoNoteMenuRemake_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        //Transpiler way
        public static IEnumerable<CodeInstruction> SkipObjectCheck(IEnumerable<CodeInstruction> instructions)
        {
            // We want to skip `if (dataOrErrorItem.HasTypeObject())`, just after `ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(representativeItemId);`

            // [1371 11 - 1371 97]
            // IL_0193: ldloc.s representativeItemId
            // IL_0195: call         class StardewValley.ItemTypeDefinitions.ParsedItemData StardewValley.ItemRegistry::GetDataOrErrorItem(string)
            // IL_019a: stloc.s dataOrErrorItem

            // [1372 11 - 1372 47]
            // IL_019c: ldloc.s dataOrErrorItem
            // IL_019e: call bool StardewValley.Extensions.ItemExtensions::HasTypeObject(class StardewValley.ItemTypeDefinitions.IHaveItemTypeId)
            // IL_01a3: brfalse IL_0330

            var getDataOrErrorItemMethodInfo = AccessTools.Method(typeof(ItemExtensions), nameof(ItemExtensions.HasTypeObject));

            var foundMethod = false;
            Label? label = null;
            var enumerator = instructions.GetEnumerator();
            enumerator.MoveNext();
            var previousInstruction = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var newInstruction = enumerator.Current;
                // Recognize the call of the method
                if (newInstruction.Calls(getDataOrErrorItemMethodInfo))
                {
                    if (foundMethod) // Second trigger of this method, which means Stardew code added a second unexpected call
                    {
                        _logger.LogError("Community Center object checking has been skipped more than once");
                    }
                    foundMethod = true;
                    enumerator.MoveNext();
                    label = (Label)enumerator.Current.operand;
                    enumerator.MoveNext();
                    previousInstruction = enumerator.Current;
                }
                else
                {
                    if (label is not null)
                    {
                        if (previousInstruction.labels.Remove(label.Value))
                        {
                            label = null;
                        }
                    }
                    yield return previousInstruction;
                    previousInstruction = newInstruction;
                }
            }
            yield return previousInstruction;
            enumerator.Dispose();

            if (!foundMethod)
            {
                _logger.LogError("Community Center couldn't be patched to show anything");
            }
            if (label != null)
            {
                _logger.LogError("The label somehow couldn't be cleared, expect issues on the Community Center bundles");
            }
        }

        //Transpiler way
        //public static IEnumerable<CodeInstruction> ReplaceVaultCheckWithBundleType(IEnumerable<CodeInstruction> instructions)
        //{
        //    // We want to replace `if (this.whichArea == 4)`, just after `this.specificBundlePage = true;`

        //    // [1335 7 - 1335 37]
        //    // IL_0011: ldarg.0      // this
        //    // IL_0012: ldc.i4.1
        //    // IL_0013: stfld        bool StardewValley.Menus.JunimoNoteMenu::specificBundlePage

        //    // [1336 7 - 1336 31]
        //    // IL_0018: ldarg.0      // this
        //    // IL_0019: ldfld int32 StardewValley.Menus.JunimoNoteMenu::whichArea
        //    // IL_001e: ldc.i4.4
        //    // IL_001f: bne.un IL_00b2

        //    CodeMatcher matcher = new(instructions);

        //    var whichAreaFieldInfo = AccessTools.Field(typeof(JunimoNoteMenu), nameof(JunimoNoteMenu.whichArea));

        //    var isBundleCurrencyBasedInfo = AccessTools.Method(typeof(BundleMenuInjection), nameof(IsBundleCurrencyBased));

        //    matcher.MatchEndForward(
        //        new CodeMatch(OpCodes.Ldarg_0),
        //        new CodeMatch(OpCodes.Ldfld),
        //        new CodeMatch(OpCodes.Ldc_I4_4),
        //        new CodeMatch(OpCodes.Bne_Un)
        //    )
        //    .SetOpcodeAndAdvance(OpCodes.Brfalse)
        //    .Advance(-1)
        //    .Insert(
        //        new CodeInstruction(OpCodes.Pop),
        //        new CodeInstruction(OpCodes.Pop),
        //        new CodeInstruction(OpCodes.Ldarg_0),
        //        new CodeInstruction(OpCodes.Call, isBundleCurrencyBasedInfo)
        //        );

        //    return matcher.InstructionEnumeration();
        //}

        //public static bool IsBundleCurrencyBased(JunimoNoteMenu menu)
        //{
        //    var ingredient = menu.currentPageBundle.ingredients.Last();
        //    var ingredientId = ingredient.id;
        //    return CurrencyBundle.CurrencyIds.ContainsValue(ingredientId);
        //}
    }
}
