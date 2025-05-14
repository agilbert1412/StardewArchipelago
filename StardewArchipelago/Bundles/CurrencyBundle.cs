using System.Collections.Generic;
using StardewArchipelago.Constants;

namespace StardewArchipelago.Bundles
{
    public class CurrencyBundle : Bundle
    {
        public string Currency { get; }
        public int Amount { get; set; }

        public CurrencyBundle(string roomName, string bundleName, Dictionary<string, string> bundleContent) : base(roomName, bundleName)
        {
            foreach (var (key, itemDetails) in bundleContent)
            {
                if (key == NUMBER_REQUIRED_KEY)
                {
                    continue;
                }

                var itemFields = itemDetails.Split("|");
                Currency = itemFields[0];
                Amount = int.Parse(itemFields[1]);
                return;
            }
        }

        public override string GetItemsString()
        {
            return $"{CurrencyIds[Currency]} {Amount} {Amount}";
        }

        public override string GetNumberRequiredItems()
        {
            return "";
        }

        public static readonly Dictionary<string, string> CurrencyIds = new()
        {
            { "Money", IDProvider.MONEY },
            { "Star Token", IDProvider.STAR_TOKEN },
            { "Qi Coin", IDProvider.QI_COIN },
            // {"Golden Walnut", IDProvider.GOLDEN_WALNUT},
            { "Qi Gem", IDProvider.QI_GEM },

            { "Blood", MemeIDProvider.BLOOD },
            { "Energy", MemeIDProvider.ENERGY },
            { "Time", MemeIDProvider.TIME },
            { "Clic", MemeIDProvider.CLIC },
            { "CookieClics", MemeIDProvider.COOKIES_CLICKS },
            { "Death", MemeIDProvider.DEATH },
            { "Steps", MemeIDProvider.STEP },
            { "Time Elapsed", MemeIDProvider.TIME_ELAPSED },
            { "Dead Crop", MemeIDProvider.DEAD_CROP },
            { "Dead Pumpkins", MemeIDProvider.DEAD_PUMPKIN },
            { "Child", MemeIDProvider.CHILD },
        };
    }
}
