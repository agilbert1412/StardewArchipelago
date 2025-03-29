using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.GameModifications.MoveLink
{
    public class ReceivedMovement
    {
        public float TimeRemaining { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public ReceivedMovement(float timeRemaining, float x, float y)
        {
            TimeRemaining = timeRemaining;
            X = x;
            Y = y;
        }
    }
}
