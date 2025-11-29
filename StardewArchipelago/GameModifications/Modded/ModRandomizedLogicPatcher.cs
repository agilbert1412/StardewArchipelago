using HarmonyLib;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.Modded
{
    public class ModRandomizedLogicPatcher
    {
        private readonly Harmony _harmony;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;
        private readonly JunimoShopStockModifier _junimoShopStockModifier;
        private readonly IModHelper _modHelper;

        public ModRandomizedLogicPatcher(ILogger logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, SeedShopStockModifier seedShopStockModifier, StardewItemManager stardewItemManager)
        {
            _harmony = harmony;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _modHelper = modHelper;
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                _junimoShopStockModifier = new JunimoShopStockModifier(logger, modHelper, archipelago, _stardewItemManager);
            }
        }

        public void PatchAllGameLogic()
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                _modHelper.Events.Content.AssetRequested += _junimoShopStockModifier.OnShopStockRequested;
            }
        }

        public void CleanEvents()
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                _modHelper.Events.Content.AssetRequested -= _junimoShopStockModifier.OnShopStockRequested;
            }
        }
    }
}
