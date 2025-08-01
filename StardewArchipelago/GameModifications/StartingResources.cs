﻿using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Extensions;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewArchipelago.GameModifications
{
    public class StartingResources
    {
        private const int UNLIMITED_MONEY_AMOUNT = 9999999;
        private const int MINIMUM_UNLIMITED_MONEY = 1000000;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;

        public StartingResources(StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager stardewItemManager)
        {
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
        }

        public void GivePlayerStartingResources()
        {
            GivePlayerStartingMoney();
            if (Game1.Date.TotalDays == 0)
            {
                GivePlayerQuickStart();
                RemoveStartingTools();
            }

            RemoveShippingBin();
            RemovePetBowls();
            SendGilTelephoneLetter();
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
            var startCrop = _stardewItemManager.GetItemByName(GetStartingCropForThisSeason()).PrepareForGivingToFarmer(15);
            CreateGiftBoxItemInEmptySpot(farmhouse, startCrop);
            var telephone = _stardewItemManager.GetItemByName("Telephone").PrepareForGivingToFarmer();
            CreateGiftBoxItemInEmptySpot(farmhouse, telephone);
            var calendar = _stardewItemManager.GetItemByName("Calendar").PrepareForGivingToFarmer();
            CreateGiftBoxItemInEmptySpot(farmhouse, calendar);

            if (!_archipelago.SlotData.QuickStart)
            {
                var chest = _stardewItemManager.GetItemByName("Chest").PrepareForGivingToFarmer(1);
                CreateGiftBoxItemInEmptySpot(farmhouse, chest);
                return;
            }

            var chests = _stardewItemManager.GetItemByName("Chest").PrepareForGivingToFarmer(5);
            var iridiumBand = _stardewItemManager.GetItemByName("Iridium Band").PrepareForGivingToFarmer();
            var qualitySprinklers = _stardewItemManager.GetItemByName("Quality Sprinkler").PrepareForGivingToFarmer(4);
            var autoPetters = _stardewItemManager.GetItemByName("Auto-Petter").PrepareForGivingToFarmer(2);
            var autoGrabbers = _stardewItemManager.GetItemByName("Auto-Grabber").PrepareForGivingToFarmer(2);

            CreateGiftBoxItemInEmptySpot(farmhouse, chests);
            CreateGiftBoxItemInEmptySpot(farmhouse, iridiumBand);
            CreateGiftBoxItemInEmptySpot(farmhouse, qualitySprinklers);
            CreateGiftBoxItemInEmptySpot(farmhouse, autoPetters);
            CreateGiftBoxItemInEmptySpot(farmhouse, autoGrabbers);

#if TILESANITY
            var paths = _stardewItemManager.GetItemByName("Crystal Path").PrepareForGivingToFarmer(100);
            var seed_maker = _stardewItemManager.GetItemByName("Seed Maker").PrepareForGivingToFarmer(1);
            CreateGiftBoxItemInEmptySpot(farmhouse, paths);
            CreateGiftBoxItemInEmptySpot(farmhouse, seed_maker);
#endif
        }
        
        private void RemoveStartingTools()
        {
            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.NoStartingTools))
            {
                return;
            }

            Game1.player.Items.Clear();
            if (_archipelago.SlotData.BackpackProgression != BackpackProgression.Vanilla)
            {
                Game1.player.MaxItems = 0;
                while (Game1.player.Items.Count > Game1.player.MaxItems)
                {
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

        private void CreateGiftBoxItemInEmptySpot(FarmHouse farmhouse, Item itemToGift)
        {
            var origSpot = new Vector2(3f, 7f);
            var emptySpot = origSpot;
            var maxStep = 3;
            while (farmhouse.objects.ContainsKey(emptySpot))
            {
                emptySpot.X = emptySpot.X + 1;
                if (emptySpot.X > origSpot.X + maxStep)
                {
                    emptySpot.X = origSpot.X;
                    emptySpot.Y += 1;
                }

                if (emptySpot.Y > origSpot.Y + maxStep)
                {
                    emptySpot.Y = origSpot.Y - maxStep;
                }
            }

            farmhouse.objects.Add(emptySpot, new Chest(new List<Item> { itemToGift }, emptySpot, true, giftboxIsStarterGift: true));
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
    }
}
