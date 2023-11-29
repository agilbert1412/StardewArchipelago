using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
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
        private static NetString railroadKey = new NetString("Clint2Again");
        public const string MORRIS_FRIENDSHIP = "Friendsanity: Morris {0} <3";
        public const int AURORA_EVENT = 658059254;
        public const int MORGAN_EVENT = 658078924;
        private const int JOJA_COLA = 167;
        private const string BEAR_ITEM_1 = "Learn Recipe Baked Berry Oatmeal";
        private const string BEAR_ITEM_2 = "Learn Recipe Flower Cookie";
        private const string ALESIA_ITEM = "Alesia: Tempered Galaxy Dagger";
        private const string ISSAC_ITEM_1 = "Issac: Tempered Galaxy Sword";
        private const string ISSAC_ITEM_2 = "Issac: Tempered Galaxy Hammer";
        private const string LANCE_CHEST = "Lance's Diamond Wand";
        private const string RUSTY_KEY_ALT = "Rusty Key";
        private static readonly Dictionary<string, string[]> warpKeys = new(){ // Contains the original event requirements to be edited
            { "IslandWest", new[]{"65360191/f Lance 2000"}}, // Lance giving you the Fable Reef Warp
            { "FarmHouseFarmRune", new[]{"908074/e 908072/t 600 630"}}, // Unlocking the Farm Rune
            { "FarmHouseOutpostRune", new[]{"908078/e 908072/t 600 2400"}}, // Unlocking Galmoran Outpost
            { "Backwoods", new[]{"908072/e 908071"}}, // Unlocking Wizard Rune
            { "Custom_AdventurerSummit", new[]{"908073/e 908072/z winter/t 600 1900/w sunny", "908073/e 908072/z winter/t 1910 2400/w sunny", 
            "908073/e 908072/z winter/w rainy", "908073/e 908072/z spring/z summer/z fall"}}, // Unlocking Summit Rune
            { "Custom_AuroraVineyard", new[]{"908075/e 908072", "908075/e 908072/f Apples 10"}}, // Unlocking Vineyard Rune
            { "Custom_SpriteSpring2", new[]{"908076/e 908072"}}, // Unlocking Springs Rune
            { "Custom_JunimoWoods", new[]{"908077/e 908072"}}, // Unlocking Junimo Rune
            { "Town", new[]{"3691371"}}, // The initializer for Scarlett to be a villager.
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
                    _shopReplacer.ReplaceShopItem(itemPriceAndStock, salableItem, ISSAC_ITEM_2, (Object item) =>  item.Name == "Tempered Galaxy Hammer", myActiveHints);
                }
                return true; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShopMenu_IssacShop_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
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

    // There's an alternative Rusty Key event that's added from Marlon by being friends with him.
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

        public static void ReleaseMorrisWhenCommunityCenter(bool completionLogic)
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
                _monitor.Log($"Failed in {nameof(SkipEvent_ReleaseMorris_Postfix)}:\n{ex}", LogLevel.Error);
                return; // run original logic
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
            foreach( var kvp in warpKeys)
            {
                var mapName = kvp.Key;
                if(kvp.Key.Contains("FarmHouse"))
                {
                    mapName = "FarmHouse";
                }
                var currentEventData = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + mapName);
                foreach( var eventKey in kvp.Value)
                {
                    string[] totalEventKey = eventKey.Split("/");
                    var eventID = totalEventKey[0];
                    // Sometimes, CP has not added the cutscene to the event list yet due to its dynamic adding methods...
                    if(!currentEventData.ContainsKey(eventKey))
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

        private static void AppendMadeUpOrder()
        {
            var getsveOrderType = AccessTools.TypeByName("HarmonyPatch_UntimedSpecialOrders");
            var specialOrderKeysType = _modHelper.Reflection.GetField<List<string>>(getsveOrderType, "SpecialOrderKeys");
            var specialOrderKeys = specialOrderKeysType.GetValue();
            specialOrderKeys.Add("Clint2Again");
        }

        // Done as JojaMart was changed to be two different shop tenders (Claire and Martin); just force every shop in Joja to be the same.
        public static bool ShopMenu_ForceJojaShop_Prefix(ShopMenu __instance, ref Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
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