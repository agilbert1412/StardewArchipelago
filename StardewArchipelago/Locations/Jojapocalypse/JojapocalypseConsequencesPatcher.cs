using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.GingerIsland;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Locations.Jojapocalypse.Consequences;
using StardewValley;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseConsequencesPatcher
    {
        private readonly ILogger _logger;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly Harmony _harmony;
        private readonly IModHelper _modHelper;
        private readonly JojaLocationChecker _jojaLocationChecker;

        public JojapocalypseConsequencesPatcher(ILogger logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, JojaLocationChecker jojaLocationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _harmony = harmony;
            _modHelper = modHelper;
            _jojaLocationChecker = jojaLocationChecker;

            InitializeAllConsequences();
        }

        private void InitializeAllConsequences()
        {
            ToolConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker);
            CropConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker);
            // Elevators should cost money
            // Skills should reduce natural resources
            // Blueprints
            // Story Quests
            // Arcade Machines
            // Traveling Merchant should increase TC prices
            // Fishsanity should reduce bite rate of fish
            // Museumsanity should increase price of breaking geodes
            // Festivals
            // Desert Festival and skull cavern should increase Bus cost (?)
            // Special Orders should increase amounts
            // Ginger island should increase boat cost (?)
            // Baby should decrease friendship with spouse
            // Monstersanity should decrease combat prowess
            // Shipsanity should reduce profit margin
            // Cooksanity should increase chance of failing cooking
            // Chefsanity
            // Craftsanity should increase chance of failing crafting
            // Booksanity should increase bookseller prices
            // Secretsanity should decrease secret note spawn chance
            // Movie should increase ticket and snack costs
            // Hatsanity
            // Eatsanity should decrease food efficiency
        }

        public void PatchAllConsequences()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)),
                postfix: new HarmonyMethod(typeof(ToolConsequences), nameof(ToolConsequences.DoFunction_ConsumeExtraEnergy_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                postfix: new HarmonyMethod(typeof(CropConsequences), nameof(CropConsequences.NewDay_ChanceOfNotGrowing_Prefix))
            );
        }
    }
}
