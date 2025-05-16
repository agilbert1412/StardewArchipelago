using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Bundles
{
    public class BundleRoom
    {
        private readonly ILogger _logger;
        public string Name { get; }
        public Dictionary<string, Bundle> Bundles { get; }

        public BundleRoom(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager itemManager, string name, Dictionary<string, Dictionary<string, string>> bundles)
        {
            _logger = logger;
            Name = name;
            Bundles = new Dictionary<string, Bundle>();
            foreach (var (bundleName, bundleContent) in bundles)
            {
                try
                {
                    var bundle = Bundle.Parse(archipelago, itemManager, Name, bundleName, bundleContent);
                    Bundles.Add(bundleName, bundle);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error Parsing Bundle {bundleName}, with content {bundleContent}. Error: {ex}");
                }
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
