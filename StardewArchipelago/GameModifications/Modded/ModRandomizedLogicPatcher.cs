using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.GameModifications.CodeInjections.Modded;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.GameModifications.Tooltips;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.Modded
{
    public class ModRandomizedLogicPatcher
    {
        private readonly Harmony _harmony;
        private readonly ArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;

        public ModRandomizedLogicPatcher(IMonitor monitor, IModHelper modHelper, Harmony harmony, ArchipelagoClient archipelago, ShopStockGenerator shopStockGenerator, StardewItemManager stardewItemManager, JunimoShopGenerator junimoShopGenerator)
        {
            _harmony = harmony;
            _archipelago = archipelago;
            JunimoShopInjections.Initialize(monitor, modHelper, archipelago, shopStockGenerator, stardewItemManager, junimoShopGenerator);

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
        }
    }
}