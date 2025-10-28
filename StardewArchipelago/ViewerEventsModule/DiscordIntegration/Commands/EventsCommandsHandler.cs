using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StardewArchipelago.ViewerEventsModule.Events;
using StardewArchipelago.ViewerEventsModule.EventsExecution;

namespace StardewArchipelago.ViewerEventsModule.DiscordIntegration.Commands
{
    internal class EventsCommandsHandler
    {
        private readonly IBotCommunicator _communications;
        private readonly CommandReader _commandReader;
        private readonly HelpProvider _helpProvider;

        public EventsCommandsHandler(IBotCommunicator discord, CommandReader commandReader, HelpProvider helpProvider)
        {
            _communications = discord;
            _commandReader = commandReader;
            _helpProvider = helpProvider;
        }

        public void HandleEventsAdminCommands(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            HandleQueueEvent(message, messageText, eventExecutor);
            HandleTriggerEvent(message, messageText, eventExecutor);
            HandleSetBank(message, messageText, eventExecutor);
            HandleSetGlobalPriceMultiplier(message, messageText, eventExecutor);
            HandleGlobalPause(message, messageText, eventExecutor);
        }

        public async Task HandleEventsUserCommands(SocketUserMessage message, string messageText, CreditAccounts creditAccounts, ViewerEventsExecutor eventExecutor)
        {
            HandleCommandBank(message, messageText, eventExecutor);
            await HandleCommandPurchase(message, messageText, creditAccounts, eventExecutor);
            await HandleCommandPay(message, messageText, creditAccounts, eventExecutor);
            HandleGetGlobalPriceMultiplier(message, messageText, eventExecutor);
        }

        private void HandleQueueEvent(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!queueevent "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out string eventName))
            {
                _communications.ReplyTo(message, "Usage: !queueevent [eventName]");
                return;
            }

            var chosenEvent = eventExecutor.Events.GetEvent(eventName);

            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            eventExecutor.AddOrIncrementEventInQueue(message.Author.Username, chosenEvent);
            _communications.ReplyTo(message, $"Queued up one instance of {chosenEvent.name}.");
            eventExecutor.Queue.PrintToConsole();
        }

        private void HandleTriggerEvent(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!triggerevent "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out string eventName))
            {
                _communications.ReplyTo(message, "Usage: !triggerevent [eventName]");
                return;
            }

            var chosenEvent = eventExecutor.Events.GetEvent(eventName);

            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            var forcedEvent = new ViewerEvent();
            forcedEvent.name = eventName;
            forcedEvent.SetBank(1);
            forcedEvent.SetCost(1);
            var queuedForcedEvent = new QueuedEvent(forcedEvent);
            queuedForcedEvent.queueCount = 1;
            queuedForcedEvent.username = message.Author.Username;
            eventExecutor.Queue.PushAtBeginning(queuedForcedEvent);
            _communications.ReplyTo(message, $"Forced {forcedEvent.name} immediately.");

            eventExecutor.Queue.PrintToConsole();
        }

        private void HandleSetBank(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!setbank "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out string eventName, out var bankAmount))
            {
                _communications.ReplyTo(message, "Usage: !setbank [eventName] [bankAmount]");
                return;
            }

            if (bankAmount < 0)
            {
                _communications.ReplyTo(message, $"{bankAmount} is not a valid amount of credits");
                return;
            }

            eventName = eventName.ToLower().Replace(" ", "");

            foreach (var e in eventExecutor.Events.ToList())
            {
                if (e.name.ToLower().Replace(" ", "") == eventName)
                {
                    e.SetBank(bankAmount);
                    _communications.ReplyTo(message, $"{e.name} set to {e.GetBank()} credits.");
                }
            }
        }

        private void HandleSetGlobalPriceMultiplier(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!setmultiplier "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out double multiplier))
            {
                _communications.ReplyTo(message, "Usage: !setmultiplier [multiplier]");
                return;
            }

            eventExecutor.Events.CurrentMultiplier = multiplier;
            _communications.SetStatusMessage($"your donations. Price Multiplier: {eventExecutor.Events.CurrentMultiplier}", ActivityType.Listening);
            _communications.ReplyTo(message, $"Set global event price multiplier to {multiplier}");
            _helpProvider.SendAllEventsHelpMessages(eventExecutor.Events);
        }

        private void HandleGlobalPause(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (messageText.StartsWith("!pause"))
            {
                eventExecutor.Queue.Pause();
                _communications.ReplyTo(message, $"All eventExecutor.Events are now paused");
                return;
            }

            if (messageText.StartsWith("!unpause") || messageText.StartsWith("!resume"))
            {
                eventExecutor.Queue.Unpause();
                _communications.ReplyTo(message, $"All eventExecutor.Events are now resumed");
                return;
            }
        }

        private void HandleGetGlobalPriceMultiplier(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.Equals("!prices"))
            {
                return;
            }

            _communications.ReplyTo(message, $"The global event price is currently {eventExecutor.Events.CurrentMultiplier}");
        }

        private void HandleCommandBank(SocketUserMessage message, string messageText, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!bank "))
            {
                return;
            }

            if (!_commandReader.IsCommandValid(messageText, out string eventName))
            {
                _communications.ReplyTo(message, "Usage: !bank [eventName]");
                return;
            }

            CheckEventBank(message, eventExecutor, eventName);
        }

