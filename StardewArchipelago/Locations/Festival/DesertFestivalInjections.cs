using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.Festival
{
    internal class DesertFestivalInjections
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

        // public virtual void CollectRacePrizes()
        public static bool CollectRacePrizes_RaceWinner_Prefix(DesertFestival __instance)
        {
            try
            {
                var rewards = new List<Item>();
                if (__instance.specialRewardsCollected.ContainsKey(Game1.player.UniqueMultiplayerID) && !__instance.specialRewardsCollected[Game1.player.UniqueMultiplayerID])
                {
                    __instance.specialRewardsCollected[Game1.player.UniqueMultiplayerID] = true;
                    // No 100 eggs reward
                    // rewards.Add(ItemRegistry.Create("CalicoEgg", 100));
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.CALICO_RACE);

                for (var index = 0; index < __instance.rewardsToCollect[Game1.player.UniqueMultiplayerID]; ++index)
                {
                    rewards.Add(ItemRegistry.Create("CalicoEgg", 20));
                }
                __instance.rewardsToCollect[Game1.player.UniqueMultiplayerID] = 0;
                Game1.activeClickableMenu = new ItemGrabMenu(rewards, false, true, null, null, "Rewards", canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, context: __instance);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CollectRacePrizes_RaceWinner_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public virtual void ReceiveMakeOver(int randomSeedOverride = -1)
        public static void ReceiveMakeOver_RaceWinner_Postfix(DesertFestival __instance, int randomSeedOverride)
        {
            try
            {
                _locationChecker.AddCheckedLocation(FestivalLocationNames.EMILYS_OUTFIT_SERVICES);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ReceiveMakeOver_RaceWinner_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }
    }
}
