using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewModdingAPI;

namespace StardewArchipelago.GameModifications.Testing
{
    public class TesterFeatures
    {
        private const string TESTER_FEATURES_FILE = "tester.json";
        public readonly TesterFeature Multiplayer = new(TesterFeatureNames.MULTIPLAYER, 0);
        public readonly TesterFeature UnstableMods = new(TesterFeatureNames.UNSTABLE_MODS, VerifyMods.MODS_AND_VERSIONS);
        public readonly TesterFeature MoveLink = new(TesterFeatureNames.MOVE_LINK, 0);
        private readonly Dictionary<string, TesterFeature> _featuresByName = new();

        public TesterFeatures(ILogger logger, IModHelper modHelper)
        {
            _featuresByName.Add(Multiplayer.Name, Multiplayer);
            _featuresByName.Add(UnstableMods.Name, UnstableMods);
            _featuresByName.Add(MoveLink.Name, MoveLink);

#if DEBUG
            // UnstableMods.Value = VerifyMods.NOTHING;
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
                logger.LogError($"Failed at reading the TesterFeatures file. The file is probably corrupted and should be deleted to start fresh, or fixed manually. Exception: {ex}");
            }

            if (MoveLink.Value > 0 && !FoolManager.ShouldPrank())
            {
                FoolManager.TogglePrank(true);
            }
        }
    }
}
