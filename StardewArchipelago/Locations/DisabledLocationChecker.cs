using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewValley;
using StardewValley.Extensions;

namespace StardewArchipelago.Locations
{
    public class DisabledLocationChecker : StardewLocationChecker
    {
        public HashSet<string> LocationsAlreadyAttemptedToCheck;

        public DisabledLocationChecker(ILogger logger, StardewArchipelagoClient archipelago, List<string> locationsAlreadyChecked, List<string> locationsAlreadyFailedToCheck) : base(logger, archipelago, locationsAlreadyChecked)
        {
            LocationsAlreadyAttemptedToCheck = new HashSet<string>(locationsAlreadyFailedToCheck);
        }

        public override void AddCheckedLocations(string[] locationNames)
        {
            if (locationNames.All(x => LocationsAlreadyAttemptedToCheck.Contains(x)))
            {
                return;
            }

            Game1.chatBox.addMessage($"Did you know that Joja has a wide selection of items for sale?. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
            LocationsAlreadyAttemptedToCheck.AddRange(locationNames);
            return;
        }

        public override void AddCheckedLocation(string locationName)
        {
            if (LocationsAlreadyAttemptedToCheck.Contains(locationName))
            {
                return;
            }

            if (IsLocationMissing(locationName))
            {
                AdvertiseScoutedLocationAsync(locationName).FireAndForget();
            }
            LocationsAlreadyAttemptedToCheck.Add(locationName);
            return;
        }

        private async Task AdvertiseScoutedLocationAsync(string locationName)
        {
            var scout = _archipelago.ScoutSingleLocation(locationName, false);
            Game1.chatBox.addMessage($"Did you know that Joja sell a '{scout.ItemName}'. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
        }
    }
}
