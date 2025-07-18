﻿using System;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;

namespace StardewArchipelago.Integrations.GenericModConfigMenu
{
    /// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
    public interface IGenericModConfigMenuApi
    {
        /*********
        ** Methods
        *********/
        /// <summary>Register a mod whose config can be edited through the UI.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="reset">Reset the mod's config to its default values.</param>
        /// <param name="save">Save the mod's current config to the <c>config.json</c> file.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        /// <remarks>
        ///     Each mod can only be registered once, unless it's deleted via <see cref="Unregister" /> before calling this
        ///     again.
        /// </remarks>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        /****
        ** Basic options
        ****/
        /// <summary>Add a section title at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The title text shown in the form.</param>
        /// <param name="tooltip">
        ///     The tooltip text shown when the cursor hovers on the title, or <c>null</c> to disable the
        ///     tooltip.
        /// </param>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);

        /// <summary>Add a paragraph of text at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="text">The paragraph text to display.</param>
        void AddParagraph(IManifest mod, Func<string> text);

        /// <summary>Add a boolean option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">
        ///     The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the
        ///     tooltip.
        /// </param>
        /// <param name="fieldId">
        ///     The unique field ID for use with <see cref="OnFieldChanged" />, or <c>null</c> to auto-generate a
        ///     randomized ID.
        /// </param>
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

        /// <summary>Add an integer option at the current position in the form.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="getValue">Get the current value from the mod config.</param>
        /// <param name="setValue">Set a new value in the mod config.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
        /// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
        /// <param name="interval">The interval of values that can be selected.</param>
        /// <param name="formatValue">Get the display text to show for a value, or <c>null</c> to show the number as-is.</param>
        /// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);

        /// <summary>Set whether the options registered after this point can only be edited from the title screen.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
        /// <remarks>This lets you have different values per-field. Most mods should just set it once in <see cref="Register" />.</remarks>
        void SetTitleScreenOnlyForNextOptions(IManifest mod, bool titleScreenOnly);

        /// <summary>Register a method to notify when any option registered by this mod is edited through the config UI.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="onChange">The method to call with the option's unique field ID and new value.</param>
        /// <remarks>
        ///     Options use a randomized ID by default; you'll likely want to specify the <c>fieldId</c> argument when adding
        ///     options if you use this.
        /// </remarks>
        void OnFieldChanged(IManifest mod, Action<string, object> onChange);

        /// <summary>Remove a mod from the config UI and delete all its options and pages.</summary>
        /// <param name="mod">The mod's manifest.</param>
        void Unregister(IManifest mod);
    }

    class GenericModConfig
    {
        private readonly IModHelper Helper;
        private readonly IManifest ModManifest;
        private ModConfig Config;

        public GenericModConfig(ModEntry mod)
        {
            Helper = mod.Helper;
            ModManifest = mod.ModManifest;
            Config = mod.Config;
        }

