using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace StardewArchipelago.GameModifications.CodeInjections.Bundles
{
    public class BundleMenuInjection
    {
        private static ILogger _logger;

        public static void setupLogging(ILogger logger)
        {
            _logger = logger;
        }

        //Transpiler way
        public static IEnumerable<CodeInstruction> SkipObjectCheck(IEnumerable<CodeInstruction> instructions)
        {
            // We want to skip `if (dataOrErrorItem.HasTypeObject())`, just after `ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(representativeItemId);`
            // In 1.6.8.24119, it looks like this
            // IL_019c: ldloc.s 7
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
    }
}
