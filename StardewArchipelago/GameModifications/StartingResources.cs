using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
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
        private ArchipelagoClient _archipelago;
        private StardewItemManager _stardewItemManager;

        public StartingResources(ArchipelagoClient archipelago, StardewItemManager stardewItemManager)
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
                RemoveShippingBin();
            }

            SendGilTelephoneLetter();
        }

        private void GivePlayerStartingMoney()
        {
            var startingMoney = _archipelago.SlotData.StartingMoney;
            var isUnlimitedMoney = startingMoney < 0;
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
            if (!_archipelago.SlotData.QuickStart)
            {
                return;
            }

            if (Game1.getLocationFromName("FarmHouse") is not FarmHouse farmhouse)
            {
                return;
            }

            var chest = _stardewItemManager.GetItemByName("Chest").PrepareForGivingToFarmer(1);
            var iridiumBand = _stardewItemManager.GetItemByName("Iridium Band").PrepareForGivingToFarmer(4);
            var qualitySprinklers = _stardewItemManager.GetItemByName("Quality Sprinkler").PrepareForGivingToFarmer(4);
            var autoPetters = _stardewItemManager.GetItemByName("Auto-Petter").PrepareForGivingToFarmer(2);
            var autoGrabbers = _stardewItemManager.GetItemByName("Auto-Grabber").PrepareForGivingToFarmer(2);

            CreateGiftBoxItemInEmptySpot(farmhouse, chest);
            CreateGiftBoxItemInEmptySpot(farmhouse, iridiumBand);
            CreateGiftBoxItemInEmptySpot(farmhouse, qualitySprinklers);
            CreateGiftBoxItemInEmptySpot(farmhouse, autoPetters);
            CreateGiftBoxItemInEmptySpot(farmhouse, autoGrabbers);
        }

        private void RemoveShippingBin()
        {
            if (_archipelago.SlotData.BuildingProgression == BuildingProgression.Vanilla)
            {
                return;
            }

            var farm = Game1.getFarm();
            ShippingBin shippingBin = null;
            foreach (var building in Game1.getFarm().buildings)
            {
                if (building is ShippingBin bin)
                {
                    shippingBin = bin;
                    break;
                }
            }

            shippingBin.BeforeDemolish();
            farm.destroyStructure(shippingBin);
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

            farmhouse.objects.Add(emptySpot, new Chest(0, new List<Item>()
            {
                itemToGift
            }, emptySpot, true));
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
