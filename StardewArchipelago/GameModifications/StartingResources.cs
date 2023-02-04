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
        private ArchipelagoClient _archipelago;
        private StardewItemManager _stardewItemManager;

        public StartingResources(ArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
        }

        public void GivePlayerStartingResources()
        {
            if (Game1.Date.TotalDays == 0)
            {
                Game1.player.Money = _archipelago.SlotData.StartingMoney;
                GivePlayerQuickStart();
                RemoveShippingBin();
            }

            SendGilTelephoneLetter();
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

            var iridiumSprinklers = _stardewItemManager.GetItemByName("Quality Sprinkler").PrepareForGivingToFarmer(4);
            var iridiumBand = _stardewItemManager.GetItemByName("Iridium Band").PrepareForGivingToFarmer(4);
            var autoPetters = _stardewItemManager.GetItemByName("Auto-Petter").PrepareForGivingToFarmer(2);
            var autoGrabbers = _stardewItemManager.GetItemByName("Auto-Grabber").PrepareForGivingToFarmer(2);

            CreateGiftBoxItemInEmptySpot(farmhouse, iridiumSprinklers);
            CreateGiftBoxItemInEmptySpot(farmhouse, iridiumBand);
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
