using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.GameModifications.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded;
using StardewArchipelago.Locations.CodeInjections.Modded.SVE;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.Patcher
{
    public class ModLocationPatcher : ILocationPatcher
    {
        private readonly StardewArchipelagoClient _archipelago;
        private readonly Harmony _harmony;
        private readonly ILogger _logger;
        private readonly IModHelper _modHelper;
        private readonly ModsManager _modsManager;
        private readonly TemperedShopStockModifier _temperedShopStockModifier;
        private readonly BearShopStockModifier _bearShopStockModifier;

        public ModLocationPatcher(Harmony harmony, ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _archipelago = archipelago;
            _harmony = harmony;
            _logger = logger;
            _modHelper = modHelper;
            _modsManager = archipelago.SlotData.Mods;
            _temperedShopStockModifier = new TemperedShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _bearShopStockModifier = new BearShopStockModifier(logger, modHelper, archipelago, stardewItemManager);

        }

        public void ReplaceAllLocationsRewardsWithChecks()
        {
            try
            {
                AddModSkillInjections();
                AddDeepWoodsModInjections();
                AddMagicModInjections();
                AddSkullCavernElevatorModInjections();
                AddSVEModInjections();
                AddBoardingHouseInjections();
                PatchSVEShops();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed at Initializing mod content. Message: {ex.Message}");
            }
        }

        public void CleanEvents()
        {
            if (_modsManager.HasMod(ModNames.SVE))
            {
                UnpatchSVEShops();
            }
        }

        private void AddModSkillInjections()
        {
            if (!_modsManager.HasModdedSkill() || _archipelago.SlotData.SkillProgression == SkillsProgression.Vanilla)
            {
                return;
            }

            var _spaceCoreInterfaceType = AccessTools.TypeByName("SpaceCore.Interface.SkillLevelUpMenu");
            var spaceCoreSkillsType = AccessTools.TypeByName("SpaceCore.Skills");
            if (_archipelago.SlotData.SkillProgression != SkillsProgression.Vanilla)
            {
                _harmony.Patch(
                    original: AccessTools.Method(spaceCoreSkillsType, "AddExperience"),
                    prefix: new HarmonyMethod(typeof(SkillInjections), nameof(SkillInjections.AddExperience_ArchipelagoModExperience_Prefix))
                );
            }

            if (_archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.update)),
                    postfix: new HarmonyMethod(typeof(MagicModInjections), nameof(MagicModInjections.Update_ReplaceMarlonShopChecks_Postfix))
                );
            }

            _harmony.Patch(
                original: AccessTools.Constructor(_spaceCoreInterfaceType, new[] { typeof(string), typeof(int) }),
                postfix: new HarmonyMethod(typeof(RecipeLevelUpInjections), nameof(RecipeLevelUpInjections.SkillLevelUpMenuConstructor_SendModdedSkillRecipeChecks_Postfix))
            );

            InjectSocializingExperienceMultiplier();
            InjectArchaeologyExperienceMultiplier();
        }

        private void InjectSocializingExperienceMultiplier()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SOCIALIZING))
            {
                return;
            }

            var socializingConfigPatcher = new SocializingConfigPatcher(_logger, _modHelper);
            socializingConfigPatcher.PatchConfigValues();
        }

        private void InjectArchaeologyExperienceMultiplier()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.ARCHAEOLOGY))
            {
                return;
            }

            var archaeologyConfigType = AccessTools.TypeByName("ArchaeologySkill.Config");
            _harmony.Patch(
                original: AccessTools.PropertyGetter(archaeologyConfigType, "ExperienceFromArtifactSpots"),
                postfix: new HarmonyMethod(typeof(ArchaeologyConfigCodeInjections), nameof(ArchaeologyConfigCodeInjections.ExperienceFromArtifactSpots_APMultiplier_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(archaeologyConfigType, "ExperienceFromPanSpots"),
                postfix: new HarmonyMethod(typeof(ArchaeologyConfigCodeInjections), nameof(ArchaeologyConfigCodeInjections.ExperienceFromPanSpots_APMultiplier_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(archaeologyConfigType, "ExperienceFromMinesDigging"),
                postfix: new HarmonyMethod(typeof(ArchaeologyConfigCodeInjections), nameof(ArchaeologyConfigCodeInjections.ExperienceFromMinesDigging_APMultiplier_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.PropertyGetter(archaeologyConfigType, "ExperienceFromWaterShifter"),
                postfix: new HarmonyMethod(typeof(ArchaeologyConfigCodeInjections), nameof(ArchaeologyConfigCodeInjections.ExperienceFromWaterShifter_APMultiplier_Postfix))
            );
        }

        private void AddDeepWoodsModInjections()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.DEEP_WOODS))
            {
                return;
            }

            var _deepWoodsType = AccessTools.TypeByName("DeepWoodsMod.DeepWoods");
            var _swordType = AccessTools.TypeByName("DeepWoodsMod.ExcaliburStone");
            var _enterDirectionType = AccessTools.TypeByName("DeepWoodsMod.DeepWoodsEnterExit+EnterDirection");
            var constructorParameterTypes = new[] { _deepWoodsType, typeof(int), _enterDirectionType, typeof(bool) };
            var _unicornType = AccessTools.TypeByName("DeepWoodsMod.Unicorn");
            var _gingerbreadType = AccessTools.TypeByName("DeepWoodsMod.GingerBreadHouse");
            var _iridiumtreeType = AccessTools.TypeByName("DeepWoodsMod.IridiumTree");
            var _treasureType = AccessTools.TypeByName("DeepWoodsMod.TreasureChest");
            var _fountainType = AccessTools.TypeByName("DeepWoodsMod.HealingFountain");
            var _infestedType = AccessTools.TypeByName("DeepWoodsMod.InfestedTree");

            _harmony.Patch(
                original: AccessTools.Method(_swordType, "performUseAction"),
                prefix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.PerformUseAction_ExcaliburLocation_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_unicornType, "checkAction"),
                prefix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.CheckAction_PetUnicornLocation_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_unicornType, "CheckScared"),
                prefix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.CheckScared_MakeUnicornLessScared_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_treasureType, "checkForAction"),
                prefix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.CheckForAction_TreasureChestLocation_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_gingerbreadType, "PlayDestroyedSounds"),
                postfix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.PlayDestroyedSounds_GingerbreadLocation_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_iridiumtreeType, "PlayDestroyedSounds"),
                postfix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.PlayDestroyedSounds_IridiumTreeLocation_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_fountainType, "performUseAction"),
                prefix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.PerformUseAction_HealingFountainLocation_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(_infestedType, "DeInfest"),
                postfix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.Deinfest_DeinfestLocation_Postfix))
            );
            if (_archipelago.SlotData.ElevatorProgression != ElevatorProgression.Vanilla)
            {
                _harmony.Patch(
                    original: AccessTools.Constructor(_deepWoodsType, constructorParameterTypes),
                    postfix: new HarmonyMethod(typeof(DeepWoodsModInjections), nameof(DeepWoodsModInjections.Constructor_WoodsDepthChecker_Postfix))
                );
            }
        }

        private void AddMagicModInjections()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.MAGIC))
            {
                return;
            }

            var _analyzeSpellType = AccessTools.TypeByName("AnalyzeSpell");
            _harmony.Patch(
                original: AccessTools.Method(_analyzeSpellType, "OnCast"),
                prefix: new HarmonyMethod(typeof(MagicModInjections),
                    nameof(MagicModInjections.OnCast_AnalyzeGivesLocations_Prefix))
            );
        }

        private void AddSkullCavernElevatorModInjections()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SKULL_CAVERN_ELEVATOR))
            {
                return;
            }

            if (_archipelago.SlotData.ElevatorProgression == ElevatorProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(SkullCavernInjections), nameof(SkullCavernInjections.EnterMine_SendSkullCavernElevatorCheck_PostFix))
            );

            var constructorParameterTypes = new[] { typeof(int), typeof(double), typeof(int) };
            var myElevatorMenuType = AccessTools.TypeByName("MyElevatorMenu");
            var myElevatorMenuConstructor = AccessTools.Constructor(myElevatorMenuType, constructorParameterTypes);
            _harmony.Patch(
                original: myElevatorMenuConstructor,
                prefix: new HarmonyMethod(typeof(SkullCavernInjections), nameof(SkullCavernInjections.MyElevatorMenuConstructor_SkullCavernElevator_Prefix)),
                postfix: new HarmonyMethod(typeof(SkullCavernInjections), nameof(SkullCavernInjections.MyElevatorMenuConstructor_SkullCavernElevator_Postfix))
            );

            var myElevatorMenuWithScrollBarType = AccessTools.TypeByName("MyElevatorMenuWithScrollbar");
            var myElevatorMenuWithScrollBarConstructor = AccessTools.Constructor(myElevatorMenuWithScrollBarType, constructorParameterTypes);
            _harmony.Patch(
                original: myElevatorMenuWithScrollBarConstructor,
                prefix: new HarmonyMethod(typeof(SkullCavernInjections), nameof(SkullCavernInjections.MyElevatorMenuConstructor_SkullCavernElevator_Prefix)),
                postfix: new HarmonyMethod(typeof(SkullCavernInjections), nameof(SkullCavernInjections.MyElevatorMenuConstructor_SkullCavernElevator_Postfix))
            );
        }

        private void AddSVEModInjections()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(SVECutsceneInjections), nameof(SVECutsceneInjections.CheckForAction_LanceChest_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.endBehaviors), parameters: new[] { typeof(string[]), typeof(GameLocation) }),
                prefix: new HarmonyMethod(typeof(SVECutsceneInjections), nameof(SVECutsceneInjections.EndBehaviors_AddSpecialOrderAfterEvent_Prefix))
            );
            var specialOrderAfterEventsType = AccessTools.TypeByName("AddSpecialOrdersAfterEvents");

            _harmony.Patch(
                original: AccessTools.Method(specialOrderAfterEventsType, "UpdateSpecialOrders"),
                prefix: new HarmonyMethod(typeof(SVECutsceneInjections), nameof(SVECutsceneInjections.UpdateSpecialOrders_StopDeletingSpecialOrders_Prefix))
            );

            var disableShadowAttacksType = AccessTools.TypeByName("DisableShadowAttacks");

            _harmony.Patch(
                original: AccessTools.Method(disableShadowAttacksType, "FixMonsterSlayerQuest"),
                postfix: new HarmonyMethod(typeof(SVECutsceneInjections), nameof(SVECutsceneInjections.FixMonsterSlayerQuest_IncludeReleaseofGoals_Postfix))
            );
        }

        private void PatchSVEShops()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.SVE))
            {
                return;
            }
            _modHelper.Events.Content.AssetRequested += _temperedShopStockModifier.OnShopStockRequested;
            _modHelper.Events.Content.AssetRequested += _bearShopStockModifier.OnShopStockRequested;
        }

        private void UnpatchSVEShops()
        {
            _modHelper.Events.Content.AssetRequested -= _temperedShopStockModifier.OnShopStockRequested;
            _modHelper.Events.Content.AssetRequested -= _bearShopStockModifier.OnShopStockRequested;
        }

        private void AddBoardingHouseInjections()
        {
            if (!_archipelago.SlotData.Mods.HasMod(ModNames.BOARDING_HOUSE))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(BoardingHouseInjections), nameof(BoardingHouseInjections.CheckForAction_TreasureChestLocation_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.update)),
                postfix: new HarmonyMethod(typeof(BoardingHouseInjections), nameof(BoardingHouseInjections.Update_ReplaceDwarfShopChecks_Postfix))
            );
        }
    }
}
