namespace StardewArchipelago.GameModifications.Entrances
{
    public class LocationTransport
    {
        public Entrance Origin { get; set; }
        public Entrance Destination { get; set; }

        public LocationTransport()
        {

        }

        public LocationTransport(Entrance origin, Entrance destination)
        {
            Origin = origin;
            Destination = destination;
        }

        public override bool Equals(object obj)
        {
            if (obj is not LocationTransport otherTransport)
            {
                return false;
            }

            return Origin.Equals(otherTransport.Origin) && Destination.Equals(otherTransport.Destination);
        }

        public override int GetHashCode()
        {
            return Origin.GetHashCode() ^ Destination.GetHashCode();
        }

        public LocationTransport DeepCopy()
        {
            return new LocationTransport(new Entrance(Origin.LocationName, Origin.Tile),
                new Entrance(Destination.LocationName, Destination.Tile));
        }
    }
}
