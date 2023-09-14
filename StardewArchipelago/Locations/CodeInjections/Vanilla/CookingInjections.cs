using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Unlocks;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class CookingInjections
    {
        private const string RERUN_DAY = "Wed";
        private const string COOKING_LOCATION_PREFIX = "Cook ";

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
        }

        // public void cookedRecipe(int index)
        public static void CookedRecipe_CheckCooksanityLocation_Postfix(Farmer __instance, int index)
        {
            try
            {
                if (!_itemManager.ObjectExists(index))
                {
                    _monitor.Log($"Unrecognized cooked recipe: {index}", LogLevel.Warn);
                    return;
                }

                var cookedItem = _itemManager.GetObjectById(index);
                var cookedItemName = cookedItem.Name;
                var apLocation = $"{COOKING_LOCATION_PREFIX}{cookedItemName}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                    return;
                }

                _monitor.Log($"Unrecognized Cooksanity Location: {cookedItemName} [{index}]", LogLevel.Error);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CookedRecipe_CheckCooksanityLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // protected virtual string[] getWeeklyRecipe()
        public static bool GetWeeklyRecipe_UseArchipelagoSchedule_Prefix(TV __instance, ref string[] __result)
        {
            try
            {
                var weeklyRecipe = new string[2];
                var season = Game1.currentSeason;
                var week = Game1.stats.DaysPlayed % 28 / 7; // 0-3
                var isRerunDay = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals(RERUN_DAY);
                
                var cookingRecipes = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
                var team = Game1.player.team;
                if (isRerunDay)
                {
                    var allRerunRecipes = GetRerunRecipes();
                    var seed = (int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed);
                    var random = new Random(seed);
                    var rerunRecipe = allRerunRecipes[random.Next(allRerunRecipes.Length)];
                    __result = rerunRecipe;
                    return false; // don't run original logic
                }
                try
                {
                    var str = cookingRecipes[week.ToString() ?? ""].Split('/')[0];
                    weeklyRecipe[0] = cookingRecipes[week.ToString() ?? ""].Split('/')[1];
                    if (CraftingRecipe.cookingRecipes.ContainsKey(str))
                    {
                        var strArray = CraftingRecipe.cookingRecipes[str].Split('/');
                        weeklyRecipe[1] = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? (Game1.player.cookingRecipes.ContainsKey(cookingRecipes[week.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", (object)str) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", (object)str)) : (Game1.player.cookingRecipes.ContainsKey(cookingRecipes[week.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", (object)strArray[strArray.Length - 1]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", (object)strArray[strArray.Length - 1]));
                    }
                    else
                    {
                        weeklyRecipe[1] = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? (Game1.player.cookingRecipes.ContainsKey(cookingRecipes[week.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", (object)str) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", (object)str)) : (Game1.player.cookingRecipes.ContainsKey(cookingRecipes[week.ToString() ?? ""].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", (object)((IEnumerable<string>)cookingRecipes[week.ToString() ?? ""].Split('/')).Last<string>()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", (object)((IEnumerable<string>)cookingRecipes[week.ToString() ?? ""].Split('/')).Last<string>()));
                    }

                    if (!Game1.player.cookingRecipes.ContainsKey(str))
                        Game1.player.cookingRecipes.Add(str, 0);
                }
                catch (Exception ex)
                {
                    var str = cookingRecipes["1"].Split('/')[0];
                    weeklyRecipe[0] = cookingRecipes["1"].Split('/')[1];
                    weeklyRecipe[1] = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? (Game1.player.cookingRecipes.ContainsKey(cookingRecipes["1"].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", (object)str) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", (object)str)) : (Game1.player.cookingRecipes.ContainsKey(cookingRecipes["1"].Split('/')[0]) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13151", (object)((IEnumerable<string>)cookingRecipes["1"].Split('/')).Last<string>()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13153", (object)((IEnumerable<string>)cookingRecipes["1"].Split('/')).Last<string>()));
                    if (!Game1.player.cookingRecipes.ContainsKey(str))
                        Game1.player.cookingRecipes.Add(str, 0);
                }
                __result = weeklyRecipe;

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CookedRecipe_CheckCooksanityLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
