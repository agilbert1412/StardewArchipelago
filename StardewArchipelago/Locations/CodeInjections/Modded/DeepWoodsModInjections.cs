using System;
using System.Reflection;
using Netcode;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public static class DeepWoodsModInjections
    {
        //private const string EXCALIBUR_AP_LOCATION = "Pull Excalibur From the Stone";
        private const string MEET_UNICORN_AP_LOCATION = "Pet the Deep Woods Unicorn";
        private const string DESTROY_HOUSE_AP_LOCATION = "Breaking Up Deep Woods Gingerbread House";
        private const string DESTROY_TREE_AP_LOCATION = "Chop Down a Deep Woods Iridium Tree";
        private const string WOODS_OBELISK_PROGRESSION = "Progressive Woods Obelisk";
        private const string FOUNTAIN_DRINK_LOCATION = "Drinking From Deep Woods Fountain";
        private const string TREASURE1_AP_LOCATION = "Deep Woods Trash Bin";
        private const string TREASURE2_AP_LOCATION = "Deep Woods Treasure Chest";
        private static int previousDepth = 0;
        private static NetBool stardropGet = new NetBool(false);
        private static NetBool isPetted = new NetBool(false);
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;


        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }
        // Future Goal; Currently Unimplemented - Albrekka
        /*public static bool PerformUseAction_ExcaliburLocation_Prefix(Vector2 tileLocation, GameLocation location, bool __result)
        {
            try
            {
                var swordPulledOutType = AccessTools.TypeByName("DeepWoodsMod.Unicorn");
                var swordPulledOutField = _helper.Reflection.GetField<NetBool>(swordPulledOutType, "DeepWoodsMod.ExcaliburStone:swordPulledOut");
                var swordPulledOut = swordPulledOutField.GetValue();
                if (swordPulledOut.Value)
                    return false; //don't run original logic

                if (Game1.player.DailyLuck >= 0.07f
                    && Game1.player.LuckLevel >= 8
                    && Game1.player.MiningLevel >= 10
                    && Game1.player.ForagingLevel >= 10
                    && Game1.player.FishingLevel >= 10
                    && Game1.player.FarmingLevel >= 10
                    && Game1.player.CombatLevel >= 10
                    && (Game1.player.timesReachedMineBottom >= 1 || Game1.MasterPlayer.timesReachedMineBottom >= 1)
                    && Game1.getFarm().grandpaScore.Value >= 4
                    && (!Game1.player.mailReceived.Contains("JojaMember"))
                    && (Game1.player.hasCompletedCommunityCenter()))
                {
                    Game1.playSound("yoba");
                    _locationChecker.AddCheckedLocation(EXCALIBUR_AP_LOCATION);
                    swordPulledOut.Value = true;
                }
                else
                {
                    Game1.playSound("thudStep");
                    Game1.showRedMessage("It won't budge.");
                }
                return false; //don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformUseAction_ExcaliburLocation_Prefix)}:\n{ex}", LogLevel.Error);
                return true; //run original logic
            }
        }*/
        //It makes the chime if you pet after reload, but not really a problem.  Also patches out being scared
        //since its highly likely by the time you check unicorn, you'll be moving too fast naturally from buffs. - Albrekka
        public static bool CheckAction_PetUnicornLocation_Prefix(Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                var farmer = Game1.player;
                var unicornType = AccessTools.TypeByName("DeepWoodsMod.Unicorn");
                if (!isPetted)
                {
                    isPetted.Value = true;
                    farmer.farmerPassesThrough = true;
                    who.health = who.maxHealth;
                    who.Stamina = who.MaxStamina;
                    who.addedLuckLevel.Value = Math.Max(10, who.addedLuckLevel.Value);
                    if (!_locationChecker.IsLocationChecked(MEET_UNICORN_AP_LOCATION))
                    {
                        _locationChecker.AddCheckedLocation(MEET_UNICORN_AP_LOCATION);
                    }
                    Game1.playSound("achievement");
                    Game1.playSound("healSound");
                    Game1.playSound("reward");
                    Game1.playSound("secret1");
                    Game1.playSound("shiny4");
                    Game1.playSound("yoba");
                }
                return false; // don't use original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_PetUnicornLocation_Prefix)}:\n{ex}", LogLevel.Error);
                return true; //run original logic
            }
        }
        public static bool CheckScared_MakeUnicornLessScared_Prefix()
        {
            try
            {
                return false;  //don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckScared_MakeUnicornLessScared_Prefix)}:\n{ex}", LogLevel.Error);
                return true; //run original logic
            }
        }
        //Likely fixable if I knew how to utilize the DeepWoods type call in the original method, perhaps better if
        //it was the 'elevator' call.  Currently turned off. - Albrekka
        public static void DetermineExits_ChangeFromLevelHook_Postfix()
        {
            try
            {
                var deepWoodsSettingsType = AccessTools.TypeByName("DeepWoodsMod.DeepWoodsSettings");
                var deepWoodsDataField = deepWoodsSettingsType.GetProperty("DeepWoodsState");
                var deepWoodsDataType = AccessTools.TypeByName("DeepWoodsMod.DeepWoodsStateData");
                var deepWoodsType = AccessTools.TypeByName("DeepWoodsMod.DeepWoods");
                var lowestLevelReachedField = _helper.Reflection.GetProperty<int>(deepWoodsDataField, "DeepWoodsState.LowestLevelReached");
                var lowestLevelReached = lowestLevelReachedField.GetValue();
                var levelField = _helper.Reflection.GetField<NetInt>(deepWoodsType, "Level");
                var level = levelField.GetValue();
                var progression = _archipelago.SlotData.ElevatorProgression;
                if (level.Value == 1)
                    _locationChecker.AddCheckedLocation($"Entering the Deep Woods");
                if (progression == ElevatorProgression.ProgressiveFromPreviousFloor && level.Value != previousDepth + 1)
                {
                    return;
                }
                previousDepth = level.Value - 1;
                _locationChecker.AddCheckedLocation($"Deep Woods Depth {level}");
                var totalFloorsReceived = _archipelago.GetReceivedItemCount("Progressive Woods Obeliesk");
                switch (totalFloorsReceived)
                {
                    case < 1:
                        lowestLevelReached = 1;
                        return;
                    case < 3:
                        lowestLevelReached = 10;
                        return;
                    case < 4:
                        lowestLevelReached = 30;
                        return;
                    case < 5:
                        lowestLevelReached = 50;
                        return;
                    case < 6:
                        lowestLevelReached = 70;
                        return;
                    case < 7:
                        lowestLevelReached = 90;
                        return;
                    case >= 8:
                        lowestLevelReached = 100;
                        return;
                }
                lowestLevelReached = 0;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DetermineExits_ChangeFromLevelHook_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
        //Later, should also make it so the spawned drops for chest and also gingerbread + tree
        ///don't show up first time, but don't know how to just yet due to DeepWoods type reference in method call. - Albrekka
        public static void CheckForAction_TreasureChestLocation_Postfix(Farmer __instance, bool justCheckingForActivity = false)
        {
            try
            {
                if (justCheckingForActivity)
                    return;
                if (__instance.displayName != "Chest")
                {
                    _locationChecker.AddCheckedLocation(TREASURE1_AP_LOCATION);
                    return;
                }
                _locationChecker.AddCheckedLocation(TREASURE2_AP_LOCATION);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_TreasureChestLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
        public static void PlayDestroyedSounds_GingerbreadLocation_Postfix(GameLocation __instance)
        {
            try
            {
                if (!_locationChecker.IsLocationChecked(DESTROY_HOUSE_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(DESTROY_HOUSE_AP_LOCATION);
                    return;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PlayDestroyedSounds_GingerbreadLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
        public static void PlayDestroyedSounds_IridiumTreeLocation_Postfix(GameLocation __instance)
        {
            try
            {
                if (!_locationChecker.IsLocationChecked(DESTROY_TREE_AP_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(DESTROY_TREE_AP_LOCATION);
                    return;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PlayDestroyedSounds_IridiumTreeLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
        public static bool PerformUseAction_HealingFountainLocation_Prefix(Vector2 tileLocation, GameLocation location, ref bool __result)
        {
            try
            {
                if (!_locationChecker.IsLocationChecked(FOUNTAIN_DRINK_LOCATION))
                {
                    _locationChecker.AddCheckedLocation(FOUNTAIN_DRINK_LOCATION);
                }

                return true; //run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformUseAction_HealingFountainLocation_Prefix)}:\n{ex}", LogLevel.Error);
                return true; //run original logic
            }
        }
    }
}