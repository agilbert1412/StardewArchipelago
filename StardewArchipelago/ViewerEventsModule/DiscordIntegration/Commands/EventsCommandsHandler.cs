using Discord;
using Discord.WebSocket;
using StardewArchipelago.ViewerEventsModule.Events;

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

        public void HandleEventsAdminCommands(SocketUserMessage message, string messageText, EventCollection events, EventQueue eventQueue)
        {
            HandleQueueEvent(message, messageText, events, eventQueue);
            HandleTriggerEvent(message, messageText, events, eventQueue);
            HandleSetBank(message, messageText, events);
            HandleSetGlobalPriceMultiplier(message, messageText, events);
            HandleGlobalPause(message, messageText, eventQueue);
        }

        public async Task HandleEventsUserCommands(SocketUserMessage message, string messageText, CreditAccounts creditAccounts, EventCollection events,
            EventQueue eventQueue)
        {
            HandleCommandBank(message, messageText, events);
            await HandleCommandPurchase(message, messageText, creditAccounts, events, eventQueue);
            await HandleCommandPay(message, messageText, creditAccounts, events, eventQueue);
            HandleGetGlobalPriceMultiplier(message, messageText, events);
        }

        private void HandleQueueEvent(SocketUserMessage message, string messageText, EventCollection events, EventQueue eventQueue)
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

            var chosenEvent = events.GetEvent(eventName);

            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            AddOrIncrementEventInQueue(message.Author.Username, chosenEvent, eventQueue);
            _communications.ReplyTo(message, $"Queued up one instance of {chosenEvent.name}.");
            eventQueue.PrintToConsole();
        }

        private void HandleTriggerEvent(SocketUserMessage message, string messageText, EventCollection events, EventQueue eventQueue)
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

            var chosenEvent = events.GetEvent(eventName);

            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            var forcedEvent = new Event();
            forcedEvent.name = eventName;
            forcedEvent.SetBank(1);
            forcedEvent.SetCost(1);
            var queuedForcedEvent = new QueuedEvent(forcedEvent);
            queuedForcedEvent.queueCount = 1;
            queuedForcedEvent.username = message.Author.Username;
            eventQueue.PushAtBeginning(queuedForcedEvent);
            _communications.ReplyTo(message, $"Forced {forcedEvent.name} immediately.");

            eventQueue.PrintToConsole();
        }

        private void HandleSetBank(SocketUserMessage message, string messageText, EventCollection events)
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

            foreach (var e in events.ToList())
            {
                if (e.name.ToLower().Replace(" ", "") == eventName)
                {
                    e.SetBank(bankAmount);
                    _communications.ReplyTo(message, $"{e.name} set to {e.GetBank()} credits.");
                }
            }
        }

        private void HandleSetGlobalPriceMultiplier(SocketUserMessage message, string messageText, EventCollection events)
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

            events.CurrentMultiplier = multiplier;
            _communications.SetStatusMessage($"your donations. Price Multiplier: {events.CurrentMultiplier}", ActivityType.Listening);
            _communications.ReplyTo(message, $"Set global event price multiplier to {multiplier}");
            _helpProvider.SendAllEventsHelpMessages(events);
        }

        private void HandleGlobalPause(SocketUserMessage message, string messageText, EventQueue eventQueue)
        {
            if (messageText.StartsWith("!pause"))
            {
                eventQueue.Pause();
                _communications.ReplyTo(message, $"All events are now paused");
                return;
            }

            if (messageText.StartsWith("!unpause") || messageText.StartsWith("!resume"))
            {
                eventQueue.Unpause();
                _communications.ReplyTo(message, $"All events are now resumed");
                return;
            }
        }

        private void HandleGetGlobalPriceMultiplier(SocketUserMessage message, string messageText, EventCollection events)
        {
            if (!messageText.Equals("!prices"))
            {
                return;
            }

            _communications.ReplyTo(message, $"The global event price is currently {events.CurrentMultiplier}");
        }

        private void HandleCommandBank(SocketUserMessage message, string messageText, EventCollection events)
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

            CheckEventBank(message, events, eventName);
        }

        private async Task HandleCommandPurchase(SocketUserMessage message, string messageText, CreditAccounts creditAccounts, EventCollection events,
            EventQueue eventQueue)
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

            var chosenEvent = events.GetEvent(eventName);
            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            var costForNextStack = chosenEvent.GetCostToNextActivation(events.CurrentMultiplier);

            LogPay(message.Author.Username, costForNextStack);
            await PayForEvent(message, creditAccounts, events, eventQueue, chosenEvent, costForNextStack);
        }

        private async Task HandleCommandPay(SocketUserMessage message, string messageText, CreditAccounts creditAccounts, EventCollection events, EventQueue eventQueue)
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

            await PayForEvent(message, creditAccounts, events, eventQueue, eventName, creditsToPay);
        }

        private async Task PayForEvent(SocketUserMessage message, CreditAccounts creditAccounts, EventCollection events,
            EventQueue eventQueue, string eventName, int creditsToPay)
        {
            var chosenEvent = events.GetEvent(eventName);
            if (chosenEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid event");
                return;
            }

            await PayForEvent(message, creditAccounts, events, eventQueue, chosenEvent, creditsToPay);
        }

        private async Task PayForEvent(SocketUserMessage message, CreditAccounts creditAccounts, EventCollection events,
            EventQueue eventQueue, Event chosenEvent, int creditsToPay)
        {
            var userAccount = creditAccounts[message.Author.Id];

            if (!chosenEvent.IsStackable())
            {
                var costToNextActivation = chosenEvent.GetCostToNextActivation(events.CurrentMultiplier);
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

            var numberOfActivations = TriggerEventAsNeeded(message.Author.Username, chosenEvent, eventQueue, events);

            if (numberOfActivations > 0)
            {
                _communications.ReplyTo(message,
                    $"Received {creditsToPay} credits from {message.Author.Username} to activate {chosenEvent.name} {numberOfActivations} times!");
            }
            else
            {
                _communications.ReplyTo(message,
                    $"Received {creditsToPay} credits from {message.Author.Username}.  {chosenEvent.name} is now at {chosenEvent.GetBank()}/{chosenEvent.GetMultiplierCost(events.CurrentMultiplier)}.");
            }

            eventQueue.PrintToConsole();
        }

        private int TriggerEventAsNeeded(string senderName, Event chosenEvent, EventQueue eventQueue, EventCollection events)
        {
            var numberOfActivations = 0;
            while (chosenEvent.GetBank() >= chosenEvent.GetMultiplierCost(events.CurrentMultiplier))
            {
                chosenEvent.CallEvent(events.CurrentMultiplier);
                LogEvent(senderName, chosenEvent);
                AddOrIncrementEventInQueue(senderName, chosenEvent, eventQueue);
                numberOfActivations++;
            }

            return numberOfActivations;
        }

        private void AddOrIncrementEventInQueue(string senderName, Event chosenEvent, EventQueue eventQueue)
        {
            var isInQueue = false;
            foreach (var qe in eventQueue)
            {
                if (qe.baseEventName == chosenEvent.name)
                {
                    qe.queueCount += 1;

                    isInQueue = true;
                    Console.WriteLine($"Increased queue count of {chosenEvent.name} to {qe.queueCount}.");
                }
            }

            AddEventToQueueIfNeeded(senderName, isInQueue, chosenEvent, eventQueue);
        }

        private void AddEventToQueueIfNeeded(string senderName, bool isInQueue, Event chosenEvent, EventQueue eventQueue)
        {
            if (isInQueue)
            {
                return;
            }

            var invokedEvent = new QueuedEvent(chosenEvent);
            invokedEvent.username = senderName;

            eventQueue.QueueEvent(invokedEvent);
            invokedEvent.queueCount = 1;

            Console.WriteLine($"Added {invokedEvent.baseEventName} to queue.");
        }

        public void LogEvent(string senderName, Event calledEvent)
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

        private void CheckEventBank(SocketUserMessage message, EventCollection events, string eventName)
        {
            var invokedEvent = events.GetEvent(eventName);
            if (invokedEvent == null)
            {
                _communications.ReplyTo(message, $"{eventName} is not a valid Event.");
                return;
            }

            _communications.ReplyTo(message, $"{invokedEvent.name} is at {invokedEvent.GetBank()}/{invokedEvent.GetMultiplierCost(events.CurrentMultiplier)} credits.");
        }
    }
}