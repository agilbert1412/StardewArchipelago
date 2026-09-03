using System;

namespace StardewArchipelago.Extensions
{
    public static class FloatExtensions
    {
        public static bool IsApproximately(this float value1, float value2, float tolerance = 0.01f)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }
    }
}
