using Microsoft.Xna.Framework;
using StardewArchipelago.Serialization;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StardewArchipelago.Items.Traps
{
    public class DebtManager
    {
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

            await AddMessageAndWait($"Current Debt: {_permanentState.CurrentDebt}g (Interest Rate: {INTEREST_RATE*100}%)");
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
            await AddMessageAndWait($"Remaining Debt: {_permanentState.CurrentDebt}g");
        }

        private async Task AddMessageAndWait(string message, double delayInSeconds = 0.5)
        {
            Game1.chatBox.addMessage(message, Color.Blue);
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
    }
}
