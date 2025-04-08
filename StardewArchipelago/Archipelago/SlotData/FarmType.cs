using System;
using System.Linq;
using StardewValley;
using StardewValley.GameData;

namespace StardewArchipelago.Archipelago.SlotData
{
    public class FarmType
    {
        private readonly SupportedFarmType _supportedFarmType;

        internal FarmType(SupportedFarmType supportedFarmType)
        {
            _supportedFarmType = supportedFarmType;
        }

        public SupportedFarmType GetFarmType()
        {
            return _supportedFarmType;
        }

        public int GetWhichFarm()
        {
            return (int)_supportedFarmType;
        }

        public ModFarmType GetWhichModFarm()
        {
            switch (_supportedFarmType)
            {
                case SupportedFarmType.Meadowlands:
                    var modFarmTypeList = DataLoader.AdditionalFarms(Game1.content);
                    return modFarmTypeList.First(x => x.Id.Contains(_supportedFarmType.ToString(), StringComparison.InvariantCultureIgnoreCase));
                case SupportedFarmType.Standard:
                case SupportedFarmType.Riverland:
                case SupportedFarmType.Forest:
                case SupportedFarmType.HillTop:
                case SupportedFarmType.Wilderness:
                case SupportedFarmType.FourCorners:
                case SupportedFarmType.Beach:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException($"Unrecognized SupportedFarmType Type: {_supportedFarmType}");
            }
        }

        public bool GetSpawnMonstersAtNight()
        {
            return _supportedFarmType == SupportedFarmType.Wilderness;
        }
    }
}