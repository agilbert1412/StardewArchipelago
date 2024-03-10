
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.GameModifications.CodeInjections.Modded;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.Modded
{
    public class ModRandomizedLogicPatcher
    {
        private readonly Harmony _harmony;
        private readonly ArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;

        public ModRandomizedLogicPatcher(IMonitor monitor, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago, SeedShopStockModifier seedShopStockModifier, StardewItemManager stardewItemManager)
        {
            _harmony = harmony;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                JunimoShopInjections.Initialize(monitor, modHelper, archipelago, seedShopStockModifier, _stardewItemManager);
            }

        }

        public void PatchAllModGameLogic()
        {
            PatchJunimoShops();
        }

        private void PatchJunimoShops()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.update)),
                prefix: new HarmonyMethod(typeof(JunimoShopInjections), nameof(JunimoShopInjections.Update_JunimoWoodsAPShop_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(JunimoShopInjections), nameof(JunimoShopInjections.AnswerDialogueAction_Junimoshop_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.resetFriendshipsForNewDay)),
                postfix: new HarmonyMethod(typeof(JunimoShopInjections), nameof(JunimoShopInjections.ResetFriendshipsForNewDay_KissForeheads_Postfix))
            );
        }
    }
}