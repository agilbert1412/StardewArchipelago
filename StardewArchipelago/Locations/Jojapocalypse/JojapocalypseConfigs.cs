using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseConfigs
    {
        public static JojapocalypseSetting Jojapocalypse { get; set; } = JojapocalypseSetting.Disabled;
        public static int StartPrice { get; set; } = 100;
        public static int EndPrice { get; set; } = 100000;
        public static bool UseExponentialPricing { get; set; } = true;
        public static int NumberOfPurchasesBeforeMembership { get; set; } = 10;
    }

    public enum JojapocalypseSetting
    {
        Disabled,
        Allowed,
        Forced,
    }
}
