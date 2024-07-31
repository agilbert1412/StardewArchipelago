using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Locations;
using StardewModdingAPI;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class RecipeLevelUpInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public LevelUpMenu(int skill, int level)
        public static void LevelUpMenuConstructor_SendSkillRecipeChecks_Postfix(LevelUpMenu __instance, int skill, int level)
        {
            try
            {
                SendSkillRecipeChecks((Skill)skill, level);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(LevelUpMenuConstructor_SendSkillRecipeChecks_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void SendSkillRecipeChecks(Skill skill, int level)
        {
            SendSkillCookingRecipeChecks(skill, level);
            SendSkillCraftingRecipeChecks(skill, level);
        }

        private static void SendSkillCookingRecipeChecks(Skill skill, int level)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Skills) || !_cookingRecipesBySkill.ContainsKey(skill))
            {
                return;
            }

            var skillRecipes = _cookingRecipesBySkill[skill];
            for (var i = 0; i <= level; i++)
            {
                if (!skillRecipes.ContainsKey(i))
                {
                    continue;
                }

                var skillRecipesAtLevel = skillRecipes[i];
                foreach (var skillRecipe in skillRecipesAtLevel)
                {
                    _locationChecker.AddCheckedLocation($"{skillRecipe}{Suffix.CHEFSANITY}");
                }
            }
        }

        private static void SendSkillCraftingRecipeChecks(Skill skill, int level)
        {
            // There are no skill crafting recipe learning checks yet
        }

        // public SkillLevelUpMenu(string skillName, int level)
        public static void SkillLevelUpMenuConstructor_SendModdedSkillRecipeChecks_Postfix(IClickableMenu __instance, string skillName, int level)
        {
            try
            {
                var skillActualName = skillName.Split('.').Last().Replace("Skill", "");
                var skillCheck = Enum.TryParse<Skill>(skillActualName, out var skill);
                if (!skillCheck)
                {
                    _logger.LogError($"Leveled up unrecognized Skill: {skillActualName} [{skillName}]");
                    return;
                }
                SendSkillRecipeChecks(skill, level);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkillLevelUpMenuConstructor_SendModdedSkillRecipeChecks_Postfix)}:\n{ex}");
                return;
            }
        }

        private static readonly Dictionary<Skill, Dictionary<int, string[]>> _cookingRecipesBySkill = new()
        {
            {
                Skill.Farming, new Dictionary<int, string[]>()
                {
                    { 3, new[] { "Farmer's Lunch" } },
                }
            },
            {
                Skill.Fishing, new Dictionary<int, string[]>()
                {
                    { 3, new[] { "Dish O' The Sea" } },
                    { 9, new[] { "Seafoam Pudding" } },
                }
            },
            {
                Skill.Foraging, new Dictionary<int, string[]>()
                {
                    { 2, new[] { "Survival Burger" } },
                    { 3, new[] { "Moss Soup" } },
                }
            },
            {
                Skill.Mining, new Dictionary<int, string[]>()
                {
                    { 3, new[] { "Miner's Treat" } },
                }
            },
            {
                Skill.Combat, new Dictionary<int, string[]>()
                {
                    { 3, new[] { "Roots Platter" } },
                    { 9, new[] { "Squid Ink Ravioli" } },
                }
            },
            {
                Skill.Luck, new Dictionary<int, string[]>()
                {
                    { 8, new[] { "Lucky Lunch" } },
                }
            },
            {
                Skill.Archaeology, new Dictionary<int, string[]>()
                {
                    { 3, new [] { "Digger's Delight" } },
                    { 7, new [] { "Rocky Root Coffee" } },
                    { 9, new [] { "Ancient Jello" } },
                }
            },
            {
                Skill.Binning, new Dictionary<int, string[]>()
                {
                    {1, new [] { "Grilled Cheese" } },
                    {8, new []{ "Fish Casserole" } }, 
                }
            }
        };

        /*private static readonly Dictionary<Skill, Dictionary<int, string[]>> _craftingRecipesBySkill = new()
        {
            {
                Skill.Archaeology, new Dictionary<int, string[]>()
                {
                    { 1, new[] { "Glass Bazier", "Glass Path", "Glass Fence" } },
                    { 2, new[] { "Preservation Chamber", "Wooden Display" } },
                    { 3, new[] { "Bone Path" } },
                    { 4, new[] { "Water Shifter" } },
                    { 6, new[] { "Ancient Battery Production Station" } },
                    { 7, new[] { "hardwood Preservation Chamber", "Hardwood Display" } },
                    { 8, new[] { "Grinder" } },
                    { 9, new[] { "Dwarf Gadget: Infinite Volcano Simulation" } },
                }
            },
            {
                Skill.Binning, new Dictionary<int, string[]>()
                {
                    { 1, new[] { "Recycling Machine" } },
                    { 2, new[] { "Trash Can" } },
                    { 4, new[] { "Composter" } },
                    { 7, new[] { "Recycling Bin" } },
                    { 9, new[] { "Advanced Recycling Machine" } },
                }
            }
        };*/
    }
}
