using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class PhoneInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static WeaponsManager _weaponsManager;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, WeaponsManager weaponsManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _weaponsManager = weaponsManager;
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
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CallAdventureGuild_AllowRecovery_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
