using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Linq;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.CodeInjections;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class PhoneInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static WeaponsManager _weaponsManager;
        private static JojaLocationChecker _jojaLocationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, WeaponsManager weaponsManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _weaponsManager = weaponsManager;
        }

        public static void Initialize(JojaLocationChecker jojaLocationChecker)
        {
            _jojaLocationChecker = jojaLocationChecker;
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

        //public string CheckForIncomingCall(Random random)
        public static void CheckForIncomingCall_AdjustCalls_Postfix(DefaultPhoneHandler __instance, Random random, ref string __result)
        {
            try
            {
                if (TryReplaceCallWithJojaAd(random, ref __result))
                {
                    return;
                }

                RemoveCallIfInvalid(ref __result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForIncomingCall_AdjustCalls_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool TryReplaceCallWithJojaAd(Random random, ref string __result)
        {
            if (!string.IsNullOrWhiteSpace(__result) || _jojaLocationChecker == null)
            {
                return false;
            }

            var chanceOfAd = (_jojaLocationChecker.GetPercentCheckedLocationsByJoja() * 0.25) + 0.02;
            if (random.NextDouble() < chanceOfAd)
            {
                __result = JojaConstants.JOJA_INCOMING_CALL;
                return true;
            }

            return false;
        }

        private static void RemoveCallIfInvalid(ref string __result)
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.Villagers) || string.IsNullOrWhiteSpace(__result))
            {
                return;
            }

            var calls = DataLoader.IncomingPhoneCalls(Game1.content);
            var thisCall = calls[__result];
            var npc = thisCall.FromNpc;
            if (string.IsNullOrWhiteSpace(npc))
            {
                return;
            }

            if (!Game1.characterData.ContainsKey(npc))
            {
                return;
            }

            var arrivalItem = VillagerExistenceInjections.GetArrivalItem(npc);
            if (_archipelago.HasReceivedItem(arrivalItem))
            {
                return;
            }

            __result = null;
        }
    }
}
