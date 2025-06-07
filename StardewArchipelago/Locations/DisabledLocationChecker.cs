using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Locations.Jojapocalypse;
using StardewValley;

namespace StardewArchipelago.Locations
{
    public class DisabledLocationChecker : StardewLocationChecker
    {
        public DisabledLocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked) : base(logger, archipelago, locationsAlreadyChecked)
        {
        }

        public override void AddWalnutCheckedLocation(string locationName)
        {
            Game1.chatBox.addMessage($"Did you know that Joja also sells golden walnuts?. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
            return;
        }

        public override void AddCheckedLocations(string[] locationNames)
        {
            Game1.chatBox.addMessage($"Did you know that Joja also sells location checks?. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
            return;
        }

        public override void AddCheckedLocation(string locationName)
        {
            Game1.chatBox.addMessage($"Did you know that Joja also sells '{locationName}'. Pay us (a visit)!", JojaConstants.JOJA_COLOR);
            return;
        }
    }
}
