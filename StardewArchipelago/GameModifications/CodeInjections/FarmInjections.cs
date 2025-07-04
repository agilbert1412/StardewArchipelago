﻿using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Goals;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Serialization;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class FarmInjections
    {
        private static readonly List<string> _locationsWithGoldenClockEffect = new()
        {
            "Farm",
            "IslandWest",
            "Custom_GrandpasShedOutside",
        };

        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, ArchipelagoStateDto state)
        {
            _logger = logger;
            _archipelago = archipelago;
            _state = state;
        }

        public static bool CheckAction_GrandpaNote_PreFix(Farm __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var rect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
                if (!__instance.objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)) &&
                    __instance.CheckPetAnimal(rect, who))
                {
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                var grandpaShrinePosition = __instance.GetGrandpaShrinePosition();
                if (tileLocation.X < grandpaShrinePosition.X - 1 || tileLocation.X > grandpaShrinePosition.X + 1 ||
                    tileLocation.Y != grandpaShrinePosition.Y)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (__instance.hasSeenGrandpaNote)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Game1.addMail("hasSeenGrandpaNote", true);
                __instance.hasSeenGrandpaNote = true;
                var noteContentTemplate = "{0}^^I may be gone, but I am still watching over you^Don't forget why you are here:^{1}^^-Grandpa";
                var goalGrandpaString = GoalCodeInjection.GetGoalStringGrandpa();
                var noteContent = string.Format(noteContentTemplate, _archipelago.SlotData.SlotName, goalGrandpaString);
                Game1.activeClickableMenu = new LetterViewerMenu(noteContent);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_GrandpaNote_PreFix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool SpawnWeedsAndStones_ConsiderUserPreference_PreFix(GameLocation __instance, ref int numDebris, bool weedsOnly, bool spawnFromOldWeeds)
        {
            try
            {
                if (numDebris <= 0)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;;
                }

                if (Game1.getFarm().isBuildingConstructed("Gold Clock") && _locationsWithGoldenClockEffect.Contains(__instance.Name))
                {
                    numDebris = 0;
                }

                switch (_archipelago.SlotData.DebrisMultiplier)
                {
                    case DebrisMultiplier.Vanilla:
                        break;
                    case DebrisMultiplier.HalfDebris:
                        numDebris /= 2;
                        break;
                    case DebrisMultiplier.QuarterDebris:
                        numDebris /= 4;
                        break;
                    case DebrisMultiplier.NoDebris:
                        numDebris = 0;
                        break;
                    case DebrisMultiplier.StartClear:
                        if (Game1.Date.TotalDays == 0)
                        {
                            numDebris = 0;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SpawnWeedsAndStones_ConsiderUserPreference_PreFix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static void DeleteStartingDebris()
        {
            try
            {
                if (Game1.Date.TotalDays >= 1)
                {
                    return;
                }

                var farm = Game1.getFarm();
                var chanceOfStaying = GetChanceOfStaying();
                for (var i = farm.resourceClumps.Count - 1; i >= 0; i--)
                {
                    var clump = farm.resourceClumps[i];
                    if (Game1.random.NextDouble() > chanceOfStaying)
                    {
                        farm.removeEverythingFromThisTile((int)clump.Tile.X, (int)clump.Tile.Y);
                    }
                }

                foreach (var (tile, feature) in farm.terrainFeatures.Pairs)
                {
                    if (!(feature is Tree) && !(feature is Grass))
                    {
                        continue;
                    }
                    if (Game1.random.NextDouble() > chanceOfStaying)
                    {
                        farm.removeEverythingFromThisTile((int)tile.X, (int)tile.Y);
                    }
                }
                foreach (var (tile, obj) in farm.Objects.Pairs)
                {
                    if (obj.name != "Stone" && !obj.name.StartsWith("Weed") && obj.name != "Twig")
                    {
                        continue;
                    }

                    if (Game1.random.NextDouble() > chanceOfStaying)
                    {
                        farm.removeEverythingFromThisTile((int)tile.X, (int)tile.Y);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DeleteStartingDebris)}:\n{ex}");
                return;
            }
        }

        private static double GetChanceOfStaying()
        {
            switch (_archipelago.SlotData.DebrisMultiplier)
            {
                case DebrisMultiplier.Vanilla:
                    return 1;
                case DebrisMultiplier.HalfDebris:
                    return 0.5;
                case DebrisMultiplier.QuarterDebris:
                    return 0.25;
                case DebrisMultiplier.NoDebris:
                    return 0;
                case DebrisMultiplier.StartClear:
                    if (Game1.Date.TotalDays < 1)
                    {
                        return 0;
                    }
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void PlaceAutoBuildings()
        {
            try
            {
                var farm = Game1.getFarm();
                ConstructStarterShippingBin(farm);
                ConstructStarterPetBowl(farm);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlaceAutoBuildings)}:\n{ex}");
                return;
            }
        }

        private static void ConstructStarterShippingBin(Farm farm)
        {
            var blueprint = "Shipping Bin";
            if (!_archipelago.HasReceivedItem(blueprint))
            {
                return;
            }

            if (FindShippingBins(farm).Any())
            {
                return;
            }

            var tile = farm.GetStarterShippingBinLocation();
            var collisionMask = CollisionMask.Buildings | CollisionMask.Characters | CollisionMask.Farmers | CollisionMask.Flooring | CollisionMask.Furniture | CollisionMask.Objects;
            for (var x = 0; x < 2; x++)
            {
                var tileLocation = new Vector2(tile.X + x, tile.Y);
                var isBuildable = !farm.IsTileOccupiedBy(tileLocation, collisionMask) &&
                                  farm.GetFurnitureAt(tileLocation) == null;
                if (!isBuildable)
                {
                    return;
                }
            }

            foreach (var building in farm.buildings)
            {
                if (building.buildingType.Value == blueprint)
                {
                    return;
                }
            }

            var shippingBin = Building.CreateInstanceFromId(blueprint, tile);
            shippingBin.load();
            farm.buildings.Add(shippingBin);
        }

        private static void ConstructStarterPetBowl(Farm farm)
        {
            var blueprint = "Pet Bowl";
            if (!_archipelago.HasReceivedItem(blueprint))
            {
                return;
            }

            if (FindPetBowls(farm).Any())
            {
                return;
            }

            farm.AddDefaultBuilding(blueprint, farm.GetStarterPetBowlLocation());
        }

        public static List<ShippingBin> FindShippingBins(Farm farm)
        {
            var shippingBins = new List<ShippingBin>();
            foreach (var building in farm.buildings)
            {
                if (building is ShippingBin bin)
                {
                    shippingBins.Add(bin);
                }
            }

            return shippingBins;
        }

        public static List<PetBowl> FindPetBowls(Farm farm)
        {
            var petBowls = new List<PetBowl>();
            foreach (var building in farm.buildings)
            {
                if (building is PetBowl petBowl)
                {
                    petBowls.Add(petBowl);
                }
            }

            return petBowls;
        }

        public static void ForcePetIfNeeded(Mailman mailman)
        {
            try
            {
                if (!Game1.player.hasOrWillReceiveMail("rejectedPet") || !IsPetRequired())
                {
                    return;
                }

                const string forcedPetName = "alwaysintreble";
                var pet = new Pet(68, 13, Game1.player.whichPetBreed, Game1.player.whichPetType);
                pet.warpToFarmHouse(Game1.player);
                pet.Name = forcedPetName;
                pet.displayName = pet.Name;
                foreach (var building in Game1.getFarm().buildings)
                {
                    if (building is PetBowl petBowl && !petBowl.HasPet())
                    {
                        petBowl.AssignPet(pet);
                        break;
                    }
                }
                Game1.player.RemoveMail("rejectedPet");

                const string forcedPetMailKey = "petOverride";
                const string forcedPetMailTitle = "Don't dodge destiny";
                var animalType = Game1.player.catPerson ? "cat" : "dog";
                var scoutedInfo = GetScoutedInfoForPet();
                var forcedPetMailContent = $"I heard you rejected this poor {animalType} that she brought you.^" +
                                           "Look kid, you and I both know you'll need it down the line.^" +
                                           $"{scoutedInfo}^" +
                                           $"  Your friend, Mr. Qi[#]{forcedPetMailTitle}";

                mailman.GenerateMail(forcedPetMailKey, forcedPetMailContent);
                mailman.SendMail(forcedPetMailKey);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ForcePetIfNeeded)}:\n{ex}");
                return;
            }
        }

        private static string GetScoutedInfoForPet()
        {
            if (!IsPetFriendsanity())
            {
                return "What would your grandfather say if you abandoned this poor animal?";
            }

            var location = string.Format(FriendshipInjections.FRIENDSANITY_PATTERN, Friends.PET_NAME, 5);
            var scouted = _archipelago.ScoutStardewLocation(location);
            return $"After all, what would {scouted.PlayerName} do without their {scouted.ItemName}?";
        }

        private static bool IsPetRequired()
        {
            var isPetFriendsanity = IsPetFriendsanity();
            var isPetNeededForGoal = _archipelago.SlotData.Goal == Goal.GrandpaEvaluation;

            return isPetFriendsanity || isPetNeededForGoal;
        }

        private static bool IsPetFriendsanity()
        {
            return _archipelago.SlotData.Friendsanity != Friendsanity.None && _archipelago.SlotData.Friendsanity != Friendsanity.Bachelors;
        }

        // private void doLightningStrike(Farm.LightningStrikeEvent lightning)

        public static bool DoLightningStrike_ChanceToHitPlayer_Prefix(Farm __instance, Farm.LightningStrikeEvent lightning)
        {
            try
            {
                if (_state.NumberTimesCursed <= 0 || !Game1.currentLocation.IsOutdoors)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var hitChance = 0.1;
                var shouldHit = false;
                var iteration = _state.NumberTimesCursed;
                var random = Utility.CreateDaySaveRandom();
                while (iteration > 0 && !shouldHit)
                {
                    if (random.NextDouble() < hitChance)
                    {
                        shouldHit = true;
                    }
                    iteration--;
                }

                if (!shouldHit)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                HitPlayerWithLightning(lightning);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoLightningStrike_ChanceToHitPlayer_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void HitPlayerWithLightning(Farm.LightningStrikeEvent lightning)
        {
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
            Game1.playSound("thunder");
            var boltPosition = Game1.player.Position;
            if (lightning.destroyedTerrainFeature)
            {
                Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, boltPosition, false, false));
            }
            Utility.drawLightningBolt(boltPosition, Game1.currentLocation);
            Game1.player.health -= 100;
        }

    }
}
