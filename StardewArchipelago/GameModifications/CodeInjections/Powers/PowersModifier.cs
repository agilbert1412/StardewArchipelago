using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Powers;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections.Powers
{
    public class PowersModifier
    {
        protected static ILogger _logger;
        protected static IModHelper _helper;
        protected static StardewArchipelagoClient _archipelago;

        private static HashSet<ArchipelagoPower> _customPowers = new()
        {
            new ArchipelagoPower("Community Center Key", "The Community Center door is unlocked", new Point(0, 0)),
            new ArchipelagoPower("Wizard Invitation", "The Wizard invited you to meet him at the tower", new Point(16, 0)),
            new ArchipelagoPower("Landslide Removed", "The path to the mines is cleared"),
            new ArchipelagoPower("Railroad Boulder Removed", "The entrance to the railroad is open"),
            new ArchipelagoPower("Beach Bridge", "The bridge on the beach is repaired"),
            new ArchipelagoPower("Mr Qi's Plane Ride", "You can now find mystery boxes"),
            new ArchipelagoPower("Trash Bear Arrival", "A Trash Bear has arrived in the Forest"),
            new ArchipelagoPower("Fruit Bats", "Fruit bats will periodically bring fruit to your farm cave"),
            new ArchipelagoPower("Mushroom Boxes", "Demetrius installed mushroom boxes in your farm cave"),
            new IslandArchipelagoPower("Boat Repair", "You can use Willy's Boat to travel to Ginger Island"),
            new IslandArchipelagoPower("Island North Turtle", "The turtle has left the path to Island North"),
            new IslandArchipelagoPower("Island West Turtle", "The turtle has left the path to Island West"),
            new IslandArchipelagoPower("Island Farmhouse", "The Farmhouse on the island is repaired and the Gourmand frog cave is open"),
            // new IslandArchipelagoPower("Island Mailbox", ""),
            // new IslandArchipelagoPower("Farm Obelisk", ""),
            new IslandArchipelagoPower("Open Professor Snail Cave", "Professor Snail is free and returned to his field office"),
            new IslandArchipelagoPower("Island Trader", "The Island Trader has opened a shop on Island North"),
            // new IslandArchipelagoPower("Volcano Bridge", ""),
            // new IslandArchipelagoPower("Volcano Exit Shortcut", ""),
            new IslandArchipelagoPower("Island Resort", "The Island Resort is operational and the debris to Island SouthEast was cleared"),
            new IslandArchipelagoPower("Parrot Express", "The Parrot Express Transportation is available"),
            new IslandArchipelagoPower("Qi Walnut Room", "Yes, kid, you can come see me. I won't bite."),
        };

        private static Dictionary<string, ArchipelagoPower> _powersByName = _customPowers.ToDictionary(x => x.Name, x => x);

        public PowersModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
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

            var powerData = new PowersData()
            {
                DisplayName = customPower.Name,
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
                    if (!_powersByName.TryGetValue(powerIcon.name, out var customPower))
                    {
                        continue;
                    }

                    powerIcon.texture = customPower.Texture;
                    powerIcon.sourceRect = customPower.TextureRectangle;
                    powerIcon.scale = customPower.TextureScale;
                    powerIcon.baseScale = customPower.TextureScale;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PopulateClickableComponentList_AddTextures_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
