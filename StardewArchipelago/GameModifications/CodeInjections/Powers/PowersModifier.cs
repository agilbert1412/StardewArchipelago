using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Powers;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections.Powers
{
    public class PowersModifier
    {
        protected static ILogger _logger;
        protected static IModHelper _helper;
        protected static StardewArchipelagoClient _archipelago;
        private static Hint[] _hints;

        private static HashSet<ArchipelagoPower> _customPowers = new()
        {
            new VanillaArchipelagoPower("ForestMagic", APItem.FOREST_MAGIC),
            new VanillaArchipelagoPower("DwarvishTranslationGuide", APItem.DWARVISH_TRANSLATION_GUIDE),
            new VanillaArchipelagoPower("RustyKey", APItem.RUSTY_KEY),
            new VanillaArchipelagoPower("ClubCard", APItem.CLUB_CARD, (x) => StoryQuestsEnabled(x) || x.Secretsanity.HasFlag(Secretsanity.SecretNotes)),
            new VanillaArchipelagoPower("SpecialCharm", APItem.SPECIAL_CHARM, (x) => x.Secretsanity.HasFlag(Secretsanity.SecretNotes)),
            new VanillaArchipelagoPower("SkullKey", APItem.SKULL_KEY),
            new VanillaArchipelagoPower("MagnifyingGlass", APItem.MAGNIFYING_GLASS, StoryQuestsEnabled),
            new VanillaArchipelagoPower("DarkTalisman", APItem.DARK_TALISMAN, StoryQuestsEnabled),
            new VanillaArchipelagoPower("MagicInk", APItem.MAGIC_INK, StoryQuestsEnabled),
            new VanillaArchipelagoPower("BearPaw", APItem.BEARS_KNOWLEDGE, StoryQuestsEnabled),
            new VanillaArchipelagoPower("KeyToTheTown", APItem.KEY_TO_THE_TOWN),

            new VanillaArchipelagoPower("Book_PriceCatalogue", Power(PowerBooks.PRICE_CATALOGUE), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Marlon", Power(PowerBooks.MAPPING_CAVE_SYSTEMS), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Speed", Power(PowerBooks.WAY_OF_THE_WIND_1), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Speed2", Power(PowerBooks.WAY_OF_THE_WIND_2), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Void", Power(PowerBooks.MONSTER_COMPENDIUM), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Friendship", Power(PowerBooks.FRIENDSHIP_101), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Defense", Power(PowerBooks.JACK_BE_NIMBLE_JACK_BE_THICK), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Woodcutting", Power(PowerBooks.WOODYS_SECRET), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_WildSeeds", Power(PowerBooks.RACCOON_JOURNAL), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Roe", Power(PowerBooks.JEWELS_OF_THE_SEA), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Bombs", Power(PowerBooks.DWARVISH_SAFETY_MANUAL), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Crabbing", Power(PowerBooks.THE_ART_O_CRABBING), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Trash", Power(PowerBooks.THE_ALLEYWAY_BUFFET), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Diamonds", Power(PowerBooks.THE_DIAMOND_HUNTER), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Mystery", Power(PowerBooks.BOOK_OF_MYSTERIES), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Horse", Power(PowerBooks.HORSE_THE_BOOK), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Artifact", Power(PowerBooks.TREASURE_APPRAISAL_GUIDE), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_Grass", Power(PowerBooks.OL_SLITHERLEGS), PowerBooksEnabled),
            new VanillaArchipelagoPower("Book_AnimalCatalogue", Power(PowerBooks.ANIMAL_CATALOGUE), PowerBooksEnabled),

            new VanillaArchipelagoPower("Mastery_Farming", Mastery.FARMING, MasteriesEnabled),
            new VanillaArchipelagoPower("Mastery_Fishing", Mastery.FISHING, MasteriesEnabled),
            new VanillaArchipelagoPower("Mastery_Foraging", Mastery.FORAGING, MasteriesEnabled),
            new VanillaArchipelagoPower("Mastery_Mining", Mastery.MINING, MasteriesEnabled),
            new VanillaArchipelagoPower("Mastery_Combat", Mastery.COMBAT, MasteriesEnabled),

            new ArchipelagoPower("Community Center Key", "The Community Center door is unlocked", 0, 0),
            new ArchipelagoPower("Wizard Invitation", "The Wizard invited you to meet him at the tower", 16, 0),
            new ArchipelagoPower("Landslide Removed", "The path to the mines is cleared", 32, 0),
            new ArchipelagoPower("Railroad Boulder Removed", "The entrance to the railroad is open", 48, 0),
            new ArchipelagoPower(APItem.MENS_LOCKER_KEY, "You can enter the men's locker room", 64, 16),
            new ArchipelagoPower(APItem.WOMENS_LOCKER_KEY, "You can enter the women's locker room", 80, 16),
            new ArchipelagoPower("Beach Bridge", "The bridge on the beach is repaired", 64, 0),
            new ArchipelagoPower("Fruit Bats", "Fruit bats will periodically bring fruit to your farm cave", 0, 16),
            new ArchipelagoPower("Mushroom Boxes", "Demetrius installed mushroom boxes in your farm cave", 16, 16),
            new ArchipelagoPower("Mr Qi's Plane Ride", "You can now find mystery boxes", 32, 16),
            new ArchipelagoPower("Trash Bear Arrival", "A Trash Bear has arrived in the Forest", 48, 16),
            new IslandArchipelagoPower("Boat Repair", "You can use Willy's Boat to travel to Ginger Island", 0, 32),
            new IslandArchipelagoPower("Island West Turtle", "The turtle has left the path to Island West", 16, 32),
            new IslandArchipelagoPower("Island North Turtle", "The turtle has left the path to Island North", 32, 32),
            new IslandArchipelagoPower("Island Farmhouse", "The Farmhouse on the island is repaired and the Gourmand frog cave is open", 48, 32),
            // new IslandArchipelagoPower("Island Mailbox", ""),
            // new IslandArchipelagoPower("Farm Obelisk", ""),
            new IslandArchipelagoPower("Dig Site Bridge", "The bridge to the dig site on Island North is repaired", 64, 32),
            new IslandArchipelagoPower("Island Trader", "The Island Trader has opened a shop on Island North", 80, 32),
            new IslandArchipelagoPower("Open Professor Snail Cave", "Professor Snail is free and returned to his field office", 0, 48),
            // new IslandArchipelagoPower("Volcano Bridge", ""),
            // new IslandArchipelagoPower("Volcano Exit Shortcut", ""),
            new IslandArchipelagoPower("Island Resort", "The Island Resort is operational and the debris to Island SouthEast was cleared", 16, 48),
            new IslandArchipelagoPower("Parrot Express", "The Parrot Express Transportation is available", 32, 48),
            new IslandArchipelagoPower("Qi Walnut Room", "Yes, kid, you can come see me. I won't bite.", 48, 48),
        };

        private static string Power(string bookName)
        {
            return $"Power: {bookName}";
        }

        private static bool StoryQuestsEnabled(SlotData slotData)
        {
            return slotData.QuestLocations.StoryQuestsEnabled;
        }

        private static bool PowerBooksEnabled(SlotData slotData)
        {
            return slotData.Booksanity >= Booksanity.Power;
        }

        private static bool MasteriesEnabled(SlotData slotData)
        {
            return slotData.SkillProgression >= SkillsProgression.ProgressiveWithMasteries;
        }

        private static Dictionary<string, ArchipelagoPower> _powersByName = _customPowers.ToDictionary(x => x.Name, x => x);

        public PowersModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _hints = Array.Empty<Hint>();
        }

        public void OnPowersRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsPowers(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var powersData = asset.AsDictionary<string, PowersData>().Data;
                    foreach (var customPower in _customPowers)
                    {
                        AddPower(powersData, customPower);
                    }
                },
                AssetEditPriority.Late
            );
        }

        protected bool AssetIsPowers(AssetRequestedEventArgs e)
        {
            return e.NameWithoutLocale.IsEquivalentTo("Data\\Powers");
        }

        private void AddPower(IDictionary<string, PowersData> powersData, ArchipelagoPower customPower)
        {
            if (powersData.ContainsKey(customPower.Name))
            {
                return;
            }

            if (!customPower.IsIncluded(_archipelago.SlotData))
            {
                return;
            }

            if (customPower.IsVanillaPower())
            {
                return;
            }

            var powerData = new PowersData()
            {
                DisplayName = customPower.DisplayName,
                Description = customPower.Description,
                TexturePath = "LooseSprites\\Cursors",
                TexturePosition = customPower.TextureRectangle.Location,
                UnlockedCondition = customPower.Condition,
            };
            powersData.Add(customPower.Name, powerData);
        }

        // public override void populateClickableComponentList()
        public static void PopulateClickableComponentList_AddTextures_Postfix(PowersTab __instance)
        {
            try
            {
                foreach (var powerIcon in __instance.powers.SelectMany(x => x))
                {
                    if (!_powersByName.TryGetValue(powerIcon.name, out var customPower) || customPower.IsVanillaPower())
                    {
                        continue;
                    }

                    powerIcon.texture = customPower.Texture;
                    powerIcon.sourceRect = customPower.TextureRectangle;
                    powerIcon.scale = customPower.TextureScale;
                    powerIcon.baseScale = customPower.TextureScale;
                }

                _hints = _archipelago.GetHints();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PopulateClickableComponentList_AddTextures_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void performHoverAction(int x, int y)
        public static void PerformHoverAction_AddTooltipsOnApItems_Postfix(PowersTab __instance, int x, int y)
        {
            try
            {
                if (!_archipelago.MakeSureConnected())
                {
                    return;
                }

                __instance.hoverText = "";
                __instance.descriptionText = "";
                foreach (var textureComponent in __instance.powers[__instance.currentPage])
                {
                    if (!textureComponent.containsPoint(x, y))
                    {
                        continue;
                    }

                    var showTitle = textureComponent.drawShadow || (_powersByName.ContainsKey(textureComponent.name) && _powersByName[textureComponent.name].IsIncluded(_archipelago.SlotData));
                    var showDescription = textureComponent.drawShadow;

                    var itemName = showTitle ? textureComponent.label : "???";
                    var description = Game1.parseText(textureComponent.hoverText, Game1.smallFont, Math.Max((int)Game1.dialogueFont.MeasureString(__instance.hoverText).X, 320));
                    if (!showDescription)
                    {
                        if (showTitle)
                        {
                            //var hintItemNames = _hints.Where(x => x.ReceivingPlayer == _archipelago.GetCurrentPlayer().Slot).Select(x => _archipelago.GetItemName(x.ItemId)).ToArray();
                            var hintForThisItem = _hints.FirstOrDefault(hint => hint.ReceivingPlayer == _archipelago.GetCurrentPlayer().Slot &&
                                                                             _archipelago.GetItemName(hint.ItemId) == itemName);
                            if (hintForThisItem == null)
                            {
                                description = "You can hint this item";
                            }
                            else
                            {
                                var findingPlayerName = _archipelago.GetPlayerName(hintForThisItem.FindingPlayer);
                                var locationName = _archipelago.GetLocationName(hintForThisItem);
                                description = $"At {findingPlayerName}'s {locationName}";
                            }
                        }
                        else
                        {
                            description = "";
                        }
                    }

                    __instance.hoverText = itemName;
                    __instance.descriptionText = description;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformHoverAction_AddTooltipsOnApItems_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
