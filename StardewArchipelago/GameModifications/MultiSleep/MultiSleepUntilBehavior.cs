using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewArchipelago.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;

namespace StardewArchipelago.GameModifications.MultiSleep
{
    public class MultiSleepUntilBehavior : MultiSleepBehavior
    {
        public const string RAIN = "Rain";
        public const string STORM = "Storm";
        public const string FESTIVAL = "Festival";
        public const string BIRTHDAY = "Birthday";
        public const string TRAVELING_CART = "Traveling Cart";
        public const string BOOKSELLER = "Bookseller";
        public const string ANY_CROP_READY = "Any Crop Ready";
        public const string ALL_CROPS_READY = "All Crops Ready";
        public const string END_OF_MONTH = "End of Month";
        public const string HIBERNATE = "Hibernate";

        private string _untilWhat;

        public MultiSleepUntilBehavior(string untilWhat)
        {
            _untilWhat = untilWhat;
        }

        public override bool ShouldKeepSleeping()
        {
            bool allReady;
            switch (_untilWhat)
            {
                case RAIN:
                    return !Game1.isRaining && !Game1.isLightning && !Game1.isGreenRain;
                case STORM:
                    return !Game1.isLightning;
                case FESTIVAL:
                    return !IsFestivalDay();
                case BIRTHDAY:
                    NPC birthdayPerson = null;
                    Utility.ForEachVillager(n =>
                    {
                        if (n.Birthday_Season == Game1.currentSeason && n.Birthday_Day == Game1.dayOfMonth)
                        {
                            birthdayPerson = n;
                        }
                        return birthdayPerson == null;
                    });
                    return birthdayPerson == null;
                case TRAVELING_CART:
                    return !TravelingMerchantInjections.IsTravelingMerchantDay(Game1.dayOfMonth);
                case BOOKSELLER:
                    return !Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth);
                case ANY_CROP_READY:
                    CheckAllCrops(out _, out var anyReady, out allReady);
                    return !anyReady && !allReady;
                case ALL_CROPS_READY:
                    CheckAllCrops(out var anyNotReady, out _, out allReady);
                    return anyNotReady && !allReady;
                case HIBERNATE:
                    return ArchipelagoJunimoNoteMenu.IsBundleRemaining(MemeBundleNames.HIBERNATION);
                default:
                    return false;
            }
        }

        private static bool IsFestivalDay()
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.season))
            {
                return true;
            }

            foreach (var passiveFestivalData in DataLoader.PassiveFestivals(Game1.content).Values)
            {
                if (Game1.dayOfMonth < passiveFestivalData.StartDay || Game1.dayOfMonth > passiveFestivalData.EndDay || Game1.season != passiveFestivalData.Season || !GameStateQuery.CheckConditions(passiveFestivalData.Condition))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public static void CheckAllCrops(out bool anyNotReady, out bool anyReady, out bool allReady)
        {
            anyNotReady = false;
            anyReady = false;
            allReady = true;
            foreach (var location in Game1.locations)
            {
                CheckAllCrops(location, out var anyNotReadyHere, out var anyReadyHere, out var allReadyHere);
                anyNotReady = anyNotReady || anyNotReadyHere;
                anyReady = anyReady || anyReadyHere;
                allReady = allReady && allReadyHere;

                foreach (var building in location.buildings)
                {
                    if (building.GetIndoorsType() != IndoorsType.Instanced)
                    {
                        continue;
                    }

                    var indoors = building.GetIndoors();
                    if (indoors == null)
                    {
                        continue;
                    }

                    CheckAllCrops(indoors, out var anyNotReadyInside, out var anyReadyInside, out var allReadyInside);
                    anyNotReady = anyNotReady || anyNotReadyInside;
                    anyReady = anyReady || anyReadyInside;
                    allReady = allReady && allReadyInside;
                }
            }
        }

        public static void CheckAllCrops(GameLocation location, out bool anyNotReady, out bool anyReady, out bool allReady)
        {
            anyNotReady = false;
            anyReady = false;
            allReady = true;
            foreach (var terrainFeature in location.terrainFeatures.Values)
            {
                if (terrainFeature is not HoeDirt hoeDirt)
                {
                    continue;
                }
                if (!location.IsOutdoors && hoeDirt.needsWatering() && !hoeDirt.isWatered())
                {
                    continue;
                }
                if (location.IsOutdoors && hoeDirt.needsWatering() && !hoeDirt.isWatered() && location.GetSeason() == Season.Winter)
                {
                    continue;
                }
                CheckCrop(hoeDirt, ref anyNotReady, ref anyReady, ref allReady);
            }
            foreach (var locationObject in location.objects.Values)
            {
                if (locationObject is not IndoorPot indoorPot)
                {
                    continue;
                }
                var hoeDirt = indoorPot.hoeDirt.Value;
                if (hoeDirt.needsWatering() && !hoeDirt.isWatered())
                {
                    continue;
                }
                CheckCrop(hoeDirt, ref anyNotReady, ref anyReady, ref allReady);
            }
        }

        public static void CheckCrop(HoeDirt hoeDirt, ref bool anyNotReady, ref bool anyReady, ref bool allReady)
        {
            if (hoeDirt == null || hoeDirt.crop == null || hoeDirt.crop.forageCrop.Value || hoeDirt.crop.dead.Value)
            {
                return;
            }

            if (hoeDirt.readyForHarvest())
            {
                anyReady = true;
            }
            else
            {
                anyNotReady = true;
                allReady = false;
            }
        }

        public override bool ShouldPromptForMultisleep()
        {
            return false;
        }
    }
}
