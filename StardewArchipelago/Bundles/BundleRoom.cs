﻿using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class BundleRoom
    {
        public string Name { get; }
        public Dictionary<string, Bundle> Bundles { get; }

        public BundleRoom(StardewItemManager itemManager, string name, Dictionary<string, Dictionary<string, string>> bundles)
        {
            Name = name;
            Bundles = new Dictionary<string, Bundle>();
            foreach (var (bundleName, bundleContent) in bundles)
            {
                var bundle = Bundle.Parse(itemManager, Name, bundleName, bundleContent);
                Bundles.Add(bundleName, bundle);
            }
        }

        public Dictionary<string, string> ToStardewStrings()
        {
            var stardewStrings = new Dictionary<string, string>();
            foreach (var (bundleName, bundle) in Bundles)
            {
                var nameWithoutBundle = bundle.NameWithoutBundle;
                var spriteIndex = bundle.SpriteIndex;
                var colorIndex = bundle.ColorIndex;
                var itemsString = bundle.GetItemsString();
                var numberRequiredItems = bundle.GetNumberRequiredItems();

                var key = $"{Name}/{spriteIndex}";
                var value = $"{nameWithoutBundle}//{itemsString}/{colorIndex}/{numberRequiredItems}//{nameWithoutBundle}";


                stardewStrings.Add(key, value);
            }

            return stardewStrings;
        }
    }
}