        public void RegisterConfig()
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu == null)
            {
                return;
            }

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.SetTitleScreenOnlyForNextOptions(ModManifest, true);

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Seed Shop Overhaul",
                tooltip: () => "Seed shops are rebalanced to be more in line with the lore and intended gameplay design of Archipelago",
                getValue: () => Config.EnableSeedShopOverhaul,
                setValue: (value) => Config.EnableSeedShopOverhaul = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Hide Empty Archipelago Letters",
                tooltip: () => "Letters that contain neither an item nor an unlock will be skipped, reducing clutter",
                getValue: () => Config.HideEmptyArchipelagoLetters,
                setValue: (value) => Config.HideEmptyArchipelagoLetters = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable Letter Templates",
                tooltip: () => "All Archipelago letters will now use a very short and concise format instead of the funny ones full of fluff",
                getValue: () => Config.DisableLetterTemplates,
                setValue: (value) => Config.DisableLetterTemplates = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Use Custom Archipelago Icons",
                tooltip: () => "Alternate art style for the Archipelago icons used in various places in the mod",
                getValue: () => Config.UseCustomArchipelagoIcons,
                setValue: (value) => Config.UseCustomArchipelagoIcons = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Skip Hold Up Animations",
                tooltip: () => "Skip all the 'Hold an item above your head' animations",
                getValue: () => Config.SkipHoldUpAnimations,
                setValue: (value) => Config.SkipHoldUpAnimations = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable Friendship Decay",
                tooltip: () => "Friendship points no longer go down when you don't talk to NPCs",
                getValue: () => Config.DisableFriendshipDecay,
                setValue: (value) => Config.DisableFriendshipDecay = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bonus Per Movement Speed",
                tooltip: () => "Speed increase per movement speed bonus received.",
                min: 0,
                max: 20,
                interval: 1,
                getValue: () => Config.BonusPerMovementSpeed,
                setValue: (value) => Config.BonusPerMovementSpeed = value,
                formatValue: (value) => $"{value * 5}%"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Anonymize names in chat",
                tooltip: () => "Replace every player name with a random animal, in chat and menus, for privacy reasons",
                getValue: () => Config.AnonymizeNamesInChat,
                setValue: (value) => Config.AnonymizeNamesInChat = value
            );

            var chatFilterValues = Enum.GetValues(typeof(ChatItemsFilter)).Cast<int>().ToArray();
            var chatFilterValuesMin = chatFilterValues.Min();
            var chatFilterValuesMax = chatFilterValues.Max();
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Display Items In Chat",
                tooltip: () => "Which items notifications should display in chat",
                min: chatFilterValuesMin,
                max: chatFilterValuesMax,
                interval: 1,
                getValue: () => (int)Config.DisplayItemsInChat,
                setValue: (value) => Config.DisplayItemsInChat = (ChatItemsFilter)value,
                formatValue: (value) => ((ChatItemsFilter)value).ToString()
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Connection Messages",
                tooltip: () => "Whether to display connection messages in chat when players join or leave",
                getValue: () => Config.EnableConnectionMessages,
                setValue: (value) => Config.EnableConnectionMessages = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Chat Messages",
                tooltip: () => "Whether to display chat messages from other players in the multiworld",
                getValue: () => Config.EnableChatMessages,
                setValue: (value) => Config.EnableChatMessages = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Calendar Indicators",
                tooltip: () => "Whether to display Archipelago indicators on calendar dates with remaining location checks",
                getValue: () => Config.ShowCalendarIndicators,
                setValue: (value) => Config.ShowCalendarIndicators = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Elevator Indicators",
                tooltip: () => "Whether to display Archipelago indicators on elevator floors with remaining location checks",
                getValue: () => Config.ShowElevatorIndicators,
                setValue: (value) => Config.ShowElevatorIndicators = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Strict Logic",
                tooltip: () => "Disables some out-of-logic obtention methods for some critical progression items. Examples: Mystery boxes, Prize Tickets",
                getValue: () => Config.StrictLogic,
                setValue: (value) => Config.StrictLogic = value
            );

            var itemIndicatorValues = Enum.GetValues(typeof(ItemIndicatorPreference)).Cast<int>().ToArray();
            var itemIndicatorMin = itemIndicatorValues.Min();
            var itemIndicatorMax = itemIndicatorValues.Max();
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Show Item Indicators",
                tooltip: () => "Whether to display Archipelago indicators on items with related remaining location checks",
                min: itemIndicatorMin,
                max: itemIndicatorMax,
                interval: 1,
                getValue: () => (int)Config.ShowItemIndicators,
                setValue: (value) => Config.ShowItemIndicators = (ItemIndicatorPreference)value,
                formatValue: (value) => ((ItemIndicatorPreference)value).ToString()
            );

            var seasonPreferenceValues = Enum.GetValues(typeof(SeasonPreference)).Cast<int>().ToArray();
            var seasonPreferenceMin = seasonPreferenceValues.Min();
            var seasonPreferenceMax = seasonPreferenceValues.Max();
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "MultiSleep Season Behavior",
                tooltip: () => "When multisleeping across a month transition, what season is picked next?",
                min: seasonPreferenceMin,
                max: seasonPreferenceMax,
                interval: 1,
                getValue: () => (int)Config.MultiSleepSeasonPreference,
                setValue: (value) => Config.MultiSleepSeasonPreference = (SeasonPreference)value,
                formatValue: (value) => ((SeasonPreference)value).ToString()
            );

            var grandpaShrinePreferenceValues = Enum.GetValues(typeof(GrandpaShrinePreference)).Cast<int>().ToArray();
            var grandpaShrinePreferenceMin = grandpaShrinePreferenceValues.Min();
            var grandpaShrinePreferenceMax = grandpaShrinePreferenceValues.Max();
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Grandpa Shrine Icons",
                tooltip: () => "When to display Grandpa Evaluation Icons over the shrine",
                min: grandpaShrinePreferenceMin,
                max: grandpaShrinePreferenceMax,
                interval: 1,
                getValue: () => (int)Config.ShowGrandpaShrineIndicators,
                setValue: (value) => Config.ShowGrandpaShrineIndicators = (GrandpaShrinePreference)value,
                formatValue: (value) => ((GrandpaShrinePreference)value).ToString()
            );

            var spriteRandomizerValues = Enum.GetValues(typeof(AppearanceRandomization)).Cast<int>().ToArray();
            var spriteRandomizerMin = spriteRandomizerValues.Min();
            var spriteRandomizerMax = spriteRandomizerValues.Max();
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Sprite Randomizer",
                tooltip: () => "Whether to randomize npc sprites",
                min: spriteRandomizerMin,
                max: spriteRandomizerMax,
                interval: 1,
                getValue: () => (int)Config.SpriteRandomizer,
                setValue: (value) => Config.SpriteRandomizer = (AppearanceRandomization)value,
                formatValue: (value) => ((AppearanceRandomization)value).ToString()
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Start with Livin' Off the Land",
                tooltip: () => $"Whether to start with the \"Livin' Off the Land\" TV Channel unlocked. Otherwise, wait for it as an unlock from the multiworld.{Environment.NewLine}The Archipelago episodes are on Tuesdays and Fridays, and runs vanilla episodes on the vanilla days of Mondays and Thursdays",
                getValue: () => Config.StartWithLivingOffTheLand,
                setValue: (value) => Config.StartWithLivingOffTheLand = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Start with The Gateway Gazette",
                tooltip: () => $"Whether to start with the \"Gateway Gazette\" TV Channel unlocked. Otherwise, wait for it as an unlock from the multiworld.{Environment.NewLine}The channel gives information about the current entrance randomizer roll, on Mondays and Fridays",
                getValue: () => Config.StartWithGatewayGazette,
                setValue: (value) => Config.StartWithGatewayGazette = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Allow Hand Breaking",
                tooltip: () => $"Whether to allow the player to break items with their bare hands.{Environment.NewLine}It takes many more hits than with the proper tool, and is solely intended to avoid getting physically stuck behind things.",
                getValue: () => Config.AllowHandBreaking,
                setValue: (value) => Config.AllowHandBreaking = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bookseller Price Multiplier",
                tooltip: () => "Multiplier to apply to the bookseller book prices",
                min: 5,
                max: 1000,
                interval: 1,
                getValue: () => Config.BooksellerPriceMultiplier,
                setValue: (value) => Config.BooksellerPriceMultiplier = value,
                formatValue: (value) => $"{value}%"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Scout Hint In Solo Multiworlds",
                tooltip: () => $"Whether shops and other scoutable locations should auto-hint themselves, even when playing solo. Bigger Multiworlds always scout-hint.",
                getValue: () => Config.ScoutHintInSoloMultiworld,
                setValue: (value) => Config.ScoutHintInSoloMultiworld = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Custom Assets",
                tooltip: () => $"Use custom assets for scouted items when available",
                getValue: () => Config.CustomAssets,
                setValue: (value) => Config.CustomAssets = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Custom Assets flexibility across games",
                tooltip: () => $"If a custom asset for the correct game doesn't exist, allow using an asset for the same item name in a different game",
                getValue: () => Config.CustomAssetGameFlexible,
                setValue: (value) => Config.CustomAssetGameFlexible = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Custom assets generic game icons",
                tooltip: () => $"If a custom asset for the correct item doesn't exist, use a generic custom asset for the game itself",
                getValue: () => Config.CustomAssetGenericGame,
                setValue: (value) => Config.CustomAssetGenericGame = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Limit hoe and watering can levels",
                tooltip: () => $"Makes hoes and watering cans behave as if they were at the basic level no matter what. Generally used for animation cancelling.",
                getValue: () => Config.LimitHoeWateringCanLevel,
                setValue: (value) => Config.LimitHoeWateringCanLevel = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Use Legacy Randomization",
                tooltip: () => $"Equivalent to the vanilla advanced option of the same name. Only applies at the moment of creating the save file",
                getValue: () => Config.UseLegacyRandomization,
                setValue: (value) => Config.UseLegacyRandomization = value
            );
        }
    }
}
