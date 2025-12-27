using System.Collections.Generic;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class BundleRooms
    {
        private readonly ILogger _logger;
        public Dictionary<string, BundleRoom> Rooms { get; set; }
        public Dictionary<string, Bundle> BundlesByName { get; set; }

        public BundleRooms(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager itemManager, Dictionary<string, Dictionary<string, Dictionary<string, string>>> bundlesDictionary)
        {
            _logger = logger;
            Rooms = new Dictionary<string, BundleRoom>();
            BundlesByName = new Dictionary<string, Bundle>();

            foreach (var (roomName, roomBundles) in bundlesDictionary)
            {
                var room = new BundleRoom(_logger, archipelago, itemManager, roomName, roomBundles);
                Rooms.Add(roomName, room);
                foreach (var (name, bundle) in room.Bundles)
                {
                    BundlesByName.Add(name, bundle);
                }
            }
        }

        public Dictionary<string, string> ToStardewStrings()
        {
            var stardewStrings = new Dictionary<string, string>();
            foreach (var (roomName, bundleRoom) in Rooms)
            {
                if (roomName == APName.RACCOON_REQUESTS_ROOM)
                {
                    continue;
                }

                foreach (var (key, value) in bundleRoom.ToStardewStrings())
                {
                    stardewStrings.Add(key, value);
                }
            }

            return stardewStrings;
        }
    }
}
