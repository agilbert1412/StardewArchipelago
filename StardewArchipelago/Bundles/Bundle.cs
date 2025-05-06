using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Constants;
using StardewArchipelago.Extensions;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public abstract class Bundle
    {
        protected const string NUMBER_REQUIRED_KEY = "number_required";
        private const string BUNDLE_SUFFIX = " Bundle";
        private string _name;

        public string RoomName { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NameWithoutBundle = _name.EndsWith(BUNDLE_SUFFIX) ? _name[..^BUNDLE_SUFFIX.Length] : _name;
            }
        }

        public string NameWithoutBundle { get; private set; }

        public int SpriteIndex
        {
            get
            {
                if (BundleIndexes.BundleSpriteIndexes.TryGetValue(NameWithoutBundle, out var spriteIndex))
                {
                    return spriteIndex;
                }

                var hash = NameWithoutBundle.GetHash();
                while (hash < 10000)
                {
                    hash *= 2;
                }
                return hash;
            }
        }

        public int ColorIndex
        {
            get
            {
                if (BundleIndexes.BundleColorIndexes.TryGetValue(NameWithoutBundle, out var colorIndex))
                {
                    return colorIndex;
                }

                var colors = BundleIndexes.BundleColorIndexes.Values.Distinct().ToArray();
                var random = new Random(NameWithoutBundle.GetHash());
                var chosenColor = random.Next(0, colors.Length);
                return colors[chosenColor];
            }
        }

        public Bundle(string roomName, string bundleName)
        {
            RoomName = roomName;
            Name = bundleName;
        }

        public static Bundle Parse(StardewItemManager itemManager, string name, string bundleName, Dictionary<string, string> bundleContent)
        {
            if (bundleContent.Count() == 2 && bundleContent.Values.Any(x => CurrencyBundle.CurrencyIds.Keys.Contains(x.Split("|")[0])))
            {
                return new CurrencyBundle(name, bundleName, bundleContent);
            }

            if (bundleContent.Values.Any(x => MemeIDProvider.MemeItemIds.ContainsKey(x.Split("|")[0])))
            {
                return new MemeItemBundle(name, bundleName, bundleContent);
            }

            return new ItemBundle(itemManager, name, bundleName, bundleContent);
        }

        public abstract string GetItemsString();
        public abstract string GetNumberRequiredItems();
    }
}
