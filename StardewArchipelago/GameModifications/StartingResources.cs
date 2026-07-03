using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Extensions;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewArchipelago.GameModifications
{
    public class StartingResources
    {
        private const int UNLIMITED_MONEY_AMOUNT = 9999999;
        private const int MINIMUM_UNLIMITED_MONEY = 1000000;
        private readonly ILogger _logger;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;

        public StartingResources(ILogger logger, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
        }

        public void GivePlayerStartingResources()
        {
            _logger.LogDebug($"Starting {nameof(GivePlayerStartingResources)} (Game1.Date.TotalDays: {Game1.Date.TotalDays})");
            GivePlayerStartingMoney();
            if (Game1.Date.TotalDays == 0)
            {
                _logger.LogDebug($"It's the first day! Taking care of Quick Start, Tools and Backpacks!");
                GivePlayerQuickStart();
                RemoveStartingTools();
                RemoveStartingBackpack();
            }

            RemoveHouse();
            RemoveShippingBin();
            RemovePetBowls();
            SendGilTelephoneLetter();
            GrantStartingQuest();
        }

        private void GivePlayerStartingMoney()
        {
            var startingMoney = _archipelago.SlotData.StartingMoney;
            var isUnlimitedMoney = startingMoney.IsUnlimited();
            if (isUnlimitedMoney)
            {
                startingMoney = UNLIMITED_MONEY_AMOUNT;
            }

            if (Game1.Date.TotalDays == 0 || (isUnlimitedMoney && Game1.player.Money < MINIMUM_UNLIMITED_MONEY))
            {
                Game1.player.Money = startingMoney;
            }
        }

        private void GivePlayerQuickStart()
        {
            if (Game1.getLocationFromName("FarmHouse") is not FarmHouse farmhouse)
            {
                return;
            }

            RemoveGiftBoxes(farmhouse);

            var (location, originalTile) = GetGiftboxPlacement(farmhouse);

            var startCrop = _stardewItemManager.GetItemByName(GetStartingCropForThisSeason()).PrepareForGivingToFarmer(15);
            CreateGiftBoxItemInEmptySpot(location, originalTile, startCrop);
            var telephone = _stardewItemManager.GetItemByName("Telephone").PrepareForGivingToFarmer();
            CreateGiftBoxItemInEmptySpot(location, originalTile, telephone);
            var calendar = _stardewItemManager.GetItemByName("Calendar").PrepareForGivingToFarmer();
            CreateGiftBoxItemInEmptySpot(location, originalTile, calendar);

            if (!_archipelago.SlotData.QuickStart)
            {
                var chest = _stardewItemManager.GetItemByName("Chest").PrepareForGivingToFarmer(1);
                CreateGiftBoxItemInEmptySpot(location, originalTile, chest);
                return;
            }

            var chests = _stardewItemManager.GetItemByName("Chest").PrepareForGivingToFarmer(5);
            var iridiumBand = _stardewItemManager.GetItemByName("Iridium Band").PrepareForGivingToFarmer();
            var qualitySprinklers = _stardewItemManager.GetItemByName("Quality Sprinkler").PrepareForGivingToFarmer(4);
            var autoPetters = _stardewItemManager.GetItemByName("Auto-Petter").PrepareForGivingToFarmer(2);
            var autoGrabbers = _stardewItemManager.GetItemByName("Auto-Grabber").PrepareForGivingToFarmer(2);

            CreateGiftBoxItemInEmptySpot(location, originalTile, chests);
            CreateGiftBoxItemInEmptySpot(location, originalTile, iridiumBand);
            CreateGiftBoxItemInEmptySpot(location, originalTile, qualitySprinklers);
            CreateGiftBoxItemInEmptySpot(location, originalTile, autoPetters);
            CreateGiftBoxItemInEmptySpot(location, originalTile, autoGrabbers);

#if TILESANITY
            var paths = _stardewItemManager.GetItemByName("Crystal Path").PrepareForGivingToFarmer(100);
            var seed_maker = _stardewItemManager.GetItemByName("Seed Maker").PrepareForGivingToFarmer(1);
            CreateGiftBoxItemInEmptySpot(location, originalTile, paths);
            CreateGiftBoxItemInEmptySpot(location, originalTile, seed_maker);
#endif
        }

        private (GameLocation, Vector2) GetGiftboxPlacement(FarmHouse farmhouse)
        {
            GameLocation location = farmhouse;
            var originalTile = new Vector2(3f, 7f);

            if (_archipelago.SlotData.StartWithout.HasFlag(StartWithout.House))
            {
                var farm = (Farm)Game1.RequireLocation("Farm");
                location = farm;
                var startingTile = farm.GetMainFarmHouseEntry();
                originalTile = new Vector2(startingTile.X - 2, startingTile.Y + 3);
            }
            return (location, originalTile);
        }

        private void RemoveStartingTools()
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.Tools))
            {
                return;
            }

            Game1.player.Items.Clear();
        }

        private void RemoveStartingBackpack()
        {
            if (!_archipelago.SlotData.StartWithout.HasFlag(StartWithout.Backpack))
            {
                return;
            }

            if (_archipelago.SlotData.BackpackProgression != BackpackProgression.Vanilla)
            {
                Game1.player.MaxItems = 0;
                var farmhouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                var (location, originalTile) = GetGiftboxPlacement(farmhouse);
                while (Game1.player.Items.Count > Game1.player.MaxItems)
                {
                    var item = Game1.player.Items[0];
                    if (item != null)
                    {
                        CreateGiftBoxItemInEmptySpot(location, originalTile, item);
                        Game1.player.Items[0] = null;
                    }
                    Game1.player.Items.RemoveAt(0);
                }
            }
        }

        private void RemoveGiftBoxes(FarmHouse farmhouse)
        {
            foreach (var position in farmhouse.Objects.Keys.ToList())
            {
                if (!(farmhouse.Objects[position] is Chest chest) || !chest.giftbox.Value)
                {
                    continue;
                }

                farmhouse.Objects.Remove(position);
            }
        }

        private string GetStartingCropForThisSeason()
        {
            if (_archipelago.SlotData.FarmType.GetWhichFarm() == (int)SupportedFarmType.Meadowlands)
            {
                return "Hay";
            }

            if (_archipelago.SlotData.Cropsanity == Cropsanity.Shuffled)
            {
                return "Mixed Seeds";
            }

            return Game1.currentSeason switch
            {
                "spring" => "Parsnip Seeds",
                "summer" => "Wheat Seeds",
                "fall" => "Bok Choy Seeds",
                "winter" => "Winter Seeds",
                _ => "Mixed Seeds",
            };
        }

        private void CreateGiftBoxItemInEmptySpot(GameLocation map, Vector2 originTile, Item itemToGift)
        {
            var emptySpot = originTile;
            var maxStep = 3;
            while (map.objects.ContainsKey(emptySpot) || map.IsTileBlockedBy(emptySpot) || map.IsTileOccupiedBy(emptySpot))
            {
                emptySpot.X = emptySpot.X + 1;
                if (emptySpot.X > originTile.X + maxStep)
                {
                    emptySpot.X = originTile.X;
                    emptySpot.Y += 1;
                }

                if (emptySpot.Y > originTile.Y + maxStep)
                {
                    emptySpot.Y = originTile.Y - maxStep;
                }
            }

            map.objects.Add(emptySpot, new Chest(new List<Item> { itemToGift }, emptySpot, true, giftboxIsStarterGift: true));
        }

        private void RemoveHouse()
        {
            if (!_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive) || _archipelago.HasReceivedItem(CarpenterInjections.BUILDING_PROGRESSIVE_HOUSE))
            {
                return;
            }

            var farm = Game1.getFarm();
            var houses = FarmInjections.FindHouses(farm);
            foreach (var house in houses)
            {
                house.BeforeDemolish();
                farm.destroyStructure(house);
            }
        }

        private void RemoveShippingBin()
        {
            if (!_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive) || _archipelago.HasReceivedItem("Shipping Bin"))
            {
                return;
            }

            var farm = Game1.getFarm();
            var shippingBins = FarmInjections.FindShippingBins(farm);
            foreach (var shippingBin in shippingBins)
            {
                shippingBin.BeforeDemolish();
                farm.destroyStructure(shippingBin);
            }
        }

        private void RemovePetBowls()
        {
            if (!_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive) || _archipelago.HasReceivedItem("Pet Bowl"))
            {
                return;
            }

            var farm = Game1.getFarm();
            var petBowls = FarmInjections.FindPetBowls(farm);
            foreach (var petBowl in petBowls)
            {
                petBowl.BeforeDemolish();
                farm.destroyStructure(petBowl);
            }
        }

        private void SendGilTelephoneLetter()
        {
            const string mailId = "Gil_Telephone";
            if (Game1.player.hasOrWillReceiveMail(mailId))
            {
                return;
            }
            Game1.player.mailReceived.Add(mailId);
        }

        public void GrantStartingQuest()
        {
            Game1.player.addQuest(Game1.GetFarmTypeID() == "MeadowlandsFarm" ? "132" : "6");
            Game1.dayTimeMoneyBox.PingQuestLog();
        }
    }
}
