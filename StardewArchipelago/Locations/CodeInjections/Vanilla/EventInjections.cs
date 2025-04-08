using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Locations;
using StardewModdingAPI;
using StardewValley;
using EventRecipes = StardewArchipelago.Constants.EventRecipes;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class EventInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
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

                DoEndBehaviors(eventBeingSkipped);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BaseSkipEvent)}:\n{ex}");
                throw;
            }
        }

        public static bool SkipEvent_ReplaceRecipe_Prefix(Event __instance)
        {
            try
            {
                if (!EventRecipes.CraftingRecipeEvents.ContainsKey(__instance.id) && !EventRecipes.CookingRecipeEvents.ContainsKey(__instance.id))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (OnCheckRecipeLocation(__instance.id))
                {
                    BaseSkipEvent(__instance);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SkipEvent_ReplaceRecipe_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
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
                var commandName = ArgUtility.Get(args, 0);
                switch (commandName)
                {
                    case "addCraftingRecipe":
                    {
                        if (CheckCraftsanityLocation(__instance.id))
                        {
                            __instance.CurrentCommand++;
                            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                        }
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }
                    case "addCookingRecipe":
                    {
                        if (CheckChefsanityLocation(__instance.id))
                        {
                            __instance.CurrentCommand++;
                            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                        }
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryEventCommand_ReplaceRecipeWithCheck_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
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

        private static void DoEndBehaviors(Event eventBeingSkipped)
        {
            switch (eventBeingSkipped.id)
            {
                case "-157039427":
                    eventBeingSkipped.endBehaviors(new[] { "End", "islandDepart" }, Game1.currentLocation);
                    break;
                case "-78765":
                    eventBeingSkipped.endBehaviors(new[] { "End", "tunnelDepart" }, Game1.currentLocation);
                    break;
                case EventIds.COMMUNITY_CENTER_COMPLETE:
                    eventBeingSkipped.endBehaviors(new[] { "End", "position", "52", "20" }, Game1.currentLocation);
                    break;
                case "2123343":
                    eventBeingSkipped.endBehaviors(new[] { "End", "newDay" }, Game1.currentLocation);
                    break;
                case "558292":
                    eventBeingSkipped.endBehaviors(new[] { "End", "bed" }, Game1.currentLocation);
                    break;
                case "60367":
                    eventBeingSkipped.endBehaviors(new[] { "End", "beginGame" }, Game1.currentLocation);
                    break;
                case "6497428":
                    eventBeingSkipped.endBehaviors(new[] { "End", "Leo" }, Game1.currentLocation);
                    break;
                case EventIds.BAMBOO_POLE:
                    eventBeingSkipped.endBehaviors(new[] { "End", "position", "43", "36" }, Game1.currentLocation);
                    break;
                default:
                    eventBeingSkipped.endBehaviors();
                    break;
            }
        }
    }
}
