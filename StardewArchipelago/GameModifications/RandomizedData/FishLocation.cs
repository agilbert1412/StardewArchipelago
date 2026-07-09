using System;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class FishLocation
    {
        public string FishName { get; }
        public string LocationId { get; }

        public FishLocation(string fishName, string locationId)
        {
            FishName = fishName;
            LocationId = locationId;
        }

        public override bool Equals(object obj)
        {
            if (obj is not FishLocation otherFishLocation)
            {
                return false;
            }

            return Equals(otherFishLocation);
        }

        protected bool Equals(FishLocation other)
        {
            return FishName == other.FishName && LocationId == other.LocationId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FishName, LocationId);
        }

        public override string ToString()
        {
            return $"{FishName} ({LocationId})";
        }
    }
}
