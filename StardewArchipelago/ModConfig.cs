namespace StardewArchipelago
{
    public class ModConfig
    { 
        /// <summary>
        /// When enabled, Pierre and Sandy sell limited stocks of seeds.
        /// Jojamart sells cheaper seeds, but forces you to purchase large packs instead of one at a time
        /// </summary>
        public bool EnableSeedShopOverhaul { get; set; } = true;

        /// <summary>
        /// Automatically hides the archipelago letters that are "empty".
        /// A letter is considered empty when it contains neither a physical item, nor a code snippet to run
        /// Examples include NPC Hearts, Seasons, Carpenter building unlocks, etc
        /// </summary>
        public bool HideEmptyArchipelagoLetters { get; set; } = false;
    }
}