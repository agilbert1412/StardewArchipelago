﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewArchipelago.Stardew;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship
{
    public static class FriendshipInjections
    {
        private const int AUTO_PETTER = 272;
        private const int AUTO_GRABBER = 165;
        private const int DECAY_SPOUSE = -20;
        private const int DECAY_PARTNER = -8;
        private const int DECAY_OTHER = -2;
        private const int AUTOPET_POINTS = 5;
        private const int DECAY_GRAB = -12;
        private const int POINTS_PER_HEART = 250;
        private const int POINTS_PER_PET_HEART = 200;
        private const string HEARTS_PATTERN = "{0} <3";
        public const string FRIENDSANITY_PATTERN = "Friendsanity: {0} {1} <3";

        private static List<string> _notImmediatelyAccessible = new()
        {
            "Leo", "Krobus", "Dwarf", "Sandy", "Kent",
        };

        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Friends _friends;
        private static VillagerGrabber _grabber;
        private static Dictionary<string, double> _friendshipPoints = new();
        private static Dictionary<string, Dictionary<int, Texture2D>> _apLogos;

        private static string[] _hintedFriendshipLocations;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, Friends friends, StardewItemManager itemManager)
        {
            _logger = logger;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _friends = friends;
            _grabber = new VillagerGrabber(archipelago, itemManager);
            _apLogos = new Dictionary<string, Dictionary<int, Texture2D>>();

            foreach (var logoName in ArchipelagoTextures.ValidLogos)
            {
                _apLogos.Add(logoName, new Dictionary<int, Texture2D>());
                for (var i = 12; i <= 48; i *= 2)
                {
                    _apLogos[logoName].Add(i, ArchipelagoTextures.GetArchipelagoLogo(i, logoName));
                }
            }
            _hintedFriendshipLocations = Array.Empty<string>();
        }

        private static IReflectedField<NetInt> GetPrivatePointsField(Friendship friendship)
        {
            return _helper.Reflection.GetField<NetInt>(friendship, "points");
        }

        public static Dictionary<string, int> GetArchipelagoFriendshipPoints()
        {
            return _friendshipPoints.ToDictionary(x => x.Key, x => (int)Math.Round(x.Value));
        }

        public static string GetArchipelagoFriendshipPointsForPrinting(string characterName)
        {
            var points = GetFriendshipPoints(characterName);
            if (points <= 0)
            {
                return $"You have never met someone named {characterName}";
            }
            return $"{characterName}: {points} ({GetHearts(points)} <)";
        }

        public static void SetArchipelagoFriendshipPoints(Dictionary<string, int> values)
        {
            if (values == null)
            {
                _friendshipPoints = new Dictionary<string, double>();
                return;
            }

            _friendshipPoints = values.ToDictionary(x => x.Key, x => (double)x.Value);
        }

        public static void ResetArchipelagoFriendshipPoints()
        {
            _friendshipPoints = new Dictionary<string, double>();
        }

        public static bool GetPoints_ArchipelagoHearts_Prefix(Friendship __instance, ref int __result)
        {
            try
            {
                var friend = _friends.GetFriend(__instance);
                if (friend == null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var friendshipPoints = GetEffectiveFriendshipPoints(friend);
                SetBackendFriendshipPoints(__instance, friendshipPoints);

                __result = friendshipPoints;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetPoints_ArchipelagoHearts_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void SetBackendFriendshipPoints(Friendship __instance, int friendshipPoints)
        {
            var pointsField = GetPrivatePointsField(__instance);
            pointsField.GetValue().Value = friendshipPoints;
        }

        private static int GetEffectiveFriendshipPoints(ArchipelagoFriend friend)
        {
            var archipelagoHeartItems =
                _archipelago.GetReceivedItemCount(string.Format(HEARTS_PATTERN, friend.ArchipelagoName));
            var receivedHearts = archipelagoHeartItems * _archipelago.SlotData.FriendsanityHeartSize;

            var maxShuffled = friend.ShuffledUpTo(_archipelago);
            if (receivedHearts > maxShuffled)
            {
                receivedHearts = maxShuffled;
            }

            var friendshipPoints = receivedHearts * POINTS_PER_HEART;
            friendshipPoints = GetBoundedToCurrentRelationState(friendshipPoints, friend.StardewName);
            if (receivedHearts >= maxShuffled)
            {
                var earnedPoints = (int)GetFriendshipPoints(friend.StardewName);
                var earnedPointsAboveMaxShuffled = Math.Max(0, earnedPoints - (maxShuffled * POINTS_PER_HEART));
                friendshipPoints += earnedPointsAboveMaxShuffled;
            }

            return friendshipPoints;
        }

        // public SocialPage(int x, int y, int width, int height)
        public static void SocialPageCtor_CheckHints_Postfix(SocialPage __instance, int x, int y, int width, int height)
        {
            try
            {
                var hints = _archipelago.GetMyActiveDesiredHints();
                var hintedLocationNames = hints.Select(hint => _archipelago.GetLocationName(hint)).Where(hintName => hintName.StartsWith($"Friendsanity: ")).ToArray();
                _hintedFriendshipLocations = hintedLocationNames;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SocialPageCtor_CheckHints_Postfix)}:\n{ex}");
                return;
            }
        }

        // private void drawNPCSlot(SpriteBatch spriteBatch, int i)
        public static void DrawNPCSlot_DrawEarnedHearts_Postfix(SocialPage __instance, SpriteBatch b, int i)
        {
            try
            {
                var socialEntry = __instance.SocialEntries[i];
                var friend = _friends.GetFriend(socialEntry.InternalName);
                if (friend == null)
                {
                    return;
                }
                var apPoints = (int)GetFriendshipPoints(friend.StardewName);
                var maxShuffled = friend.ShuffledUpTo(_archipelago);
                var heartSize = _archipelago.SlotData.FriendsanityHeartSize;
                var maxHeartForCurrentRelation = GetMaximumHeartsWithRelationState(friend.StardewName);
                var apHearts = apPoints / POINTS_PER_HEART;
                var spritesField = _helper.Reflection.GetField<List<ClickableTextureComponent>>(__instance, "sprites");
                var sprites = spritesField.GetValue();
                var characterClickableComponent = sprites[i];
                for (var heartIndex = 0; heartIndex < maxShuffled; ++heartIndex)
                {
                    DrawEarnedHeart(__instance, b, characterClickableComponent, heartIndex, heartSize, maxShuffled, apHearts, friend, maxHeartForCurrentRelation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DrawNPCSlot_DrawEarnedHearts_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawEarnedHeart(SocialPage socialPage, SpriteBatch spriteBatch, ClickableTextureComponent characterClickableComponent, int heartIndex, int heartSize,
            int maxShuffled, int apHearts, ArchipelagoFriend friend, int maxHeartForCurrentRelation)
        {
            var heartNumber = heartIndex + 1;
            var imageSize = 24;
            var textureName = ArchipelagoTextures.RED;
            var scale = 1f;

            var positionX = socialPage.xPositionOnScreen + 320 - 2 + heartIndex * 32;
            var smallHeartOffset = 28;
            var positionY = characterClickableComponent.bounds.Y + 64 - 28;
            var position = new Vector2(positionX, positionY - smallHeartOffset);
            var color = Color.White;
            if (heartIndex >= 10)
            {
                var reverseX = 9 - (heartIndex - 10);
                positionX = socialPage.xPositionOnScreen + 320 - 2 + reverseX * 32;
                position = new Vector2(positionX, positionY + smallHeartOffset);
            }

            if (heartNumber % heartSize != 0 && heartNumber != maxShuffled)
            {
                imageSize = 12;
                position = new Vector2(position.X + 6, position.Y + 6);
            }

            if (heartIndex >= apHearts)
            {
                var apLocation = string.Format(FRIENDSANITY_PATTERN, friend.ArchipelagoName, heartNumber);
                if (_hintedFriendshipLocations.Any(x => x.Contains($"{friend.ArchipelagoName} {heartNumber} ")))
                {
                    textureName = ArchipelagoTextures.PLEADING;
                }
                else if (heartIndex >= maxHeartForCurrentRelation)
                {
                    textureName = ArchipelagoTextures.BLACK;
                }
                else if (_locationChecker.IsLocationChecked(apLocation) || !_archipelago.LocationExists(apLocation))
                {
                    textureName = ArchipelagoTextures.WHITE;
                }
                else if (heartIndex < maxHeartForCurrentRelation)
                {
                    textureName = ArchipelagoTextures.COLOR;
                }
            }

            var texture = _apLogos[textureName][imageSize];
            var sourceRectangle = new Rectangle(0, 0, imageSize, imageSize);
            spriteBatch.Draw(texture, position, sourceRectangle, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.88f);
        }

        private static int GetBoundedToCurrentRelationState(double friendshipPoints, string npcName)
        {
            return GetBoundedToCurrentRelationState((int)friendshipPoints, npcName);
        }

        private static int GetBoundedToCurrentRelationState(int friendshipPoints, string npcName)
        {
            var npc = Game1.getCharacterFromName(npcName);
            return Math.Max(0, Math.Min(friendshipPoints, (Utility.GetMaximumHeartsForCharacter(npc) + 1) * POINTS_PER_HEART - 1));
        }

        private static int GetMaximumHeartsWithRelationState(string npcName)
        {
            var npc = Game1.getCharacterFromName(npcName);
            return Utility.GetMaximumHeartsForCharacter(npc);
        }

        public static bool DayUpdate_ArchipelagoPoints_Prefix(Pet __instance, int dayOfMonth)
        {
            try
            {
                if (_archipelago.SlotData.Friendsanity is Friendsanity.None or Friendsanity.Bachelors)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;;
                }
                if (__instance.currentLocation is FarmHouse)
                {
                    __instance.setAtFarmPosition();
                }

                var wasPet = __instance.grantedFriendshipForPet.Value;
                var farm = __instance.currentLocation as Farm;
                var petBowl = farm?.getBuildingByType(nameof(PetBowl)) as PetBowl;
                var wasWatered = petBowl?.watered?.Value ?? false;
                var pointIncrease = (wasPet ? 12 : 0) + (wasWatered ? 6 : 0);
                var multipliedPointIncrease = GetMultipliedFriendship(pointIncrease);

                var petName = Game1.player.getPetName();
                _friends.AddPet(petName);
                var petFriend = _friends.GetFriend(petName);
                var newApPoints = GetFriendshipPoints(petFriend.StardewName) + multipliedPointIncrease;
                SetFriendshipPoints(petFriend.StardewName, Math.Min(1000, newApPoints));
                for (var i = 1; i < newApPoints / POINTS_PER_PET_HEART; i++)
                {
                    _locationChecker.AddCheckedLocation(string.Format(FRIENDSANITY_PATTERN, petFriend.ArchipelagoName, i));
                }
                petBowl?.watered?.Set(false);

                var archipelagoHeartItems = _archipelago.GetReceivedItemCount(string.Format(HEARTS_PATTERN, petFriend.ArchipelagoName));
                var receivedHearts = archipelagoHeartItems * _archipelago.SlotData.FriendsanityHeartSize;
                var maxShuffled = petFriend.ShuffledUpTo(_archipelago);
                if (receivedHearts > maxShuffled)
                {
                    receivedHearts = maxShuffled;
                }

                __instance.friendshipTowardFarmer.Set(Math.Min(1000, receivedHearts * POINTS_PER_PET_HEART));
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_ArchipelagoPoints_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool ChangeFriendship_ArchipelagoPoints_Prefix(Farmer __instance, int amount, NPC n)
        {
            try
            {
                var isValidTarget = n != null && (n is Child || n.IsVillager);
                if (!isValidTarget)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                //  Checks if actual name is a value in the dictionary and updates if necessary.
                var name = n.Name;
                var friend = _friends.GetFriend(name);
                if (friend == null)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var canCommunicateWithNpc = !friend.RequiresDwarfLanguage || __instance.canUnderstandDwarves;
                if (amount > 0 && !canCommunicateWithNpc)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (__instance.friendshipData.ContainsKey(friend.StardewName))
                {
                    if (n.isDivorcedFrom(__instance) && amount > 0)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    var pointDifference = amount;
                    var multipliedPointDifference = GetMultipliedFriendship(pointDifference);
                    var apPoints = GetFriendshipPoints(friend.StardewName);
                    var newApPoints = apPoints + multipliedPointDifference;
                    newApPoints = GetBoundedToCurrentRelationState(newApPoints, friend.StardewName);
                    SetFriendshipPoints(friend.StardewName, newApPoints);
                    var earnedHearts = (int)newApPoints / POINTS_PER_HEART;
                    earnedHearts = Math.Min(earnedHearts, friend.ShuffledUpTo(_archipelago));
                    for (var i = 1; i <= earnedHearts; i++)
                    {
                        _locationChecker.AddCheckedLocation(string.Format(FRIENDSANITY_PATTERN, friend.ArchipelagoName, i));
                    }

                    if (n.datable.Value && __instance.friendshipData[friend.StardewName].Points >= 2000 && !__instance.hasOrWillReceiveMail("Bouquet"))
                    {
                        Game1.addMailForTomorrow("Bouquet");
                    }

                    if (n.datable.Value && __instance.friendshipData[friend.StardewName].Points >= 2500 && !__instance.hasOrWillReceiveMail("SeaAmulet"))
                    {
                        Game1.addMailForTomorrow("SeaAmulet");
                    }

                    var effectiveFriendshipPoints = GetEffectiveFriendshipPoints(friend);
                    SetBackendFriendshipPoints(__instance.friendshipData[friend.StardewName], effectiveFriendshipPoints);
                }
                else
                {
                    Game1.debugOutput = "Tried to change friendship for a friend that wasn't there.";
                }
                
                Game1.stats.checkForFriendshipAchievements();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ChangeFriendship_ArchipelagoPoints_Prefix)}:\n{ex}");
                _logger.LogError($"NPC: {n?.Name ?? "null"}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void resetFriendshipsForNewDay()
        public static bool ResetFriendshipsForNewDay_AutopetHumans_Prefix(Farmer __instance)
        {
            try
            {
                var seed = Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed;
                var random = new Random((int)seed);
                foreach (var npcName in __instance.friendshipData.Keys)
                {
                    PerformFriendshipChanges(__instance, npcName, random);
                }
                var date = new WorldDate(Game1.Date);
                ++date.TotalDays;
                __instance.updateFriendshipGifts(date);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetFriendshipsForNewDay_AutopetHumans_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void PerformFriendshipChanges(Farmer farmer, string npcName, Random random)
        {
            if (!TryGetNpc(npcName, out var npc))
            {
                return;
            }

            var isSingleBachelor = IsSingleBachelor(farmer, npcName, npc);

            AutoPetNpc(farmer, npcName, npc);
            AutoGrabNpc(farmer, npcName, npc, random);
            DecayNpc(farmer, npcName, npc, isSingleBachelor);
        }

        private static void DecayNpc(Farmer farmer, string npcName, NPC npc, bool isSingleBachelor)
        {
            if (farmer.hasPlayerTalkedToNPC(npcName))
            {
                farmer.friendshipData[npcName].TalkedToToday = false;
                return;
            }

            if (ModEntry.Instance.Config.DisableFriendshipDecay)
            {
                return;
            }

            DoDecay(farmer, npcName, npc, isSingleBachelor);
        }

        private static void DoDecay(Farmer farmer, string npcName, NPC npc, bool isSingleBachelor)
        {
            const int bachelorNoDecayThreshold = 2000;
            const int nonBachelorNoDecayThreshold = 2500;
            var earnedPoints = GetFriendshipPoints(npcName);

            if (NpcIsSpouse(farmer, npcName))
            {
                farmer.changeFriendship(DECAY_SPOUSE, npc);
            }
            else if (NpcIsUnmaxedPartner(farmer, npcName))
            {
                farmer.changeFriendship(DECAY_PARTNER, npc);
            }
            else if ((!isSingleBachelor && farmer.friendshipData[npcName].Points < nonBachelorNoDecayThreshold && earnedPoints < nonBachelorNoDecayThreshold) ||
                     (isSingleBachelor && farmer.friendshipData[npcName].Points < bachelorNoDecayThreshold && earnedPoints < bachelorNoDecayThreshold))
            {
                farmer.changeFriendship(DECAY_OTHER, npc);
            }
        }

        private static bool IsSingleBachelor(Farmer farmer, string npcName, NPC npc)
        {
            var isSingleBachelor = npc.datable.Value && !farmer.friendshipData[npcName].IsDating() && !npc.isMarried();
            return isSingleBachelor;
        }

        private static bool TryGetNpc(string npcName, out NPC npc)
        {
            npc = Game1.getCharacterFromName(npcName) ?? Game1.getCharacterFromName<Child>(npcName, false);
            return npc != null;
        }

        private static void AutoPetNpc(Farmer farmer, string npcName, NPC npc)
        {
            var npcLocation = npc.currentLocation;
            foreach (var (_, objectInSameRoom) in npcLocation.Objects.Pairs)
            {
                if (!objectInSameRoom.bigCraftable.Value || objectInSameRoom.ParentSheetIndex != AUTO_PETTER)
                {
                    continue;
                }

                farmer.friendshipData[npcName].TalkedToToday = true;
                farmer.changeFriendship(AUTOPET_POINTS, npc);
            }
        }

        private static void AutoGrabNpc(Farmer farmer, string npcName, NPC npc, Random random)
        {
            if (!_grabber.GrabberItems.ContainsKey(npcName) || !_grabber.GrabberItems[npcName].Any())
            {
                return;
            }

            var npcLocation = npc.currentLocation;
            AutograbRoom(farmer, npcName, npc, npcLocation, random);
            if (npcName.Equals("Caroline"))
            {
                AutograbRoom(farmer, npcName, VillagerGrabber.CAROLINE_SUN_ROOM, npc, Game1.getLocationFromName("Sunroom"), random, 0.25);
            }
        }

        private static void AutograbRoom(Farmer farmer, string npcName, NPC npc, GameLocation room, Random random)
        {
            AutograbRoom(farmer, npcName, npcName, npc, room, random, 1.0);
        }

        private static void AutograbRoom(Farmer farmer, string npcName, string grabbedItemsKey, NPC npc, GameLocation room, Random random, double chanceMultiplier)
        {
            foreach (var (_, autoGrabber) in room.Objects.Pairs)
            {
                if (!IsAutoGrabber(autoGrabber, out var autoGrabberInventory))
                {
                    continue;
                }

                TryAutoGrabOneItem(farmer, npcName, grabbedItemsKey, npc, random, autoGrabber, autoGrabberInventory, chanceMultiplier);
            }
        }

        private static bool IsAutoGrabber(Object objectInRoom, out Chest autoGrabberInventory)
        {
            autoGrabberInventory = null;
            if (objectInRoom == null || !objectInRoom.bigCraftable.Value || objectInRoom.ParentSheetIndex != AUTO_GRABBER || objectInRoom.heldObject.Value is not Chest inventory)
            {
                return false;
            }

            autoGrabberInventory = inventory;
            return true;
        }

        private static void TryAutoGrabOneItem(Farmer farmer, string friendName, string grabbedItemsKey, NPC npc, Random random, Object autoGrabber, Chest autoGrabberInventory, double chanceMultiplier)
        {
            farmer.changeFriendship(DECAY_GRAB, npc);
            var hearts = farmer.friendshipData[friendName].Points / 250;
            var chanceOfProduction = (hearts / 56.0) * chanceMultiplier;
            if (random.NextDouble() > chanceOfProduction)
            {
                return;
            }

            AutoGrabOneItem(grabbedItemsKey, random, autoGrabber, autoGrabberInventory);
        }

        private static void AutoGrabOneItem(string grabberItemKey, Random random, Object autoGrabber, Chest autoGrabberInventory)
        {
            var possibleItems = _grabber.GrabberItems[grabberItemKey];
            var index = random.Next(0, possibleItems.Count);
            var item = possibleItems.Keys.ElementAt(index);
            var amount = possibleItems[item];
            var stardewItem = item.PrepareForGivingToFarmer(amount);
            autoGrabberInventory.addItem(stardewItem);
            autoGrabber.showNextIndex.Value = true;
        }

        private static bool NpcIsUnmaxedPartner(Farmer farmer, string npcName)
        {
            return farmer.friendshipData[npcName].IsDating() && farmer.friendshipData[npcName].Points < 2500;
        }

        private static bool NpcIsSpouse(Farmer farmer, string npcName)
        {
            return farmer.spouse != null && npcName.Equals(farmer.spouse);
        }

        private static double GetMultipliedFriendship(int amount)
        {
            return amount * _archipelago.SlotData.FriendshipMultiplier;
        }

        private static int GetHearts(double friendshipPoints)
        {
            return (int)Math.Floor(friendshipPoints) / 250;
        }

        private static double GetFriendshipPoints(string npc)
        {
            if (!_friendshipPoints.ContainsKey(npc))
            {
                _friendshipPoints.Add(npc, 0);
            }

            return _friendshipPoints[npc];
        }

        private static void SetFriendshipPoints(string npc, double points)
        {
            if (!_friendshipPoints.ContainsKey(npc))
            {
                _friendshipPoints.Add(npc, 0);
            }

            if (points < 0)
            {
                points = 0;
            }

            _friendshipPoints[npc] = points;
        }
    }
}
