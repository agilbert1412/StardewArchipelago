using Microsoft.Xna.Framework;

namespace StardewArchipelago.GameModifications.Entrances
{
    public class Entrance
    {
        public string LocationName { get; set; }
        public Point Tile { get; set; }

        public Entrance()
        {

        }

        public Entrance(string locationName, Point tile)
        {
            LocationName = locationName;
            Tile = tile;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Entrance otherEntrance)
            {
                return false;
            }

            return LocationName == otherEntrance.LocationName && Tile == otherEntrance.Tile;
        }

        public override int GetHashCode()
        {
            return LocationName.GetHashCode() ^ Tile.GetHashCode();
        }
    }
}