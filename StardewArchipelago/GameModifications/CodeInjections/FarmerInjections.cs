using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewArchipelago.Constants.Vanilla;
using System.Linq;
using StardewArchipelago.Archipelago.SlotData;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class FarmerInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public void doneEating()
        public static void DoneEating_StardropFavoriteThingKaito_Postfix(Farmer __instance)
        {
            try
            {
                var itemToEat = __instance.itemToEat as Object;
                if (itemToEat.QualifiedItemId != QualifiedItemIds.STARDROP)
                {
                    return;
                }

                if (__instance.favoriteThing.Value.Contains("Kaito", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (Game1.delayedActions.Any())
                    {
                        // Remove the stardrop dialogue that the game pushed, make my own
                        Game1.delayedActions.Clear();
                    }

                    var message = $"Your mind is filled with thoughts of... {__instance.favoriteThing.Value}? ^";
                    var anyHardSetting = false;
                    if (_archipelago.SlotData.BundlePrice == BundlePrice.Maximum)
                    {
                        message += "Even with these bundles?^";
                        anyHardSetting = true;
                    }
                    if (_archipelago.SlotData.TrapItemsDifficulty == TrapItemsDifficulty.Nightmare)
                    {
                        message += "Even with these traps??^";
                        anyHardSetting = true;
                    }
                    if (_archipelago.SlotData.EntranceRandomization == EntranceRandomization.Chaos)
                    {
                        message += "Even on Chaos ER?!? You scare me.^";
                        anyHardSetting = true;
                    }
                    if (!anyHardSetting)
                    {
                        message += "Try harder settings, you'll change your mind.^";
                    }

                    DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100") + message + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101").Substring(3), 6000);
                    DelayedAction.stopFarmerGlowing(6000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneEating_StardropFavoriteThingKaito_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}