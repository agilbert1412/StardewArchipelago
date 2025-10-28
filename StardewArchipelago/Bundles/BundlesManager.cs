using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using Netcode;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;

namespace StardewArchipelago.Bundles
{
    public class BundlesManager
    {
        private readonly ILogger _logger;
        private readonly IModHelper _modHelper;
        private readonly Dictionary<string, string> _currentBundlesData;
        public BundleRooms BundleRooms { get; }
        public Dictionary<string, List<string>> TrashBearRequests { get; }

        public BundlesManager(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            var bundlesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(archipelago.SlotData.BundlesData);
            BundleRooms = new BundleRooms(_logger, archipelago, itemManager, bundlesDictionary);
            _currentBundlesData = BundleRooms.ToStardewStrings();
            _modHelper.Events.Content.AssetRequested += OnBundlesRequested;
            modHelper.GameContent.InvalidateCache(x => x.NameWithoutLocale.IsEquivalentTo("Data/Bundles"));
            ReplaceAllBundles();

            TrashBearRequests = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(archipelago.SlotData.TrashBearRequestsData);
        }

        public void CleanEvents()
        {
            _modHelper.Events.Content.AssetRequested -= OnBundlesRequested;
        }

        private void OnBundlesRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Bundles"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var bundlesData = asset.AsDictionary<string, string>().Data;
                    bundlesData.Clear();
                    foreach (var (key, value) in _currentBundlesData)
                    {
                        bundlesData.Add(key, value);
                    }
                },
                AssetEditPriority.Late
            );
        }

        public void ReplaceAllBundles()
        {
            if (Game1.netWorldState.Value is not { } worldState)
            {
                throw new InvalidCastException($"World State was unexpected type: {Game1.netWorldState.GetType()}");
            }

            // private readonly NetStringDictionary<string, NetString> netBundleData = new NetStringDictionary<string, NetString>();
            var netBundleDataField = _modHelper.Reflection.GetField<NetStringDictionary<string, NetString>>(worldState, "netBundleData");
            var netBundleData = netBundleDataField.GetValue();

            // protected bool _bundleDataDirty;
            var _bundleDataDirtyField = _modHelper.Reflection.GetField<bool>(worldState, "_bundleDataDirty");
            var _bundleDataDirty = _bundleDataDirtyField.GetValue();

            // protected Dictionary<string, string> _bundleData;
            var _bundleDataField = _modHelper.Reflection.GetField<Dictionary<string, string>>(worldState, "_bundleData");
            var _bundleData = new Dictionary<string, string>();

            worldState.SetBundleData(DataLoader.Bundles(Game1.content));
            _bundleDataDirtyField.SetValue(false);
            foreach (var key in netBundleData.Keys)
            {
                _bundleData[key] = netBundleData[key];
            }
            _bundleDataField.SetValue(_bundleData);
            worldState.UpdateBundleDisplayNames();

            netBundleData.Clear();
            var bundlesState = BackupBundleState(worldState);
            worldState.Bundles.Clear();
            worldState.BundleRewards.Clear();
            worldState.BundleData.Clear();
            worldState.SetBundleData(_currentBundlesData);
            RestoreBundleState(worldState, bundlesState);
        }

        private static Dictionary<int, bool[]> BackupBundleState(NetWorldState worldState)
        {
            var bundlesState = new Dictionary<int, bool[]>();
            foreach (var (key, values) in worldState.Bundles.Pairs)
            {
                bundlesState.Add(key, values);
            }

            return bundlesState;
        }

        private static void RestoreBundleState(NetWorldState worldState, Dictionary<int, bool[]> bundlesState)
        {
            foreach (var (key, values) in bundlesState)
            {
                if (worldState.Bundles.ContainsKey(key))
                {
                    worldState.Bundles.Remove(key);
                    worldState.Bundles.Add(key, values);
                }
            }
        }
    }
}
