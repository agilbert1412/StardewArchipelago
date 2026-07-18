using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.Extensions
{
    public static class ColorExtensions
    {
        private static readonly Dictionary<int, Color> _cache = new();

        public static Color GetAsBrightColor(this string name)
        {
            if (name == null)
            {
                Debugger.Break();
                return "".GetHash().GetAsBrightColor();
            }
            var hash = name.GetHash();
            return hash.GetAsBrightColor();
        }

        public static Color GetAsBrightColor(this int hashedValue)
        {
            if (_cache.ContainsKey(hashedValue))
            {
                return _cache[hashedValue];
            }

            var random = new Random(hashedValue);
            var red = random.Next(0, 256);
            var green = random.Next(0, 256);
            var blue = random.Next(0, 256);
            var components = ComputeComponentStats(red, green, blue, out var total, out var average, out var difference);
            while (total < 384 || difference < 48 || components.Any(x => Math.Abs(average - x) < 16) || total > 768)
            {
                var whichToChange = random.Next(0, 3);
                switch (whichToChange)
                {
                    case 0:
                        red = random.Next(0, 256);
                        break;
                    case 1:
                        green = random.Next(0, 256);
                        break;
                    case 2:
                        blue = random.Next(0, 256);
                        break;
                }
                components = ComputeComponentStats(red, green, blue, out total, out average, out difference);
            }

            var resultingColor = new Color(red, green, blue);
            _cache.Add(hashedValue, resultingColor);
            return resultingColor;
        }

        private static int[] ComputeComponentStats(int red, int green, int blue, out int total, out double average, out int difference)
        {

            var components = new[] { red, green, blue };
            total = components.Sum();
            var highest = components.Max();
            var lowest = components.Min();
            average = components.Average();
            difference = highest - lowest;
            return components;
        }
    }
}
