using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Goals;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Logging;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class EatInjections
    {
        public const string EAT_PREFIX = "Eat ";
        public const string DRINK_PREFIX = "Drink ";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static NameSimplifier _nameSimplifier;
        private static CompoundNameMapper _nameMapper;

        private static BuffEffects _maxBuffEffects;
        private static int _lastDayCheckedBuffEffects;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, NameSimplifier nameSimplifier)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _nameSimplifier = nameSimplifier;
            _nameMapper = new CompoundNameMapper(archipelago.SlotData);
            _lastDayCheckedBuffEffects = -1;
        }

        // public void doneEating()
        public static bool DoneEating_EatingPatches_Prefix(Farmer __instance)
        {
            try
            {
                DoOriginalDoneEating(__instance);
                FarmerInjections.DoneEatingFavoriteThingKaito(__instance);
                DifficultSecretsInjections.DoneEatingStardropSecret(__instance);
                DoneEatingEatsanityLocation(__instance);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneEating_EatingPatches_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void DoOriginalDoneEating(Farmer farmer)
        {
            farmer.isEating = false;
            farmer.tempFoodItemTextureName.Value = null;
            farmer.completelyStopAnimatingOrDoingAction();
            farmer.forceCanMove();
            if (farmer.mostRecentlyGrabbedItem == null || !farmer.IsLocalPlayer)
            {
                return;
            }
            var itemToEat = farmer.itemToEat as Object;
            if (itemToEat.QualifiedItemId == "(O)434")
            {
                DoneEatingStardrop(farmer);
            }
            else
            {
                if (itemToEat.HasContextTag("ginger_item"))
                {
                    farmer.buffs.Remove("25");
                }
                foreach (var foodOrDrinkBuff in itemToEat.GetFoodOrDrinkBuffs())
                    farmer.applyBuff(foodOrDrinkBuff);
                switch (itemToEat.QualifiedItemId)
                {
                    //case "(O)773":
                    //    farmer.health = farmer.maxHealth;
                    //    break;
                    case "(O)351":
                        farmer.exhausted.Value = false;
                        break;
                    //case "(O)349":
                    //    farmer.Stamina = (float)farmer.MaxStamina;
                    //    break;
                }
                var stamina = farmer.Stamina;
                var health = farmer.health;
                var num3 = itemToEat.staminaRecoveredOnConsumption();
                var num4 = itemToEat.healthRecoveredOnConsumption();
                if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && farmer.currentLocation is MineShaft && Game1.mine.getMineArea() == 121 && farmer.team.calicoStatueEffects.ContainsKey(6))
                {
                    num3 = Math.Max(1, num3 / 2);
                    num4 = Math.Max(1, num4 / 2);
                }
                farmer.Stamina = Math.Min(farmer.MaxStamina, farmer.Stamina + num3);
                farmer.health = Math.Min(farmer.maxHealth, farmer.health + num4);
                if (stamina < (double)farmer.Stamina)
                {
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3116", (int)(farmer.Stamina - (double)stamina)), 4));
                }
                if (health < farmer.health)
                {
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3118", farmer.health - health), 5));
                }
            }
            if (itemToEat == null || itemToEat.Edibility >= 0)
            {
                return;
            }

            farmer.CanMove = false;

            // private readonly NetEvent0 sickAnimationEvent;
            var sickAnimationEventField = _modHelper.Reflection.GetField<NetEvent0>(farmer, "sickAnimationEvent");
            sickAnimationEventField.GetValue().Fire();
        }

        private static void DoneEatingStardrop(Farmer farmer)
        {
            Game1.stats.checkForStardropAchievement(true);
            farmer.yOffset = 0.0f;
            farmer.yJumpOffset = 0;
            Game1.changeMusicTrack("none");
            Game1.playSound("stardrop");
            var str = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs." + Game1.random.Choose("3094", "3095"));
            if (farmer.favoriteThing.Value != null)
            {
                str = !farmer.favoriteThing.Value.Contains("Stardew") ? (!farmer.favoriteThing.Equals("ConcernedApe") ? str + farmer.favoriteThing.Value : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3099")) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3097");
            }
            DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3100") + str + Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3101"), 6000);
            farmer.maxStamina.Value += 34;
            farmer.stamina = farmer.MaxStamina;
            farmer.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
            {
                new(57, 6000),
            });
            farmer.startGlowing(new Color(200, 0, byte.MaxValue), false, 0.1f);
            farmer.jitterStrength = 1f;
            Game1.staminaShakeTimer = 12000;
            Game1.screenGlowOnce(new Color(200, 0, byte.MaxValue), true);
            farmer.CanMove = false;
            farmer.freezePause = 8000;
            farmer.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(368, 16, 16, 16), 60f, 8, 40, farmer.Position + new Vector2(-8f, sbyte.MinValue), false, false, 1f, 0.0f, Color.White, 4f, 0.0075f, 0.0f, 0.0f)
            {
                alpha = 0.75f,
                alphaFade = 1f / 400f,
                motion = new Vector2(0.0f, -0.25f),
            });
            if (Game1.displayHUD && !Game1.eventUp)
            {
                for (var index = 0; index < 40; ++index)
                {
                    var overlayTempSprites = Game1.uiOverlayTempSprites;
                    var rowInAnimationTexture = Game1.random.Next(10, 12);
                    var titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea;
                    var x = titleSafeArea.Right / (double)Game1.options.uiScale - 48.0 - 8.0 - Game1.random.Next(64);
                    var num1 = (double)Game1.random.Next(-64, 64);
                    titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea;
                    var num2 = titleSafeArea.Bottom / (double)Game1.options.uiScale;
                    var y = num1 + num2 - 224.0 - 16.0 - (int)((farmer.MaxStamina - 270) * 0.715);
                    var position = new Vector2((float)x, (float)y);
                    var color = Game1.random.Choose(Color.White, Color.Lime);
                    overlayTempSprites.Add(new TemporaryAnimatedSprite(rowInAnimationTexture, position, color, animationInterval: 50f)
                    {
                        layerDepth = 1f,
                        delayBeforeAnimationStart = 200 * index,
                        interval = 100f,
                        local = true,
                    });
                }
            }
            var tilePoint = farmer.TilePoint;
            Utility.addSprinklesToLocation(farmer.currentLocation, tilePoint.X, tilePoint.Y, 9, 9, 6000, 100, new Color(200, 0, byte.MaxValue), motionTowardCenter: true);
            DelayedAction.stopFarmerGlowing(6000);
            Utility.addSprinklesToLocation(farmer.currentLocation, tilePoint.X, tilePoint.Y, 9, 9, 6000, 300, Color.Cyan, motionTowardCenter: true);
            farmer.mostRecentlyGrabbedItem = null;
        }

        private static void DoneEatingEatsanityLocation(Farmer farmer)
        {
            if (_archipelago.SlotData.Eatsanity == Eatsanity.None)
            {
                return;
            }

            var eatenItem = farmer.itemToEat as Object;
            if (!Game1.objectData.ContainsKey(eatenItem.ItemId))
            {
                return;
            }

            var objectData = Game1.objectData[eatenItem.ItemId];

            var name = _nameSimplifier.GetSimplifiedName(eatenItem);
            name = _nameMapper.GetEnglishName(name); // For the Name vs Display Name discrepencies in Mods.

            var apLocation = objectData.IsDrink ? $"{DRINK_PREFIX}{name}" : $"{EAT_PREFIX}{name}";
            if (_archipelago.GetLocationId(apLocation) > -1)
            {
                _locationChecker.AddCheckedLocation(apLocation);
            }
            else
            {
                _logger.LogError($"Unrecognized Eatsanity Location: {apLocation} ({name} [{eatenItem.QualifiedItemId}])");
            }

            GoalCodeInjection.CheckUltimateFoodieGoalCompletion();
        }

        // public override int staminaRecoveredOnConsumption()
        public static void StaminaRecoveredOnConsumption_LimitToEnzymes_Postfix(Object __instance, ref int __result)
        {
            try
            {
                if (__result < 0)
                {
                    // Penalties are not blocked!
                    return;
                }

                var numberEnzymes = _archipelago.GetReceivedItemCount("Stamina Enzyme");
                if (numberEnzymes <= 0)
                {
                    // No enzyme, no stamina!
                    __result = 0;
                    return;
                }
                
                const double enzymeThresholdForBonus = 5;
                const double enzymeMultiplier = 1.15;
                const double enzymeCapMultiplier = 1.5;
                const double enzymeCapStartingFactor = 5;

                /*
                 * Multiplier (Approximately)
                 * 0 -> 0.5
                 * 1 -> 0.57
                 * 2 -> 0.65
                 * 3 -> 0.75
                 * 4 -> 0.85
                 * 5 -> 1
                 * 6 -> 1.15
                 * 7 -> 1.3
                 * 8 -> 1.5
                 * 9 -> 1.75
                 * 10 -> 2
                 */

                __result = (int)Math.Round(__result * Math.Pow(enzymeMultiplier, numberEnzymes - enzymeThresholdForBonus));

                if (numberEnzymes >= 10)
                {
                    // Max enzymes, max stamina!
                    return;
                }

                /*
                 * Cap (Approximately)
                 * 0 -> 16 (No stamina)
                 * 1 -> 22
                 * 2 -> 34
                 * 3 -> 52
                 * 4 -> 76
                 * 5 -> 116
                 * 6 -> 172
                 * 7 -> 260
                 * 8 -> 390
                 * 9 -> 586
                 * 10 -> 876 (uncapped)
                 */

                var cap = (int)Math.Round(2 * Math.Pow(enzymeCapMultiplier, numberEnzymes + enzymeCapStartingFactor));
                __result = Math.Min(cap, __result);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StaminaRecoveredOnConsumption_LimitToEnzymes_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override int healthRecoveredOnConsumption()
        public static void HealthRecoveredOnConsumption_LimitToEnzymes_Postfix(Object __instance, ref int __result)
        {
            try
            {
                if (__result < 0)
                {
                    // Penalties are not blocked!
                    return;
                }

                var numberEnzymes = _archipelago.GetReceivedItemCount("Health Enzyme");
                if (numberEnzymes <= 0)
                {
                    // No enzyme, no health!
                    __result = 0;
                    return;
                }

                __result = BaseHealthValue(__instance);

                const double enzymeThresholdForBonus = 5;
                const double enzymeMultiplier = 1.15;
                const double enzymeCapMultiplier = 1.5;
                const double enzymeCapStartingFactor = 5;

                /*
                 * Multiplier (Approximately)
                 * 0 -> 0.5
                 * 1 -> 0.57
                 * 2 -> 0.65
                 * 3 -> 0.75
                 * 4 -> 0.85
                 * 5 -> 1
                 * 6 -> 1.15
                 * 7 -> 1.3
                 * 8 -> 1.5
                 * 9 -> 1.75
                 * 10 -> 2
                 */

                __result = (int)Math.Round(__result * Math.Pow(enzymeMultiplier, numberEnzymes - enzymeThresholdForBonus));

                if (numberEnzymes >= 10)
                {
                    // Max enzymes, max health!
                    return;
                }

                /*
                 * Cap (Approximately)
                 * 0 -> 8 (No health)
                 * 1 -> 11
                 * 2 -> 17
                 * 3 -> 26
                 * 4 -> 38
                 * 5 -> 58
                 * 6 -> 86
                 * 7 -> 130
                 * 8 -> 195
                 * 9 -> 292
                 * 10 -> 438 (uncapped)
                 */

                var cap = (int)Math.Round(Math.Pow(enzymeCapMultiplier, numberEnzymes + enzymeCapStartingFactor));
                __result = Math.Min(cap, __result);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HealthRecoveredOnConsumption_LimitToEnzymes_Postfix)}:\n{ex}");
                return;
            }
        }

        private static int BaseHealthValue(Object item)
        {
            if (item.Edibility < 0)
            {
                return 0;
            }

            var normalValue = (int)Math.Ceiling(item.Edibility * 2.5) + item.Quality * item.Edibility;
            switch (item.QualifiedItemId)
            {
                case "(O)434":
                case "(O)349":
                    return 0;
                case $"(O){ObjectIds.LIFE_ELIXIR}":
                    return 999;

                case "(O)874":
                    return (int)(normalValue * 0.68000000715255737);
                default:
                    return (int)(normalValue * 0.44999998807907104);
            }
        }

        // public static IEnumerable<Buff> TryCreateBuffsFromData(ObjectData obj, string name, string displayName, float durationMultiplier = 1f, Action<BuffEffects> adjustEffects = null)
        public static void TryCreateBuffsFromData_LimitToEnzymes_Postfix(ObjectData obj, string name, string displayName, float durationMultiplier, Action<BuffEffects> adjustEffects, ref IEnumerable<Buff> __result)
        {
            try
            {
                var defaultReturnedValues = __result.ToArray();
                var newReturnedValues = new List<Buff>();
                foreach (var defaultBuff in defaultReturnedValues)
                {
                    if (TryGetAllowedBuff(defaultBuff, out var allowedBuff))
                    {
                        newReturnedValues.Add(allowedBuff);
                    }
                }

                __result = newReturnedValues;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryCreateBuffsFromData_LimitToEnzymes_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool TryGetAllowedBuff(Buff defaultBuff, out Buff buff)
        {
            switch (defaultBuff.id)
            {
                case BuffIds.SQUID_INK_RAVIOLI:
                    buff = defaultBuff;
                    return _archipelago.HasReceivedItem("Squid Ink Enzyme");
                case BuffIds.MONSTER_MUSK:
                    buff = defaultBuff;
                    return _archipelago.HasReceivedItem("Monster Musk Enzyme");
                case BuffIds.OIL_OF_GARLIC:
                    buff = defaultBuff;
                    return _archipelago.HasReceivedItem("Oil Of Garlic Enzyme");
                case BuffIds.TIPSY:
                    buff = defaultBuff;
                    return _archipelago.HasReceivedItem("Tipsy Enzyme");
            }

            var maxBuffEffects = GetMaxBuffEffects();
            var defaultEffects = defaultBuff.effects;
            var newBuffEffects = new BuffEffects();
            var newBuffData = new BuffAttributesData();

            newBuffData.Attack = Math.Min(maxBuffEffects.Attack.Value, defaultEffects.Attack.Value);
            newBuffData.Speed = Math.Min(maxBuffEffects.Speed.Value, defaultEffects.Speed.Value);
            newBuffData.LuckLevel = Math.Min(maxBuffEffects.LuckLevel.Value, defaultEffects.LuckLevel.Value);
            newBuffData.FarmingLevel = Math.Min(maxBuffEffects.FarmingLevel.Value, defaultEffects.FarmingLevel.Value);
            newBuffData.ForagingLevel = Math.Min(maxBuffEffects.ForagingLevel.Value, defaultEffects.ForagingLevel.Value);
            newBuffData.FishingLevel = Math.Min(maxBuffEffects.FishingLevel.Value, defaultEffects.FishingLevel.Value);
            newBuffData.MiningLevel = Math.Min(maxBuffEffects.MiningLevel.Value, defaultEffects.MiningLevel.Value);
            newBuffData.MagneticRadius = Math.Min(maxBuffEffects.MagneticRadius.Value, defaultEffects.MagneticRadius.Value);
            newBuffData.Defense = Math.Min(maxBuffEffects.Defense.Value, defaultEffects.Defense.Value);
            newBuffData.MaxStamina = Math.Min(maxBuffEffects.MaxStamina.Value, defaultEffects.MaxStamina.Value);

            buff = new Buff(defaultBuff.id, defaultBuff.source, defaultBuff.displaySource, defaultBuff.totalMillisecondsDuration, defaultBuff.iconTexture, defaultBuff.iconSheetIndex, newBuffEffects);
            buff.effects.Clear();
            buff.effects.Add(newBuffData);
            return true;
        }

        private static BuffEffects GetMaxBuffEffects()
        {
            if (_lastDayCheckedBuffEffects == Game1.stats.DaysPlayed)
            {
                return _maxBuffEffects;
            }

            var newBuffData = new BuffAttributesData();

            newBuffData.Speed = _archipelago.GetReceivedItemCount("Speed Enzyme");
            newBuffData.LuckLevel = _archipelago.GetReceivedItemCount("Luck Enzyme");
            newBuffData.FarmingLevel = _archipelago.GetReceivedItemCount("Farming Enzyme");
            newBuffData.ForagingLevel = _archipelago.GetReceivedItemCount("Foraging Enzyme");
            newBuffData.FishingLevel = _archipelago.GetReceivedItemCount("Fishing Enzyme");
            newBuffData.MiningLevel = _archipelago.GetReceivedItemCount("Mining Enzyme");
            newBuffData.MagneticRadius = _archipelago.GetReceivedItemCount("Magnetism Enzyme") * 32;
            newBuffData.Defense = _archipelago.GetReceivedItemCount("Defense Enzyme");
            newBuffData.Attack = _archipelago.GetReceivedItemCount("Attack Enzyme");
            newBuffData.MaxStamina = _archipelago.GetReceivedItemCount("Max Stamina Enzyme") * 20;

            return new BuffEffects(newBuffData);
        }

        /*
           items.extend(item_factory(item) for item in ["Speed Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Luck Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Farming Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Foraging Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Fishing Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Mining Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Magnetism Enzyme"]*2)
           items.extend(item_factory(item) for item in ["Defense Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Attack Enzyme"]*5)
           items.extend(item_factory(item) for item in ["Max Stamina Enzyme"]*3)

           items.extend(item_factory(item) for item in ["Squid Ink Enzyme"])
           items.extend(item_factory(item) for item in ["Monster Musk Enzyme"])
           items.extend(item_factory(item) for item in ["Oil Of Garlic Enzyme"])
           items.extend(item_factory(item) for item in ["Tipsy Enzyme"])*/
    }
}
