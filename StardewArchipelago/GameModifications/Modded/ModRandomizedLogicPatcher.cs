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
        private JunimoShopStockModifier _junimoShopStockModifier;
        private IModHelper _modHelper;

        public ModRandomizedLogicPatcher(IMonitor monitor, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago, SeedShopStockModifier seedShopStockModifier, StardewItemManager stardewItemManager)
        {
            _harmony = harmony;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _modHelper = modHelper;
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                _junimoShopStockModifier = new JunimoShopStockModifier(monitor, modHelper, archipelago, _stardewItemManager);
                PatchJunimoShops();
            }
        }

        private void PatchJunimoShops()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                return;
            }

            _modHelper.Events.Content.AssetRequested += _junimoShopStockModifier.OnShopStockRequested;
        }
    }
}
