using Discord.WebSocket;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.ViewerEventsModule.Events;
using StardewValley;
using System;
using System.Threading.Tasks;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.ViewerEventsModule.EventsExecution
{
    public class ViewerEventsExecutor
    {
        private readonly ILogger _logger;
        public EventCollection Events { get; }
        public EventQueue Queue { get; }

        public ViewerEventsExecutor(ILogger logger)
        {
            _logger = logger;
            Events = new EventCollection();
            Queue = new EventQueue(logger);

            _logger.LogInfo(Events.Count + " is the total events count.");
        }

        public void DequeueEvent()
        {
            if (Queue.IsEmpty || Queue.IsPaused())
            {
                return;
            }

            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                return;
            }

            var eventToSend = Queue.First;
            var baseEvent = eventToSend.BaseEvent;
            var baseEventName = baseEvent.name;
            _logger.LogInfo($"Attempting to dequeue {eventToSend.queueCount} instances of {eventToSend.baseEventName} triggered by {eventToSend.username}.");
            Queue.RemoveFirst();
            Queue.PrintToConsole();

            var eventToExecute = Events.GetEvent(baseEventName);
            var executableEvent = eventToSend.GetExecutableEvent();
            executableEvent.Execute();
        }

        public void AddOrIncrementEventInQueue(string senderName, ViewerEvent chosenEvent)
        {
            var isInQueue = false;
            foreach (var qe in Queue)
            {
                if (qe.baseEventName == chosenEvent.name)
                {
                    qe.queueCount += 1;

                    isInQueue = true;
                    Console.WriteLine($"Increased queue count of {chosenEvent.name} to {qe.queueCount}.");
                }
            }

            AddEventToQueueIfNeeded(senderName, isInQueue, chosenEvent);
        }

        private void AddEventToQueueIfNeeded(string senderName, bool isInQueue, ViewerEvent chosenEvent)
        {
            if (isInQueue)
            {
                return;
            }

            var invokedEvent = new QueuedEvent(chosenEvent);
            invokedEvent.username = senderName;

            Queue.QueueEvent(invokedEvent);
            invokedEvent.queueCount = 1;

            Console.WriteLine($"Added {invokedEvent.baseEventName} to queue.");
        }
    }
}
