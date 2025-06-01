using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.ItemSprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Internal;
using StardewValley.Objects;
using Color = Microsoft.Xna.Framework.Color;

namespace StardewArchipelago.Locations.InGameLocations
{
    internal class JojaObtainableArchipelagoLocation : ObtainableArchipelagoLocation
    {
        public JojaObtainableArchipelagoLocation(string locationName, LogHandler logger, IModHelper modHelper, LocationChecker locationChecker, StardewArchipelagoClient archipelago, Hint[] myActiveHints) : this(locationName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints)
        {
        }

        public JojaObtainableArchipelagoLocation(string locationDisplayName, string locationName, LogHandler logger, IModHelper modHelper, LocationChecker locationChecker, StardewArchipelagoClient archipelago, Hint[] myActiveHints) : base(locationDisplayName, locationName, logger, modHelper, locationChecker, archipelago, myActiveHints, false)
        {
        }
    }
}
