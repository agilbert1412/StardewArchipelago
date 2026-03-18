using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.GameModifications.MultiplayerVision.FoolVision
{
    public class FoolPlayerPath
    {
        public string MapName { get; set; }
        public List<FoolPlayerDataPoint> DataPoints { get; set; }

        public FoolPlayerPath(string mapName)
        {
            MapName = mapName;
        }

        public void AddDataPoint(Vector2 position, Vector2 velocity)
        {
            var dataPoint = new FoolPlayerDataPoint()
            {
                Position = position,
                Velocity = velocity,
            };
            AddDataPoint(dataPoint);
        }

        public void AddDataPoint(FoolPlayerDataPoint dataPoint)
        {
            DataPoints.Add(dataPoint);
        }
    }
}
