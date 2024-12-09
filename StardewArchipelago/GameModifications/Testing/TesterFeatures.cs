using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.Testing
{
    public class TesterFeatures
    {
        private const string TESTER_FEATURES_FILE = "tester.json";
        public readonly TesterFeature Multiplayer = new(TesterFeatureNames.MULTIPLAYER, 0);
        public readonly TesterFeature UnstableMods = new(TesterFeatureNames.UNSTABLE_MODS, VerifyMods.MODS_AND_VERSIONS);
        private readonly Dictionary<string, TesterFeature> _featuresByName = new();

        public TesterFeatures(ILogger logger, IModHelper modHelper)
        {
            _featuresByName.Add(Multiplayer.Name, Multiplayer);
            _featuresByName.Add(UnstableMods.Name, UnstableMods);

#if DEBUG
            UnstableMods.Value = VerifyMods.NOTHING;
#endif

            try
            {
                var entries = modHelper.Data.ReadJsonFile<Dictionary<string, int>>(TESTER_FEATURES_FILE);
                if (entries == null || !entries.Any())
                {
                    return;
                }

                foreach (var (name, value) in entries)
                {
                    if (!_featuresByName.ContainsKey(name))
                    {
                        continue;
                    }

                    _featuresByName[name].Value = value;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed at reading the TesterFeatures file. The file is probably corrupted and should be deleted to start fresh, or fixed manually");
            }
        }
    }
}
