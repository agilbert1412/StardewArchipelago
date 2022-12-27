using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago
{
    internal class LocationsCodeInjection
    {
        private const string BACKPACK_UPGRADE_LEVEL_KEY = "Backpack_Upgrade_Level_Key";

        private static IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private Action<long> _addCheckedLocation;

        public LocationsCodeInjection(IMonitor monitor, ArchipelagoClient archipelago, Action<long> addCheckedLocation)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _addCheckedLocation = addCheckedLocation;
        }

        public void DoAreaCompleteReward(int whichArea)
        {
            string AreaAPLocationName = "";
            switch ((Area)whichArea)
            {
                case Area.Pantry:
                    AreaAPLocationName = "Complete Pantry";
                    break;
                case Area.CraftsRoom:
                    AreaAPLocationName = "Complete Crafts Room";
                    break;
                case Area.FishTank:
                    AreaAPLocationName = "Complete Fish Tank";
                    break;
                case Area.BoilerRoom:
                    AreaAPLocationName = "Complete Boiler Room";
                    break;
                case Area.Vault:
                    AreaAPLocationName = "Complete Vault";
                    break;
                case Area.Bulletin:
                    AreaAPLocationName = "Complete Bulletin";
                    break;
            }
            var completedAreaAPLocationId = _archipelago.GetLocationId(AreaAPLocationName);
            _addCheckedLocation(completedAreaAPLocationId);
        }

        public bool answerDialogueAction_Prefix(SeedShop __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Backpack_Purchase")
                {
                    return true; // run original logic
                }

                var modData = Game1.getFarm().modData;
                if (!modData.ContainsKey(BACKPACK_UPGRADE_LEVEL_KEY))
                {
                    modData.Add(BACKPACK_UPGRADE_LEVEL_KEY, "0");
                }

                if (Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] == "0" && Game1.player.Money >= 2000)
                {
                    Game1.player.Money -= 2000;
                    Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] = "1";
                    var completedAreaAPLocationId = _archipelago.GetLocationId("Backpack Upgrade 1");
                    _addCheckedLocation(completedAreaAPLocationId);
                    return false; // don't run original logic
                }

                if (Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] == "1" && Game1.player.Money >= 10000)
                {
                    Game1.player.Money -= 10000;
                    Game1.getFarm().modData[BACKPACK_UPGRADE_LEVEL_KEY] = "2";
                    var completedAreaAPLocationId = _archipelago.GetLocationId("Backpack Upgrade 2");
                    _addCheckedLocation(completedAreaAPLocationId);
                    return false; // don't run original logic
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(answerDialogueAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
