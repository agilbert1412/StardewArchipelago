using System;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using xTile.Dimensions;
using Microsoft.Xna.Framework;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework.Graphics;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class PhoneInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static WeaponsManager _weaponsManager;
        private static EntranceManager _entranceManager;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, WeaponsManager weaponsManager, EntranceManager entranceManager)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _weaponsManager = weaponsManager;
            _entranceManager = entranceManager;
        }

        // public void CallAdventureGuild()
        public static bool CallAdventureGuild_AllowRecovery_Prefix(DefaultPhoneHandler __instance)
        {
            try
            {
                Game1.currentLocation.playShopPhoneNumberSounds("AdventureGuild");
                Game1.player.freezePause = 4950;
                DelayedAction.functionAfterDelay(() =>
                {
                    Game1.playSound("bigSelect");
                    var character = Game1.getCharacterFromName("Marlon");
                    if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
                    {
                        Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_AlreadyRecovering");
                    }
                    else
                    {
                        Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_Open");
                        Game1.afterDialogues += () =>
                        {
                            var equipmentsToRecover = _weaponsManager.GetEquipmentsForSale(IDProvider.ARCHIPELAGO_EQUIPMENTS_RECOVERY);
                            if (equipmentsToRecover.Any())
                            {
                                Game1.player.forceCanMove();
                                Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon");
                            }
                            else
                            {
                                Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_NoDeathItems");
                            }
                        };
                    }
                }, 4950);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CallAdventureGuild_AllowRecovery_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
