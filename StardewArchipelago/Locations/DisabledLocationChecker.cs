using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewValley;
using StardewValley.Extensions;

namespace StardewArchipelago.Locations
{
    public class DisabledLocationChecker : StardewLocationChecker
    {
        public HashSet<string> LocationsAlreadyAttemptedToCheck;

        public DisabledLocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked, List<string> locationsAlreadyFailedToCheck) : base(logger, archipelago, locationsAlreadyChecked)
        {
            LocationsAlreadyAttemptedToCheck = new HashSet<string>(locationsAlreadyFailedToCheck);
        }

        public override void AddCheckedLocations(string[] locationNames)
        {
            if (locationNames.All(x => LocationsAlreadyAttemptedToCheck.Contains(x)))
            {
                return;
            }

            Game1.chatBox.addMessage($"Did you know that Joja also sells location checks?. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
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
                Game1.chatBox.addMessage($"Did you know that Joja also sells '{locationName}'. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
            }
            LocationsAlreadyAttemptedToCheck.Add(locationName);
            return;
        }
    }
}
