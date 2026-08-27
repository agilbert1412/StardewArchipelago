using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Serialization;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Internal;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace StardewArchipelago.Items.Traps
{
    public class DebtManager
    {
        public static readonly Color JOJA_COLOR = new Color(61, 105, 168);
        private const int MINIMUM_PAYMENT = 10;
        private const int ALWAYS_ALLOWED_PAYMENT = 1000;
        private const double MINIMUM_PAYMENT_RATE = 0.05;
        private const double MAXIMUM_PAYMENT_RATE = 0.5;
        private const double INTEREST_RATE = 0.15;
        private TrapsStateDto _permanentState;

        public DebtManager(TrapsStateDto permanentState)
        {
            _permanentState = permanentState;
        }

        public async Task DayUpdateDebt()
        {
            if (_permanentState.CurrentDebt <= 0)
            {
                return;
            }

            var minimumPayment = Ceiling(Math.Max(MINIMUM_PAYMENT, _permanentState.CurrentDebt * MINIMUM_PAYMENT_RATE));
            var currentMoney = Game1.player.Money;
            var expectedInterests = Ceiling(_permanentState.CurrentDebt * INTEREST_RATE);
            var maximumPayment = Math.Min(_permanentState.CurrentDebt, Ceiling(Math.Max(ALWAYS_ALLOWED_PAYMENT, _permanentState.CurrentDebt * MAXIMUM_PAYMENT_RATE)) + expectedInterests);

            await AddMessageAndWait($"[50] Current Debt: {_permanentState.CurrentDebt}g (Interest Rate: {INTEREST_RATE*100}%)");
            await AddMessageAndWait($"Minimum Payment: {minimumPayment}g");
            await AddMessageAndWait($"Maximum Payment: {maximumPayment}g");

            var payment = Math.Max(minimumPayment, Math.Min(Ceiling(currentMoney*0.75), maximumPayment));
            var addedDebt = 0;
            if (payment > currentMoney)
            {
                addedDebt = (payment - currentMoney) * 2;
                payment = currentMoney;
                await AddMessageAndWait($"Cannot afford minimum payment. A {addedDebt}g fee will be charged to your account");
            }

            Game1.player.Money -= payment;
            _permanentState.CurrentDebt = _permanentState.CurrentDebt - payment + addedDebt;
            await AddMessageAndWait($"Paid: {payment}g");
            var interests = Ceiling(_permanentState.CurrentDebt * INTEREST_RATE);
            await AddMessageAndWait($"Interests Added: {interests}g");
            _permanentState.CurrentDebt += interests;
            await AddMessageAndWait($"Remaining Debt: {_permanentState.CurrentDebt}g. Thank you for financing with [50]oja Capital");
            if (_permanentState.CurrentDebt > Game1.player.totalMoneyEarned || _permanentState.CurrentDebt > 1000000)
            {
                await AddMessageAndWait($"If you wish to declare bankruptcy, you can request it with the command {ChatForwarder.COMMAND_PREFIX}bankruptcy");
            }
        }

        private async Task AddMessageAndWait(string message, double delayInSeconds = 0.5)
        {
            Game1.chatBox.addMessage(message, JOJA_COLOR);
            await Task.Run(() => Thread.Sleep(Round(delayInSeconds * 1000)));
        }

        public int GetInterest()
        {
            return Ceiling(_permanentState.CurrentDebt * INTEREST_RATE);
        }

        private int Ceiling(double val)
        {
            return (int)Math.Ceiling(val);
        }

        private int Round(double val)
        {
            return (int)Math.Round(val);
        }

        private int Floor(double val)
        {
            return (int)Math.Floor(val);
        }

        public void PerformBankruptcy()
        {
            bool DeleteNonChestItems(in ForEachItemContext itemHandler)
            {
                var item = itemHandler.Item;
                if (item.canBeShipped() || item.canBeDropped() || item.canBeTrashed() || item.CanBeLostOnDeath())
                {
                    if (item is Chest || item.specialItem || item.isLostItem || (item is Object obj && obj.questItem.Value))
                    {
                        return true;
                    }
                    itemHandler.RemoveItem();
                }

                return true;
            }

            bool DeleteChests(in ForEachItemContext itemHandler)
            {
                var item = itemHandler.Item;
                if (item is not Chest chest)
                {
                    return true;
                }

                foreach (var chestItem in chest.Items)
                {
                    if (chestItem != null)
                    {
                        var remainder = Game1.player.addItemToInventory(chestItem);
                        if (remainder != null)
                        {
                            Game1.createItemDebris(chestItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation, flopFish: true);
                        }
                    }
                }

                itemHandler.RemoveItem();
                return true;
            }

            Utility.ForEachItemContext(DeleteNonChestItems);
            Utility.ForEachItemContext(DeleteChests);
            var allBuildings = new List<Building>();
            Utility.ForEachBuilding(building =>
            {
                if (building is GreenhouseBuilding)
                {
                    return true;
                }
                allBuildings.Add(building);
                return true;
            });
            foreach (var building in allBuildings)
            {
                var map = building.GetParentLocation();
                var indoors = building.GetIndoors();
                if (indoors != null)
                {
                    foreach (var animal in indoors.Animals.Values.ToArray())
                    {
                        ((AnimalHouse)animal.homeInterior).animalsThatLiveHere.Remove(animal.myID.Value);
                        animal.health.Value = -1;
                        if (animal.foundGrass != null && FarmAnimal.reservedGrass.Contains(animal.foundGrass))
                        {
                            FarmAnimal.reservedGrass.Remove(animal.foundGrass);
                        }
                    }
                }
                if (map is Farm farm)
                {
                    building.BeforeDemolish();
                    farm.destroyStructure(building);
                }
            }

            Game1.player.Equip(null, Game1.player.shirtItem);
            Game1.player.Equip(null, Game1.player.pantsItem);
            OutfitChanger.GiveBackPanIfWearingIt();
            Game1.player.Equip(null, Game1.player.hat);
            Game1.player.Equip(null, Game1.player.boots);
            Game1.player.Equip(null, Game1.player.leftRing);
            Game1.player.Equip(null, Game1.player.rightRing);
            Game1.player.changeHairStyle(BundleCurrencyManager.BALD_HAIR);

            Game1.player.Money = 0;
            _permanentState.CurrentDebt = 0;
        }
    }
}
