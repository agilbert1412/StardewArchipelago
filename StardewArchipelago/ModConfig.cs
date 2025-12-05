using StardewArchipelago.Archipelago;

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
        /// Automatically removes random gifts received in mail from NPCs
        /// </summary>
        public bool HideNpcGiftMail { get; set; } = false;

        /// <summary>
        /// All Archipelago letters will now use a very short and concise format
        /// instead of the funny ones full of fluff
        /// </summary>
        public bool DisableLetterTemplates { get; set; } = false;

        /// <summary>
        /// When the archipelago icon shows up in-game, two version of the icons are available.
        /// The default are the "flat" icons, that come from Archipelago itself and are used in many games
        /// The Custom ones are made by candycaneannihalator and are intended to ressemble the Stardew Valley style
        /// </summary>
        public bool UseCustomArchipelagoIcons { get; set; } = true;

        /// <summary>
        /// Skips all Zelda-style animations where the character holds an item above their head
        /// </summary>
        public bool SkipHoldUpAnimations { get; set; } = false;

        /// <summary>
        /// Disables decaying of friendship points.
        /// On friendsanity, this applies to earned points.
        /// Outside of friendsanity, this applies to real points (hearts).
        /// </summary>
        public bool DisableFriendshipDecay { get; set; } = false;

        /// <summary>
        /// Amount of speed gained per movement speed bonus received
        /// Multiplied by 0.05 in-game
        /// </summary>
        public int BonusPerMovementSpeed { get; set; } = 5;

        /// <summary>
        /// Whether to Anonymize Player names in the chatbox
        /// </summary>
        public bool AnonymizeNamesInChat { get; set; } = false;

        /// <summary>
        /// Which Item messages should be displayed in the game chatbox
        /// </summary>
        public ChatItemsFilter DisplayItemsInChat { get; set; } = ChatItemsFilter.RelatedToMe;

        /// <summary>
        /// Should the join and leave messages of other players be displayed in the game chatbox
        /// </summary>
        public bool EnableConnectionMessages { get; set; } = true;

        /// <summary>
        /// Should the chat messages of other players be displayed in the game chatbox
        /// </summary>
        public bool EnableChatMessages { get; set; } = true;

        /// <summary>
        /// Whether to display archipelago icons on the calendar for dates where the player has checks to do
        /// </summary>
        public bool ShowCalendarIndicators { get; set; } = true;

        /// <summary>
        /// Whether to display archipelago icons on the mine elevator menu for floors where the player has checks to do
        /// </summary>
        public bool ShowElevatorIndicators { get; set; } = true;

        /// <summary>
        /// Disables some out of logic obtention methods of important progression items (seeds, saplings, etc)
        /// </summary>
        public bool StrictLogic { get; set; } = true;

        /// <summary>
        /// Whether to display archipelago icons on inventory items for items that the player can do checks with
        /// </summary>
        public ItemIndicatorPreference ShowItemIndicators { get; set; } = ItemIndicatorPreference.True;

        /// <summary>
        /// What Season to pick when multisleeping across a month transition
        /// </summary>
        public SeasonPreference MultiSleepSeasonPreference { get; set; } = SeasonPreference.Repeat;

        /// <summary>
        /// Whether to display archipelago icons on grandpa's shrine to see current points
        /// </summary>
        public GrandpaShrinePreference ShowGrandpaShrineIndicators { get; set; } = GrandpaShrinePreference.GrandpaGoal;

        /// <summary>
        /// Whether to randomize sprites
        /// </summary>
        public AppearanceRandomization SpriteRandomizer { get; set; } = AppearanceRandomization.Disabled;

        /// <summary>
        /// Whether to start with the "Livin' Off the Land" TV Channel unlocked. Otherwise, wait for it as an unlock from the multiworld.
        /// The channel serves as a tutorial for Archipelago behaviors, in addition to the vanilla episodes
        /// This defaults to true, with the target audience being the new players. Once a player is experienced enough to find this config and change it, they are experienced enough to start without the channel
        /// The Archipelago episodes are on Tuesdays and Fridays, and runs vanilla episodes on the vanilla days of Mondays and Thursdays
        /// </summary>
        public bool StartWithLivingOffTheLand { get; set; } = true;

        /// <summary>
        /// Whether to start with the "Gateway Gazette" TV Channel unlocked. Otherwise, wait for it as an unlock from the multiworld.
        /// The channel gives information about the current entrance randomizer roll, on Mondays and Fridays
        /// </summary>
        public bool StartWithGatewayGazette { get; set; } = false;

        /// <summary>
        /// Whether to allow the player to break items they wouldn't usually be able to, with their empty hand.
        /// It takes many more hits than it would with the proper tool, and is intended solely to avoid getting physically stuck
        /// </summary>
        public bool AllowHandBreaking { get; set; } = false;

        /// <summary>
        /// Multiplier to apply to all bookseller books.
        /// Can help to reduce the booksanity grinding
        /// </summary>
        public int BooksellerPriceMultiplier { get; set; } = 100;

        /// <summary>
        /// Which items in shops and other previews will automatically generate a server hint. When playing in a multiworld, `DontHint` is not allowed.
        /// </summary>
        public ScoutingPreference ScoutHintBehavior { get; set; } = ScoutingPreference.DontHint;

        /// <summary>
        /// Preferred Hinting behaviors for scouted locations. If playing in a multiworld with others, DontHint will be changed to HintOnlyProgression
        /// </summary>
        // public ScoutingPreference ScoutHintInSoloMultiworld { get; set; } = ScoutingPreference.HintProgressionUseful;

        /// <summary>
        /// When seeing a scouted item, and a custom asset is available for this game and item, display the custom asset instead of a generic logo
        /// </summary>
        public bool CustomAssets { get; set; } = true;

        /// <summary>
        /// When seeing a scouted item, and a custom asset is available for this item in a different game, display the custom asset instead of a generic logo
        /// </summary>
        public bool CustomAssetGameFlexible { get; set; } = true;

        /// <summary>
        /// When seeing a scouted item, and a custom asset is available for this game but not for this item, display the generic game asset instead of a generic logo
        /// </summary>
        public bool CustomAssetGenericGame { get; set; } = false;

        /// <summary>
        /// Makes all hoes and watering cans behave as if they were at the basic level. Generally used for Animation Cancelling purposes
        /// </summary>
        public bool LimitHoeWateringCanLevel { get; set; } = false;

        /// <summary>
        /// Equivalent to the vanilla toggle "Use Legacy Randomization". Only applies at the save creation
        /// </summary>
        public bool UseLegacyRandomization { get; set; } = false;

        /// <summary>
        /// Should you be allowed to get more than 5 repeatable walnuts per source?
        /// </summary>
        public BonusRepeatableWalnutsPreference BonusRepeatableWalnuts { get; set; } = BonusRepeatableWalnutsPreference.VeryRare;

        /// <summary>
        /// Some Jojapocalypse goals are triggerable pretty easily. This will make it so these goals have extra conditions, in addition to locations checked, to trigger.
        /// </summary>
        public bool JojapocalypseHarderGoals { get; set; } = true;

        /// <summary>
        /// Prevent sending goal as a Joja member until this percent of locations are checked.
        /// </summary>
        public int JojapocalypseMinimumCompletionPercentToGoal { get; set; } = 50;
    }

    public enum ItemIndicatorPreference
    {
        False = 0,
        True = 1,
        OnlyShipsanity = 2,
    }

    public enum SeasonPreference
    {
        Prompt = 0,
        Repeat = 1,
        Cycle = 2,
    }

    public enum GrandpaShrinePreference
    {
        Never = 0,
        GrandpaGoal = 1,
        Always = 2,
    }

    public enum AppearanceRandomization
    {
        Disabled = 0,
        Enabled = 1,
        // All = 2,
        Chaos = 3,
    }

    public enum ScoutingPreference
    {
        DontHint = 0,
        HintOnlyProgression = 1,
        HintProgressionUseful = 2,
        HintProgressionUsefulFiller = 3,
        HintEverything = 4,
    }

    public enum BonusRepeatableWalnutsPreference
    {
        Never = 0,
        VeryRare = 1,
        Rare = 2,
        Regular = 3,
        Frequent = 4,
    }
}
