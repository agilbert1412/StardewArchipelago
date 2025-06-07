using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Serialization
{
    public class ArchipelagoStateDto
    {
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        public List<ReceivedItem> ItemsReceived { get; set; }
        public List<string> LocationsChecked { get; set; }
        public List<string> ArtisanalLocationsChecked { get; set; }
        public List<string> JojaLocationsChecked { get; set; }
        public Dictionary<string, ScoutedLocation> LocationsScouted { get; set; }
        public Dictionary<string, string> LettersGenerated { get; set; }
        public List<string> SeasonsOrder { get; set; }
        public TrapItemsDifficulty? TrapDifficultyOverride { get; set; }
        public int TravelingMerchantPurchases { get; set; }
        public ArchipelagoWalletDto Wallet { get; set; }
        public List<string> EntrancesTraversed { get; set; }
        public List<bool> CurrentRaccoonBundleStatus { get; set; }
        public int NumberOfLOTLEpisodesWatched { get; set; }
        public int NumberTimesCursed { get; set; }
        public List<string> QualifiedIdsClothesDonated { get; set; }

        public ArchipelagoStateDto()
        {
            ItemsReceived = new List<ReceivedItem>();
            LocationsChecked = new List<string>();
            ArtisanalLocationsChecked = new List<string>();
            JojaLocationsChecked = new List<string>();
            LocationsScouted = new Dictionary<string, ScoutedLocation>();
            LettersGenerated = new Dictionary<string, string>();
            SeasonsOrder = new List<string>();
            TrapDifficultyOverride = null;
            TravelingMerchantPurchases = 0;
            Wallet = new ArchipelagoWalletDto();
            EntrancesTraversed = new List<string>();
            CurrentRaccoonBundleStatus = new List<bool>();
            NumberOfLOTLEpisodesWatched = 0;
            NumberTimesCursed = 0;
            QualifiedIdsClothesDonated = new List<string>();
        }
    }
}
