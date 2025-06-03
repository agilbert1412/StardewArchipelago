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
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

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
            SkillsConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker);
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
                postfix: new HarmonyMethod(typeof(CropConsequences), nameof(CropConsequences.NewDay_ChanceOfDying_Prefix))
            );

            PatchSkillConsequences();
        }

        private void PatchSkillConsequences()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                postfix: new HarmonyMethod(typeof(SkillsConsequences), nameof(SkillsConsequences.NewDay_ChanceOfNotGrowing_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
                postfix: new HarmonyMethod(typeof(SkillsConsequences), nameof(SkillsConsequences.DayUpdate_ChanceToNotGrowTree_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), "calculateTimeUntilFishingBite"),
                postfix: new HarmonyMethod(typeof(SkillsConsequences), nameof(SkillsConsequences.CalculateTimeUntilFishingBite_AddTimePerFish_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "adjustLevelChances"),
                postfix: new HarmonyMethod(typeof(SkillsConsequences), nameof(SkillsConsequences.AdjustLevelChances_FewerThingsInMine_Postfix))
            );
        }

        public static bool RollConsequenceChance(double chancePerPurchase, int numberPurchased, double seedA = 0.0, double seedB = 0.0, double seedC = 0.0)
        {
            var chanceEvent = GetEventChance(chancePerPurchase, numberPurchased);
            var random = Utility.CreateDaySaveRandom(seedA, seedB, seedC);
            return random.NextDouble() <= chanceEvent;
        }

        public static double GetEventChance(double chancePerPurchase, int numberPurchased)
        {
            var chanceEventPerPurchase = chancePerPurchase;
            var chanceNotEventPerPurchase = 1 - chanceEventPerPurchase;
            var chanceNotEvent = Math.Pow(chanceNotEventPerPurchase, numberPurchased);
            var chanceEvent = 1 - chanceNotEvent;

            return chanceEvent;
        }
    }
}
