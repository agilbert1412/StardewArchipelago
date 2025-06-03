using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago.ApworldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.Festival;
using StardewValley;
using xTile.Dimensions;
using StardewValley.Locations;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.ShopStockModifiers;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseFiltering
    {
        private readonly ILogger _logger;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly LocationChecker _locationChecker;

        public JojapocalypseFiltering(ILogger logger, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public bool CanPurchaseJojapocalypseLocation(StardewArchipelagoLocation location)
        {
            var nameWords = location.Name.Split(" ");
            for (var i = 0; i < nameWords.Length; i++)
            {
                if (!ValidateNumberedLocation(nameWords, i))
                {
                    return false;
                }

                if (!ValidateKeywordsLocation(nameWords, i))
                {
                    return false;
                }
            }

            if (!ValidateMasteriesLocation(location.Name))
            {
                return false;
            }

            if (!ValidateNPCLocation(location.Name))
            {
                return false;
            }

            if (!ValidateSeasonalLocation(location))
            {
                return false;
            }

            if (!ValidateSpecialOrderLocation(location))
            {
                return false;
            }

            if (!ValidateHarvestLocation(location))
            {
                return false;
            }

            if (!ValidateSecretNoteLocation(location.Name))
            {
                return false;
            }

            if (!ValidateBundleLocation(location))
            {
                return false;
            }

            if (!ValidateFishsanityLocation(location))
            {
                return false;
            }

            if (!ValidateShipsanityLocation(location))
            {
                return false;
            }

            if (!ValidateCooksanityLocation(location))
            {
                return false;
            }

            if (!ValidateMovieLocation(location))
            {
                return false;
            }

            return true;
        }

        private bool ValidateNumberedLocation(string[] nameWords, int i)
        {
            if (!int.TryParse(nameWords[i], out var number))
            {
                return true;
            }

            if (number <= 1)
            {
                return true;
            }

            for (var j = number - 1; j >= 0; j--)
            {
                var previousNumberLocation = MergeWords(nameWords, i, j.ToString());
                if (_locationChecker.IsLocationMissing(previousNumberLocation))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateKeywordsLocation(string[] nameWords, int i)
        {
            var word = nameWords[i];

            if (!_keywordsToReplace.ContainsKey(word))
            {
                return true;
            }

            var keywords = _keywordsToReplace[word];

            foreach (var keyword in keywords)
            {
                var replacedLocation = MergeWords(nameWords, i, keyword);
                if (_locationChecker.IsLocationMissing(replacedLocation))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateMasteriesLocation(string locationName)
        {
            var words = locationName.Split(" ");
            if (words.Length != 2)
            {
                return true;
            }

            var skill = words[0];
            var mastery = words[1];
            if (mastery != "Mastery")
            {
                return true;
            }

            for (var i = 0; i <= 10; i++)
            {
                var skillLocation = $"Level {i} {skill}";
                if (_locationChecker.IsLocationMissing(skillLocation))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateNPCLocation(string locationName)
        {
            foreach (var (name, characterData) in Game1.characterData)
            {
                if (characterData.CanSocialize != null && characterData.CanSocialize.Equals("FALSE", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (!locationName.Contains(name))
                {
                    continue;
                }

                if (!Game1.player.friendshipData.ContainsKey(name) || Game1.player.friendshipData[name].Points <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateSeasonalLocation(StardewArchipelagoLocation location)
        {
            if (!ValidateFestivalLocation(location))
            {
                return false;
            }
            if (!ValidateQuestLocation(location))
            {
                return false;
            }
            return true;
        }

        private bool ValidateFestivalLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.FESTIVAL) && !location.LocationTags.Contains(LocationTag.FESTIVAL_HARD))
            {
                return true;
            }

            foreach (var (festivalIdentifier, locations) in FestivalLocationNames.LocationsByFestival)
            {
                if (!locations.Contains(location.Name))
                {
                    continue;
                }

                var season = festivalIdentifier.Split(" ").First();
                return SeasonsRandomizer.GetUnlockedSeasons().Contains(season);
            }

            return true;
        }

        private bool ValidateQuestLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.STORY_QUEST))
            {
                return true;
            }

            return location.Name switch
            {
                "Robin's Lost Axe" or "Jodi's Request" or "Fresh Fruit" or "Granny's Gift" => SeasonsRandomizer.GetUnlockedSeasons().Contains("Spring"),
                "Crop Research" or "Knee Therapy" or "Aquatic Research" or "A Soldier's Star" => SeasonsRandomizer.GetUnlockedSeasons().Contains("Summer"),
                "Blackberry Basket" or "Cow's Delight" or "Carving Pumpkins" => SeasonsRandomizer.GetUnlockedSeasons().Contains("Fall"),
                "A Winter Mystery" or "Catch A Squid" or "Fish Stew" or "Catch a Lingcod" => SeasonsRandomizer.GetUnlockedSeasons().Contains("Winter"),
                _ => true,
            };
        }

        private bool ValidateSpecialOrderLocation(StardewArchipelagoLocation location)
        {
            if (!ValidateOrderBoardLocation(location))
            {
                return false;
            }

            if (!ValidateQiOrderLocation(location))
            {
                return false;
            }

            return true;
        }

        private bool ValidateOrderBoardLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.SPECIAL_ORDER_BOARD))
            {
                return true;
            }

            return _archipelago.HasReceivedItem(VanillaUnlockManager.SPECIAL_ORDER_BOARD_AP_NAME);
        }

        private bool ValidateQiOrderLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.SPECIAL_ORDER_QI))
            {
                return true;
            }

            return true;
        }

        private bool ValidateHarvestLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.CROPSANITY))
            {
                return true;
            }

            var cropName = location.Name["Harvest ".Length..];
            var seedNames = new[]
            {
                $"{cropName} Seeds",
                $"{cropName} Starter",
                $"{cropName} Bulb",
                $"{cropName} Shoot",
                $"{cropName} Sapling",
                $"{cropName} Tuber",
                $"{cropName} Bean",
            };

            var oneItemExists = seedNames.Any(x => _archipelago.DataPackageCache.GetLocalItemId(x) != -1);
            if (!oneItemExists)
            {
                return true;
            }

            return seedNames.Any(x => _archipelago.HasReceivedItem(x));
        }

        private bool ValidateSecretNoteLocation(string locationName)
        {
            if (!locationName.Contains("Secret Note"))
            {
                return true;
            }

            return _archipelago.HasReceivedItem("Magnifying Glass");
        }

        private bool ValidateBundleLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.COMMUNITY_CENTER_ROOM))
            {
                return true;
            }

            var room = location.Name["Complete ".Length..];

            var communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");
            foreach (var (bundleKey, bundleData) in Game1.netWorldState.Value.BundleData)
            {
                var bundleRoom = bundleKey.Split("/").First();
                if (!bundleRoom.Equals(room))
                {
                    continue;
                }

                var bundleIndex = int.Parse(bundleKey.Split("/").Last());
                var bundleName = bundleData.Split("/").First();
                var isComplete = communityCenter.isBundleComplete(bundleIndex);
                if (!isComplete || _locationChecker.IsLocationMissing($"{bundleName} Bundle"))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateFishsanityLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.FISHSANITY))
            {
                return true;
            }

            return _archipelago.SlotData.Fishsanity == Fishsanity.None || _archipelago.HasReceivedItem(ToolUnlockManager.PROGRESSIVE_FISHING_ROD);
        }

        private bool ValidateShipsanityLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.SHIPSANITY))
            {
                return true;
            }

            return !_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive) || _archipelago.HasReceivedItem(CarpenterShopStockModifier.BUILDING_SHIPPING_BIN);
        }

        private bool ValidateCooksanityLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.COOKSANITY))
            {
                return true;
            }

            return !_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive) || _archipelago.HasReceivedItem(CarpenterInjections.BUILDING_PROGRESSIVE_HOUSE);
        }

        private bool ValidateMovieLocation(StardewArchipelagoLocation location)
        {
            if (!location.LocationTags.Contains(LocationTag.ANY_MOVIE) && !location.LocationTags.Contains(LocationTag.MOVIE) && !location.LocationTags.Contains(LocationTag.MOVIE_SNACK))
            {
                return true;
            }

            return _archipelago.GetReceivedItemCount(APItem.MOVIE_THEATER) >= 2;
        }

        private string MergeWords(string[] nameWords, int splitIndex, string replacementItem)
        {
            var previousNumberLocationNameWords = new List<string>();
            previousNumberLocationNameWords.AddRange(nameWords.Take(splitIndex));
            if (!string.IsNullOrWhiteSpace(replacementItem))
            {
                previousNumberLocationNameWords.Add(replacementItem);
            }
            previousNumberLocationNameWords.AddRange(nameWords.Skip(splitIndex + 1));
            var previousNumberLocation = string.Join(" ", previousNumberLocationNameWords);
            return previousNumberLocation;
        }

        private readonly Dictionary<string, string[]> _keywordsToReplace = new()
        {
            // Tools
            { "Basic", new[] { "" } },
            { "Copper", new[] { "", "Basic" } },
            { "Iron", new[] { "", "Basic", "Copper" } },
            { "Steel", new[] { "", "Basic", "Copper" } },
            { "Gold", new[] { "", "Basic", "Copper", "Iron", "Steel" } },
            { "Iridium", new[] { "", "Basic", "Copper", "Iron", "Steel", "Gold" } },

            // Backpacks & Buildings
            { "Big", new[] { "" } },
            { "Large", new[] { "" } },
            { "Deluxe", new[] { "", "Big", "Large" } },
            { "Premium", new[] { "", "Large", "Deluxe" } },
        };
    }
}