        private async Task HandleCommandPurchase(SocketUserMessage message, string messageText, CreditAccounts creditAccounts, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!purchase "))
            {
                return;
            }

            Console.WriteLine("Purchase: " + messageText);

            if (!_commandReader.IsCommandValid(messageText, out string eventName))
            {
                _communications.ReplyTo(message, "Usage: !purchase [eventName]");
                return;
            }

            var chosenEvent = eventExecutor.Events.GetEvent(eventName);
            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            var costForNextStack = chosenEvent.GetCostToNextActivation(eventExecutor.Events.CurrentMultiplier);

            LogPay(message.Author.Username, costForNextStack);
            await PayForEvent(message, creditAccounts, eventExecutor, chosenEvent, costForNextStack);
        }

        private async Task HandleCommandPay(SocketUserMessage message, string messageText, CreditAccounts creditAccounts, ViewerEventsExecutor eventExecutor)
        {
            if (!messageText.StartsWith("!pay "))
            {
                return;
            }

            Console.WriteLine("Pay: " + messageText);
            if (!_commandReader.IsCommandValid(messageText, out string eventName, out var creditsToPay))
            {
                _communications.ReplyTo(message, "Usage: !pay [eventName] [creditAmount]");
                return;
            }

            LogPay(message.Author.Username, creditsToPay);

            if (creditsToPay <= 0)
            {
                _communications.ReplyTo(message, $"{creditsToPay} is not a valid amount of credits");
                return;
            }

            await PayForEvent(message, creditAccounts, eventExecutor, eventName, creditsToPay);
        }

        private async Task PayForEvent(SocketUserMessage message, CreditAccounts creditAccounts, ViewerEventsExecutor eventExecutor, string eventName, int creditsToPay)
        {
            var chosenEvent = eventExecutor.Events.GetEvent(eventName);
            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            await PayForEvent(message, creditAccounts, eventExecutor, chosenEvent, creditsToPay);
        }

        private async Task PayForEvent(SocketUserMessage message, CreditAccounts creditAccounts, ViewerEventsExecutor eventExecutor, ViewerEvent chosenEvent, int creditsToPay)
        {
            var userAccount = creditAccounts[message.Author.Id];

            if (!chosenEvent.IsStackable())
            {
                var costToNextActivation = chosenEvent.GetCostToNextActivation(eventExecutor.Events.CurrentMultiplier);
                if (creditsToPay > costToNextActivation)
                {
                    creditsToPay = costToNextActivation;
                }
            }

            if (creditsToPay > userAccount.GetCredits())
            {
                _communications.ReplyTo(message, $"You cannot afford to pay {creditsToPay} credits. Balance: {userAccount.GetCredits()}");
                return;
            }

            chosenEvent.AddToBank(creditsToPay);
            userAccount.RemoveCredits(creditsToPay);

            var numberOfActivations = TriggerEventAsNeeded(message.Author.Username, chosenEvent, eventExecutor);

            if (numberOfActivations > 0)
            {
                _communications.ReplyTo(message,
                    $"Received {creditsToPay} credits from {message.Author.Username} to activate {chosenEvent.name} {numberOfActivations} times!");
            }
            else
            {
                _communications.ReplyTo(message,
                    $"Received {creditsToPay} credits from {message.Author.Username}.  {chosenEvent.name} is now at {chosenEvent.GetBank()}/{chosenEvent.GetMultiplierCost(eventExecutor.Events.CurrentMultiplier)}.");
            }

            eventExecutor.Queue.PrintToConsole();
        }

        private int TriggerEventAsNeeded(string senderName, ViewerEvent chosenEvent, ViewerEventsExecutor eventExecutor)
        {
            var numberOfActivations = 0;
            while (chosenEvent.GetBank() >= chosenEvent.GetMultiplierCost(eventExecutor.Events.CurrentMultiplier))
            {
                chosenEvent.CallEvent(eventExecutor.Events.CurrentMultiplier);
                LogEvent(senderName, chosenEvent);
                eventExecutor.AddOrIncrementEventInQueue(senderName, chosenEvent);
                numberOfActivations++;
            }

            return numberOfActivations;
        }

        public void LogEvent(string senderName, ViewerEvent calledEvent)
        {
            var localDate = DateTime.Now;
            using var w = File.AppendText("EventLog.txt");
            w.WriteLine($"[{localDate}] {senderName} activated {calledEvent.name}.");
        }

        public void LogPay(string senderName, int payAmount)
        {
            var localDate = DateTime.Now;
            using var w = File.AppendText("PayLog.txt");
            w.WriteLine($"[{localDate}] {senderName} paid {payAmount} credits.");
        }

        private void CheckEventBank(SocketUserMessage message, ViewerEventsExecutor eventExecutor, string eventName)
        {
            var invokedEvent = eventExecutor.Events.GetEvent(eventName);
            if (invokedEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid Event.");
                return;
            }

            _communications.ReplyTo(message, $"{invokedEvent.name} is at {invokedEvent.GetBank()}/{invokedEvent.GetMultiplierCost(eventExecutor.Events.CurrentMultiplier)} credits.");
        }
    }
}