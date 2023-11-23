using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;
using StardewValley.Objects;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class SVELocationsInjections{
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;
        public const string MORRIS_FRIENDSHIP = "Friendsanity: Morris {0} <3";
        public const int AURORA_EVENT = 658059254;
        public const int MORGAN_EVENT = 658078924;

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
    public static bool ShopMenu_BearShop_Prefix(ShopMenu __instance, ref Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
        {
            try
            {
                var myActiveHints = _archipelago.GetMyActiveHints();
                foreach (var salableItem in itemPriceAndStock.Keys.ToArray())
                {
                    _shopReplacer.ReplaceShopItem(itemPriceAndStock, salableItem, BEAR_ITEM_1, (Object item) =>  item.Name == "Baked Berry Oatmeal Recipe", myActiveHints);
                    _shopReplacer.ReplaceShopItem(itemPriceAndStock, salableItem, BEAR_ITEM_2, (Object item) => item.Name == "Flower Cookie Recipe", myActiveHints);
                }
                return true; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShopMenu_BearShop_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool ShopMenu_AlesiaShop_Prefix(ShopMenu __instance, ref Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
        {
            try
            {
                var myActiveHints = _archipelago.GetMyActiveHints();
                foreach (var salableItem in itemPriceAndStock.Keys.ToArray())
                {
                    _shopReplacer.ReplaceShopItem(itemPriceAndStock, salableItem, ALESIA_ITEM, (Object item) =>  item.Name == "Tempered Galaxy Dagger", myActiveHints);
                }
                return true; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShopMenu_AlesiaShop_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

            public static bool ShopMenu_IssacShop_Prefix(ShopMenu __instance, ref Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
        {
            try
            {
                var myActiveHints = _archipelago.GetMyActiveHints();
                foreach (var salableItem in itemPriceAndStock.Keys.ToArray())
                {
                    _shopReplacer.ReplaceShopItem(itemPriceAndStock, salableItem, ISSAC_ITEM_1, (Object item) =>  item.Name == "Tempered Galaxy Sword", myActiveHints);
                    _shopReplacer.ReplaceShopItem(itemPriceAndStock, salableItem, ISSAC_ITEM_1, (Object item) =>  item.Name == "Tempered Galaxy Hammer", myActiveHints);
                }
                return true; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShopMenu_IssacShop_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

//Locations to be added as checks
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

    public static bool SkipEvent_AlternativeRustyKey_Prefix(Event __instance)
        {
            try
            {
                if (__instance.id != 1090501)
                {
                    return true; // run original logic
                }


                if (__instance.playerControlSequence)
                {
                    __instance.EndPlayerControlSequence();
                }

                Game1.playSound("drumkit6");

                var actorPositionsAfterMoveField = _modHelper.Reflection.GetField<Dictionary<string, Vector3>>(__instance, "actorPositionsAfterMove");
                actorPositionsAfterMoveField.GetValue().Clear();

                foreach (var actor in __instance.actors)
                {
                    var ignoreStopAnimation = actor.Sprite.ignoreStopAnimation;
                    actor.Sprite.ignoreStopAnimation = true;
                    actor.Halt();
                    actor.Sprite.ignoreStopAnimation = ignoreStopAnimation;
                    __instance.resetDialogueIfNecessary(actor);
                }

                __instance.farmer.Halt();
                __instance.farmer.ignoreCollisions = false;
                Game1.exitActiveMenu();
                Game1.dialogueUp = false;
                Game1.dialogueTyping = false;
                Game1.pauseTime = 0.0f;

                _locationChecker.AddCheckedLocation(RUSTY_KEY_ALT);
                
                Game1.player.Position = new Vector2(-9999f, -99999f);
                __instance.endBehaviors(new string[1] { "end" }, Game1.currentLocation);
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkipEvent_AlternativeRustyKey_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void SkipEvent_ReleaseMorris_Postfix(Event __instance)
        {
            try
            {
                if (__instance.id != 191393)
                {
                    return; // run original logic
                }

                for (var i = 1; i <= 10; i++)
                {
                    _locationChecker.AddCheckedLocation(string.Format(MORRIS_FRIENDSHIP, i));
                }


            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkipEvent_AlternativeRustyKey_Prefix)}:\n{ex}", LogLevel.Error);
                return; // run original logic
            }
        }

        public static void SVEEventInitializer()
        {
            VillagerInitializer();
            WarpInitializer();
        }

        public static void VillagerInitializer()
        {
            var farm_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Farm");
            var farmhouse_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\FarmHouse");
            var oldApplesRule = "77759254/e 191393/e 1000037/j 138/t 600 630/H";
            var newApplesRule = "77759254/e " + AURORA_EVENT.ToString(); //Only shows up if AP event plays
            var oldMorganRule = "5978924/y 3/d Mon Tue/e 1724095";
            var newMorganRule = "5978924/e " + MORGAN_EVENT.ToString(); //Only shows up if AP event plays
            var morganEvent = farm_events[oldMorganRule];
            var applesEvent = farmhouse_events[oldApplesRule];
            farm_events.Remove(oldMorganRule);
            farmhouse_events.Remove(oldApplesRule);
            farm_events[newMorganRule] = morganEvent;
            farmhouse_events[newApplesRule] = applesEvent;
        }    
        // Force every warp to have itself as a prerequisite (so AP items can only unlock them)
        public static void WarpInitializer()
        {
            var currentEventData = new Dictionary<string, string>();
            string mapName;
            string eventData;
            string newEventKey;
            string eventID;
            foreach( var kvp in warpKeys)
            {
                mapName = kvp.Key;
                if(kvp.Key.Contains("FarmHouse"))
                {
                    mapName = "FarmHouse";
                }
                currentEventData = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + mapName);
                foreach( var eventKey in kvp.Value)
                {
                    string[] totalEventKey = eventKey.Split("/");
                    eventID = totalEventKey[1];
                    if(!currentEventData.ContainsKey(eventKey))
                    {
                        currentEventData[eventKey] = "continue/-200 -200 0/Lewis -10 -10 0/pause 50/end";
                    }
                    newEventKey = eventKey + "/e " + eventID;
                    eventData = currentEventData[eventKey];
                    currentEventData.Remove(eventKey);
                    currentEventData[newEventKey] = eventData;
                }
            }


        }

//Table of Names
        public const string BEAR_ITEM_1 = "Learn Recipe Baked Berry Oatmeal";
        public const string BEAR_ITEM_2 = "Learn Recipe Flower Cookie";
        public const string ALESIA_ITEM = "Alesia: Tempered Galaxy Dagger";
        public const string ISSAC_ITEM_1 = "Issac: Tempered Galaxy Sword";
        public const string ISSAC_ITEM_2 = "Issac: Tempered Galaxy Hammer";
        public const string LANCE_CHEST = "Lance's Diamond Wand";
        public const string RUSTY_KEY_ALT = "Rusty Key";
        private static readonly Dictionary<string, string[]> warpKeys = new(){ // Contains the original event requirements to be edited
            { "IslandWest", new[]{"65360191/f Lance 2000"}},
            { "FarmHouseFarmRune", new[]{"908074/e 908072/t 600 630"}},
            { "FarmHouseOutpostRune", new[]{"908078/e 908072/t 600 2400"}},
            { "Backwoods", new[]{"908072/e 908071"}},
            { "Custom_AdventurerSummit", new[]{"908073/e 908072/z winter/t 600 1900/w sunny", "908073/e 908072/z winter/t 1910 2400/w sunny", 
            "908073/e 908072/z winter/w rainy", "908073/e 908072/z spring/z summer/z fall"}},
            { "Custom_AuroraVineyard", new[]{"908075/e 908072", "908075/e 908072/f Apples 10"}},
            { "Custom_SpriteSpring2", new[]{"908076/e 908072"}},
            { "Custom_JunimoWoods", new[]{"908077/e 908072"}},
        };
    }
}