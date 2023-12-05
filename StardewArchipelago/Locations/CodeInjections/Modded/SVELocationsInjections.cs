using System;
using HarmonyLib;
using Netcode;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using StardewArchipelago.Goals;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class SVELocationsInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;
        private static NetString railroadKey = new NetString("Clint2Again");
        public const int AURORA_EVENT = 658059254;
        public const int MORGAN_EVENT = 658078924;
        private const int JOJA_COLA = 167;
        private const string ALESIA_DAGGER = "Tempered Galaxy Dagger";
        private const string ISAAC_SWORD = "Tempered Galaxy Sword";
        private const string ISAAC_HAMMER = "Tempered Galaxy Hammer";
        private const string LANCE_CHEST = "Lance's Diamond Wand";
        private const string RUSTY_KEY_ALT = "Rusty Key";
        private static ShopMenu _lastShopMenuUpdated = null;

        private static readonly string[] craftsanityRecipes = new[]
        {
            "Haste Elixir",
            "Armor Elixir",
            "Hero Elixir",
        };

        private static readonly string[] chefsanityRecipes = new[]
        {
            "Big Bark Burger",
            "Glazed Butterfish",
            "Mixed Berry Pie",
            "Baked Berry Oatmeal",
            "Flower Cookie",
            "Frog Legs",
            "Mushroom Berry Rice",
            "Seaweed Salad",
            "Void Delight",
            "Void Salmon Sushi",
        };

        private static readonly Dictionary<string, string[]> warpKeys = new()
        {
            // Contains the original event requirements to be edited
            { "IslandWest", new[] { "65360191/f Lance 2000" } }, // Lance giving you the Fable Reef Warp
            { "FarmHouseFarmRune", new[] { "908074/e 908072/t 600 630" } }, // Unlocking the Farm Rune
            { "FarmHouseOutpostRune", new[] { "908078/e 908072/t 600 2400" } }, // Unlocking Galmoran Outpost
            { "Backwoods", new[] { "908072/e 908071" } }, // Unlocking Wizard Rune
            {
                "AdventureGuild", new[]
                {
                    "908073/e 908072/z winter/t 600 1900/w sunny", "908073/e 908072/z winter/t 1910 2400/w sunny",
                    "908073/e 908072/z winter/w rainy", "908073/e 908072/z spring/z summer/z fall"
                }
            }, // Unlocking Summit Rune
            { "Custom_AuroraVineyard", new[] { "908075/e 908072", "908075/e 908072/f Apples 10" } }, // Unlocking Vineyard Rune
            { "Custom_SpriteSpring2", new[] { "908076/e 908072" } }, // Unlocking Springs Rune
            { "Custom_JunimoWoods", new[] { "908077/e 908072" } }, // Unlocking Junimo Rune
            { "Town", new[] { "3691371" } }, // The initializer for Scarlett to be a villager.
            // Event where Marlon gives the player a second Rusty Key.
            { "Custom_AdventurerSummit", new[] { "1090501/e 1000034/k 1090502/f MarlonFay 1250", "1090501/e 1000034/k 1090502/b 1"}}
        };

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _shopReplacer = shopReplacer;
            SVEEventInitializer();
        }




        // Unique Shop Locations
        public static void Update_ShopReplacer_Postfix(ShopMenu __instance, GameTime time)
        {
            try
            {
                // We only run this once for each menu
                if (_lastShopMenuUpdated == __instance)
                {
                    return;
                }

                _lastShopMenuUpdated = __instance;
                var myActiveHints = _archipelago.GetMyActiveHints();
                foreach (var salableItem in __instance.itemPriceAndStock.Keys.ToArray())
                {
                    ReplaceTemperedGalaxyWeapons(__instance, salableItem, myActiveHints);
                    ReplaceCraftsanityRecipes(__instance, salableItem, myActiveHints);
                    ReplaceChefsanityRecipes(__instance, salableItem, myActiveHints);
                }

                __instance.forSale = __instance.itemPriceAndStock.Keys.ToList();
                return; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Update_ShopReplacer_Postfix)}:\n{ex}", LogLevel.Error);
                return; // run original logic
            }
        }

        private static void ReplaceTemperedGalaxyWeapons(ShopMenu shopMenu, ISalable salableItem, Hint[] myActiveHints)
        {
            _shopReplacer.ReplaceShopItem(shopMenu.itemPriceAndStock, salableItem, ALESIA_DAGGER, "Tempered Galaxy Dagger", myActiveHints);
            _shopReplacer.ReplaceShopItem(shopMenu.itemPriceAndStock, salableItem, ISAAC_SWORD, "Tempered Galaxy Sword", myActiveHints);
            _shopReplacer.ReplaceShopItem(shopMenu.itemPriceAndStock, salableItem, ISAAC_HAMMER, "Tempered Galaxy Hammer", myActiveHints);
        }

        private static void ReplaceCraftsanityRecipes(ShopMenu shopMenu, ISalable salableItem, Hint[] myActiveHints)
        {
            if (!_archipelago.SlotData.Craftsanity.HasFlag(Craftsanity.All))
            {
                return;
            }

            foreach (var recipeItem in craftsanityRecipes)
            {
                _shopReplacer.ReplaceShopRecipe(shopMenu.itemPriceAndStock, salableItem, $"{recipeItem} Recipe", recipeItem, myActiveHints);
            }
        }

        private static void ReplaceChefsanityRecipes(ShopMenu shopMenu, ISalable salableItem, Hint[] myActiveHints)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
            {
                return;
            }

            foreach (var recipeItem in chefsanityRecipes)
            {
                _shopReplacer.ReplaceShopRecipe(shopMenu.itemPriceAndStock, salableItem, $"{recipeItem} Recipe", recipeItem, myActiveHints);
            }
        }


        public static bool CheckForAction_LanceChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value)
                {
                    return true; // run original logic
                }

                if (__instance.items.Count <= 0 || __instance.items.Count > 1 || __instance.items.First().Name != "Diamond Wand")
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                var obj = __instance.items[0];
                __instance.items[0] = null;
                __instance.items.RemoveAt(0);
                __result = true;

                _locationChecker.AddCheckedLocation(LANCE_CHEST);

                return false; // don't run original logic

            }

            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_LanceChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void ResetLocalState_PlayCutsceneIfConditionsAreMet_Postfix(GameLocation __instance)
        {
            try
            {
                var railroadBoulderID = 8050108;
                var iridiumBombID = 8050109;
                var bombBeforeQuest = Game1.player.eventsSeen.Contains(railroadBoulderID) & Game1.player.eventsSeen.Contains(iridiumBombID);
                if (!Game1.player.hasSkullKey || !bombBeforeQuest)
                {
                    return;
                }

                var railroadBoulderOrder = SpecialOrder.GetSpecialOrder("Clint2", null);
                railroadBoulderOrder.questKey = railroadKey;
                Game1.player.team.specialOrders.Add(railroadBoulderOrder);

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ResetLocalState_PlayCutsceneIfConditionsAreMet_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void SVEEventInitializer()
        {
            ViewableCutsceneInitializer();
            LockedCutsceneInitializer();
            AppendMadeUpOrder();
        }

        // Some events we want to see, so this will let them play out, so we make up AP event scene requirements.
        public static void ViewableCutsceneInitializer()
        {
            var farm_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Farm");
            var farmhouse_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse");
            var oldApplesRule = "77759254/e 191393/e 1000037/j 138/t 600 630/H";
            var newApplesRule = "77759254/e " + AURORA_EVENT.ToString(); //Only shows up if AP event plays
            var oldMorganRule = "5978924/y 3/d Mon Tue/e 1724095";
            var newMorganRule = "5978924/e " + MORGAN_EVENT.ToString(); //Only shows up if AP event plays
            var morganEvent = farm_events[oldMorganRule];
            var applesEvent = farmhouse_events[oldApplesRule];
            var scarlettEvent =
                farm_events.Remove(oldMorganRule);
            farmhouse_events.Remove(oldApplesRule);
            farm_events[newMorganRule] = morganEvent;
            farmhouse_events[newApplesRule] = applesEvent;
        }

        // Otherwise, we should just lock the events out of being seen at all and simply toggled by eventSeen.
        public static void LockedCutsceneInitializer()
        {
            foreach (var kvp in warpKeys)
            {
                var mapName = kvp.Key;
                if (kvp.Key.Contains("FarmHouse"))
                {
                    mapName = "FarmHouse";
                }

                if (kvp.Key.Contains("Custom_AdventurerSummit"))
                {
                    mapName = "Custom_AdventurerSummit";
                }

                var currentEventData = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + mapName);
                foreach (var eventKey in kvp.Value)
                {
                    string[] totalEventKey = eventKey.Split("/");
                    var eventID = totalEventKey[0];
                    // Sometimes, CP has not added the cutscene to the event list yet due to its dynamic adding methods...
                    if (!currentEventData.ContainsKey(eventKey))
                    {
                        // ... so just add a dummy event if that's the case to make sure its locked and move on to the next event.
                        currentEventData[eventKey] = "continue/-200 -200 0/Lewis -10 -10 0/pause 50/end";
                    }

                    // Everything exists now, so just make the requirement for the event itself.
                    var newEventKey = eventKey + "/e " + eventID;
                    var eventData = currentEventData[eventKey];
                    currentEventData.Remove(eventKey);
                    currentEventData[newEventKey] = eventData;
                }
            }
        }

        // Railroad Boulder Special Order won't load if Iridium Bomb is sent early, so we duplicate it so the player gets it.
        private static void AppendMadeUpOrder()
        {
            var getsveOrderType = AccessTools.TypeByName("HarmonyPatch_UntimedSpecialOrders");
            var specialOrderKeysType = _modHelper.Reflection.GetField<List<string>>(getsveOrderType, "SpecialOrderKeys");
            var specialOrderKeys = specialOrderKeysType.GetValue();
            specialOrderKeys.Add("Clint2Again");
        }

        // Done as JojaMart was changed to be two different shop tenders (Claire and Martin); just force every shop in Joja to be the same.
        public static bool ShopMenu_ForceJojaShop_Prefix(ShopMenu __instance, ref Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null,
            Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
        {
            try
            {
                if (Game1.currentLocation is JojaMart)
                {
                    var jojaStock = new Dictionary<ISalable, int[]>();
                    SeedShopsInjections.AddToJojaStock(jojaStock, JOJA_COLA, false, 75, 6);
                    SeedShopsInjections.AddJojaFurnitureToShop(jojaStock);
                    SeedShopsInjections.AddSeedsToJojaStock(jojaStock);
                    SeedShopsInjections.AddGrassStarterToJojaStock(jojaStock);
                    SeedShopsInjections.AddCookingIngredientsToJojaStock(jojaStock);

                    itemPriceAndStock = jojaStock;
                    return true; // run original logic
                }

                return true; // Run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShopMenu_ForceJojaShop_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic   
            }

        }
    }
}