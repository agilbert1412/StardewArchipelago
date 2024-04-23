using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class BundleRooms
    {
        public Dictionary<string, BundleRoom> Rooms { get; set; }

        public BundleRooms(StardewItemManager itemManager, Dictionary<string, Dictionary<string, Dictionary<string, string>>> bundlesDictionary)
        {
            Rooms = new Dictionary<string, BundleRoom>();

            foreach (var (roomName, roomBundles) in bundlesDictionary)
            {
                var room = new BundleRoom(itemManager, roomName, roomBundles);
                Rooms.Add(roomName, room);
            }
        }

        public Dictionary<string, string> ToStardewStrings()
        {
            var stardewStrings = new Dictionary<string, string>();
            foreach (var (_, bundleRoom) in Rooms)
            {
                foreach (var (key, value) in bundleRoom.ToStardewStrings())
                {
                    stardewStrings.Add(key, value);
                }
            }

            return stardewStrings;
        }
    }
}
