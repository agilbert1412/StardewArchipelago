using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.Extensions
{
    public static class MoneyExtensions
    {
        public static bool IsUnlimited(this int money)
        {
            return money < 0;
        }
    }
}
