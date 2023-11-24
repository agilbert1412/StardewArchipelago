using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class CurrencyBundle : Bundle
    {
        public string Currency { get; }
        public int Amount { get; set; }

        public CurrencyBundle(string roomName, string bundleName, Dictionary<string, string> bundleContent) : base(roomName, bundleName)
        {
            RoomName = roomName;
            Name = bundleName;

            foreach (var (currency, details) in bundleContent)
            {
                if (currency == NUMBER_REQUIRED_KEY)
                {
                    continue;
                }

                Currency = currency;
                var splitDetails = details.Split(' ');
                Amount = int.Parse(splitDetails[0]);
                return;
            }
        }

        public override string GetItemsString()
        {
            return $"{CurrencyIds[Currency]} {Amount} {Amount}";
        }

        public override string GetNumberRequiredItemsWithSeparator()
        {
            return "";
        }

        public static readonly Dictionary<string, int> CurrencyIds = new Dictionary<string, int>()
        {
            {"Money", -1}, 
            {"Star Token", -2},
            {"Qi Coin", -3},
            {"Golden Walnut", -4},
            {"Qi Gem", -5}
        };
    }
}
