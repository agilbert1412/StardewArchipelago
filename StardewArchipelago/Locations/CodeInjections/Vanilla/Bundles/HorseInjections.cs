using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class HorseInjections
    {
        private const string POMNUT_FILE = "pomnut.wav";
        private const string POMNUT_CUE = "pomnut";

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;


        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            //var currentModFolder = _modHelper.DirectoryPath;
            //var soundsFolder = "Sounds";
            //var relativePathToSound = Path.Combine(currentModFolder, soundsFolder, HONEYWELL_FILE);
            //var honeywellCueDefinition = new CueDefinition(HONEYWELL_CUE, SoundEffect.FromFile(relativePathToSound), 0);
            //Game1.soundBank.AddCue(honeywellCueDefinition);
        }

        // public override bool checkAction(Farmer who, GameLocation l)
        public static bool CheckAction_FeedPomnutItems_Prefix(Horse __instance, Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                if (who == null || !who.canMove || __instance.rider != null || who.mount != null || who.FarmerSprite.PauseForSingleAnimation || __instance.currentLocation != who.currentLocation)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // private int munchingCarrotTimer;
                var munchingCarrotTimerField = _modHelper.Reflection.GetField<int>(__instance, "munchingCarrotTimer");
                var munchingCarrotTimer = munchingCarrotTimerField.GetValue();

                if (munchingCarrotTimer > 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }


                var activeObject = who.ActiveObject;
                var validIds = new[] { QualifiedItemIds.POMEGRANATE, QualifiedItemIds.HAZELNUT, QualifiedItemIds.CARROT };
                if (activeObject == null || !validIds.Contains(activeObject.QualifiedItemId) || __instance.ateCarrotToday)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __instance.mutex.RequestLock(() =>
                {
                    __instance.Sprite.StopAnimation();
                    __instance.Sprite.faceDirection(__instance.FacingDirection);
                    Game1.playSound("eat");
                    DelayedAction.playSoundAfterDelay("eat", 600);
                    DelayedAction.playSoundAfterDelay("eat", 1200);
                    munchingCarrotTimerField.SetValue(1500);
                    __instance.doEmote(20, 32);
                    who.reduceActiveItemByOne();
                    __instance.ateCarrotToday = true;
                    ArchipelagoJunimoNoteMenu.TryDonateToBundle(MemeBundleNames.POMNUT, activeObject.Name, 1);
                    __instance.mutex.ReleaseLock();
                    return;
                });

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_FeedPomnutItems_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
