using System;

namespace StardewArchipelago.Extensions
{
    public static class FloatExtensions
    {
        public static bool IsApproximately(this float value1, float value2)
        {
            return Math.Abs(value1 - value2) < 0.01;
        }
    }
}
