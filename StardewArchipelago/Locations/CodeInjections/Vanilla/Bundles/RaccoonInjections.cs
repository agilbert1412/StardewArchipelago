using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using Bundle = StardewValley.Menus.Bundle;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public static class RaccoonInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static LocationChecker _locationChecker;
        private static BundlesManager _bundlesManager;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, ArchipelagoStateDto state,
            LocationChecker locationChecker, BundlesManager bundlesManager)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
            _bundlesManager = bundlesManager;
        }

        // public void activate()
        public static bool Activate_DisplayDialogueOrBundle_Prefix(Raccoon __instance)
        {
            try
            {
                if (__instance.mrs_raccoon.Value)
                {
                    Utility.TryOpenShopMenu(nameof(Raccoon), __instance.Name);
                }
                else
                {
                    var receivedRaccoons = _archipelago.GetReceivedItemCount(APItem.PROGRESSIVE_RACCOON);
                    if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
                    {
                        receivedRaccoons += 1;
                    }

                    var nextRaccoonRequestNumber = GetCurrentRaccoonBundleNumber();
                    var raccoonBundleAvailable = nextRaccoonRequestNumber < receivedRaccoons;

                    // private bool wasTalkedTo;
                    var wasTalkedToField = _modHelper.Reflection.GetField<bool>(__instance, "wasTalkedTo");
                    var wasTalkedTo = wasTalkedToField.GetValue();

                    if (!wasTalkedTo)
                    {
                        var timesFedRaccoons = Game1.netWorldState.Value.TimesFedRaccoons;
                        if (timesFedRaccoons >= 5 && raccoonBundleAvailable)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_intro"));
                        }
                        else if (timesFedRaccoons > 5 & !raccoonBundleAvailable)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_interim"));
                        }
                        else
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_" + (!raccoonBundleAvailable ? "interim_" : "intro_") + timesFedRaccoons));
                        }
                        if (!raccoonBundleAvailable)
                        {
                            return false; // don't run original logic
                        }
                        Game1.afterDialogues = () => ActivateRaccoonBundleMutex(__instance);
                    }
                    else
                    {
                        if (raccoonBundleAvailable)
                        {
                            return false; // don't run original logic
                        }
                        ActivateRaccoonBundleMutex(__instance);
                    }
                }
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Activate_DisplayDialogueOrBundle_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void ActivateRaccoonBundleMutex(Raccoon raccoon)
        {
            raccoon.mutex.RequestLock(() => ActivateMrRaccoon(raccoon), () => Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_busy")));
        }

        // private void _activateMrRaccoon()
        private static void ActivateMrRaccoon(Raccoon raccoon)
        {
            // private bool wasTalkedTo;
            var wasTalkedToField = _modHelper.Reflection.GetField<bool>(raccoon, "wasTalkedTo");
            wasTalkedToField.SetValue(true);

            var currentRaccoonBundleNumber = GetCurrentRaccoonBundleNumber();
            if (currentRaccoonBundleNumber <= 0)
            {
                return;
            }

            if (Game1.netWorldState.Value.SeasonOfCurrentRacconBundle != currentRaccoonBundleNumber)
            {
                Game1.netWorldState.Value.raccoonBundles.Clear();
                Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = currentRaccoonBundleNumber;
            }

            var ingredients = new List<BundleIngredientDescription>();
            var raccoonRequestsRoom = _bundlesManager.BundleRooms.Rooms[APName.RACCOON_REQUESTS_ROOM];
            var currentRaccoonBundleName = $"{APName.RACCOON_REQUEST_PREFIX}{currentRaccoonBundleNumber}";
            var raccoonBundle = (ItemBundle)raccoonRequestsRoom.Bundles[currentRaccoonBundleName];
            for (var i = 0; i < raccoonBundle.Items.Count; i++)
            {
                if (Game1.netWorldState.Value.raccoonBundles.Length <= i)
                {
                    Game1.netWorldState.Value.raccoonBundles.Add(false);
                }
                var bundleItem = raccoonBundle.Items[i];
                var bundleIngredient = new BundleIngredientDescription(bundleItem.StardewObject.GetQualifiedId(),
                    bundleItem.Amount, bundleItem.Quality,
                    Game1.netWorldState.Value.raccoonBundles[i],
                    bundleItem.Flavor.Id);
                ingredients.Add(bundleIngredient);
            }

            var whichBundle = (currentRaccoonBundleNumber - 1) % 5;
            var raccoonNoteMenu = new JunimoNoteMenu(new Bundle("Seafood", null, ingredients, new bool[1])
            {
                bundleTextureOverride = Game1.content.Load<Texture2D>("LooseSprites\\BundleSprites"),
                bundleTextureIndexOverride = 14 + whichBundle
            }, "LooseSprites\\raccoon_bundle_menu");
            raccoonNoteMenu.onIngredientDeposit = x => Game1.netWorldState.Value.raccoonBundles[x] = true;
            raccoonNoteMenu.onBundleComplete = _ => BundleComplete(raccoon);
            raccoonNoteMenu.onScreenSwipeFinished = _ => BundleCompleteAfterSwipe(raccoon);
            raccoonNoteMenu.behaviorBeforeCleanup = _ => raccoon.mutex?.ReleaseLock();
            Game1.activeClickableMenu = raccoonNoteMenu;
        }

        // private void bundleComplete(JunimoNoteMenu menu)
        private static void BundleComplete(Raccoon raccoon)
        {
            JunimoNoteMenu.screenSwipe = new ScreenSwipe(1);
            _locationChecker.AddCheckedLocation($"{APName.RACCOON_REQUEST_PREFIX}{Game1.netWorldState.Value.SeasonOfCurrentRacconBundle}");
            Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = -1;
            Game1.netWorldState.Value.raccoonBundles.Clear();

            // private bool wasTalkedTo;
            var wasTalkedToField = _modHelper.Reflection.GetField<bool>(raccoon, "wasTalkedTo");
            wasTalkedToField.SetValue(false);
        }

        // private void bundleCompleteAfterSwipe(JunimoNoteMenu menu)
        private static void BundleCompleteAfterSwipe(Raccoon raccoon)
        {
            Game1.activeClickableMenu = null;
            raccoon.mutex?.ReleaseLock();
            Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.Date.TotalDays;
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_receive"));
        }

        private static int GetCurrentRaccoonBundleNumber()
        {
            var remainingRacoonBundleNumbers = _locationChecker.GetAllMissingLocationNames()
                .Where(x => x.StartsWith(APName.RACCOON_REQUEST_PREFIX))
                .Select(x => int.Parse(x[APName.RACCOON_REQUEST_PREFIX.Length..]));
            if (remainingRacoonBundleNumbers.Any())
            {
                return remainingRacoonBundleNumbers.Min();
            }

            return -1;
        }
    }
}
