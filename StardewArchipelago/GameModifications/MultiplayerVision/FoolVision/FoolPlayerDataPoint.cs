using Microsoft.Xna.Framework;

namespace StardewArchipelago.GameModifications.MultiplayerVision.FoolVision
{
    public class FoolPlayerDataPoint
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public uint Duration { get; set; }

        public FoolPlayerDataPoint()
        {
            Duration = 5;
        }
    }
}
