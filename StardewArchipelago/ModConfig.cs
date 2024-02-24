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

        /// <summary>
        /// When the archipelago icon shows up in-game, two version of the icons are available.
        /// The default are the "flat" icons, that come from Archipelago itself and are used in many games
        /// The Custom ones are made by candycaneanniahlator and are intended to ressemble the Stardew Valley style
        /// </summary>
        public bool UseCustomArchipelagoIcons { get; set; } = false;
    }
}