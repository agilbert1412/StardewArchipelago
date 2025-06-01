using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json.Linq;

namespace StardewArchipelago.Archipelago.ApworldData
{
    public class StardewArchipelagoLocation : ArchipelagoLocation
    {
        public HashSet<string> LocationTags { get; private set; }
        public HashSet<string> ContentPacks { get; private set; }

        public StardewArchipelagoLocation(string name, long id, HashSet<string> locationTags, HashSet<string> contentPacks) : base(name, id)
        {
            LocationTags = locationTags;
            ContentPacks = contentPacks;
        }
    }
}
