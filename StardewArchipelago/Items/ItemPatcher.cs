using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Menus;
using StardewValley.Tools;

namespace StardewArchipelago.Items
{
    public class ItemPatcher
    {
        private readonly Harmony _harmony;

        public ItemPatcher(IMonitor monitor, IModHelper helper, Harmony harmony, ArchipelagoClient archipelago)
        {
            _harmony = harmony;
            PlayerBuffInjections.Initialize(monitor, helper, archipelago);
        }

        public void PatchApItems()
        {
            PatchPlayerBuffs();
        }

        private void PatchPlayerBuffs()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getMovementSpeed)),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.GetMovementSpeed_AddApBuffs_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.DailyLuck)),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.DailyLuck_AddApBuffs_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(BuffManager), nameof(BuffManager.AttackMultiplier)),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.GetAttackMultiplier_AddApBuffs_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(BuffManager),nameof(BuffManager.Defense)),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.GetDefense_AddApBuffs_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(BuffManager), nameof(BuffManager.Immunity)),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.GetImmunity_AddApBuffs_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(BuffManager), nameof(BuffManager.MaxStamina)),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.GetMaxStamina_AddApBuffs_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), "calculateTimeUntilFishingBite"),
                postfix: new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.CalculateTimeUntilFishingBite_AddApBuffs_Postfix))
            );
            var bobberBarConstructor = AccessTools.Constructor(typeof(BobberBar));
            var postfix = new HarmonyMethod(typeof(PlayerBuffInjections), nameof(PlayerBuffInjections.BobberBarConstructor_AddApBuffs_Postfix));
            _harmony.Patch(
                original: bobberBarConstructor,
                postfix: postfix
            );
        }
    }
}
