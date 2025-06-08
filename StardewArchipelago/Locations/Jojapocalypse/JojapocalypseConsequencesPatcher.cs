using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley.Locations;
using System;
using StardewArchipelago.Locations.Jojapocalypse.Consequences;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
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
            ToolConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Tools take extra energy to swing
            CropConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Crops sometimes die
            MineshaftConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // The elevator undergoes maintenance
            SkillsConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Natural resources spawn less
            // Blueprints
            // Story Quests
            ArcadeConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Arcade Machines timescale increases
            TravelingCartConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Sometimes she doesn't come to pelican town
            FishingConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Sometimes you catch trash instead of fish
            GeodeConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Museumsanity should make geodes sometimes empty
            // Festivals get sometimes cancelled
            // Desert Festival and skull cavern should increase Bus cost (?)
            SpecialOrderConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Special orders require extras
            // Ginger island should increase boat cost (?)
            // Baby should decrease friendship with spouse
            // Monstersanity should decrease combat prowess
            ShippingConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Your global profit margin reduces
            CraftingCookingConsequences.Initialize(_logger, _modHelper, _archipelago, _jojaLocationChecker); // Chance of failing crafts, chance of using extra unrelated items
            // Booksanity should increase bookseller prices
            // Secretsanity should decrease secret note spawn chance
            // Movie should increase ticket and snack costs
            // Hatsanity sometimes your hat falls off lmao
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
                prefix: new HarmonyMethod(typeof(CropConsequences), nameof(CropConsequences.NewDay_ChanceOfDying_Prefix))
            );

            PatchSkillConsequences();

            _harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.pullFishFromWater)),
                prefix: new HarmonyMethod(typeof(FishingConsequences), nameof(FishingConsequences.PullFishFromWater_ReplaceWithTrash_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe"),
                prefix: new HarmonyMethod(typeof(CraftingCookingConsequences), nameof(CraftingCookingConsequences.ClickCraftingRecipe_ChanceOfFailingCraft_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getTreasureFromGeode)),
                prefix: new HarmonyMethod(typeof(GeodeConsequences), nameof(GeodeConsequences.GetTreasureFromGeode_SometimesTrash_Prefix))
            );

            PatchArcadeConsequences();
        }

        private void PatchSkillConsequences()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                prefix: new HarmonyMethod(typeof(SkillsConsequences), nameof(SkillsConsequences.NewDay_ChanceOfNotGrowing_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
                prefix: new HarmonyMethod(typeof(SkillsConsequences), nameof(SkillsConsequences.DayUpdate_ChanceToNotGrowTree_Prefix))
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

        private void PatchArcadeConsequences()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.tick)),
                prefix: new HarmonyMethod(typeof(ArcadeConsequences), nameof(ArcadeConsequences.Tick_IncreaseJotPKTimescale_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), nameof(MineCart.tick)),
                prefix: new HarmonyMethod(typeof(ArcadeConsequences), nameof(ArcadeConsequences.Tick_IncreaseJunimoKartTimescale_Prefix))
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
