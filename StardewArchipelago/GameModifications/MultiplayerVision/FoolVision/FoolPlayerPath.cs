using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.EntranceRandomizer;

namespace StardewArchipelago.GameModifications.MultiplayerVision.FoolVision
{
    public class FoolPlayerPath
    {
        public string MapName { get; set; }
        public List<FoolPlayerDataPoint> DataPoints { get; set; }

        public FoolPlayerPath(string mapName)
        {
            MapName = mapName;
            DataPoints = new List<FoolPlayerDataPoint>();
        }

        public void AddDataPoint(Vector2 position, Vector2 velocity, int facingDirection)
        {
            var dataPoint = new FoolPlayerDataPoint()
            {
                Position = position,
                Velocity = velocity,
                FacingDirection = facingDirection,
            };
            AddDataPoint(dataPoint);
        }

        public void AddDataPoint(FoolPlayerDataPoint dataPoint)
        {
            DataPoints.Add(dataPoint);
        }
    }
}
