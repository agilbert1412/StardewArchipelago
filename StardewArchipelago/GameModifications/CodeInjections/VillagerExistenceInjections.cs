using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class VillagerExistenceInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
        public static bool AddCharacterIfNecessary_ConsiderArrivals_Prefix(string characterId, ref bool bypassConditions)
        {
            try
            {
                var allowed = AllowedToExist(characterId);

                if (!allowed)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                bypassConditions = true;
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddCharacterIfNecessary_ConsiderArrivals_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static void AddNPCs()
        public static bool AddNPCs_RemoveNPCsThatDontExistYet_Prefix()
        {
            try
            {
                Utility.ForEachLocation(location =>
                {
                    location.characters.RemoveWhere(npc =>
                    {
                        if (!AllowedToExist(npc.Name))
                        {
                            Game1.player.friendshipData.Remove(npc.Name);
                            return true;
                        }
                        return false;
                    });
                    return true;
                });

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddNPCs_RemoveNPCsThatDontExistYet_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static bool PlayEvent(string eventId, bool checkPreconditions = true, bool checkSeen = true)
        public static bool PlayEvent_DontPlayEventsWithNPCsThatDontExistYet_Prefix(string eventId, bool checkPreconditions, bool checkSeen, ref bool __result)
        {
            try
            {
                if (!CanPlayEventWithPet(eventId))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var illegalVillagers = GetIllegalVillagers();
                if (!CanPlayEventWithVillagers(eventId, Game1.currentLocation, illegalVillagers))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlayEvent_DontPlayEventsWithNPCsThatDontExistYet_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static bool CheckPrecondition(GameLocation location, string eventId, string precondition)
        public static bool CheckPrecondition_DontPlayEventsWithNPCsThatDontExistYet_Prefix(GameLocation location, string eventId, string precondition, ref bool __result)
        {
            try
            {
                if (!CanPlayEventWithPet(eventId))
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var illegalVillagers = GetIllegalVillagers();
                if (!CanPlayEventWithVillagers(eventId, location, illegalVillagers))
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckPrecondition_DontPlayEventsWithNPCsThatDontExistYet_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static bool TryOpenShopMenu(string shopId, string ownerName, bool playOpenSound = true)
        public static bool TryOpenShopMenuSimple_NoShopsWithoutOwnerExisting_Prefix(string shopId, string ownerName, bool playOpenSound, ref bool __result)
        {
            try
            {
                if (!IsShopAllowed(shopId))
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryOpenShopMenuSimple_NoShopsWithoutOwnerExisting_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public static bool TryOpenShopMenu(string shopId, GameLocation location, Microsoft.Xna.Framework.Rectangle? ownerArea = null, int? maxOwnerY = null, bool forceOpen = false, bool playOpenSound = true, Action<string> showClosedMessage = null)
        public static bool TryOpenShopMenuComplex_NoShopsWithoutOwnerExisting_Prefix(string shopId, GameLocation location, Microsoft.Xna.Framework.Rectangle? ownerArea, int? maxOwnerY, bool forceOpen, bool playOpenSound, Action<string> showClosedMessage, ref bool __result)
        {
            try
            {
                if (!IsShopAllowed(shopId))
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryOpenShopMenuComplex_NoShopsWithoutOwnerExisting_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool IsShopAllowed(string shopId)
        {
            var parts = shopId.Split("_");

            var illegalVillagers = GetIllegalVillagers();
            foreach (var part in parts)
            {
                if (illegalVillagers.Any(x => x.Equals(part)))
                {
                    return false;
                }
            }

            return true;
        }

        private static HashSet<string> GetIllegalVillagers()
        {
            var villagers = Game1.characterData.Select(x => x.Key).Where(x => !AllowedToExist(x)).ToHashSet();
            return villagers;
        }

        private static bool CanPlayEventWithPet(string eventId)
        {
            if (!Pet.TryGetData(Game1.player.whichPetType, out var petData) || eventId != petData.AdoptionEventId)
            {
                return true;
            }

            var petArrival = GetArrivalItem("Pet");
            var hasArrival = _archipelago.HasReceivedItem(petArrival);
            if (!hasArrival)
            {
                return false;
            }

            return true;
        }

        private static bool CanPlayEventWithVillagers(string eventId, GameLocation location, HashSet<string> illegalVillagers)
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.Villagers))
            {
                return true;
            }

            if (!location.TryGetLocationEvents(out _, out var locationEvents))
            {
                return true;
            }

            string eventData = null;
            foreach (var (eventKey, eventValue) in locationEvents)
            {
                if (eventKey.Split('/')[0] == eventId)
                {
                    eventData = $"{eventKey} --- {eventValue}";
                    break;
                }
            }

            if (eventData == null)
            {
                return true;
            }

            foreach (var characterId in illegalVillagers)
            {
                if (eventData.Contains(characterId))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllowedToExist(string villagerId)
        {
            var alwaysNeedArrival = new[] { "Kent" };
            var needsArrival = _archipelago.SlotData.StartWithout.HasFlag(StartWithout.Villagers) || alwaysNeedArrival.Contains(villagerId);
            if (!Game1.characterData.ContainsKey(villagerId))
            {
                return true;
            }
            var npcCanSocialize = Game1.characterData[villagerId].CanSocialize;
            var canSocialize = npcCanSocialize == null || npcCanSocialize.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase);
            needsArrival = needsArrival && canSocialize;

            if (!NPC.TryGetData(villagerId, out var npcData))
            {
                return true;
            }

            if (!needsArrival)
            {
                return true;
            }

            var arrivalItem = GetArrivalItem(villagerId);
            var hasArrival = _archipelago.HasReceivedItem(arrivalItem);
            return hasArrival;

        }

        private static string GetArrivalItem(string npcName)
        {
            var arrivalItem = $"{npcName} Arrival";
            return arrivalItem;
        }
    }
}
