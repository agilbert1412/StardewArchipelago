using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Registry
{
    public interface IRegistry
    {
        void Initialize(StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, LocationChecker locationChecker, IGiftHandler _giftHandler, WeaponsManager weaponsManager, ArchipelagoStateDto state);
        void RegisterOnModEntry();
    }
}
