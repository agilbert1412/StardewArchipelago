using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class EventInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static void BaseSkipEvent(Event eventBeingSkipped, Action checkLocationAction = null)
        {
            try
            {
                if (eventBeingSkipped.playerControlSequence)
                {
                    eventBeingSkipped.EndPlayerControlSequence();
                }

                Game1.playSound("drumkit6");

                var actorPositionsAfterMoveField = _modHelper.Reflection.GetField<Dictionary<string, Vector3>>(eventBeingSkipped, "actorPositionsAfterMove");
                actorPositionsAfterMoveField.GetValue().Clear();

                foreach (var actor in eventBeingSkipped.actors)
                {
                    var ignoreStopAnimation = actor.Sprite.ignoreStopAnimation;
                    actor.Sprite.ignoreStopAnimation = true;
                    actor.Halt();
                    actor.Sprite.ignoreStopAnimation = ignoreStopAnimation;
                    eventBeingSkipped.resetDialogueIfNecessary(actor);
                }

                eventBeingSkipped.farmer.Halt();
                eventBeingSkipped.farmer.ignoreCollisions = false;
                Game1.exitActiveMenu();
                Game1.dialogueUp = false;
                Game1.dialogueTyping = false;
                Game1.pauseTime = 0.0f;

                checkLocationAction?.Invoke();

                eventBeingSkipped.endBehaviors(new[]
                {
                    "end",
                    "position",
                    "43",
                    "36",
                }, Game1.currentLocation);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(BaseSkipEvent)}:\n{ex}", LogLevel.Error);
                throw;
            }
        }

        public static bool SkipEvent_ReplaceRecipe_Prefix(Event __instance)
        {
            try
            {
                if (!EventRecipes.CraftingRecipeEvents.ContainsKey(__instance.id) && !EventRecipes.CookingRecipeEvents.ContainsKey(__instance.id))
                {
                    return true; // run original logic
                }
                
                if (OnCheckRecipeLocation(__instance.id))
                {
                    BaseSkipEvent(__instance);
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkipEvent_ReplaceRecipe_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static bool OnCheckRecipeLocation(string eventID)
        {
            if (EventRecipes.CookingRecipeEvents.ContainsKey(eventID))
            {
                var recipeName = EventRecipes.CookingRecipeEvents[eventID];
                var nameWithRecipe = $"{recipeName}{Suffix.CHEFSANITY}";
                if (!_archipelago.LocationExists(nameWithRecipe))
                {
                    return false;
                }

                _locationChecker.AddCheckedLocation(nameWithRecipe);
                if (_archipelago.GetReceivedItemCount(nameWithRecipe) <= 0)
                {
                    Game1.player.cookingRecipes.Remove(recipeName);
                }

                return true;
            }

            if (EventRecipes.CraftingRecipeEvents.ContainsKey(eventID))
            {
                var recipeName = EventRecipes.CraftingRecipeEvents[eventID];
                var nameWithRecipe = $"{recipeName}{Suffix.CRAFTSANITY}";
                if (!_archipelago.LocationExists(nameWithRecipe))
                {
                    return false;
                }

                _locationChecker.AddCheckedLocation(nameWithRecipe);
                if (_archipelago.GetReceivedItemCount(nameWithRecipe) <= 0)
                {
                    Game1.player.craftingRecipes.Remove(recipeName);
                }

                return true;
            }

            return false;
        }

        //public tryEventCommand(GameLocation location, GameTime time, string[] args)
        public static bool TryEventCommand_ReplaceRecipeWithCheck_Prefix(Event __instance, GameLocation location, GameTime time, string[] args)
        {
            try
            {
                string commandName = ArgUtility.Get(args, 0);
                switch (commandName)
                {
                    case "addCraftingRecipe":
                        {
                            if (CheckCraftsanityLocation(__instance.id))
                            {
                                __instance.CurrentCommand++;
                                return false; // don't run original logic
                            }
                            return true; // run original logic
                        }
                    case "addCookingRecipe":
                        {
                            if (CheckChefsanityLocation(__instance.id))
                            {
                                __instance.CurrentCommand++;
                                return false; // don't run original logic
                            }
                            return true; // run original logic
                        }
                }
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(TryEventCommand_ReplaceRecipeWithCheck_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool CheckChefsanityLocation(string id)
        {
            var isEventChefsanityLocation = _archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Friendship) && EventRecipes.CookingRecipeEvents.ContainsKey(id);
            var isRecipeFromQuest = _archipelago.SlotData.QuestLocations.StoryQuestsEnabled && EventRecipes.QuestEventsWithRecipes.Contains(id);
            if (isRecipeFromQuest || !isEventChefsanityLocation)
            {
                return false;
            }

            var locationName = $"{EventRecipes.CookingRecipeEvents[id]}{Suffix.CHEFSANITY}";
            _locationChecker.AddCheckedLocation(locationName);
            return _archipelago.LocationExists(locationName);
        }

        public static bool CheckCraftsanityLocation(string id)
        {
            var isEventCraftsanityLocation = _archipelago.SlotData.Craftsanity == Craftsanity.All && EventRecipes.CraftingRecipeEvents.ContainsKey(id);
            var isRecipeFromQuest = _archipelago.SlotData.QuestLocations.StoryQuestsEnabled && EventRecipes.QuestEventsWithRecipes.Contains(id);
            if (isRecipeFromQuest || !isEventCraftsanityLocation)
            {
                return false;
            }

            var locationName = $"{EventRecipes.CraftingRecipeEvents[id]}{Suffix.CRAFTSANITY}";
            _locationChecker.AddCheckedLocation(locationName);
            return _archipelago.LocationExists(locationName);
        }
    }
}
