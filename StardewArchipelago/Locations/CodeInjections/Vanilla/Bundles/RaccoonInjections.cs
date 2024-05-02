using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private const string PROGRESSIVE_RACCOON = "Progressive Raccoon";
        private const string RACCOON_REQUEST = "Raccoon Request ";

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
                    var receivedRaccoons = _archipelago.GetReceivedItemCount(PROGRESSIVE_RACCOON);
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

            var currentRaccoonBundle = GetCurrentRaccoonBundleNumber();
            if (currentRaccoonBundle <= 0)
            {
                return;
            }

            var ingredients = new List<BundleIngredientDescription>();
            //var raccoonBundle = _bundlesManager
            //ingredients.Add(new BundleIngredientDescription("DriedFruit", 1, 0, Game1.netWorldState.Value.raccoonBundles[0], r.ChooseFrom<string>((IList<string>)strArray1[currentRacconBundle])));

            //var whichBundle = timesFedRaccoons < 5 ? timesFedRaccoons % 5 : random.Next(5);
            //this.addNextIngredient(ingredients, whichBundle, random);
            //this.addNextIngredient(ingredients, whichBundle, random);
            //this.addNextIngredient(ingredients, whichBundle, random);
            //var junimoNoteMenu = new JunimoNoteMenu(new Bundle("Seafood", (string)null, ingredients, new bool[1])
            //{
            //    bundleTextureOverride = Game1.content.Load<Texture2D>("LooseSprites\\BundleSprites"),
            //    bundleTextureIndexOverride = 14 + whichBundle
            //}, "LooseSprites\\raccoon_bundle_menu");
            //junimoNoteMenu.onIngredientDeposit = (Action<int>)(x => Game1.netWorldState.Value.raccoonBundles[x] = true);
            //junimoNoteMenu.onBundleComplete = new Action<JunimoNoteMenu>(this.bundleComplete);
            //junimoNoteMenu.onScreenSwipeFinished = new Action<JunimoNoteMenu>(this.bundleCompleteAfterSwipe);
            //junimoNoteMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)(x => this.mutex?.ReleaseLock());
            //Game1.activeClickableMenu = (IClickableMenu)junimoNoteMenu;
        }

        private static int GetCurrentRaccoonBundleNumber()
        {
            var remainingRacoonBundleNumbers = _locationChecker.GetAllMissingLocationNames()
                .Where(x => x.StartsWith(RACCOON_REQUEST))
                .Select(x => int.Parse(x[RACCOON_REQUEST.Length..]));
            if (remainingRacoonBundleNumbers.Any())
            {
                return remainingRacoonBundleNumbers.Min();
            }

            return -1;
        }

        // private void bundleComplete(JunimoNoteMenu menu)
        // private void bundleCompleteAfterSwipe(JunimoNoteMenu menu)
    }
}
