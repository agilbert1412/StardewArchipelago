using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewArchipelago.GameModifications.MultiSleep;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class VillagerExistenceInjections
    {
        public const string MEET_PREFIX = "Meet ";
        public const string ARRIVAL_SUFFIX = " Arrival";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;

        private static Dictionary<string, FakeEventNPC> _fakeNpcs;

        public static FakeEventNPC GetFakeNpc(string name)
        {
            if (!_fakeNpcs.ContainsKey(name))
            {
                _fakeNpcs.Add(name, CreateFakeNPC(name));
            }
            return _fakeNpcs[name];
        }

        private static FakeEventNPC CreateFakeNPC(string name)
        {
            return new FakeEventNPC(name);
        }

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _helper = helper;

            _fakeNpcs = new Dictionary<string, FakeEventNPC>();
        }

        // public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
        public static bool AddCharacterIfNecessary_ConsiderArrivals_Prefix(string characterId, ref bool bypassConditions)
        {
            try
            {
                if (_fakeNpcs.ContainsKey(characterId))
                {
                    _fakeNpcs.Remove(characterId);
                }
                var allowed = AllowedToExist(characterId);

                if (!allowed)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (!NPC.TryGetData(characterId, out var data))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (data.UnlockConditions == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (data.UnlockConditions.Equals("YEAR 2", StringComparison.InvariantCultureIgnoreCase))
                {
                    bypassConditions = true;
                }

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
                _fakeNpcs.Clear();
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
                if (MultiSleepManager.IsCurrentlyMultisleeping())
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

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

        // public override void MakeMapModifications(bool force = false)
        public static bool MakeMapModifications_SewerWhenKrobusDoesntExit_Prefix(Sewer __instance, bool force)
        {
            try
            {
                if (force)
                {
                    // protected HashSet<string> _appliedMapOverrides;
                    var appliedMapOverridesField = _helper.Reflection.GetField<HashSet<string>>(__instance, "_appliedMapOverrides");
                    var appliedMapOverrides = appliedMapOverridesField.GetValue();
                    appliedMapOverrides.Clear();
                }
                __instance.interiorDoors.MakeMapModifications();

                var krobus = Game1.getCharacterFromName("Krobus");
                if (krobus != null && krobus.isMarried())
                {
                    __instance.setMapTile(31, 17, 84, "Buildings", "st");
                    __instance.setMapTile(31, 16, 1, "Front", "st");
                }
                else
                {
                    __instance.removeMapTile(31, 17, "Buildings");
                    __instance.removeMapTile(31, 16, "Front");
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MakeMapModifications_SewerWhenKrobusDoesntExit_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // Patch that doesn't do what I want...
        // public static NPC getCharacterFromName(string name, bool mustBeVillager = true, bool includeEventActors = false)
        /*public static void GetCharacterFromName_FakeNPCWhenNotArrivedYet_Postfix(string name, bool mustBeVillager, bool includeEventActors, ref NPC __result)
        {
            try
            {
                if (__result != null)
                {
                    return;
                }

                var arrivalName = GetArrivalItem(name);
                if (_archipelago.SlotData.StartWithout.HasFlag(StartWithout.Villagers) && _archipelago.LocationExists($"Meet {name}")) // This check is bullshit, until we get proper check if items exist, I check the location instead.
                {
                    __result = Nobody;
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetCharacterFromName_FakeNPCWhenNotArrivedYet_Postfix)}:\n{ex}");
                return;
            }
        }*/

        // public void lockedDoorWarp(Point tile, string locationName, int openTime, int closeTime, string npcName, int minFriendship)
        public static bool LockedDoorWarp_LockedWhenOwnerDoesntExist_Prefix(GameLocation __instance, Point tile, string locationName, int openTime, int closeTime, string npcName, int minFriendship)
        {
            try
            {
                var hasKey = Game1.player.HasTownKey;
                if (GameLocation.AreStoresClosedForFestival() && __instance.InValleyContext())
                {
                    Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FestivalDay_DoorLocked")));
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (locationName == "SeedShop" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Wed") && !Utility.HasAnyPlayerSeenEvent("191393") && !hasKey)
                {
                    Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:SeedShop_LockedWed")));
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
                {
                    openTime = 800;
                }
                if (hasKey)
                {
                    if (!__instance.InValleyContext())
                    {
                        hasKey = false;
                    }
                    if (hasKey && __instance is BeachNightMarket && locationName != "FishShop")
                    {
                        hasKey = false;
                    }
                }

                var isCorrectHour = hasKey || Game1.timeOfDay >= openTime && Game1.timeOfDay < closeTime;
                var hasEnoughFriendship = minFriendship <= 0 || __instance.IsWinterHere() || Game1.player.friendshipData.TryGetValue(npcName, out var friendship) && friendship.Points >= minFriendship;
                var canEnter = isCorrectHour && hasEnoughFriendship;
                if (__instance.IsGreenRainingHere() && Game1.year == 1 && __instance is not Beach && __instance is not Forest && !locationName.Equals("AdventureGuild"))
                {
                    canEnter = true;
                }
                if (canEnter)
                {
                    Rumble.rumble(0.15f, 200f);
                    Game1.player.completelyStopAnimatingOrDoingAction();
                    __instance.playSound("doorClose", Game1.player.Tile);
                    Game1.warpFarmer(locationName, tile.X, tile.Y, false);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (minFriendship <= 0)
                {
                    var sub1 = Game1.getTimeOfDayString(openTime).Replace(" ", "");
                    if (locationName == "FishShop" && Game1.player.mailReceived.Contains("willyHours"))
                    {
                        sub1 = Game1.getTimeOfDayString(800).Replace(" ", "");
                    }
                    var sub2 = Game1.getTimeOfDayString(closeTime).Replace(" ", "");
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_OpenRange", (object)sub1, (object)sub2));
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                if (Game1.timeOfDay < openTime || Game1.timeOfDay >= closeTime)
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var characterFromName = Game1.getCharacterFromName(npcName);
                var name = characterFromName == null ? npcName : characterFromName.displayName;
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor_FriendsOnly", name));
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(LockedDoorWarp_LockedWhenOwnerDoesntExist_Prefix)}:\n{ex}");
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

            var eventDataParts = eventData.Split("/").Select(x => x.Split(" ")).Where(eventDataPartWords => eventDataPartWords.Length >= 2).ToArray();
            foreach (var characterId in illegalVillagers)
            {
                if (eventDataParts.Any(eventDataPartWords => eventDataPartWords[1].Contains(characterId)))
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

            var canSocialize = npcCanSocialize == null || !npcCanSocialize.Equals(false.ToString(), StringComparison.InvariantCultureIgnoreCase);
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

        public static string GetArrivalItem(string npcId)
        {
            var apName = NameAliases.NPCNameAliases.ContainsKey(npcId) ? NameAliases.NPCNameAliases[npcId] : npcId;
            var arrivalItem = $"{apName}{ARRIVAL_SUFFIX}";
            return arrivalItem;
        }

        public static string GetMeetLocation(string npcId)
        {
            var apName = NameAliases.NPCNameAliases.ContainsKey(npcId) ? NameAliases.NPCNameAliases[npcId] : npcId;
            var meetLocation = $"{MEET_PREFIX}{apName}";
            return meetLocation;
        }

        // public NPC getActorByName(string name, out bool isOptionalNpc, bool legacyReplaceUnderscores = false)
        public static void GetActorByName_GiveFakeNPCIfNeeded_Postfix(Event __instance, string name, ref bool isOptionalNpc, bool legacyReplaceUnderscores, ref NPC __result)
        {
            try
            {
                if (__result != null || string.IsNullOrWhiteSpace(name) || name.Contains("spouse"))
                {
                    return;
                }

                var illegalVillagers = GetIllegalVillagers();
                if (illegalVillagers.Contains(name))
                {
                    __result = GetFakeNpc(name);
                    // private Dictionary<string, Vector3> actorPositionsAfterMove;
                    var actorPositionsAfterMoveField = _helper.Reflection.GetField<Dictionary<string, Vector3>>(__instance, "actorPositionsAfterMove");
                    var actorPositionsAfterMove = actorPositionsAfterMoveField.GetValue();
                    if (actorPositionsAfterMove.ContainsKey(name))
                    {
                        actorPositionsAfterMove.Remove(name);
                    }
                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetActorByName_GiveFakeNPCIfNeeded_Postfix)}:\n{ex}");
                return;
            }
        }
    }

    internal class FakeEventNPC : NPC
    {
        public FakeEventNPC(string name) : base(new AnimatedSprite(Game1.content, Game1.player.getTexture()), Vector2.Zero, 0, name)
        {

        }

        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
        }

        public override void update(GameTime time, GameLocation location, long id, bool move)
        {
            base.update(time, location, id, move);
        }

        public override void draw(SpriteBatch b, float alpha = 1)
        {
            base.draw(b, alpha);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
        }

        public override void draw(SpriteBatch b, int ySourceRectOffset, float alpha = 1)
        {
            base.draw(b, ySourceRectOffset, alpha);
        }

        public override void Halt()
        {
            base.Halt();
        }

        public override void MovePosition(GameTime time, Rectangle viewport, GameLocation currentLocation)
        {
            base.MovePosition(time, viewport, currentLocation);
        }

        public override void updateMovement(GameLocation location, GameTime time)
        {
            base.updateMovement(location, time);
        }
    }
}
